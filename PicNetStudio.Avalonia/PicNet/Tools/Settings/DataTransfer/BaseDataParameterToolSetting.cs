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

public class DataParameterDoubleToolSetting : BaseDataParameterNumericToolSetting<double> {
    public new DataParameterDouble Parameter => (DataParameterDouble) ((BaseDataParameterToolSetting) this).Parameter;

    public DataParameterDoubleToolSetting(DataParameterDouble parameter, string displayName, DragStepProfile stepProfile) : base(parameter, displayName, stepProfile) {
    }
}