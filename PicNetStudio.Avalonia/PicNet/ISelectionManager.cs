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

using System.Collections.Generic;

namespace PicNetStudio.Avalonia.PicNet;

public delegate void SelectionChangedEventHandler<T>(ISelectionManager<T> sender, IList<T>? oldItems, IList<T>? newItems);
public delegate void SelectionClearedEventHandler<T>(ISelectionManager<T> sender);

/// <summary>
/// An interface for an object that manages the selection state of items
/// </summary>
public interface ISelectionManager<T> {
    // this.SelectedItems.Cast<LayerObjectTreeViewItem>().Select(x => x.LayerObject!).NonNull().ToList()
    /// <summary>
    /// Gets a read-only collection of selected items
    /// </summary>
    IEnumerable<T> SelectedItems { get; }

    /// <summary>
    /// Gets the number of selected items
    /// </summary>
    int Count { get; }

    /// <summary>
    /// An event fired when the selection changes (item added or removed)
    /// </summary>
    public event SelectionChangedEventHandler<T>? SelectionChanged;
    
    /// <summary>
    /// An event fired when the selection is cleared
    /// </summary>
    public event SelectionClearedEventHandler<T>? SelectionCleared;

    bool IsSelected(T item);
    void SetSelection(T item);
    void SetSelection(IEnumerable<T> items);
    void Select(T item);
    void Select(IEnumerable<T> items);
    void Unselect(T item);
    void Unselect(IEnumerable<T> newItems);
    void Clear();
}