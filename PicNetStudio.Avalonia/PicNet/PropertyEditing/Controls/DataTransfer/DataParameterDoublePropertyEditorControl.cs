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

using PicNetStudio.DataTransfer;
using PicNetStudio.PicNet.PropertyEditing.DataTransfer;

namespace PicNetStudio.Avalonia.PicNet.PropertyEditing.Controls.DataTransfer;

public class DataParameterDoublePropertyEditorControl : BaseNumberDraggerDataParamPropEditorControl {
    public new DataParameterDoublePropertyEditorSlot SlotModel => (DataParameterDoublePropertyEditorSlot) base.SlotControl.Model;

    public DataParameterDoublePropertyEditorControl() {
    }

    protected override void UpdateControlValue() {
        this.dragger.Value = this.SlotModel.Value;
    }

    protected override void UpdateModelValue() {
        this.SlotModel.Value = this.dragger.Value;
    }

    protected override void OnConnected() {
        base.OnConnected();
        DataParameterDoublePropertyEditorSlot slot = this.SlotModel;
        DataParameterDouble param = slot.Parameter;
        this.dragger.Minimum = param.Minimum;
        this.dragger.Maximum = param.Maximum;

        DragStepProfile profile = slot.StepProfile;
        this.dragger.TinyChange = profile.TinyStep;
        this.dragger.SmallChange = profile.SmallStep;
        this.dragger.NormalChange = profile.NormalStep;
        this.dragger.LargeChange = profile.LargeStep;
    }

    protected override void ResetValue() => this.SlotModel.Value = this.SlotModel.Parameter.DefaultValue;
}