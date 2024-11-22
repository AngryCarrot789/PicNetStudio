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
using PicNetStudio.Avalonia.PicNet.PropertyEditing.DataTransfer;

namespace PicNetStudio.Avalonia.PicNet.Tools.Settings.DataTransfer;

public delegate void DataParameterToolSettingEventHandler(BaseDataParameterToolSetting sender);

public abstract class BaseDataParameterToolSetting : BaseToolSetting {
    private string displayName;

    /// <summary>
    /// Gets the parameter
    /// </summary>
    public DataParameter Parameter { get; }

    public string DisplayName {
        get => this.displayName;
        set {
            if (this.displayName == value)
                return;

            this.displayName = value;
            this.DisplayNameChanged?.Invoke(this);
        }
    }

    public event DataParameterToolSettingEventHandler? DisplayNameChanged;
    public event DataParameterToolSettingEventHandler? ValueChanged;

    protected BaseDataParameterToolSetting(DataParameter parameter, string? displayName = null) {
        this.Parameter = parameter ?? throw new ArgumentNullException(nameof(parameter));
        this.displayName = displayName ?? parameter.Name;
    }

    protected override void OnConnected() {
        base.OnConnected();
        this.Parameter.AddValueChangedHandler(this.Tool!, this.OnValueChangedForTool);
        this.QueryValueFromHandlers();
        this.OnValueChanged();
    }

    protected override void OnDisconnected() {
        base.OnDisconnected();
        this.Parameter.RemoveValueChangedHandler(this.Tool!, this.OnValueChangedForTool);
    }

    private void OnValueChangedForTool(DataParameter parameter, ITransferableData owner) {
        this.QueryValueFromHandlers();
        this.OnValueChanged();
    }

    public abstract void QueryValueFromHandlers();

    /// <summary>
    /// Raises the value changed event, and optionally updates the <see cref="HasMultipleValues"/> (e.g. for
    /// if the value of each handler was set to a new value, it can be set to false now)
    /// </summary>
    /// <param name="hasMultipleValues">The optional new value of <see cref="HasMultipleValues"/></param>
    protected void OnValueChanged() {
        this.ValueChanged?.Invoke(this);
    }
}

public delegate void BaseDataParameterNumberDraggerToolSettingValueFormatterChangedEventHandler(BaseDataParameterNumberDraggerToolSetting sender);

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
    
    public event BaseDataParameterNumberDraggerToolSettingValueFormatterChangedEventHandler? ValueFormatterChanged;

    protected BaseDataParameterNumberDraggerToolSetting(DataParameter parameter, string? displayName, DragStepProfile stepProfile) : base(parameter, displayName) {
        this.StepProfile = stepProfile;
    }
}

public abstract class BaseDataParameterNumericToolSetting<T> : BaseDataParameterNumberDraggerToolSetting {
    private T value;

    public T Value {
        get => this.value;
        set {
            value = base.Parameter is IRangedParameter<T> ranged ? ranged.Clamp(value) : value;
            this.value = value;
            this.Parameter.SetValue(this.Tool!, value);
            this.OnValueChanged();
        }
    }

    public new DataParameter<T> Parameter => (DataParameter<T>) base.Parameter;
    
    public BaseDataParameterNumericToolSetting(DataParameter<T> parameter, string displayName, DragStepProfile stepProfile) : base(parameter, displayName, stepProfile) {
    }

    public override void QueryValueFromHandlers() {
        this.value = this.Parameter.GetValue(this.Tool!)!;
    }
}

public class DataParameterFloatToolSetting : BaseDataParameterNumericToolSetting<float> {
    public new DataParameterFloat Parameter => (DataParameterFloat) ((BaseDataParameterToolSetting) this).Parameter;

    public DataParameterFloatToolSetting(DataParameterFloat parameter, string displayName, DragStepProfile stepProfile) : base(parameter, displayName, stepProfile) {
    }
}

public class AutomaticDataParameterFloatToolSetting : BaseDataParameterNumberDraggerToolSetting {
    public new DataParameterFloat Parameter => (DataParameterFloat) ((BaseDataParameterToolSetting) this).Parameter;

    private float value;

    public float Value => this.value;

    public DataParameterBool IsAutomaticParameter { get; }
    
    public AutomaticDataParameterFloatToolSetting(DataParameterFloat parameter, DataParameterBool isAutomaticParameter, string displayName, DragStepProfile stepProfile) : base(parameter, displayName, stepProfile) {
        this.IsAutomaticParameter = isAutomaticParameter;
    }

    public void SetValue(float newValue, bool? isAutomaticMode) {
        if (this.Tool == null) {
            return;
        }
        
        if (isAutomaticMode.HasValue) {
            this.IsAutomaticParameter.SetValue(this.Tool, isAutomaticMode.Value);
            
            // we assume when the automatic state parameter changes, the value parameter is changed immediately
            if (isAutomaticMode.Value) {
                return;
            }
        }
        
        newValue = this.Parameter.Clamp(newValue);
        this.value = newValue;
        this.Parameter.SetValue(this.Tool!, newValue);
        this.OnValueChanged();
    }
    
    public override void QueryValueFromHandlers() {
        this.value = this.Parameter.GetValue(this.Tool!);
    }
}

public class DataParameterDoubleToolSetting : BaseDataParameterNumericToolSetting<double> {
    public new DataParameterDouble Parameter => (DataParameterDouble) ((BaseDataParameterToolSetting) this).Parameter;

    public DataParameterDoubleToolSetting(DataParameterDouble parameter, string displayName, DragStepProfile stepProfile) : base(parameter, displayName, stepProfile) {
    }
}