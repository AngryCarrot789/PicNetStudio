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

using System.Collections.Generic;
using Avalonia.Input;
using PicNetStudio.Interactivity;

namespace PicNetStudio.Avalonia.Interactivity;

public class DataObjectWrapper : IDataObjekt {
    private readonly IDataObject mObject;

    public DataObjectWrapper(IDataObject mObject) {
        this.mObject = mObject;
    }

    public object? GetData(string format) {
        return this.mObject.Get(format);
    }

    public bool GetDataPresent(string format) {
        return this.mObject.Contains(format);
    }

    public IEnumerable<string> GetFormats() {
        return this.mObject.GetDataFormats();
    }

    public void SetData(string format, object data) {
        (this.mObject as DataObject)?.Set(format, data);
    }
}