// 
// Copyright (c) 2023-2024 REghZy
// 
// This file is part of PicNetStudio.
// 
// PicNetStudio is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
// 
// PicNetStudio is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with PicNetStudio. If not, see <https://www.gnu.org/licenses/>.
// 

using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using PicNetStudio.Avalonia.Interactivity;
using PicNetStudio.Avalonia.Utils;
using PicNetStudio.Avalonia.Utils.Collections.Observable;

namespace PicNetStudio.Avalonia.PicNet.Layers.Controls;

public abstract class BaseLayerTreeView : TreeView {
    public static readonly StyledProperty<bool> IsDroppableTargetOverProperty = AvaloniaProperty.Register<BaseLayerTreeView, bool>("IsDroppableTargetOver");
    public static readonly StyledProperty<Canvas?> CanvasProperty = AvaloniaProperty.Register<BaseLayerTreeView, Canvas?>("Canvas");

    internal readonly Stack<BaseLayerTreeViewItem> itemCache;
    private IDisposable? collectionChangeListener;
    private bool isProcessingAsyncDrop;
    internal readonly Dictionary<BaseLayerTreeViewItem, BaseLayerTreeObject> controlToModel;
    internal readonly Dictionary<BaseLayerTreeObject, BaseLayerTreeViewItem> modelToControl;
    private readonly AvaloniaList<BaseLayerTreeViewItem> selectedItemsList;

    public bool IsDroppableTargetOver {
        get => this.GetValue(IsDroppableTargetOverProperty);
        set => this.SetValue(IsDroppableTargetOverProperty, value);
    }

    public Canvas? Canvas {
        get => this.GetValue(CanvasProperty);
        set => this.SetValue(CanvasProperty, value);
    }


    public BaseLayerTreeView() {
        this.controlToModel = new Dictionary<BaseLayerTreeViewItem, BaseLayerTreeObject>();
        this.modelToControl = new Dictionary<BaseLayerTreeObject, BaseLayerTreeViewItem>();
        this.itemCache = new Stack<BaseLayerTreeViewItem>();
        DragDrop.SetAllowDrop(this, true);
        this.SelectedItems = this.selectedItemsList = new AvaloniaList<BaseLayerTreeViewItem>();
    }

    protected abstract BaseLayerTreeViewItem CreateTreeViewItem();

    private void MarkContainerSelected(Control container, bool selected) {
        container.SetCurrentValue(SelectingItemsControl.IsSelectedProperty, selected);
    }

    static BaseLayerTreeView() {
        CanvasProperty.Changed.AddClassHandler<BaseLayerTreeView, Canvas?>((o, e) => o.OnCanvasChanged(e));
        DragDrop.DragEnterEvent.AddClassHandler<BaseLayerTreeView>((o, e) => o.OnDragEnter(e));
        DragDrop.DragOverEvent.AddClassHandler<BaseLayerTreeView>((o, e) => o.OnDragOver(e));
        DragDrop.DragLeaveEvent.AddClassHandler<BaseLayerTreeView>((o, e) => o.OnDragLeave(e));
        DragDrop.DropEvent.AddClassHandler<BaseLayerTreeView>((o, e) => o.OnDrop(e));
    }

    private IEnumerable<BaseLayerTreeViewItem> GetControlsFromModels(IEnumerable<BaseLayerTreeObject> items) {
        foreach (BaseLayerTreeObject layer in items) {
            if (this.modelToControl.TryGetValue(layer, out BaseLayerTreeViewItem? control)) {
                yield return control;
            }
        }
    }

    public BaseLayerTreeViewItem GetNodeAt(int index) {
        return (BaseLayerTreeViewItem) this.Items[index]!;
    }

    public void InsertNode(BaseLayerTreeObject item, int index) {
        this.InsertNode(this.GetCachedItemOrNew(), item, index);
    }

    public void InsertNode(BaseLayerTreeViewItem control, BaseLayerTreeObject layer, int index) {
        control.OnAdding(this, null, layer);
        this.Items.Insert(index, control);
        this.AddResourceMapping(control, layer);
        control.ApplyTemplate();
        control.OnAdded();
    }

    public void RemoveNode(int index, bool canCache = true) {
        BaseLayerTreeViewItem control = (BaseLayerTreeViewItem) this.Items[index]!;
        BaseLayerTreeObject model = control.LayerObject ?? throw new Exception("Expected node to have a resource");
        control.OnRemoving();
        this.Items.RemoveAt(index);
        this.RemoveResourceMapping(control, model);
        control.OnRemoved();
        if (canCache)
            this.PushCachedItem(control);
    }

    public void MoveNode(int oldIndex, int newIndex) {
        BaseLayerTreeViewItem control = (BaseLayerTreeViewItem) this.Items[oldIndex]!;
        this.Items.RemoveAt(oldIndex);
        this.Items.Insert(newIndex, control);
    }

