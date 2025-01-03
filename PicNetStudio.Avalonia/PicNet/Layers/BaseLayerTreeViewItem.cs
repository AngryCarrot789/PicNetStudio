﻿// 
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
using PicNetStudio.Avalonia.PicNet.Effects.Controls;
using PicNetStudio.Avalonia.PicNet.Layers.StateMods;
using PicNetStudio.Avalonia.Utils;
using PicNetStudio.Interactivity;
using PicNetStudio.Interactivity.Contexts;
using PicNetStudio.PicNet;
using PicNetStudio.PicNet.Layers;
using PicNetStudio.Utils;
using PicNetStudio.Utils.Collections.Observable;
using Key = Avalonia.Input.Key;

namespace PicNetStudio.Avalonia.PicNet.Layers;

public abstract class BaseLayerTreeViewItem : TreeViewItem, ILayerNodeItem {
    public static readonly StyledProperty<bool> IsDroppableTargetOverProperty = BaseLayerTreeView.IsDroppableTargetOverProperty.AddOwner<BaseLayerTreeView>();
    public static readonly DirectProperty<BaseLayerTreeViewItem, bool> IsFolderItemProperty = AvaloniaProperty.RegisterDirect<BaseLayerTreeViewItem, bool>("IsFolderItem", o => o.IsFolderItem, null);
    
    public BaseLayerTreeView? LayerTree { get; private set; }
    public BaseLayerTreeViewItem? ParentNode { get; private set; }
    public BaseLayerTreeObject? LayerObject { get; private set; }
    
    public bool IsDroppableTargetOver {
        get => this.GetValue(IsDroppableTargetOverProperty);
        set => this.SetValue(IsDroppableTargetOverProperty, value);
    }
    
    public bool IsFolderItem {
        get => this.isFolderItem;
        private set => this.SetAndRaise(IsFolderItemProperty, ref this.isFolderItem, value);
    }

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
    private bool wasSelectedOnPress;
    private bool isFolderItem;

    private TextBlock? PART_HeaderTextBlock;
    private TextBox? PART_HeaderTextBox;
    private readonly ContextData contextData;

    private enum DragState {
        None = 0,       // No drag drop has been started yet
        Standby = 1,    // User left-clicked, so wait for enough move mvoement
        Active = 2,     // User moved their mouse enough. DragDrop is running
        Completed = 3   // Layer dropped, this is used to ensure we don't restart
                        // when the mouse moves again until they release the left mouse
    }
    
    private DragState dragBtnState;

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

    public BaseLayerTreeViewItem() {
        this.stateModifierListBoxHelper = new PropertyAutoSetter<BaseLayerTreeObject, LayerStateModifierListBox>(LayerStateModifierListBox.LayerObjectProperty);
        this.effectListBoxHelper = new PropertyAutoSetter<BaseLayerTreeObject, EffectListBox>(EffectListBox.LayerObjectProperty);
        DragDrop.SetAllowDrop(this, true);
        DataManager.SetContextData(this, this.contextData = new ContextData().Set(DataKeys.LayerNodeKey, this));
    }

