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
/// Derived from <see cref="BaseDataParameterNumberDraggerToolSetting"/>, this class implements regular value
/// querying and get/set behaviour. This class might be overridden with the true underlying data parameter data types
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class BaseDataParameterNumericToolSetting<T> : BaseDataParameterNumberDraggerToolSetting {
    private T value;

    /// <summary>
    /// Gets or sets the value of this tool setting. Setting this property will set the value of our
    /// <see cref="Parameter"/> for our <see cref="BaseToolSetting.Tool"/> (if there is one connected)
    /// </summary>
    public T Value {
        get => this.value;
        set {
            value = base.Parameter is IRangedParameter<T> ranged ? ranged.Clamp(value) : value;
            this.value = value;
            if (base.Tool != null)
                this.Parameter.SetValue(this.Tool!, value);
            this.OnValueChanged();
        }
    }

    public new DataParameter<T> Parameter => (DataParameter<T>) base.Parameter;
    
    public BaseDataParameterNumericToolSetting(DataParameter<T> parameter, string displayName, DragStepProfile stepProfile) : base(parameter, displayName, stepProfile) {
    }

    protected override void QueryValueFromHandlers() {
        this.value = this.Parameter.GetValue(this.Tool!)!;
    }
}

public class DataParameterFloatToolSetting : BaseDataParameterNumericToolSetting<float> {
    public new DataParameterFloat Parameter => (DataParameterFloat) ((BaseDataParameterToolSetting) this).Parameter;

    public DataParameterFloatToolSetting(DataParameterFloat parameter, string displayName, DragStepProfile stepProfile) : base(parameter, displayName, stepProfile) {
    }
}

public class DataParameterDoubleToolSetting : BaseDataParameterNumericToolSetting<double> {
    public new DataParameterDouble Parameter => (DataParameterDouble) ((BaseDataParameterToolSetting) this).Parameter;

    public DataParameterDoubleToolSetting(DataParameterDouble parameter, string displayName, DragStepProfile stepProfile) : base(parameter, displayName, stepProfile) {
    }
}

public class DataParameterLongToolSetting : BaseDataParameterNumericToolSetting<long> {
    public new DataParameterLong Parameter => (DataParameterLong) ((BaseDataParameterToolSetting) this).Parameter;

    public DataParameterLongToolSetting(DataParameterLong parameter, string displayName, DragStepProfile stepProfile) : base(parameter, displayName, stepProfile) {
    }
}