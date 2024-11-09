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

using System.Collections.Generic;

namespace PicNetStudio.Avalonia.Utils.Collections.ObservableEx;

public delegate void ObservableListExChangedEventHandler<T>(IObservableListEx<T> list, ObservableListChangedEventArgs<T> e);

/// <summary>
/// A list implementation that invokes a series of events when the collection changes
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IObservableListEx<T> : IList<T> {
    event ObservableListExChangedEventHandler<T>? CollectionChanged;
}