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

using PicNetStudio.Avalonia.DataTransfer;
using PicNetStudio.Avalonia.PicNet.PropertyEditing.DataTransfer;

namespace PicNetStudio.Avalonia.PicNet.PropertyEditing.Controls.DataTransfer;

public class DataParameterFloatPropertyEditorControl : BaseNumberDraggerDataParamPropEditorControl {
    public new DataParameterFloatPropertyEditorSlot SlotModel => (DataParameterFloatPropertyEditorSlot) base.SlotControl.Model;

    public DataParameterFloatPropertyEditorControl() {
    }

    protected override void UpdateControlValue() {
        this.dragger.Value = this.SlotModel.Value;
    }

    protected override void UpdateModelValue() {
        this.SlotModel.Value = (float) this.dragger.Value;
    }

    protected override void OnConnected() {
        base.OnConnected();
        DataParameterFloatPropertyEditorSlot slot = this.SlotModel;
        DataParameterFloat param = slot.Parameter;
        this.dragger.Minimum = param.Minimum;
        this.dragger.Maximum = param.Maximum;
        
        DragStepProfile profile = slot.StepProfile;
        this.dragger.TinyChange = profile.TinyStep;
        this.dragger.SmallChange = profile.SmallStep;
        this.dragger.NormalChange = profile.NormalStep;
        this.dragger.LargeChange = profile.LargeStep;
    }
}