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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using PicNetStudio.Avalonia.Utils;

namespace PicNetStudio.Avalonia.PicNet.Layers;

public delegate void SelectionManagerSelectionChangedEventHandler<TModel>(SelectionManager<TModel> sender, IList<TModel>? oldItems, IList<TModel>? newItems);

/// <summary>
/// A class which manages a set of selected objects, assumed to be equal by reference
/// </summary>
public class SelectionManager<TModel> {
    private readonly HashSet<TModel> items;

    /// <summary>
    /// An event fired when items are selected (newItems) or deselected (oldItems)
    /// </summary>
    public event SelectionManagerSelectionChangedEventHandler<TModel>? SelectionChanged;

    /// <summary>
    /// Gets the set of selected items
    /// </summary>
    public IReadOnlySet<TModel> Selection => this.items;

    public SelectionManager() {
        this.items = new HashSet<TModel>();
    }

    public bool IsSelected(TModel layer) => this.items.Contains(layer);

    public void SetSelection(TModel item) {
        Validate.NotNull(item);

        IList<TModel>? newList = this.items.Remove(item) ? null : new SingletonList<TModel>(item);
        List<TModel> oldList = this.items.ToList();
        this.items.Clear();
        this.items.Add(item);
        this.SelectionChanged?.Invoke(this, oldList.AsReadOnly(), newList);
    }

    /// <summary>
    /// Replaces our selected items with the given enumerable
    /// </summary>
    /// <param name="newItems">The new selected items</param>
    public void SetSelection(IEnumerable<TModel> selection) {
        if (this.items.Count == 0) {
            this.Select(selection);
            return;
        }

        IList<TModel> list = selection as IList<TModel> ?? selection.ToList();
        if (list.Count < 1) {
            return;
        }

        HashSet<TModel> set1 = new HashSet<TModel>(list);
        HashSet<TModel> set2 = new HashSet<TModel>(this.items);
        List<TModel> oldItems = new List<TModel>();
        List<TModel> newItems = new List<TModel>();

        foreach (TModel source in (IEnumerable<TModel>) this.items) {
            if (set1.Add(source))
                oldItems.Add(source);
        }

        foreach (TModel source in list) {
            if (set2.Add(source))
                newItems.Add(source);
        }

        this.items.Clear();
        this.items.UnionWith(list);

        if (oldItems.Count > 0 || newItems.Count > 0) {
            this.SelectionChanged?.Invoke(this, oldItems.AsReadOnly(), newItems.AsReadOnly());
        }
    }

    public void Select(TModel item) {
        Validate.NotNull(item);

        if (this.items.Add(item)) {
            this.SelectionChanged?.Invoke(this, null, new SingletonList<TModel>(item));
        }
    }

    public void Select(IEnumerable<TModel> newItems) {
        Validate.NotNull(newItems);
        List<TModel> list = this.items.UnionAddEx(newItems);
        if (list.Count > 0) {
            this.SelectionChanged?.Invoke(this, null, list.AsReadOnly());
        }
    }

    public void Unselect(TModel item) {
        Validate.NotNull(item);
        if (this.items.Remove(item)) {
            this.SelectionChanged?.Invoke(this, new SingletonList<TModel>(item), null);
        }
    }

    public void Unselect(IEnumerable<TModel> newItems) {
        Validate.NotNull(newItems);
        List<TModel> list = this.items.UnionRemoveEx(newItems);
        if (list.Count > 0) {
            this.SelectionChanged?.Invoke(this, list.AsReadOnly(), null);
        }
    }

    public void Clear() {
        if (this.items.Count > 0) {
            ReadOnlyCollection<TModel> oldItems = new ReadOnlyCollection<TModel>(this.items.ToList());
            this.items.Clear();
            this.SelectionChanged?.Invoke(this, oldItems, null);
        }
    }
}