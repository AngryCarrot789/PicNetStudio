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
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using PicNetStudio.Avalonia.Bindings;
using PicNetStudio.Avalonia.Interactivity;
using PicNetStudio.Avalonia.Interactivity.Contexts;
using PicNetStudio.Avalonia.PicNet.Effects.Controls;
using PicNetStudio.Avalonia.PicNet.Layers.StateMods.Controls;
using PicNetStudio.Avalonia.Utils;
using PicNetStudio.Avalonia.Utils.Collections.Observable;

namespace PicNetStudio.Avalonia.PicNet.Layers.Controls;

/// <summary>
/// A tree view item that represents any type of layer
/// </summary>
public class LayerObjectTreeViewItem : TreeViewItem, ILayerNodeItem {
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
    private readonly PropertyAutoSetter<BaseLayerTreeObject, EffectListBox> effectListBoxHelper;
    private LayerStateModifierListBox? PART_StateModifierListBox => this.stateModifierListBoxHelper.TargetControl;
    private LayerStateModifierListBox? PART_EffectListBox => this.stateModifierListBoxHelper.TargetControl;
    private Border? PART_DragDropMoveBorder;
    private bool isDragDropping;
    private bool isDragActive;
    private Point originMousePoint;
    private bool isProcessingAsyncDrop;
    private bool isEditingNameState;
    private string? nameBeforeEditBegin;

    private TextBlock? PART_HeaderTextBlock;
    private TextBox? PART_HeaderTextBox;
    private readonly ContextData contextData;

    BaseLayerTreeObject? ILayerNodeItem.Layer => this.LayerObject;
    
    public bool EditNameState {
        get => this.isEditingNameState;
        set {
            if (this.PART_HeaderTextBox == null)
                throw new InvalidOperationException("Too early to use this property. Let node to initialise first");
            if (this.LayerObject == null)
                throw new InvalidOperationException("Invalid node; no layer object");

            if (value == this.isEditingNameState)
                return;
            
            if (!this.isEditingNameState) {
                this.nameBeforeEditBegin = this.LayerObject.Name;
                this.isEditingNameState = true;
            }
            else {
                this.isEditingNameState = false;
            }
            
            this.UpdateHeaderEditorControls();
        }
    }

    public LayerObjectTreeViewItem() {
        this.stateModifierListBoxHelper = new PropertyAutoSetter<BaseLayerTreeObject, LayerStateModifierListBox>(LayerStateModifierListBox.LayerObjectProperty);
        this.effectListBoxHelper = new PropertyAutoSetter<BaseLayerTreeObject, EffectListBox>(EffectListBox.LayerObjectProperty);
        DragDrop.SetAllowDrop(this, true);
        DataManager.SetContextData(this, this.contextData = new ContextData().Set(DataKeys.LayerNodeKey, this));
    }

    static LayerObjectTreeViewItem() {
        DragDrop.DragEnterEvent.AddClassHandler<LayerObjectTreeViewItem>((o, e) => o.OnDragEnter(e));
        DragDrop.DragOverEvent.AddClassHandler<LayerObjectTreeViewItem>((o, e) => o.OnDragOver(e));
        DragDrop.DragLeaveEvent.AddClassHandler<LayerObjectTreeViewItem>((o, e) => o.OnDragLeave(e));
        DragDrop.DropEvent.AddClassHandler<LayerObjectTreeViewItem>((o, e) => o.OnDrop(e));
    }

