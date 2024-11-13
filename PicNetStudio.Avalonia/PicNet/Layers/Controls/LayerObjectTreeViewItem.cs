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
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Threading;
using PicNetStudio.Avalonia.Bindings;
using PicNetStudio.Avalonia.Interactivity;
using PicNetStudio.Avalonia.Interactivity.Contexts;
using PicNetStudio.Avalonia.PicNet.Layers.StateMods.Controls;
using PicNetStudio.Avalonia.Utils;
using PicNetStudio.Avalonia.Utils.Collections.Observable;

namespace PicNetStudio.Avalonia.PicNet.Layers.Controls;

/// <summary>
/// A tree view item for all layer objects
/// </summary>
public class LayerObjectTreeViewItem : TreeViewItem {
    public static readonly StyledProperty<bool> IsDroppableTargetOverProperty = LayerObjectTreeView.IsDroppableTargetOverProperty.AddOwner<LayerObjectTreeView>();
    
    public bool IsDroppableTargetOver {
        get => this.GetValue(IsDroppableTargetOverProperty);
        set => this.SetValue(IsDroppableTargetOverProperty, value);
    }
    
    public LayerObjectTreeView? LayerTree { get; private set; }
    public LayerObjectTreeViewItem? ParentNode { get; private set; }
    public BaseLayerTreeObject? LayerObject { get; private set; }

    private ObservableItemProcessorIndexing<BaseLayerTreeObject>? compositeListener;

    private readonly IBinder<BaseLayerTreeObject> displayNameBinder = new GetSetAutoUpdateAndEventPropertyBinder<BaseLayerTreeObject>(HeaderProperty, nameof(BaseLayerTreeObject.NameChanged), b => b.Model.Name, (b, v) => b.Model.Name = (string) v);
    private readonly PropertyAutoSetter<BaseLayerTreeObject, LayerStateModifierListBox> stateModifierListBoxHelper;
    private LayerStateModifierListBox? PART_StateModifierListBox => this.stateModifierListBoxHelper.TargetControl;

    public LayerObjectTreeViewItem() {
        this.stateModifierListBoxHelper = new PropertyAutoSetter<BaseLayerTreeObject, LayerStateModifierListBox>(LayerStateModifierListBox.LayerObjectProperty);
    }

    static LayerObjectTreeViewItem() {
        DragDrop.DragEnterEvent.AddClassHandler<LayerObjectTreeViewItem>((o, e) => o.OnDragEnter(e));
        DragDrop.DragOverEvent.AddClassHandler<LayerObjectTreeViewItem>((o, e) => o.OnDragOver(e));
        DragDrop.DragLeaveEvent.AddClassHandler<LayerObjectTreeViewItem>((o, e) => o.OnDragLeave(e));
        DragDrop.DropEvent.AddClassHandler<LayerObjectTreeViewItem>((o, e) => o.OnDrop(e));
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        this.stateModifierListBoxHelper.SetControl(e.NameScope.GetTemplateChild<LayerStateModifierListBox>("PART_StateModifierListBox"));
    }

    public void OnAdding(LayerObjectTreeView tree, LayerObjectTreeViewItem parentNode, BaseLayerTreeObject layer) {
        this.LayerTree = tree;
        this.ParentNode = parentNode;
        this.LayerObject = layer;
        DragDrop.SetAllowDrop(this, layer is CompositionLayer);
    }

    public void OnAdded() {
        if (this.LayerObject is CompositionLayer folder) {
            this.compositeListener = ObservableItemProcessor.MakeIndexable(folder.Layers, this.OnLayerAdded, this.OnLayerRemoved, this.OnLayerMoved);
            int i = 0;
            foreach (BaseLayerTreeObject item in folder.Layers) {
                this.InsertNode(item, i++);
            }
        }

        this.displayNameBinder.Attach(this, this.LayerObject!);
        this.stateModifierListBoxHelper.SetModel(this.LayerObject);
        if (this.LayerObject!.Canvas?.LayerSelectionManager.IsSelected(this.LayerObject) ?? false) {
            this.IsSelected = true;
        }

        DataManager.SetContextData(this, new ContextData().Set(DataKeys.LayerObjectKey, this.LayerObject));
    }

    public void OnRemoving() {
        this.compositeListener?.Dispose();
        int count = this.Items.Count;
        for (int i = count - 1; i >= 0; i--) {
            this.RemoveNode(i);
        }

        this.displayNameBinder.Detach();
        this.stateModifierListBoxHelper.SetModel(null);
    }

