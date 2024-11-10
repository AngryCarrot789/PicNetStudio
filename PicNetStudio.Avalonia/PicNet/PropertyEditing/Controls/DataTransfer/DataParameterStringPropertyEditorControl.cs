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

using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using PicNetStudio.Avalonia.PicNet.PropertyEditing.DataTransfer;
using PicNetStudio.Avalonia.Utils;

namespace PicNetStudio.Avalonia.PicNet.PropertyEditing.Controls.DataTransfer;

public class DataParameterStringPropertyEditorControl : BaseDataParameterPropertyEditorControl {
    protected TextBox textBox;

    public new DataParameterStringPropertyEditorSlot SlotModel => (DataParameterStringPropertyEditorSlot) base.SlotControl.Model!;

    public DataParameterStringPropertyEditorControl() {
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        this.textBox = e.NameScope.GetTemplateChild<TextBox>("PART_TextBox");
        this.textBox.TextChanged += (sender, args) => this.OnControlValueChanged();
    }

    protected override void UpdateControlValue() {
        this.textBox.Text = this.SlotModel.Value;
    }

    protected override void UpdateModelValue() {
        this.SlotModel.Value = this.textBox.Text!;
    }

    protected override void OnCanEditValueChanged(bool canEdit) {
        this.textBox.IsEnabled = canEdit;
    }
}