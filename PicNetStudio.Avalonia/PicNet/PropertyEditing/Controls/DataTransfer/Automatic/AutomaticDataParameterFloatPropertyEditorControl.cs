// 
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

using PicNetStudio.DataTransfer;
using PicNetStudio.PicNet.PropertyEditing.DataTransfer.Automatic;

namespace PicNetStudio.Avalonia.PicNet.PropertyEditing.Controls.DataTransfer.Automatic;

public class AutomaticDataParameterFloatPropertyEditorControl : BaseAutomaticNumericDataParameterPropertyEditorControl<float> {
    public new AutomaticDataParameterFloatPropertyEditorSlot? SlotModel => (AutomaticDataParameterFloatPropertyEditorSlot?) base.SlotControl?.Model;

    public override double SlotValue {
        get => this.SlotModel!.Value;
        set => this.SlotModel!.Value = (float) value;
    }

    protected override void OnConnected() {
        base.OnConnected();
        AutomaticDataParameterFloatPropertyEditorSlot setting = this.SlotModel!;
        DataParameterFloat param = setting.Parameter;
        this.dragger.Minimum = param.Minimum;
        this.dragger.Maximum = param.Maximum;
    }
}