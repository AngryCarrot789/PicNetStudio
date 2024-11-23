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
using PicNetStudio.Avalonia.PicNet.Controls.Dragger;
using PicNetStudio.Avalonia.PicNet.PropertyEditing.DataTransfer;

namespace PicNetStudio.Avalonia.PicNet.Tools.Settings.DataTransfer;

public delegate void BaseDataParameterNumberDraggerToolSettingEventHandler(BaseDataParameterNumberDraggerToolSetting sender);

/// <summary>
/// Derived from <see cref="BaseDataParameterToolSetting"/> designed to be used with
/// a NumberDragger UI control, which supports a drag-step profile and a custom value formatter
/// </summary>
public abstract class BaseDataParameterNumberDraggerToolSetting : BaseDataParameterToolSetting {
    private IValueFormatter valueFormatter;

    public IValueFormatter ValueFormatter {
        get => this.valueFormatter;
        set {
            if (this.valueFormatter == value)
                return;

            this.valueFormatter = value;
            this.ValueFormatterChanged?.Invoke(this);
        }
    }

    public DragStepProfile StepProfile { get; }
    
    public event BaseDataParameterNumberDraggerToolSettingEventHandler? ValueFormatterChanged;

    protected BaseDataParameterNumberDraggerToolSetting(DataParameter parameter, string? displayName, DragStepProfile stepProfile) : base(parameter, displayName) {
        this.StepProfile = stepProfile;
    }
}