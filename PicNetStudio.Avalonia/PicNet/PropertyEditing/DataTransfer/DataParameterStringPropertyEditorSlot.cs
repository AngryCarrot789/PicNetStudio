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
using PicNetStudio.Avalonia.Utils;

namespace PicNetStudio.Avalonia.PicNet.PropertyEditing.DataTransfer;

public delegate void SlotAnticipatedLineCountChangedEventHandler(DataParameterStringPropertyEditorSlot sender);

public class DataParameterStringPropertyEditorSlot : DataParameterPropertyEditorSlot {
    private string? value;
    private int anticipatedLineCount = -1;

    public string? Value {
        get => this.value;
        set {
            this.value = value;
            DataParameterString parameter = this.Parameter;
            for (int i = 0, c = this.Handlers.Count; i < c; i++) {
                parameter.SetValue((ITransferableData) this.Handlers[i], value);
            }

            this.OnValueChanged(false, true);
        }
    }

    /// <summary>
    /// Gets or sets the number of lines that will probably be taken up by this property. Default is -1, which means ignored. Value must be -1 or greater than 0
    /// </summary>
    public int AnticipatedLineCount {
        get => this.anticipatedLineCount;
        set {
            if (value < 0 && value != -1)
                throw new ArgumentOutOfRangeException(nameof(value), "Value must be -1 or greater than zero");
            
            if (this.anticipatedLineCount == value)
                return;

            this.anticipatedLineCount = value;
            this.AnticipatedLineCountChanged?.Invoke(this);
        }
    }

    public event SlotAnticipatedLineCountChangedEventHandler? AnticipatedLineCountChanged;

    public new DataParameterString Parameter => (DataParameterString) base.Parameter;

    public DataParameterStringPropertyEditorSlot(DataParameterString parameter, Type applicableType, string displayName) : base(parameter, applicableType, displayName) {
    }

    public override void QueryValueFromHandlers() {
        this.HasMultipleValues = !CollectionUtils.GetEqualValue(this.Handlers, (x) => this.Parameter.GetValue((ITransferableData) x), out this.value);
    }
}