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
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace PicNetStudio.Avalonia.PicNet.Layers.Controls;

public class TreeViewSelectionManager : ISelectionManager<BaseLayerTreeObject>, ILightSelectionManager<BaseLayerTreeObject> {
    private BaseLayerTreeView? tree;

    public BaseLayerTreeView? Tree {
        get => this.tree;
        set {
            BaseLayerTreeView? oldTree = this.tree;
            ReadOnlyCollection<BaseLayerTreeObject>? oldItems = null;
            INotifyCollectionChanged? listener;
            if (oldTree != null) {
                listener = oldTree.SelectedItems as INotifyCollectionChanged;
                if (value == null) {
                    // Tree is being set to null; clear selection first
                    oldTree.ClearSelection();
                    if (listener != null)
                        listener.CollectionChanged -= this.OnSelectionCollectionChanged;
                    
                    this.tree = null;
                    return;
                }

                // Since there's an old and new tree, we need to first say cleared then selection
                // changed from old selection to new selection, even if they're the exact same
                if ((oldItems = ProcessList(ControlToModelList(oldTree).ToList())) != null && this.KeepSelectedItemsFromOldTree)
                    oldTree.ClearSelection();
                
                if (listener != null)
                    listener.CollectionChanged -= this.OnSelectionCollectionChanged;
            }

            this.tree = value;
            if (value != null) {
                if ((listener = value.SelectedItems as INotifyCollectionChanged) != null)
                    listener.CollectionChanged += this.OnSelectionCollectionChanged;
                
                if (this.KeepSelectedItemsFromOldTree) {
                    if (oldItems != null)
                        this.Select(oldItems);
                }
                else {
                    ReadOnlyCollection<BaseLayerTreeObject>? newItems = ProcessList(ControlToModelList(value).ToList());
                    this.OnSelectionChanged(oldItems, newItems);
                }
            }
        }
    }

    public int Count => this.tree?.SelectedItems.Count ?? 0;

    /// <summary>
    /// Specifies whether to move the old tree's selected items to the new tree when our <see cref="Tree"/> property changes. True by default.
    /// <br/>
    /// <para>
    /// When true, the old tree's items are saved then the tree is cleared, and the new tree's selection becomes that saved list
    /// </para>
    /// <para>
    /// When false, the <see cref="SelectionCleared"/> event is raised (if the old tree is valid) and then the selection changed event is raised on the new tree's pre-existing selected items.
    /// </para>
    /// </summary>
    public bool KeepSelectedItemsFromOldTree { get; set; } = true;

    public IEnumerable<BaseLayerTreeObject> SelectedItems => this.tree != null ? ControlToModelList(this.tree).ToList() : ImmutableArray<BaseLayerTreeObject>.Empty;

    public event SelectionChangedEventHandler<BaseLayerTreeObject>? SelectionChanged;
    public event SelectionClearedEventHandler<BaseLayerTreeObject>? SelectionCleared;

    private LightSelectionChangedEventHandler<BaseLayerTreeObject>? LightSelectionChanged;

    event LightSelectionChangedEventHandler<BaseLayerTreeObject>? ILightSelectionManager<BaseLayerTreeObject>.SelectionChanged {
        add => this.LightSelectionChanged += value;
        remove => this.LightSelectionChanged -= value;
    }

    public TreeViewSelectionManager() {
    }

    public TreeViewSelectionManager(BaseLayerTreeView treeView) {
        this.tree = treeView;
    }
    