    private void UpdateHeaderEditorControls() {
        if (this.isEditingNameState) {
            this.PART_HeaderTextBox!.IsVisible = true;
            this.PART_HeaderTextBlock!.IsVisible = false;
            BugFix.TextBox_FocusSelectAll(this.PART_HeaderTextBox);
        }
        else {
            this.PART_HeaderTextBox!.IsVisible = false;
            this.PART_HeaderTextBlock!.IsVisible = true;
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        this.stateModifierListBoxHelper.SetControl(e.NameScope.GetTemplateChild<LayerStateModifierListBox>("PART_StateModifierListBox"));
        this.effectListBoxHelper.SetControl(e.NameScope.GetTemplateChild<EffectListBox>("PART_EffectListBox"));
        this.PART_DragDropMoveBorder = e.NameScope.GetTemplateChild<Border>(nameof(this.PART_DragDropMoveBorder));
        this.PART_HeaderTextBlock = e.NameScope.GetTemplateChild<TextBlock>(nameof(this.PART_HeaderTextBlock));
        this.PART_HeaderTextBox = e.NameScope.GetTemplateChild<TextBox>(nameof(this.PART_HeaderTextBox));
        this.PART_HeaderTextBox.KeyDown += this.PART_HeaderTextBoxOnKeyDown;
        this.PART_HeaderTextBox.LostFocus += this.PART_HeaderTextBoxOnLostFocus;
        this.UpdateHeaderEditorControls();
    }

    private void PART_HeaderTextBoxOnLostFocus(object? sender, RoutedEventArgs e) {
        this.EditNameState = false;
        if (this.LayerObject != null) {
            this.LayerObject.Name = this.nameBeforeEditBegin ?? "Layer Object";
        }
    }

    private void PART_HeaderTextBoxOnKeyDown(object? sender, KeyEventArgs e) {
        if (this.EditNameState && (e.Key == Key.Escape || e.Key == Key.Enter)) {
            string oldName = this.nameBeforeEditBegin ?? "Layer Object";
            string newName = this.PART_HeaderTextBox!.Text ?? "Layer Object";
            
            this.EditNameState = false;
            if (e.Key == Key.Escape) {
                if (this.LayerObject != null)
                    this.LayerObject.Name = oldName;
            }
            else {
                if (this.LayerObject != null)
                    this.LayerObject.Name = newName;
            }

            
            this.Focus();
            e.Handled = true;
        }

        // FIX prevent arrow key presses in text box from changing the selected tree item.
        // Weirdly, it's only an issue when pressing Down (and I guess up too) and
        // Right (when at the end of the text box)
        if (e.Key == Key.Left || e.Key == Key.Right || e.Key == Key.Up || e.Key == Key.Down) {
            e.Handled = true;
        }
    }

    public void OnAdding(LayerObjectTreeView tree, LayerObjectTreeViewItem parentNode, BaseLayerTreeObject layer) {
        this.LayerTree = tree;
        this.ParentNode = parentNode;
        this.LayerObject = layer;
        // DragDrop.SetAllowDrop(this, layer is CompositionLayer);
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
        this.effectListBoxHelper.SetModel(this.LayerObject);
        DataManager.SetContextData(this, this.contextData.Set(DataKeys.LayerObjectKey, this.LayerObject));
    }

    public void OnRemoving() {
        this.compositeListener?.Dispose();
        int count = this.Items.Count;
        for (int i = count - 1; i >= 0; i--) {
            this.RemoveNode(i);
        }

        this.displayNameBinder.Detach();
        this.stateModifierListBoxHelper.SetModel(null);
        this.effectListBoxHelper.SetModel(null);
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
            
            if (this.LayerTree != null && this.LayerTree.SelectedItems.Count > 1) {
                this.LayerTree?.SetSelection(this);
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

        if (!this.isDragActive || this.isDragDropping || this.LayerTree == null) {
            return;
        }

        if (!(this.LayerObject is BaseLayerTreeObject layer) || layer.Canvas == null) {
            return;
        }

        Point posA = point.Position;
        Point posB = this.originMousePoint;
        Point change = new Point(Math.Abs(posA.X - posB.X), Math.Abs(posA.X - posB.X));
        if (change.X > 4 || change.Y > 4) {
            IList selection = this.LayerTree.SelectedItems;
            if (selection.Count < 1 || !selection.Contains(this)) {
                this.IsSelected = true;
            }

            List<BaseLayerTreeObject> list = selection.Cast<LayerObjectTreeViewItem>().Select(x => x.LayerObject!).ToList();

            try {
                this.isDragDropping = true;
                DataObject obj = new DataObject();
                obj.Set(LayerDropRegistry.DropTypeText, list);

                DragDrop.DoDragDrop(e, obj, DragDropEffects.Move | DragDropEffects.Copy);
            }
            catch (Exception ex) {
                Debug.WriteLine("Exception while executing resource tree item drag drop: " + ex.GetToString());
            }
            finally {
                this.isDragDropping = false;
            }
        }
    }

    private static int Comparison(BaseLayerTreeObject x, BaseLayerTreeObject y) {
        int a = x.ParentLayer?.IndexOf(x) ?? 0;
        int b = y.ParentLayer?.IndexOf(x) ?? 0;
        if (a < b) return -1;
        if (a > b) return 1;
        return 0;
    }

    private void OnDragEnter(DragEventArgs e) {
        this.OnDragOver(e);
    }

    private double DropBorderTopArea => 8.0;
    private double DropBorderBottomArea => (this.Bounds.Height - this.DropBorderTopArea);

    private void OnDragOver(DragEventArgs e) {
        const DragDropEffects args = DragDropEffects.Move | DragDropEffects.Copy;
        if (this.LayerObject != null) {
            Point point = e.GetPosition(this);
            if (point.Y < this.DropBorderTopArea) {
                this.PART_DragDropMoveBorder!.BorderThickness = new Thickness(0, 1, 0, 0);
                e.DragEffects = args;
            }
            else if (point.Y >= this.DropBorderBottomArea) {
                this.PART_DragDropMoveBorder!.BorderThickness = new Thickness(0, 0, 0, 1);
                e.DragEffects = args;
            }
            else {
                if (ProcessCanDragOver(this.LayerObject, e)) {
                    this.IsDroppableTargetOver = true;
                    e.DragEffects = args;
                }

                this.PART_DragDropMoveBorder!.BorderThickness = default;
                return;
            }

            e.Handled = true;
        }
    }

    private void OnDragLeave(DragEventArgs e) {
        if (!this.IsPointerOver) {
            this.IsDroppableTargetOver = false;
            this.PART_DragDropMoveBorder!.BorderThickness = default;
        }
    }

    private async void OnDrop(DragEventArgs e) {
        e.Handled = true;
        if (this.isProcessingAsyncDrop || !(this.LayerObject is BaseLayerTreeObject layer) || layer.ParentLayer == null) {
            return;
        }

        try {
            this.isProcessingAsyncDrop = true;
            if (GetDropResourceListForEvent(e, out List<BaseLayerTreeObject>? list, out EnumDropType effects)) {
                Point point = e.GetPosition(this);
                int type = 0;
                if (point.Y < this.DropBorderTopArea) {
                    type = 1;
                }
                else if (point.Y >= this.DropBorderBottomArea) {
                    type = 2;
                }
                else {
                    await LayerDropRegistry.DropRegistry.OnDropped(layer, list, effects);
                }

                if (type != 0) {
                    if (effects == EnumDropType.Move) {
                        bool isLayerInList = list.Remove(layer);
                        BaseLayerTreeObject.RemoveListFromTree(list);
                        int index = layer.ParentLayer.IndexOf(layer);

                        if (type == 1) {
                            layer.ParentLayer.InsertLayers(index, list);
                        }
                        else {
                            layer.ParentLayer.InsertLayers(index + 1, list);
                        }

                        if (isLayerInList)
                            list.Add(layer);
                    }
                    else if (effects == EnumDropType.Copy) {
                        int index = layer.ParentLayer.IndexOf(layer);
                        List<BaseLayerTreeObject> cloneList = new List<BaseLayerTreeObject>();
                        foreach (BaseLayerTreeObject layerInList in list) {
                            BaseLayerTreeObject clone = layerInList.Clone();
                            if (!TextIncrement.GetIncrementableString((s => true), clone.Name, out string name))
                                name = clone.Name;
                            clone.Name = name;
                            cloneList.Add(clone);
                        }
                        
                        if (type == 1) {
                            layer.ParentLayer.InsertLayers(index, cloneList);
                        }
                        else {
                            layer.ParentLayer.InsertLayers(index + 1, cloneList);
                        }
                    }
                    
                    this.LayerTree!.ClearSelection();
                }
            }
            else if (!await LayerDropRegistry.DropRegistry.OnDroppedNative(layer, new DataObjectWrapper(e.Data), effects)) {
                await IoC.MessageService.ShowMessage("Unknown Data", "Unknown dropped item. Drop files here");
            }
        }
        finally {
            this.IsDroppableTargetOver = false;
            this.isProcessingAsyncDrop = false;
            this.PART_DragDropMoveBorder!.BorderThickness = default;
        }
    }

    public static bool ProcessCanDragOver(BaseLayerTreeObject layer, DragEventArgs e) {
        e.Handled = true;
        if (GetDropResourceListForEvent(e, out List<BaseLayerTreeObject>? resources, out EnumDropType effects)) {
            e.DragEffects = (DragDropEffects) LayerDropRegistry.DropRegistry.CanDrop(layer, resources, effects);
        }
        else {
            e.DragEffects = (DragDropEffects) LayerDropRegistry.DropRegistry.CanDropNative(layer, new DataObjectWrapper(e.Data), effects);
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
        if (e.Data.Contains(LayerDropRegistry.DropTypeText)) {
            object? obj = e.Data.Get(LayerDropRegistry.DropTypeText);
            if ((resources = (obj as List<BaseLayerTreeObject>)) != null) {
                return true;
            }
        }

        resources = null;
        return false;
    }

    #endregion
}