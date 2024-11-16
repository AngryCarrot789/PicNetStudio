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

using Avalonia.Controls.Primitives;
using PicNetStudio.Avalonia.Bindings;
using PicNetStudio.Avalonia.PicNet.Controls.Dragger;
using PicNetStudio.Avalonia.PicNet.PropertyEditing.DataTransfer;
using PicNetStudio.Avalonia.Utils;

namespace PicNetStudio.Avalonia.PicNet.PropertyEditing.Controls.DataTransfer;

public abstract class BaseNumberDraggerDataParamPropEditorControl : BaseDataParameterPropertyEditorControl {
    public new DataParameterFormattableNumberPropertyEditorSlot? SlotModel => (DataParameterFormattableNumberPropertyEditorSlot?) base.SlotControl?.Model;
    
    protected NumberDragger dragger;
    private readonly AutoUpdateAndEventPropertyBinder<DataParameterFormattableNumberPropertyEditorSlot> valueFormatterBinder;

    protected BaseNumberDraggerDataParamPropEditorControl() {
        this.valueFormatterBinder = new AutoUpdateAndEventPropertyBinder<DataParameterFormattableNumberPropertyEditorSlot>(NumberDragger.ValueFormatterProperty, nameof(DataParameterFormattableNumberPropertyEditorSlot.ValueFormatterChanged), (x) => ((NumberDragger) x.Control).ValueFormatter = x.Model.ValueFormatter, null);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        this.dragger = e.NameScope.GetTemplateChild<NumberDragger>("PART_Dragger");
        this.dragger.ValueChanged += (sender, args) => this.OnControlValueChanged();
        this.valueFormatterBinder.AttachControl(this.dragger);
    }

    protected override void OnCanEditValueChanged(bool canEdit) {
        this.dragger.IsEnabled = canEdit;
    }

    protected override void OnConnected() {
        this.valueFormatterBinder.AttachModel(this.SlotModel!);
        base.OnConnected();
    }

    protected override void OnDisconnected() {
        this.valueFormatterBinder.DetachModel();
        base.OnDisconnected();
    }
}