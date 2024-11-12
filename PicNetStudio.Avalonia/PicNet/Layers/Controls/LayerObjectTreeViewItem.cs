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
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using PicNetStudio.Avalonia.Bindings;
using PicNetStudio.Avalonia.Interactivity.Contexts;
using PicNetStudio.Avalonia.PicNet.Layers.StateMods.Controls;
using PicNetStudio.Avalonia.Utils;
using PicNetStudio.Avalonia.Utils.Collections.Observable;

namespace PicNetStudio.Avalonia.PicNet.Layers.Controls;

/// <summary>
/// A tree view item for all layer objects
/// </summary>
public class LayerObjectTreeViewItem : TreeViewItem {
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

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        this.stateModifierListBoxHelper.SetControl(e.NameScope.GetTemplateChild<LayerStateModifierListBox>("PART_StateModifierListBox"));
    }

    public void OnAdding(LayerObjectTreeView tree, LayerObjectTreeViewItem parentNode, BaseLayerTreeObject layer) {
        this.LayerTree = tree;
        this.ParentNode = parentNode;
        this.LayerObject = layer;
        DragDrop.SetAllowDrop(this, layer is ILayerContainer);
    }

    public void OnAdded() {
        if (this.LayerObject is ILayerContainer folder) {
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
}