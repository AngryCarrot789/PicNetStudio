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
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using PicNetStudio.Avalonia.Interactivity;
using PicNetStudio.Avalonia.Utils;
using PicNetStudio.Avalonia.Utils.Collections.Observable;

namespace PicNetStudio.Avalonia.PicNet.Layers.Controls;

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
    private bool ignoreTreeSelectionChangeEvent;
    private bool ignoreManagerSelectionChangeEvent;
    private bool isProcessingAsyncDrop;

    public LayerObjectTreeView() {
        this.controlToModel = new Dictionary<LayerObjectTreeViewItem, BaseLayerTreeObject>();
        this.modelToControl = new Dictionary<BaseLayerTreeObject, LayerObjectTreeViewItem>();
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
        if (this.ignoreTreeSelectionChangeEvent) {
            return;
        }

        if (this.Canvas is Canvas canvas) {
            // #pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
            //             List<BaseLayerTreeObject>? removedList = e.RemovedItems.Count > 0 ? e.RemovedItems.Cast<LayerObjectTreeViewItem>().Select(x => x.LayerObject).Where(x => x != null).ToList() : null;
            //             List<BaseLayerTreeObject>? addedList = e.AddedItems.Count > 0 ? e.AddedItems.Cast<LayerObjectTreeViewItem>().Select(x => x.LayerObject).Where(x => x != null).ToList() : null;
            // #pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.
            //
            //             try {
            //                 this.ignoreManagerSelectionChangeEvent = true;
            //                 if (removedList != null) {
            //                     canvas.LayerSelectionManager.Unselect(removedList);
            //                 }
            //             
            //                 if (addedList != null) {
            //                     canvas.LayerSelectionManager.Select(addedList);
            //                 }
            //             }
            //             finally {
            //                 this.ignoreManagerSelectionChangeEvent = false;
            //             }

#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
            List<BaseLayerTreeObject>? selection = this.SelectedItems.Count > 0 ? this.SelectedItems.Cast<LayerObjectTreeViewItem>().Select(x => x.LayerObject).Where(x => x != null).ToList() : null;
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.
            try {
                this.ignoreManagerSelectionChangeEvent = true;
                if (selection != null && selection.Count > 0) {
                    canvas.LayerSelectionManager.SetSelection(selection);
                }
                else {
                    canvas.LayerSelectionManager.Clear();
                }
            }
            finally {
                this.ignoreManagerSelectionChangeEvent = false;
            }
        }
    }


    private void OnLayerSelectionChanged(SelectionManager<BaseLayerTreeObject> sender, IList<BaseLayerTreeObject>? olditems, IList<BaseLayerTreeObject>? newitems) {
        if (this.ignoreManagerSelectionChangeEvent && this.SelectedItems.Count == sender.Selection.Count) {
            return;
        }

        try {
            this.ignoreTreeSelectionChangeEvent = true;
            if (newitems == null || newitems.Count == 0) {
                this.ClearSelection();
            }
            else {
                List<LayerObjectTreeViewItem> controls = new List<LayerObjectTreeViewItem>();
                foreach (BaseLayerTreeObject layer in newitems) {
                    if (this.modelToControl.TryGetValue(layer, out LayerObjectTreeViewItem? control)) {
                        controls.Add(control);
                    }
                }

                if (this.SelectedItems.CollectionEquals(controls)) {
                    return;
                }

                this.SelectedItems.Clear();
                foreach (LayerObjectTreeViewItem control in controls) {
                    this.SelectedItems.Add(control);
                }
            }
        }
        finally {
            this.ignoreTreeSelectionChangeEvent = false;
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
            oldCanvas.LayerSelectionManager.SelectionChanged -= this.OnLayerSelectionChanged;
            for (int i = this.Items.Count - 1; i >= 0; i--) {
                this.RemoveNode(i);
            }
        }

        if (e.TryGetNewValue(out Canvas? newCanvas)) {
            newCanvas.LayerSelectionManager.SelectionChanged += this.OnLayerSelectionChanged;
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
        this.SelectedItems.Clear();
        this.SelectedItems.Add(item);
    }
}