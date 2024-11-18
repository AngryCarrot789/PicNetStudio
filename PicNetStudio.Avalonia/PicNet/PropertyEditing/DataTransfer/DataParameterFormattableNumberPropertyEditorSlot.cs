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
using PicNetStudio.Avalonia.PicNet.Controls.Dragger;

namespace PicNetStudio.Avalonia.PicNet.PropertyEditing.DataTransfer;

public delegate void SlotValueFormatterChangedEventHandler(DataParameterFormattableNumberPropertyEditorSlot sender, IValueFormatter oldValueFormatter, IValueFormatter newValueFormatter);

public delegate void SlotValueFormatterForAdditionChangedEventHandler(DataParameterFormattableNumberPropertyEditorSlot sender, IValueFormatter oldValueFormatterForAddition, IValueFormatter newValueFormatterForAddition);

public abstract class DataParameterFormattableNumberPropertyEditorSlot : DataParameterPropertyEditorSlot {
    private IValueFormatter valueFormatter;
    private IValueFormatter valueFormatterForAddition;

    /// <summary>
    /// Gets or sets the value formatter used to format our numeric value in the UI
    /// </summary>
    public IValueFormatter ValueFormatter {
        get => this.valueFormatter;
        set {
            IValueFormatter oldValueFormatter = this.valueFormatter;
            if (oldValueFormatter == value)
                return;

            this.valueFormatter = value;
            this.ValueFormatterChanged?.Invoke(this, oldValueFormatter, value);
        }
    }

    public event SlotValueFormatterChangedEventHandler? ValueFormatterChanged;
    public event SlotValueFormatterForAdditionChangedEventHandler? ValueFormatterForAdditionChanged;

    protected DataParameterFormattableNumberPropertyEditorSlot(DataParameter parameter, Type applicableType, string? displayName = null) : base(parameter, applicableType, displayName) {
    }
}