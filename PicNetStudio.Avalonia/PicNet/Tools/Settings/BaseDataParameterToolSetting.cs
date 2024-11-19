using System;
using System.Diagnostics;
using PicNetStudio.Avalonia.DataTransfer;
using PicNetStudio.Avalonia.PicNet.Controls.Dragger;
using PicNetStudio.Avalonia.PicNet.PropertyEditing.DataTransfer;

namespace PicNetStudio.Avalonia.PicNet.Tools.Settings;

public delegate void DataParameterToolSettingEventHandler(BaseDataParameterToolSetting sender);

public abstract class BaseDataParameterToolSetting : BaseToolSetting {
    private string displayName;

    protected ITransferableData? SingleHandler => this.ActiveTool;

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

    /// <summary>
    /// Gets or sets if the parameter's value is inverted between the parameter and checkbox in the UI.
    /// This should only be set during the construction phase of the object and not during its lifetime
    /// </summary>
    public bool InvertIsEditableForParameter { get; set; }

    public event DataParameterToolSettingEventHandler? DisplayNameChanged;
    public event DataParameterToolSettingEventHandler? ValueChanged;

    protected BaseDataParameterToolSetting(DataParameter parameter, string? displayName = null) {
        this.Parameter = parameter ?? throw new ArgumentNullException(nameof(parameter));
        this.displayName = displayName ?? parameter.Name;
    }

    protected override void OnConnected() {
        base.OnConnected();
        this.Parameter.AddValueChangedHandler(this.SingleHandler!, this.OnValueChangedForTool);
        this.QueryValueFromHandlers();
        this.OnValueChanged();
    }

    protected override void OnDisconnected() {
        base.OnDisconnected();
        this.Parameter.RemoveValueChangedHandler(this.SingleHandler, this.OnValueChangedForTool);
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

    public event BaseDataParameterNumberDraggerToolSettingValueFormatterChangedEventHandler? ValueFormatterChanged;

    protected BaseDataParameterNumberDraggerToolSetting(DataParameter parameter, string? displayName = null) : base(parameter, displayName) {
    }
}

public class DataParameterFloatToolSetting : BaseDataParameterNumberDraggerToolSetting {
    private float value;

    public float Value {
        get => this.value;
        set {
            this.value = value;
            DataParameterFloat parameter = this.Parameter;
            float newValue = parameter.Clamp(value);
            parameter.SetValue(this.ActiveTool!, newValue);
            Debug.WriteLine(newValue);
            this.OnValueChanged();
        }
    }

    public new DataParameterFloat Parameter => (DataParameterFloat) base.Parameter;

    public DragStepProfile StepProfile { get; }

    public DataParameterFloatToolSetting(DataParameterFloat parameter, string displayName, DragStepProfile stepProfile) : base(parameter, displayName) {
        this.StepProfile = stepProfile;
    }

    public override void QueryValueFromHandlers() {
        this.value = this.Parameter.GetValue(this.ActiveTool!);
    }
}