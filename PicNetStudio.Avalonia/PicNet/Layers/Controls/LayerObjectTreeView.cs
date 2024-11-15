// 
// Copyright (c) 2024-2024 REghZy
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using PicNetStudio.Avalonia.Interactivity;
using PicNetStudio.Avalonia.Utils;
using PicNetStudio.Avalonia.Utils.Collections.Observable;

namespace PicNetStudio.Avalonia.PicNet.Layers.Controls;

/// <summary>
/// A tree view which presents a canvas' layer hierarchy in a tree system
/// </summary>
public class LayerObjectTreeView : TreeView {
    public static readonly StyledProperty<Canvas?> CanvasProperty = AvaloniaProperty.Register<LayerObjectTreeView, Canvas?>("Canvas");
    public static readonly StyledProperty<bool> IsDroppableTargetOverProperty = AvaloniaProperty.Register<LayerObjectTreeView, bool>("IsDroppableTargetOver");

    public Canvas? Canvas {
        get => this.GetValue(CanvasProperty);
        set => this.SetValue(CanvasProperty, value);
    }

    public bool IsDroppableTargetOver {
        get => this.GetValue(IsDroppableTargetOverProperty);
        set => this.SetValue(IsDroppableTargetOverProperty, value);
    }

    private readonly Dictionary<LayerObjectTreeViewItem, BaseLayerTreeObject> controlToModel;
    private readonly Dictionary<BaseLayerTreeObject, LayerObjectTreeViewItem> modelToControl;
    internal readonly Stack<LayerObjectTreeViewItem> itemCache;
    private IDisposable? collectionChangeListener;
    private bool isProcessingAsyncDrop;

    public ISelectionManager<BaseLayerTreeObject> SelectionManager { get; }
    private readonly SelectionManagerImpl selectionManagerImpl;

    public LayerObjectTreeView() {
        this.controlToModel = new Dictionary<LayerObjectTreeViewItem, BaseLayerTreeObject>();
        this.modelToControl = new Dictionary<BaseLayerTreeObject, LayerObjectTreeViewItem>();
        this.SelectionManager = this.selectionManagerImpl = new SelectionManagerImpl(this);
        this.itemCache = new Stack<LayerObjectTreeViewItem>();
        this.SelectionChanged += this.OnTreeSelectionChanged;
        DragDrop.SetAllowDrop(this, true);
    }

    static LayerObjectTreeView() {
        CanvasProperty.Changed.AddClassHandler<LayerObjectTreeView, Canvas?>((o, e) => o.OnCanvasChanged(e));
        DragDrop.DragEnterEvent.AddClassHandler<LayerObjectTreeView>((o, e) => o.OnDragEnter(e));
        DragDrop.DragOverEvent.AddClassHandler<LayerObjectTreeView>((o, e) => o.OnDragOver(e));
        DragDrop.DragLeaveEvent.AddClassHandler<LayerObjectTreeView>((o, e) => o.OnDragLeave(e));
        DragDrop.DropEvent.AddClassHandler<LayerObjectTreeView>((o, e) => o.OnDrop(e));
    }

    private void OnTreeSelectionChanged(object? sender, SelectionChangedEventArgs e) {
        this.selectionManagerImpl.RaiseSelectionChanged(e.RemovedItems, e.AddedItems);
        if (this.Canvas is Canvas canvas) {
            canvas.ActiveLayerTreeObject = this.SelectedItems.Count == 1 ? ((LayerObjectTreeViewItem) this.SelectedItems[0]!).LayerObject : null; 
        }
    }

    private IEnumerable<LayerObjectTreeViewItem> GetControlsFromModels(IEnumerable<BaseLayerTreeObject> items) {
        foreach (BaseLayerTreeObject layer in items) {
            if (this.modelToControl.TryGetValue(layer, out LayerObjectTreeViewItem? control)) {
                yield return control;
            }
        }
    }

    public LayerObjectTreeViewItem GetNodeAt(int index) {
        return (LayerObjectTreeViewItem) this.Items[index]!;
    }

    public void InsertNode(BaseLayerTreeObject item, int index) {
        this.InsertNode(this.GetCachedItemOrNew(), item, index);
    }

    public void InsertNode(LayerObjectTreeViewItem control, BaseLayerTreeObject layer, int index) {
        control.OnAdding(this, null, layer);
        this.Items.Insert(index, control);
        this.AddResourceMapping(control, layer);
        control.ApplyTemplate();
        control.OnAdded();
    }

