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
using PicNetStudio.Avalonia.PicNet.PropertyEditing.DataTransfer;

namespace PicNetStudio.Avalonia.PicNet.PropertyEditing.Controls.DataTransfer;

public class DataParameterLongPropertyEditorControl : BaseSliderDataParamPropEditorControl {
    public new DataParameterLongPropertyEditorSlot SlotModel => (DataParameterLongPropertyEditorSlot) base.SlotControl.Model;

    public DataParameterLongPropertyEditorControl() {
    }

    protected override void UpdateControlValue() {
        this.slider.Value = this.SlotModel.Value;
    }

    protected override void UpdateModelValue() {
        this.SlotModel.Value = (long) Math.Round(this.slider.Value);
    }

    protected override void OnConnected() {
        base.OnConnected();
        DataParameterLongPropertyEditorSlot slot = this.SlotModel;
        DataParameterLong param = slot.Parameter;
        this.slider.Minimum = param.Minimum;
        this.slider.Maximum = param.Maximum;

        // DragStepProfile profile = slot.StepProfile;
        // this.dragger.TinyChange = Math.Max(profile.TinyStep, 1.0);
        // this.dragger.SmallChange = Math.Max(profile.SmallStep, 1.0);
        // this.dragger.LargeChange = Math.Max(profile.NormalStep, 1.0);
        // this.dragger.MassiveChange = Math.Max(profile.LargeStep, 1.0);

        DragStepProfile profile = slot.StepProfile;
        this.slider.SmallChange = Math.Max(profile.SmallStep, 1.0);
        this.slider.LargeChange = Math.Max(profile.NormalStep, 1.0);
    }
}