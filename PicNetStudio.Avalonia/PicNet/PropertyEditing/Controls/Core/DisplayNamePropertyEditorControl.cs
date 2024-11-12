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
using PicNetStudio.Avalonia.Bindings;
using PicNetStudio.Avalonia.PicNet.PropertyEditing.Core;
using PicNetStudio.Avalonia.Utils;

namespace PicNetStudio.Avalonia.PicNet.PropertyEditing.Controls.Core;

public class DisplayNamePropertyEditorControl : BasePropEditControlContent {
    public new DisplayNamePropertyEditorSlot? SlotModel => (DisplayNamePropertyEditorSlot?) base.SlotControl.Model;

    private TextBox displayNameBox;

    private readonly GetSetAutoUpdateAndEventPropertyBinder<DisplayNamePropertyEditorSlot> displayNameBinder = new GetSetAutoUpdateAndEventPropertyBinder<DisplayNamePropertyEditorSlot>(TextBox.TextProperty, nameof(DisplayNamePropertyEditorSlot.DisplayNameChanged), binder => binder.Model.DisplayName, (binder, v) => binder.Model.SetValue((string) v));

    public DisplayNamePropertyEditorControl() {
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        this.displayNameBox = e.NameScope.GetTemplateChild<TextBox>("PART_TextBox");
    }

    protected override void OnConnected() {
        this.displayNameBinder.Attach(this.displayNameBox, this.SlotModel!);
    }

    protected override void OnDisconnected() {
        this.displayNameBinder.Detach();
    }
}