    private void OnCanvasChanged(AvaloniaPropertyChangedEventArgs<Canvas?> e) {
        this.collectionChangeListener?.Dispose();
        if (e.TryGetOldValue(out Canvas? oldCanvas)) {
            for (int i = this.Items.Count - 1; i >= 0; i--) {
                this.RemoveNode(i);
            }
        }

        if (e.TryGetNewValue(out Canvas? newCanvas)) {
            this.collectionChangeListener = ObservableItemProcessor.MakeIndexable(newCanvas.RootComposition.Layers, this.OnCanvasLayerAdded, this.OnCanvasLayerRemoved, this.OnCanvasLayerIndexMoved);
            int i = 0;
            foreach (BaseLayerTreeObject layer in newCanvas.RootComposition.Layers) {
                this.InsertNode(layer, i++);
            }
        }
    }

    private void OnCanvasLayerAdded(object sender, int index, BaseLayerTreeObject item) => this.InsertNode(item, index);

    private void OnCanvasLayerRemoved(object sender, int index, BaseLayerTreeObject item) => this.RemoveNode(index);

    private void OnCanvasLayerIndexMoved(object sender, int oldIndex, int newIndex, BaseLayerTreeObject item) => this.MoveNode(oldIndex, newIndex);

    public void AddResourceMapping(BaseLayerTreeViewItem control, BaseLayerTreeObject layer) {
        // use add so that it throws for an actual error where one or
        // more resources are associated with a control, and vice versa
        // Should probably use debug condition here
        // if (this.controlToModel.ContainsKey(control))
        //     throw new Exception("Control already exists in the map: " + control);
        // if (this.modelToControl.ContainsKey(resource))
        //     throw new Exception("Resource already exists in the map: " + resource);
        this.controlToModel.Add(control, layer);
        this.modelToControl.Add(layer, control);
    }

    public void RemoveResourceMapping(BaseLayerTreeViewItem control, BaseLayerTreeObject layer) {
        if (!this.controlToModel.Remove(control))
            throw new Exception("Control did not exist in the map: " + control);
        if (!this.modelToControl.Remove(layer))
            throw new Exception("Resource did not exist in the map: " + layer);
    }

    public BaseLayerTreeViewItem GetCachedItemOrNew() {
        return this.itemCache.Count > 0 ? this.itemCache.Pop() : this.CreateTreeViewItem();
    }

    public void PushCachedItem(BaseLayerTreeViewItem item) {
        if (this.itemCache.Count < 128) {
            this.itemCache.Push(item);
        }
    }

    #region Drag drop

    private void OnDragEnter(DragEventArgs e) {
        this.OnDragOver(e);
    }

    private void OnDragOver(DragEventArgs e) {
        if (this.Canvas != null)
            this.IsDroppableTargetOver = BaseLayerTreeViewItem.ProcessCanDragOver(this.Canvas.RootComposition, e) == true;
    }

    private void OnDragLeave(DragEventArgs e) {
        if (!this.IsPointerOver)
            this.IsDroppableTargetOver = false;
    }

    private async void OnDrop(DragEventArgs e) {
        e.Handled = true;
        if (this.isProcessingAsyncDrop || this.Canvas == null) {
            return;
        }

        try {
            this.isProcessingAsyncDrop = true;
            if (BaseLayerTreeViewItem.GetDropResourceListForEvent(e, out List<BaseLayerTreeObject>? list, out EnumDropType effects)) {
                await LayerDropRegistry.DropRegistry.OnDropped(this.Canvas.RootComposition, list, effects);
            }
            else if (!await LayerDropRegistry.DropRegistry.OnDroppedNative(this.Canvas.RootComposition, new DataObjectWrapper(e.Data), effects)) {
                await IoC.MessageService.ShowMessage("Unknown Data", "Unknown dropped item. Drop files here");
            }
        }
        finally {
            this.IsDroppableTargetOver = false;
            this.isProcessingAsyncDrop = false;
        }
    }

    #endregion

    public void ClearSelection() {
        this.SelectedItems.Clear();
    }

    public void SetSelection(BaseLayerTreeViewItem item) {
        this.ClearSelection();
        this.SelectedItems.Add(item);
    }

    public void SetSelection(IEnumerable<BaseLayerTreeViewItem> items) {
        this.ClearSelection();
        foreach (BaseLayerTreeViewItem item in items) {
            this.SelectedItems.Add(item);
        }
    }

    public void SetSelection(List<BaseLayerTreeObject> modelItems) {
        this.ClearSelection();
        foreach (BaseLayerTreeObject item in modelItems) {
            if (this.modelToControl.TryGetValue(item, out BaseLayerTreeViewItem? control)) {
                control.IsSelected = true;
            }
        }
    }
}