    public void OnRemoved() {
        this.LayerTree = null;
        this.ParentNode = null;
        this.LayerObject = null;
        DataManager.ClearContextData(this);
    }

    private void OnLayerAdded(object sender, int index, BaseLayerTreeObject item) {
        this.InsertNode(item, index);
    }

    private void OnLayerRemoved(object sender, int index, BaseLayerTreeObject item) {
        this.RemoveNode(index);
    }

    private void OnLayerMoved(object sender, int oldindex, int newindex, BaseLayerTreeObject item) {
        this.MoveNode(oldindex, newindex);
    }

    public LayerObjectTreeViewItem GetNodeAt(int index) => (LayerObjectTreeViewItem) this.Items[index];

    public void InsertNode(BaseLayerTreeObject item, int index) {
        this.InsertNode(null, item, index);
    }

    public void InsertNode(LayerObjectTreeViewItem control, BaseLayerTreeObject resource, int index) {
        LayerObjectTreeView? tree = this.LayerTree;
        if (tree == null)
            throw new InvalidOperationException("Cannot add children when we have no resource tree associated");
        if (control == null)
            control = tree.GetCachedItemOrNew();

        control.OnAdding(tree, this, resource);
        this.Items.Insert(index, control);
        tree.AddResourceMapping(control, resource);
        control.ApplyTemplate();
        control.OnAdded();
    }

    public void RemoveNode(int index, bool canCache = true) {
        LayerObjectTreeView? tree = this.LayerTree;
        if (tree == null)
            throw new InvalidOperationException("Cannot remove children when we have no resource tree associated");

        LayerObjectTreeViewItem control = (LayerObjectTreeViewItem) this.Items[index]!;
        BaseLayerTreeObject resource = control.LayerObject ?? throw new Exception("Invalid application state");
        control.OnRemoving();
        this.Items.RemoveAt(index);
        tree.RemoveResourceMapping(control, resource);
        control.OnRemoved();
        if (canCache)
            tree.PushCachedItem(control);
    }

    public void MoveNode(int oldIndex, int newIndex) {
        LayerObjectTreeView? tree = this.LayerTree;
        if (tree == null)
            throw new InvalidOperationException("Cannot remove children when we have no resource tree associated");

        LayerObjectTreeViewItem control = (LayerObjectTreeViewItem) this.Items[oldIndex]!;
        this.Items.RemoveAt(oldIndex);
        this.Items.Insert(newIndex, control);
    }

    #region Drag Drop

    private bool isDragDropping;
    private bool isDragActive;
    private Point originMousePoint;
    private bool isProcessingAsyncDrop;

