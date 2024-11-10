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
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using PicNetStudio.Avalonia.Utils;
using PicNetStudio.Avalonia.Utils.Collections.Observable;

namespace PicNetStudio.Avalonia.PicNet.Layers.Controls;

public class LayerObjectTreeView : TreeView {
    public static readonly StyledProperty<Canvas?> CanvasProperty = AvaloniaProperty.Register<LayerObjectTreeView, Canvas?>("Canvas");

    public Canvas? Canvas {
        get => this.GetValue(CanvasProperty);
        set => this.SetValue(CanvasProperty, value);
    }

    private readonly Dictionary<LayerObjectTreeViewItem, BaseLayerTreeObject> controlToModel;
    private readonly Dictionary<BaseLayerTreeObject, LayerObjectTreeViewItem> modelToControl;
    internal readonly Stack<LayerObjectTreeViewItem> itemCache;
    private IDisposable? collectionChangeListener;

    public LayerObjectTreeView() {
        this.controlToModel = new Dictionary<LayerObjectTreeViewItem, BaseLayerTreeObject>();
        this.modelToControl = new Dictionary<BaseLayerTreeObject, LayerObjectTreeViewItem>();
        this.itemCache = new Stack<LayerObjectTreeViewItem>();
        this.SelectionChanged += OnSelectionChanged;
    }

    private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e) {
        if (this.Canvas is Canvas canvas && e.AddedItems.Count > 0) {
            LayerObjectTreeViewItem item = (LayerObjectTreeViewItem) e.AddedItems[e.AddedItems.Count - 1]!;
            canvas.ActiveLayerTreeObject = item.LayerObject;
        }
    }

    static LayerObjectTreeView() {
        CanvasProperty.Changed.AddClassHandler<LayerObjectTreeView, Canvas?>((o, e) => o.OnCanvasChanged(e));
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
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
            this.collectionChangeListener = ObservableItemProcessor.MakeIndexable(newCanvas.Layers, this.OnCanvasLayerAdded, this.OnCanvasLayerRemoved, this.OnCanvasLayerIndexMoved);
            int i = 0;
            foreach (BaseLayerTreeObject layer in newCanvas.Layers) {
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
}