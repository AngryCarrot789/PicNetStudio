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

/// <summary>
/// A class which manages a set of selected objects, assumed to be equal by reference
/// </summary>
public abstract class SimpleSelectionManager<T> : ISelectionManager<T> {
    private readonly HashSet<T> items;

    public int Count => this.items.Count;

    /// <summary>
    /// An event fired when items are selected (newItems) or deselected (oldItems)
    /// </summary>
    public event SelectionChangedEventHandler<T>? SelectionChanged;

    public IEnumerable<T> SelectedItems => this.items;
    
    public SimpleSelectionManager() {
        this.items = new HashSet<T>();
    }

    public bool IsSelected(T item) => this.items.Contains(item);

    public void SetSelection(T item) {
        Validate.NotNull(item);

        IList<T>? newList = this.items.Remove(item) ? null : new SingletonList<T>(item);
        List<T> oldList = this.items.ToList();
        this.items.Clear();
        this.items.Add(item);
        this.SelectionChanged?.Invoke(this, oldList.AsReadOnly(), newList);
    }

    /// <summary>
    /// Replaces our selected items with the given enumerable
    /// </summary>
    /// <param name="newItems">The new selected items</param>
    public void SetSelection(IEnumerable<T> items) {
        if (this.items.Count == 0) {
            this.Select(items);
            return;
        }

        IList<T> list = items as IList<T> ?? items.ToList();
        if (list.Count < 1) {
            return;
        }

        HashSet<T> set1 = new HashSet<T>(list);
        HashSet<T> set2 = new HashSet<T>(this.items);
        List<T> oldItems = new List<T>();
        List<T> newItems = new List<T>();

        foreach (T source in (IEnumerable<T>) this.items) {
            if (set1.Add(source))
                oldItems.Add(source);
        }

        foreach (T source in list) {
            if (set2.Add(source))
                newItems.Add(source);
        }

        this.items.Clear();
        this.items.UnionWith(list);

        if (oldItems.Count > 0 || newItems.Count > 0) {
            this.SelectionChanged?.Invoke(this, oldItems.AsReadOnly(), newItems.AsReadOnly());
        }
    }

    public void Select(T item) {
        Validate.NotNull(item);

        if (this.items.Add(item)) {
            this.SelectionChanged?.Invoke(this, null, new SingletonList<T>(item));
        }
    }

    public void Select(IEnumerable<T> items) {
        Validate.NotNull(items);
        List<T> list = this.items.UnionAddEx(items);
        if (list.Count > 0) {
            this.SelectionChanged?.Invoke(this, null, list.AsReadOnly());
        }
    }

    public void Unselect(T item) {
        Validate.NotNull(item);
        if (this.items.Remove(item)) {
            this.SelectionChanged?.Invoke(this, new SingletonList<T>(item), null);
        }
    }

    public void Unselect(IEnumerable<T> newItems) {
        Validate.NotNull(newItems);
        List<T> list = this.items.UnionRemoveEx(newItems);
        if (list.Count > 0) {
            this.SelectionChanged?.Invoke(this, list.AsReadOnly(), null);
        }
    }

    public void Clear() {
        if (this.items.Count > 0) {
            ReadOnlyCollection<T> oldItems = new ReadOnlyCollection<T>(this.items.ToList());
            this.items.Clear();
            this.SelectionChanged?.Invoke(this, oldItems, null);
        }
    }
}