    public static bool CanBeginDragDrop(KeyModifiers modifiers) {
        return (modifiers & (KeyModifiers.Control | KeyModifiers.Shift)) == 0;
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e) {
        base.OnPointerPressed(e);
        if (e.Handled || this.LayerObject == null) {
            return;
        }

        PointerPoint point = e.GetCurrentPoint(this);
        if (point.Properties.PointerUpdateKind == PointerUpdateKind.LeftButtonPressed) {
            if (CanBeginDragDrop(e.KeyModifiers)) {
                if ((this.IsFocused || this.Focus()) && !this.isDragDropping) {
                    e.Pointer.Capture(this);
                    this.originMousePoint = point.Position;
                    this.isDragActive = true;
                    if (this.LayerTree != null && this.LayerTree.SelectedItems.Count < 2) {
                        this.LayerTree?.SetSelection(this);
                    }
                    
                    // handle to stop tree view from selecting stuff
                    e.Handled = true;
                }
            }
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e) {
        base.OnPointerReleased(e);
        PointerPoint point = e.GetCurrentPoint(this);
        if (this.isDragActive && point.Properties.PointerUpdateKind == PointerUpdateKind.LeftButtonReleased) {
            this.isDragActive = false;
            if (ReferenceEquals(e.Pointer.Captured, this)) {
                e.Pointer.Capture(null);
            }
        }
    }

    protected override void OnPointerMoved(PointerEventArgs e) {
        base.OnPointerMoved(e);
        PointerPoint point = e.GetCurrentPoint(this);
        if (!point.Properties.IsLeftButtonPressed) {
            if (ReferenceEquals(e.Pointer.Captured, this)) {
                e.Pointer.Capture(null);
            }

            this.isDragActive = false;
            this.originMousePoint = new Point(0, 0);
            return;
        }

        if (!this.isDragActive || this.isDragDropping) {
            return;
        }

        if (!(this.LayerObject is BaseLayerTreeObject layer) || layer.Canvas == null) {
            return;
        }

        Point posA = point.Position;
        Point posB = this.originMousePoint;
        Point change = new Point(Math.Abs(posA.X - posB.X), Math.Abs(posA.X - posB.X));
        if (change.X > 5 || change.Y > 5) {
            IReadOnlySet<BaseLayerTreeObject> selection = layer.Canvas.LayerSelectionManager.Selection;
            if (selection.Count < 1 || !selection.Contains(layer)) {
                this.IsSelected = true;
            }

            List<BaseLayerTreeObject> list = selection.ToList();

            try {
                this.isDragDropping = true;
                DataObject obj = new DataObject();
                obj.Set(ResourceDropRegistry.ResourceDropType, list);

                DragDrop.DoDragDrop(e, obj, DragDropEffects.Copy | DragDropEffects.Move | DragDropEffects.Link);
            }
            catch (Exception ex) {
                Debug.WriteLine("Exception while executing resource tree item drag drop: " + ex.GetToString());
            }
            finally {
                this.isDragDropping = false;
            }
        }
    }

    private void OnDragEnter(DragEventArgs e) {
        this.OnDragOver(e);
    }

    private void OnDragOver(DragEventArgs e) {
        if (this.LayerObject != null)
            this.IsDroppableTargetOver = ProcessCanDragOver(this.LayerObject, e);
    }

    private void OnDragLeave(DragEventArgs e) {
        this.IsDroppableTargetOver = false;
    }

    private async void OnDrop(DragEventArgs e) {
        e.Handled = true;
        if (this.isProcessingAsyncDrop || !(this.LayerObject is BaseLayerTreeObject layer)) {
            return;
        }

        try {
            this.isProcessingAsyncDrop = true;
            if (GetDropResourceListForEvent(e, out List<BaseLayerTreeObject>? list, out EnumDropType effects)) {
                await ResourceDropRegistry.DropRegistry.OnDropped(layer, list, effects);
            }
            else if (!await ResourceDropRegistry.DropRegistry.OnDroppedNative(layer, new DataObjectWrapper(e.Data), effects)) {
                await IoC.MessageService.ShowMessage("Unknown Data", "Unknown dropped item. Drop files here");
            }
        }
        finally {
            this.IsDroppableTargetOver = false;
            this.isProcessingAsyncDrop = false;
        }
    }

    public static bool ProcessCanDragOver(BaseLayerTreeObject layer, DragEventArgs e) {
        e.Handled = true;
        if (GetDropResourceListForEvent(e, out List<BaseLayerTreeObject>? resources, out EnumDropType effects)) {
            e.DragEffects = (DragDropEffects) ResourceDropRegistry.DropRegistry.CanDrop(layer, resources, effects);
        }
        else {
            e.DragEffects = (DragDropEffects) ResourceDropRegistry.DropRegistry.CanDropNative(layer, new DataObjectWrapper(e.Data), effects);
        }

        return e.DragEffects != DragDropEffects.None;
    }

    /// <summary>
    /// Tries to get the list of resources being drag-dropped from the given drag event. Provides the
    /// effects currently applicable for the event regardless of this method's return value
    /// </summary>
    /// <param name="e">Drag event (enter, over, drop, etc.)</param>
    /// <param name="resources">The resources in the drag event</param>
    /// <param name="effects">The effects applicable based on the event's effects and user's keys pressed</param>
    /// <returns>True if there were resources available, otherwise false, meaning no resources are being dragged</returns>
    public static bool GetDropResourceListForEvent(DragEventArgs e, [NotNullWhen(true)] out List<BaseLayerTreeObject>? resources, out EnumDropType effects) {
        effects = DropUtils.GetDropAction(e.KeyModifiers, (EnumDropType) e.DragEffects);
        if (e.Data.Contains(ResourceDropRegistry.ResourceDropType)) {
            object? obj = e.Data.Get(ResourceDropRegistry.ResourceDropType);
            if ((resources = (obj as List<BaseLayerTreeObject>)) != null) {
                return true;
            }
        }

        resources = null;
        return false;
    }

    #endregion
}