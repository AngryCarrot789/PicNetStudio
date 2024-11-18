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
using SkiaSharp;

namespace PicNetStudio.Avalonia.PicNet.PropertyEditing.DataTransfer;

public class DataParameterPointPropertyEditorSlot : DataParameterFormattableNumberPropertyEditorSlot {
    private SKPoint value;

    public SKPoint Value {
        get => this.value;
        set {
            SKPoint oldVal = this.value;
            this.value = value;
            bool useAddition = false;//this.IsMultiHandler; TODO: Fix with new NumberDragger
            SKPoint change = value - oldVal;
            DataParameterPoint parameter = this.Parameter;
            for (int i = 0, c = this.Handlers.Count; i < c; i++) {
                ITransferableData obj = (ITransferableData) this.Handlers[i];
                SKPoint newValue = parameter.Clamp(useAddition ? (parameter.GetValue(obj) + change) : value);
                parameter.SetValue(obj, newValue);
            }

            this.OnValueChanged(this.lastQueryHasMultipleValues && useAddition, true);
        }
    }

    public new DataParameterPoint Parameter => (DataParameterPoint) base.Parameter;

    public DragStepProfile StepProfileX { get; set; }
    public DragStepProfile StepProfileY { get; set; }

    public DataParameterPointPropertyEditorSlot(DataParameterPoint parameter, Type applicableType, string displayName) : base(parameter, applicableType, displayName) {
    }

    public override void QueryValueFromHandlers() {
        this.HasMultipleValues = !GetEqualValue(this.Handlers, (x) => this.Parameter.GetValue((ITransferableData) x), out this.value);
        if (this.HasMultipleValues) {
            SKPoint range = (this.Parameter.Maximum - this.Parameter.Minimum);
            this.value = new SKPoint(Math.Abs(range.X) / 2.0F, Math.Abs(range.Y) / 2.0F);
        }
    }
}