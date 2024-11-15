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

public class DataParameterFloatPropertyEditorSlot : DataParameterPropertyEditorSlot {
    private float value;

    public float Value {
        get => this.value;
        set {
            float oldVal = this.value;
            this.value = value;
            bool useAddition = this.IsMultiHandler;
            float change = value - oldVal;
            DataParameterFloat parameter = this.Parameter;
            for (int i = 0, c = this.Handlers.Count; i < c; i++) {
                ITransferableData obj = (ITransferableData) this.Handlers[i];
                float newValue = parameter.Clamp(useAddition ? (parameter.GetValue(obj) + change) : value);
                parameter.SetValue(obj, newValue);
            }

            this.OnValueChanged();
        }
    }

    public new DataParameterFloat Parameter => (DataParameterFloat) base.Parameter;

    public DragStepProfile StepProfile { get; }

    public DataParameterFloatPropertyEditorSlot(DataParameterFloat parameter, Type applicableType, string displayName, DragStepProfile stepProfile) : base(parameter, applicableType, displayName) {
        this.StepProfile = stepProfile;
    }

    public override void QueryValueFromHandlers() {
        this.value = GetEqualValue(this.Handlers, (x) => this.Parameter.GetValue((ITransferableData) x), out float d) ? d : default;
    }
}