    public void RemoveNode(int index, bool canCache = true) {
        LayerObjectTreeViewItem control = (LayerObjectTreeViewItem) this.Items[index]!;
        BaseLayerTreeObject model = control.LayerObject ?? throw new Exception("Expected node to have a resource");
        control.OnRemoving();
        this.Items.RemoveAt(index);
        this.RemoveResourceMapping(control, model);
        control.OnRemoved();
        if (canCache)
            this.PushCachedItem(control);
    }

    public void MoveNode(int oldIndex, int newIndex) {
        LayerObjectTreeViewItem control = (LayerObjectTreeViewItem) this.Items[oldIndex]!;
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

    public void AddResourceMapping(LayerObjectTreeViewItem control, BaseLayerTreeObject layer) {
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

    public void RemoveResourceMapping(LayerObjectTreeViewItem control, BaseLayerTreeObject layer) {
        if (!this.controlToModel.Remove(control))
            throw new Exception("Control did not exist in the map: " + control);
        if (!this.modelToControl.Remove(layer))
            throw new Exception("Resource did not exist in the map: " + layer);
    }

    public LayerObjectTreeViewItem GetCachedItemOrNew() {
        return this.itemCache.Count > 0 ? this.itemCache.Pop() : new LayerObjectTreeViewItem();
    }

    public void PushCachedItem(LayerObjectTreeViewItem item) {
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
            this.IsDroppableTargetOver = LayerObjectTreeViewItem.ProcessCanDragOver(this.Canvas.RootComposition, e);
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
            if (LayerObjectTreeViewItem.GetDropResourceListForEvent(e, out List<BaseLayerTreeObject>? list, out EnumDropType effects)) {
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

    public void SetSelection(LayerObjectTreeViewItem item) {
        this.ClearSelection();
        this.SelectedItems.Add(item);
    }
    
    private class SelectionManagerImpl : ISelectionManager<BaseLayerTreeObject> {
        public readonly LayerObjectTreeView Tree;

        public int Count => this.Tree.SelectedItems.Count;
        
        public IEnumerable<BaseLayerTreeObject> SelectedItems => this.Tree.SelectedItems.Cast<LayerObjectTreeViewItem>().Select(x => x.LayerObject!);

        public event SelectionChangedEventHandler<BaseLayerTreeObject>? SelectionChanged;
        
        public SelectionManagerImpl(LayerObjectTreeView treeView) {
            this.Tree = treeView;
        }

        public void RaiseSelectionChanged(IList oldItems, IList newItems) {
            this.SelectionChanged?.Invoke(
                this, 
                oldItems.Cast<LayerObjectTreeViewItem>().Select(x => x.LayerObject!).ToList().AsReadOnly(), 
                newItems.Cast<LayerObjectTreeViewItem>().Select(x => x.LayerObject!).ToList().AsReadOnly());
        }
        
        public bool IsSelected(BaseLayerTreeObject item) {
            if (this.Tree.modelToControl.TryGetValue(item, out LayerObjectTreeViewItem? treeItem))
                return treeItem.IsSelected;
            return false;
        }

        public void SetSelection(BaseLayerTreeObject item) {
            this.Tree.ClearSelection();
            this.Select(item);
        }

        public void SetSelection(IEnumerable<BaseLayerTreeObject> items) {
            this.Tree.ClearSelection();
            this.Select(items);
        }

        public void Select(BaseLayerTreeObject item) {
            if (this.Tree.modelToControl.TryGetValue(item, out LayerObjectTreeViewItem? treeItem)) {
                treeItem.IsSelected = true;
            }
        }

        public void Select(IEnumerable<BaseLayerTreeObject> items) {
            foreach (BaseLayerTreeObject item in items.ToList()) {
                if (this.Tree.modelToControl.TryGetValue(item, out LayerObjectTreeViewItem? treeItem)) {
                    treeItem.IsSelected = true;
                }
            }
        }

        public void Unselect(BaseLayerTreeObject item) {
            if (this.Tree.modelToControl.TryGetValue(item, out LayerObjectTreeViewItem? treeItem)) {
                treeItem.IsSelected = false;
            }
        }

        public void Unselect(IEnumerable<BaseLayerTreeObject> newItems) {
            List<BaseLayerTreeObject> list = newItems.ToList();
            foreach (BaseLayerTreeObject item in list) {
                if (this.Tree.modelToControl.TryGetValue(item, out LayerObjectTreeViewItem? treeItem)) {
                    treeItem.IsSelected = false;
                }
            }
        }

        public void Clear() {
            this.Tree.ClearSelection();
        }
    }
}