    static BaseLayerTreeViewItem() {
        DragDrop.DragEnterEvent.AddClassHandler<BaseLayerTreeViewItem>((o, e) => o.OnDragEnter(e));
        DragDrop.DragOverEvent.AddClassHandler<BaseLayerTreeViewItem>((o, e) => o.OnDragOver(e));
        DragDrop.DragLeaveEvent.AddClassHandler<BaseLayerTreeViewItem>((o, e) => o.OnDragLeave(e));
        DragDrop.DropEvent.AddClassHandler<BaseLayerTreeViewItem>((o, e) => o.OnDrop(e));
        IsSelectedProperty.Changed.AddClassHandler<BaseLayerTreeViewItem, bool>((o, e) => {

        });
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

    public virtual void OnAdding(BaseLayerTreeView tree, BaseLayerTreeViewItem? parentNode, BaseLayerTreeObject layer) {
        this.LayerTree = tree;
        this.ParentNode = parentNode;
        this.LayerObject = layer;
        this.IsFolderItem = layer is CompositionLayer;
    }

    public virtual void OnAdded() {
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

    public virtual void OnRemoving() {
        this.compositeListener?.Dispose();
        int count = this.Items.Count;
        for (int i = count - 1; i >= 0; i--) {
            this.RemoveNode(i);
        }

        this.displayNameBinder.Detach();
        this.stateModifierListBoxHelper.SetModel(null);
        this.effectListBoxHelper.SetModel(null);
    }

    public virtual void OnRemoved() {
        this.LayerTree = null;
        this.ParentNode = null;
        this.LayerObject = null;
        this.IsFolderItem = false;
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

    public BaseLayerTreeViewItem GetNodeAt(int index) => (BaseLayerTreeViewItem) this.Items[index]!;

    public void InsertNode(BaseLayerTreeObject item, int index) {
        this.InsertNode(null, item, index);
    }

    public void InsertNode(BaseLayerTreeViewItem? control, BaseLayerTreeObject layer, int index) {
        BaseLayerTreeView? tree = this.LayerTree;
        if (tree == null)
            throw new InvalidOperationException("Cannot add children when we have no resource tree associated");
        if (control == null)
            control = tree.GetCachedItemOrNew();

        control.OnAdding(tree, this, layer);
        this.Items.Insert(index, control);
        tree.AddResourceMapping(control, layer);
        control.ApplyStyling();
        control.ApplyTemplate();
        control.OnAdded();
    }

    public void RemoveNode(int index, bool canCache = true) {
        BaseLayerTreeView? tree = this.LayerTree;
        if (tree == null)
            throw new InvalidOperationException("Cannot remove children when we have no resource tree associated");

        BaseLayerTreeViewItem control = (BaseLayerTreeViewItem) this.Items[index]!;
        BaseLayerTreeObject resource = control.LayerObject ?? throw new Exception("Invalid application state");
        control.OnRemoving();
        this.Items.RemoveAt(index);
        tree.RemoveResourceMapping(control, resource);
        control.OnRemoved();
        if (canCache)
            tree.PushCachedItem(control);
    }

    public void MoveNode(int oldIndex, int newIndex) {
        BaseLayerTreeView? tree = this.LayerTree;
        if (tree == null)
            throw new InvalidOperationException("Cannot remove children when we have no resource tree associated");

        BaseLayerTreeViewItem control = (BaseLayerTreeViewItem) this.Items[oldIndex]!;
        this.Items.RemoveAt(oldIndex);
        this.Items.Insert(newIndex, control);
    }

    #region Drag Drop

    public static bool CanBeginDragDrop(KeyModifiers modifiers) {
        return (modifiers & (KeyModifiers.Shift)) == 0;
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e) {
        base.OnPointerPressed(e);
        if (e.Handled || this.LayerObject == null) {
            return;
        }

        PointerPoint point = e.GetCurrentPoint(this);
        if (point.Properties.PointerUpdateKind == PointerUpdateKind.LeftButtonPressed) {
            bool isToggle = (e.KeyModifiers & KeyModifiers.Control) != 0;
            if ((e.ClickCount % 2) == 0) {
                if (!isToggle) {
                    this.SetCurrentValue(IsExpandedProperty, !this.IsExpanded);
                    e.Handled = true;
                }
            }
            else if (CanBeginDragDrop(e.KeyModifiers)) {
                if ((this.IsFocused || this.Focus()) && !this.isDragDropping) {
                    this.dragBtnState = DragState.Standby;
                    e.Pointer.Capture(this);
                    this.originMousePoint = point.Position;
                    this.isDragActive = true;
                    if (this.LayerTree != null) {
                        this.wasSelectedOnPress = this.IsSelected;
                        if (isToggle) {
                            if (this.wasSelectedOnPress) {
                                // do nothing; toggle selection in mouse release
                            }
                            else {
                                this.SetCurrentValue(IsSelectedProperty, true);
                            }
                        }
                        else if (this.LayerTree.SelectedItems.Count < 2 || !this.wasSelectedOnPress) {
                            // Set as only selection if 0 or 1 items selected, or we aren't selected
                            this.LayerTree?.SetSelection(this);
                        }
                    }
                }

                // handle to stop tree view from selecting stuff
                e.Handled = true;
            }

        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e) {
        base.OnPointerReleased(e);
        PointerPoint point = e.GetCurrentPoint(this);
        if (point.Properties.PointerUpdateKind == PointerUpdateKind.LeftButtonReleased) {
            if (this.isDragActive) {
                DragState lastDragState = this.dragBtnState;
                this.dragBtnState = DragState.None;
                this.isDragActive = false;
                if (ReferenceEquals(e.Pointer.Captured, this)) {
                    e.Pointer.Capture(null);
                }

                if (this.LayerTree != null) {
                    bool isToggle = (e.KeyModifiers & KeyModifiers.Control) != 0;
                    int selCount = this.LayerTree!.SelectedItems.Count;
                    if (selCount == 0) {
                        // very rare scenario, shouldn't really occur
                        this.LayerTree?.SetSelection(this);
                    }
                    else if (isToggle && this.wasSelectedOnPress && lastDragState != DragState.Completed) {
                        // Check we want to toggle, check we were selected on click and we probably are still selected,
                        // and also check that the last drag wasn't completed/cancelled just because it feels more normal that way
                        this.SetCurrentValue(IsSelectedProperty, false);
                    }
                    else if (selCount > 1 && !isToggle && lastDragState != DragState.Completed) {
                        this.LayerTree?.SetSelection(this);
                    }
                }
                // if (this.dragBtnState != DragState.Completed && (e.KeyModifiers & KeyModifiers.Control) != 0) {
                //     this.SetCurrentValue(IsSelectedProperty, !this.IsSelected);
                //     e.Handled = true;
                // }
                // else if (this.LayerTree != null && this.LayerTree.SelectedItems.Count > 1) {
                //     this.LayerTree?.SetSelection(this);
                // }
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

        if (this.dragBtnState != DragState.Standby) {
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

            List<BaseLayerTreeObject> list = selection.Cast<BaseLayerTreeViewItem>().Select(x => x.LayerObject!).ToList();

            try {
                this.isDragDropping = true;
                DataObject obj = new DataObject();
                obj.Set(LayerDropRegistry.DropTypeText, list);

                this.dragBtnState = DragState.Active;
                DragDrop.DoDragDrop(e, obj, DragDropEffects.Move | DragDropEffects.Copy);
            }
            catch (Exception ex) {
                Debug.WriteLine("Exception while executing resource tree item drag drop: " + ex.GetToString());
            }
            finally {
                this.dragBtnState = DragState.Completed;
                this.isDragDropping = false;
            }
        }
    }

    private static int Comparison(BaseLayerTreeObject x, BaseLayerTreeObject y) {
        int a = x.ParentLayer?.IndexOf(x) ?? 0;
        int b = y.ParentLayer?.IndexOf(x) ?? 0;
        if (a < b)
            return -1;
        if (a > b)
            return 1;
        return 0;
    }

    private void OnDragEnter(DragEventArgs e) {
        this.OnDragOver(e);
    }

    private void GetDropBorder(bool useFullHeight, out double borderTop, out double borderBottom) {
        const double NormalBorder = 8.0;
        if (useFullHeight) {
            borderTop = borderBottom = this.Bounds.Height / 2.0;
        }
        else {
            borderTop = NormalBorder;
            borderBottom = this.Bounds.Height - NormalBorder;
        }
    }

    private void OnDragOver(DragEventArgs e) {
        const DragDropEffects args = DragDropEffects.Move | DragDropEffects.Copy;
        if (this.LayerObject != null) {
            bool isDropAbove;
            bool? canDragOver = ProcessCanDragOver(this, this.LayerObject, e);
            if (!canDragOver.HasValue) {
                e.DragEffects = DragDropEffects.None;
                return;
            }

            this.GetDropBorder(!(canDragOver ?? false), out double borderTop, out double borderBottom);
            Point point = e.GetPosition(this);
            if (DoubleUtils.LessThan(point.Y, borderTop)) {
                isDropAbove = true;
                e.DragEffects = args;
            }
            else if (DoubleUtils.GreaterThanOrClose(point.Y, borderBottom)) {
                isDropAbove = false;
                e.DragEffects = args;
            }
            else {
                if (canDragOver == true) {
                    this.IsDroppableTargetOver = true;
                    e.DragEffects = args;
                }

                this.PART_DragDropMoveBorder!.BorderThickness = default;
                return;
            }

            this.PART_DragDropMoveBorder!.BorderThickness = isDropAbove ? new Thickness(0, 1, 0, 0) : new Thickness(0, 0, 0, 1);
            e.Handled = true;
        }
    }

    private void OnDragLeave(DragEventArgs e) {
        if (!this.IsPointerOver) {
            this.IsDroppableTargetOver = false;
            this.PART_DragDropMoveBorder!.BorderThickness = default;
        }
    }

    public static EnumDropType CanDropItemsOnCompositionLayer(CompositionLayer target, List<BaseLayerTreeObject> items, EnumDropType dropType) {
        if (dropType == EnumDropType.None || dropType == EnumDropType.Link) {
            return EnumDropType.None;
        }

        foreach (BaseLayerTreeObject item in items) {
            if (item is CompositionLayer folder && folder.IsParentInHierarchy(target)) {
                return EnumDropType.None;
            }
            else if (dropType != EnumDropType.Copy) {
                if (target.Contains(item)) {
                    return EnumDropType.None;
                }
            }
        }

        return dropType;
    }

    private async void OnDrop(DragEventArgs e) {
        e.Handled = true;
        if (this.isProcessingAsyncDrop || !(this.LayerObject is BaseLayerTreeObject layer) || layer.ParentLayer == null) {
            return;
        }

        try {
            this.isProcessingAsyncDrop = true;
            Point point = e.GetPosition(this);
            double borderTop, borderBottom;
            if (!GetDropResourceListForEvent(e, out List<BaseLayerTreeObject>? srcItemList, out EnumDropType dropType)) {
                this.GetDropBorder(true, out borderTop, out borderBottom);
                bool thing = DoubleUtils.LessThan(point.Y, borderTop);
                ContextData ctx = new ContextData(DataManager.GetFullContextData((BaseLayerTreeViewItem) e.Source!)).Set(LayerDropRegistry.IsAboveTarget, thing);
                if (await LayerDropRegistry.DropRegistry.OnDroppedNative(layer, new DataObjectWrapper(e.Data), dropType, ctx)) {
                    return;
                }

                await IoC.MessageService.ShowMessage("Unknown Data", "Unknown dropped item. Drop files here");
                return;
            }

            bool isDropAbove;
            bool? canDragOver = ProcessCanDragOver(this, this.LayerObject, e);
            if (!canDragOver.HasValue) {
                return;
            }

            List<BaseLayerTreeObject> newList;
            this.GetDropBorder(!(canDragOver ?? false), out borderTop, out borderBottom);
            if (DoubleUtils.LessThan(point.Y, borderTop)) {
                isDropAbove = true;
            }
            else if (DoubleUtils.GreaterThanOrClose(point.Y, borderBottom)) {
                isDropAbove = false;
            }
            else if (layer is CompositionLayer folder) {
                if (dropType != EnumDropType.Copy && dropType != EnumDropType.Move) {
                    return;
                }

                newList = new List<BaseLayerTreeObject>();
                foreach (BaseLayerTreeObject item in srcItemList) {
                    if (item is CompositionLayer composition && composition.IsParentInHierarchy(folder)) {
                        continue;
                    }

                    if (dropType == EnumDropType.Copy) {
                        BaseLayerTreeObject clone = item.Clone();
                        if (!TextIncrement.GetIncrementableString((s => true), clone.Name, out string name))
                            name = clone.Name;
                        clone.Name = name;
                        newList.Add(clone);
                    }
                    else if (item.ParentLayer != null) {
                        if (item.ParentLayer != folder) {
                            item.ParentLayer?.RemoveLayer(item);
                            newList.Add(item);
                        }
                    }
                    else {
                        Debug.Assert(false, "No parent");
                        // ???
                        // AppLogger.Instance.WriteLine("A resource was dropped with a null parent???");
                    }
                }

                folder.InsertLayers(0, newList);
                return;
            }
            else {
                await LayerDropRegistry.DropRegistry.OnDropped(layer, srcItemList, dropType);
                return;
            }

            // TODO: fix stack overflow when dropping a layer into itself...
            // if (layer.ParentLayer == null || LayerDropRegistry.CanDropItems(layer.ParentLayer, list, effects, false) == EnumDropType.None)
            //     return;

            // I think this works?
            CompositionLayer? target = layer.ParentLayer;
            if (target == null || (!target.IsRootLayer && srcItemList.Any(x => x is CompositionLayer cl && cl.ParentLayer != null && cl.ParentLayer.IsParentInHierarchy(cl)))) {
                return;
            }

            int index;
            bool isLayerInList = false;
            switch (dropType) {
                case EnumDropType.Move:
                    isLayerInList = srcItemList.Remove(layer);
                    BaseLayerTreeObject.RemoveListFromTree(srcItemList);
                    index = layer.GetIndexInParent();
                    newList = srcItemList;
                    break;
                case EnumDropType.Copy: {
                    index = layer.GetIndexInParent();
                    List<BaseLayerTreeObject> cloneList = new List<BaseLayerTreeObject>();
                    foreach (BaseLayerTreeObject layerInList in srcItemList) {
                        BaseLayerTreeObject clone = layerInList.Clone();
                        if (!TextIncrement.GetIncrementableString((s => true), clone.Name, out string name))
                            name = clone.Name;
                        clone.Name = name;
                        cloneList.Add(clone);
                    }

                    newList = cloneList;
                    break;
                }
                default: return;
            }

            layer.ParentLayer.InsertLayers(isDropAbove ? index : (index + 1), newList);
            if (dropType == EnumDropType.Move && isLayerInList) {
                newList.Add(layer);
            }

            this.LayerTree!.SetSelection(newList);
        }
        finally {
            this.IsDroppableTargetOver = false;
            this.isProcessingAsyncDrop = false;
            this.PART_DragDropMoveBorder!.BorderThickness = default;
        }
    }

    // True = yes, False = no, Null = invalid due to composition layers
    public static bool? ProcessCanDragOver(AvaloniaObject sender, BaseLayerTreeObject target, DragEventArgs e) {
        e.Handled = true;
        if (GetDropResourceListForEvent(e, out List<BaseLayerTreeObject>? items, out EnumDropType effects)) {
            if (target is CompositionLayer composition) {
                if (!composition.IsRootLayer && items.Any(x => x is CompositionLayer cl && cl.ParentLayer != null && cl.ParentLayer.IsParentInHierarchy(cl))) {
                    return null;
                }
            }
            else {
                e.DragEffects = (DragDropEffects) LayerDropRegistry.DropRegistry.CanDrop(target, items, effects, DataManager.GetFullContextData(sender));
            }
        }
        else {
            e.DragEffects = (DragDropEffects) LayerDropRegistry.DropRegistry.CanDropNative(target, new DataObjectWrapper(e.Data), effects, DataManager.GetFullContextData(sender));
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