    private void OnSelectionCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) {
        switch (e.Action) {
            case NotifyCollectionChangedAction.Add:
                this.ProcessTreeSelection(null, e.NewItems ?? null);
                break;
            case NotifyCollectionChangedAction.Remove:
                this.ProcessTreeSelection(e.OldItems, null);
                break;
            case NotifyCollectionChangedAction.Replace:
                this.ProcessTreeSelection(e.OldItems, e.NewItems ?? null);
                break;
            case NotifyCollectionChangedAction.Reset:
                if (this.tree != null)
                    this.OnSelectionCleared();
                break;
            case NotifyCollectionChangedAction.Move: break;
            default: throw new ArgumentOutOfRangeException();
        }
    }
    
    internal void ProcessTreeSelection(IList? oldItems, IList? newItems) {
        ReadOnlyCollection<BaseLayerTreeObject>? oldList = oldItems?.Cast<BaseLayerTreeViewItem>().Select(x => x.LayerObject!).ToList().AsReadOnly();
        ReadOnlyCollection<BaseLayerTreeObject>? newList = newItems?.Cast<BaseLayerTreeViewItem>().Select(x => x.LayerObject!).ToList().AsReadOnly();
        if (oldList?.Count > 0 || newList?.Count > 0) {
            this.OnSelectionChanged(oldList, newList);
        }
    }

    public bool IsSelected(BaseLayerTreeObject item) {
        if (this.tree == null)
            return false;
        if (this.tree.modelToControl.TryGetValue(item, out BaseLayerTreeViewItem? treeItem))
            return treeItem.IsSelected;
        return false;
    }

    private void OnSelectionChanged(ReadOnlyCollection<BaseLayerTreeObject>? oldList, ReadOnlyCollection<BaseLayerTreeObject>? newList) {
        if (ReferenceEquals(oldList, newList) || oldList?.Count < 1 || newList?.Count < 1) {
            return;
        }
        
        this.SelectionChanged?.Invoke(this, oldList, newList);
        this.LightSelectionChanged?.Invoke(this);
    }

    private void OnSelectionCleared() {
        this.SelectionCleared?.Invoke(this);
        this.LightSelectionChanged?.Invoke(this);
    }

    public void SetSelection(BaseLayerTreeObject item) {
        if (this.tree == null) {
            return;
        }

        this.tree.ClearSelection();
        this.Select(item);
    }

    public void SetSelection(IEnumerable<BaseLayerTreeObject> items) {
        if (this.tree == null) {
            return;
        }

        this.tree.ClearSelection();
        this.Select(items);
    }

    public void Select(BaseLayerTreeObject item) {
        if (this.tree == null) {
            return;
        }

        if (this.tree.modelToControl.TryGetValue(item, out BaseLayerTreeViewItem? treeItem)) {
            treeItem.IsSelected = true;
        }
    }

    public void Select(IEnumerable<BaseLayerTreeObject> items) {
        if (this.tree == null) {
            return;
        }

        foreach (BaseLayerTreeObject item in items.ToList()) {
            if (this.tree.modelToControl.TryGetValue(item, out BaseLayerTreeViewItem? treeItem)) {
                treeItem.IsSelected = true;
            }
        }
    }

    public void Unselect(BaseLayerTreeObject item) {
        if (this.tree == null) {
            return;
        }

        if (this.tree.modelToControl.TryGetValue(item, out BaseLayerTreeViewItem? treeItem)) {
            treeItem.IsSelected = false;
        }
    }

    public void Unselect(IEnumerable<BaseLayerTreeObject> items) {
        if (this.tree == null) {
            return;
        }

        List<BaseLayerTreeObject> list = items.ToList();
        foreach (BaseLayerTreeObject item in list) {
            if (this.tree.modelToControl.TryGetValue(item, out BaseLayerTreeViewItem? treeItem)) {
                treeItem.IsSelected = false;
            }
        }
    }

    public void Clear() {
        this.tree?.ClearSelection();
    }
    
    private static IEnumerable<BaseLayerTreeObject> ControlToModelList(BaseLayerTreeView tree) => tree.SelectedItems.Cast<BaseLayerTreeViewItem>().Select(x => x.LayerObject!);
    private static ReadOnlyCollection<BaseLayerTreeObject>? ProcessList(List<BaseLayerTreeObject>? list) => list != null && list.Count > 0 ? list.AsReadOnly() : null;
}