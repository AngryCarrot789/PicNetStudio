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

using System;
using Avalonia;
using PicNetStudio.DataTransfer;

namespace PicNetStudio.Avalonia.Bindings;

public class AutoUpdateDataParameterPropertyBinder<TModel> : AutoUpdatePropertyBinder<TModel> where TModel : class, ITransferableData {
    public DataParameter Parameter { get; }

    public AutoUpdateDataParameterPropertyBinder(DataParameter parameter, Action<IBinder<TModel>>? updateControl, Action<IBinder<TModel>>? updateModel = null) : base(updateControl, updateModel) {
        this.Parameter = parameter ?? throw new ArgumentNullException(nameof(parameter));
    }

    public AutoUpdateDataParameterPropertyBinder(DataParameter parameter, AvaloniaProperty? property, Action<IBinder<TModel>>? updateControl, Action<IBinder<TModel>>? updateModel = null) : base(property, updateControl, updateModel) {
        this.Parameter = parameter ?? throw new ArgumentNullException(nameof(parameter));
    }

    protected override void OnAttached() {
        base.OnAttached();
        this.Parameter.AddValueChangedHandler(this.Model, this.OnParameterChanged);
    }

    protected override void OnDetached() {
        base.OnDetached();
        this.Parameter.RemoveValueChangedHandler(this.Model, this.OnParameterChanged);
    }

    private void OnParameterChanged(DataParameter parameter, ITransferableData owner) {
        this.UpdateControl();
    }
}