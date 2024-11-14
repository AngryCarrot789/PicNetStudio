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
using PicNetStudio.Avalonia.PicNet.PropertyEditing.Controls.DataTransfer;
using PicNetStudio.Avalonia.Utils;
using SkiaSharp;

namespace PicNetStudio.Avalonia.PicNet.Layers.CustomParameters.BlendMode;

public class DataParameterBlendModePropertyEditorControl : BaseDataParameterPropertyEditorControl {
    protected ComboBox comboBox;

    public new DataParameterBlendModePropertyEditorSlot? SlotModel => (DataParameterBlendModePropertyEditorSlot?) base.SlotControl!.Model;

    public DataParameterBlendModePropertyEditorControl() {
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        this.comboBox = e.NameScope.GetTemplateChild<ComboBox>("PART_ComboBox");
        if (this.IsConnected)
            this.comboBox.SelectionChanged += this.OnSelectionChanged;
    }

    private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e) {
        this.OnControlValueChanged();
    }

    protected override void UpdateControlValue() {
        DataParameterBlendModePropertyEditorSlot slot = this.SlotModel!;
        if (slot.TranslationInfo != null && slot.TranslationInfo.EnumToText.TryGetValue(slot.Value, out string? text)) {
            this.comboBox.SelectedItem = text;
        }
        else {
            this.comboBox.SelectedItem = slot.Value.ToString();
        }
    }

    protected override void UpdateModelValue() {
        SKBlendMode blendMode;
        DataParameterBlendModePropertyEditorSlot slot = this.SlotModel!;
        if (slot.TranslationInfo != null) {
            if (this.comboBox.SelectedItem is string selectedText) {
                blendMode = slot.TranslationInfo.TextToEnum[selectedText];
            }
            else {
                blendMode = slot.DefaultValue;
            }
        }
        else if (this.comboBox.SelectedItem is SKBlendMode skBlendMode) {
            blendMode = skBlendMode;
        }
        else {
            blendMode = slot.DefaultValue;
        }

        slot.Value = blendMode;
    }

    protected override void OnCanEditValueChanged(bool canEdit) {
        this.comboBox.IsEnabled = canEdit;
    }

    protected override void OnConnected() {
        // Initialise list first so that UpdateControlValue has something to work on when base.OnConnected invokes it eventually 

        ItemCollection list = this.comboBox.Items;
        list.Clear();

        if (this.SlotModel!.TranslationInfo != null) {
            foreach (string value in this.SlotModel.TranslationInfo.TextList) {
                list.Add(value);
            }
        }
        else {
            foreach (SKBlendMode blend in this.SlotModel!.ValueEnumerable) {
                list.Add(blend);
            }
        }

        base.OnConnected();
        this.comboBox.SelectionChanged += this.OnSelectionChanged;
    }

    protected override void OnDisconnected() {
        base.OnDisconnected();
        this.comboBox.SelectionChanged -= this.OnSelectionChanged;
        this.comboBox.Items.Clear();
    }
}