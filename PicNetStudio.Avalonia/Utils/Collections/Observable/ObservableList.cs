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
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace PicNetStudio.Avalonia.Utils.Collections.Observable;

public class ObservableList<T> : Collection<T>, IObservableList<T> {
    private readonly List<T> myItems;
    private SimpleMonitor? _monitor;
    private int blockReentrancyCount;

    public event ObservableListMultipleItemsEventHandler<T>? ItemsAdded;
    public event ObservableListMultipleItemsEventHandler<T>? ItemsRemoved;
    public event ObservableListReplaceEventHandler<T>? ItemReplaced;
    public event ObservableListSingleItemEventHandler<T>? ItemMoved;
    
    public ObservableList() : this(new List<T>()) {
    }

    public ObservableList(IEnumerable<T> collection) : this(new List<T>(collection ?? throw new ArgumentNullException(nameof(collection)))) {
    }

    public ObservableList(List<T> list) : base(new List<T>(list ?? throw new ArgumentNullException(nameof(list)))) {
        this.myItems = (List<T>?) base.Items!;
    }

    public static void Test() {
        ObservableList<int> list = new ObservableList<int>();
        
        // Indexable processor removes back to front as an optimisation, can disable in constructor
        ObservableItemProcessor.MakeIndexable(list, (s, i, o) => {
            Console.WriteLine($"Added '{o}' at {i}");
        }, (s, i, o) => {
            Console.WriteLine($"Removed '{o}' at {i}");
        }, (s, oldI, newI, o) => {
            Console.WriteLine($"Moved '{o}' from {oldI} to {newI}");
        });
        
        list.Add(0);
        list.Add(1);
        list.Add(2);
        list.Add(3);
        list.Add(4);
        list.Add(5);
        list.Add(6);
        list.Add(7);
        list.Add(8);
        
        // list = 0,1,2,3,4,5,6,7,8
        // Removing 4 items at index 2 removes 2,3,4,5
        // Remaining list = 0,1,6,7,8
        list.RemoveRange(2, 4);
        
        // assert list.Count == 5
        // assert list == [0,1,6,7,8]
    }

    protected override void ClearItems() {
        this.CheckReentrancy();
        if (this.Items.Count < 1) {
            return;
        }
        
        ObservableListMultipleItemsEventHandler<T>? handler = this.ItemsRemoved;
        if (handler == null) {
            this.myItems.Clear();
        }
        else {
            ReadOnlyCollection<T> items = this.myItems.ToList().AsReadOnly();
            this.myItems.Clear();
            try {
                this.blockReentrancyCount++;
                handler(this, items, 0);   
            }
            finally {
                this.blockReentrancyCount--;
            }
        }
    }

    protected override void RemoveItem(int index) {
        this.CheckReentrancy();
        ObservableListMultipleItemsEventHandler<T>? handler = this.ItemsRemoved;
        if (handler == null) {
            this.myItems.RemoveAt(index);
        }
        else {
            T removedItem = this[index];
            this.myItems.RemoveAt(index);
            try {
                this.blockReentrancyCount++;
                handler(this, new SingletonList<T>(removedItem), index);
            }
            finally {
                this.blockReentrancyCount--;
            }
        }
    }
    
    public void RemoveRange(int index, int count) {
        this.CheckReentrancy();
        ObservableListMultipleItemsEventHandler<T>? handler = this.ItemsRemoved;
        if (handler == null) {
            this.myItems.RemoveRange(index, count);
        }
        else {
            List<T> items = this.myItems.Slice(index, count);
            this.myItems.RemoveRange(index, count);
            try {
                this.blockReentrancyCount++;
                handler(this, items.AsReadOnly(), index);
            }
            finally {
                this.blockReentrancyCount--;
            }
        }
    }
    
    protected override void InsertItem(int index, T item) {
        this.CheckReentrancy();
        this.myItems.Insert(index, item);
        try {
            this.blockReentrancyCount++;
            this.ItemsAdded?.Invoke(this, new SingletonList<T>(item), index);
        }
        finally {
            this.blockReentrancyCount--;
        }
    }
    
    public void InsertRange(int index, IEnumerable<T> items) {
        this.CheckReentrancy();
        ObservableListMultipleItemsEventHandler<T>? handler = this.ItemsAdded;
        
        // Slight risk in passing list to the ItemsAdded event in case items mutates asynchronously... meh
        if (items is IList<T> list) {
            this.myItems.InsertRange(index, list);
        }
        else {
            // Probably enumerator method or something along those lines, convert to list for speedy insertion
            list = items.ToList();
            this.myItems.InsertRange(index, list);
            if (handler != null) // Stops the event handler modifying the list
                list = list.AsReadOnly();
        }
        
        try {
            this.blockReentrancyCount++;
            handler?.Invoke(this, list, index);
        }
        finally {
            this.blockReentrancyCount--;
        }
    }

    protected override void SetItem(int index, T newItem) {
        this.CheckReentrancy();
        T oldItem = this[index];
        base.SetItem(index, newItem);
        try {
            this.blockReentrancyCount++;
            this.ItemReplaced?.Invoke(this, oldItem, newItem, index);
        }
        finally {
            this.blockReentrancyCount--;
        }
    }

    public void Move(int oldIndex, int newIndex) => this.MoveItem(oldIndex, newIndex);
    
    protected virtual void MoveItem(int oldIndex, int newIndex) {
        this.CheckReentrancy();
        T item = this[oldIndex];
        base.RemoveItem(oldIndex);
        base.InsertItem(newIndex, item);
        try {
            this.blockReentrancyCount++;
            this.ItemMoved?.Invoke(this, item, oldIndex, newIndex);
        }
        finally {
            this.blockReentrancyCount--;
        }
    }

    protected IDisposable BlockReentrancy() {
        this.blockReentrancyCount++;
        return this.EnsureMonitorInitialized();
    }

    protected void CheckReentrancy() {
        if (this.blockReentrancyCount > 0) {
            // we can allow changes if there's only one listener - the problem
            // only arises if reentrant changes make the original event args
            // invalid for later listeners.  This keeps existing code working
            // (e.g. Selector.SelectedItems).
            if (this.ItemsAdded?.GetInvocationList().Length > 1 ||
                this.ItemsRemoved?.GetInvocationList().Length > 1 ||
                this.ItemReplaced?.GetInvocationList().Length > 1 ||
                this.ItemMoved?.GetInvocationList().Length > 1)
                throw new InvalidOperationException("Reentrancy Not Allowed");
        }
    }

    private SimpleMonitor EnsureMonitorInitialized() => this._monitor ??= new SimpleMonitor(this);

    private sealed class SimpleMonitor : IDisposable {
        internal int _busyCount; // Only used during (de)serialization to maintain compatibility with desktop. Do not rename (binary serialization)

        [NonSerialized] internal ObservableList<T> _collection;

        public SimpleMonitor(ObservableList<T> collection) {
            Debug.Assert(collection != null);
            this._collection = collection;
        }

        public void Dispose() => this._collection.blockReentrancyCount--;
    }
}