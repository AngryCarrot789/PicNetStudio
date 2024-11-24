﻿// 
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

using System.Collections.ObjectModel;

namespace PicNetStudio.Utils.Collections.ObservableEx;

public class ReadOnlyObservableListEx<T> : ReadOnlyCollection<T>, IObservableListEx<T> {
    public event ObservableListExChangedEventHandler<T>? CollectionChanged;

    public ReadOnlyObservableListEx(IObservableListEx<T> list) : base(list) {
        list.CollectionChanged += this.HandleCollectionChanged;
    }

    private void HandleCollectionChanged(IObservableListEx<T> list, ObservableListChangedEventArgs<T> e) {
        this.CollectionChanged?.Invoke(this, e);
    }
}