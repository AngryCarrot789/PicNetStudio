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

namespace PicNetStudio.PicNet.Tools.Settings.DataTransfer;

/// <summary>
/// A data parameter tool setting that supports an "automatic" calculation state. When the user explicitly sets
/// a value, the automatic state becomes false. Then the user can re-enable automatic by typing "auto" in the
/// number dragger or clicking the "auto" button (if there is one)
/// </summary>
public class AutomaticDataParameterFloatToolSetting : BaseDataParameterNumberDraggerToolSetting {
    public new DataParameterFloat Parameter => (DataParameterFloat) ((BaseDataParameterToolSetting) this).Parameter;
    
    public float Value { get; private set; }

    public DataParameterBool IsAutomaticParameter { get; }

    /// <summary>
    /// Gets or sets (init) whether the value of the parameter is updated when the automatic state becomes true. Default value is true
    /// </summary>
    public bool IsValueCalculatedOnAutoStateTrue { get; init; } = true;

    public AutomaticDataParameterFloatToolSetting(DataParameterFloat parameter, DataParameterBool isAutomaticParameter, string displayName, DragStepProfile stepProfile) : base(parameter, displayName, stepProfile) {
        this.IsAutomaticParameter = isAutomaticParameter;
    }

    public void SetValue(float newValue, bool? isAutomaticMode) {
        if (this.Tool == null) {
            return;
        }
        
        // Invoked with "false" when the NumberDragger's value explicitly changes by
        // the user, not model update. This means IsAutomatic becomes false, duh
        if (isAutomaticMode.HasValue) {
            this.IsAutomaticParameter.SetValue(this.Tool, isAutomaticMode.Value);
            
            // we assume when the automatic state parameter changes, the value
            // parameter is changed immediately, so don't try and set it again
            if (isAutomaticMode.Value && this.IsValueCalculatedOnAutoStateTrue) {
                return;
            }
        }
        
        newValue = this.Parameter.Clamp(newValue);
        this.Value = newValue;
        this.Parameter.SetValue(this.Tool!, newValue);
        this.OnValueChanged();
    }

    protected override void QueryValueFromHandlers() {
        this.Value = this.Parameter.GetValue(this.Tool!);
    }
}