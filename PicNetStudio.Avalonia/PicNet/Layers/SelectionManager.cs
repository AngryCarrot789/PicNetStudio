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
using System.Runtime.CompilerServices;
using System.Threading;
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
    
    public void SetSelection(IEnumerable<TModel> newItems) {
        // too lazy to implement an optimised version
        this.Clear();
        this.Select(newItems);
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