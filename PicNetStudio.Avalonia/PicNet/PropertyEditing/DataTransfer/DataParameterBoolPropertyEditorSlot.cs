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
using PicNetStudio.Avalonia.DataTransfer;

namespace PicNetStudio.Avalonia.PicNet.PropertyEditing.DataTransfer;

public class DataParameterBoolPropertyEditorSlot : DataParameterPropertyEditorSlot {
    private bool value;

    public bool Value {
        get => this.value;
        set {
            this.value = value;
            DataParameterBool parameter = this.Parameter;
            for (int i = 0, c = this.Handlers.Count; i < c; i++) {
                parameter.SetValue((ITransferableData) this.Handlers[i], value);
            }

            this.OnValueChanged(false, true);
        }
    }

    public new DataParameterBool Parameter => (DataParameterBool) base.Parameter;

    public DataParameterBoolPropertyEditorSlot(DataParameterBool parameter, Type applicableType, string displayName) : base(parameter, applicableType, displayName) {
    }

    public override void QueryValueFromHandlers() {
        this.HasMultipleValues = !GetEqualValue(this.Handlers, (x) => this.Parameter.GetValue((ITransferableData) x), out this.value);
    }
}