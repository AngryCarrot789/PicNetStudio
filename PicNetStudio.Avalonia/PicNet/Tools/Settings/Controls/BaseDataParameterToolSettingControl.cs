using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using PicNetStudio.Avalonia.DataTransfer;
using PicNetStudio.Avalonia.PicNet.Controls.Dragger;
using PicNetStudio.Avalonia.PicNet.PropertyEditing.DataTransfer;
using PicNetStudio.Avalonia.PicNet.Tools.Settings.DataTransfer;
using PicNetStudio.Avalonia.Utils;

namespace PicNetStudio.Avalonia.PicNet.Tools.Settings.Controls;

public abstract class BaseDataParameterToolSettingControl : BaseToolSettingControl {
    protected TextBlock? displayNameTextBlock;

    public new BaseDataParameterToolSetting? ToolSetting => (BaseDataParameterToolSetting?) base.ToolSetting;

    protected BaseDataParameterToolSettingControl() {
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        this.displayNameTextBlock = e.NameScope.GetTemplateChild<TextBlock>("PART_Label");
    }

    protected override void OnConnect() {
        base.OnConnect();
        BaseDataParameterToolSetting setting = this.ToolSetting!;
        setting.Connected += this.OnToolConnected;
        setting.Disconnected += this.OnToolDisconnected;
        setting.DisplayNameChanged += this.OnSettingDisplayNameChanged;
        setting.ValueChanged += this.OnSettingValueChanged;
        this.UpdateDisplayName();
        this.OnModelValueChanged();
    }

    protected override void OnDisconnect() {
        base.OnDisconnect();
        BaseDataParameterToolSetting setting = this.ToolSetting!;
        setting.Connected -= this.OnToolConnected;
        setting.Disconnected -= this.OnToolDisconnected;
        setting.DisplayNameChanged -= this.OnSettingDisplayNameChanged;
        setting.ValueChanged -= this.OnSettingValueChanged;
        this.OnModelValueChanged();
    }
    
    protected abstract void UpdateControlValue();

    protected abstract void UpdateModelValue();

    protected void OnModelValueChanged() {
        if (this.ToolSetting != null) {
            this.IsUpdatingPrimaryControl = true;
            try {
                this.UpdateControlValue();
            }
            finally {
                this.IsUpdatingPrimaryControl = false;
            }
        }
    }

    protected void OnControlValueChanged() {
        if (!this.IsUpdatingPrimaryControl && this.ToolSetting != null) {
            this.UpdateModelValue();
        }
    }
    
    private void OnSettingValueChanged(BaseDataParameterToolSetting sender) => this.OnModelValueChanged();
    
    private void OnSettingDisplayNameChanged(BaseDataParameterToolSetting sender) => this.UpdateDisplayName();

    private void OnToolConnected(BaseToolSetting sender) => this.OnModelValueChanged();
    
    private void OnToolDisconnected(BaseToolSetting sender) => this.OnModelValueChanged();
    
    private void UpdateDisplayName() {
        if (!this.IsConnected || this.displayNameTextBlock == null)
            return;

        string text = this.ToolSetting!.DisplayName;
        if (string.IsNullOrWhiteSpace(text)) {
            this.displayNameTextBlock.IsVisible = false;
        }
        
        this.displayNameTextBlock.Text = text;
    }
}

public abstract class DataParameterNumberDraggerToolSettingControl : BaseDataParameterToolSettingControl {
    protected NumberDragger PART_Dragger;

    public new BaseDataParameterNumberDraggerToolSetting? ToolSetting => (BaseDataParameterNumberDraggerToolSetting?) base.ToolSetting;
    
    protected abstract double SettingValue { get; set; }

    public DataParameterNumberDraggerToolSettingControl() {
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        this.PART_Dragger = e.NameScope.GetTemplateChild<NumberDragger>(nameof(this.PART_Dragger));
        this.PART_Dragger.ValueChanged += (sender, args) => this.OnControlValueChanged();
    }

    protected override void UpdateControlValue() {
        this.PART_Dragger.Value = this.SettingValue;
    }

    protected override void UpdateModelValue() {
        this.SettingValue = (float) this.PART_Dragger.Value;
    }

    protected override void OnConnect() {
        base.OnConnect();
        DragStepProfile profile = this.ToolSetting!.StepProfile;
        this.PART_Dragger.TinyChange = profile.TinyStep;
        this.PART_Dragger.SmallChange = profile.SmallStep;
        this.PART_Dragger.NormalChange = profile.NormalStep;
        this.PART_Dragger.LargeChange = profile.LargeStep;
    }

    protected abstract void ResetValue();
}

public class DataParameterFloatToolSettingControl : DataParameterNumberDraggerToolSettingControl {
    public new DataParameterFloatToolSetting? ToolSetting => (DataParameterFloatToolSetting?) base.ToolSetting;

    protected override double SettingValue {
        get => this.ToolSetting!.Value;
        set => this.ToolSetting!.Value = (float) value;
    }

    protected override void OnConnect() {
        base.OnConnect();
        DataParameterFloatToolSetting setting = this.ToolSetting!;
        DataParameterFloat param = setting.Parameter;
        this.PART_Dragger.Minimum = param.Minimum;
        this.PART_Dragger.Maximum = param.Maximum;
    }

    protected override void ResetValue() {
        this.ToolSetting!.Value = this.ToolSetting.Parameter.DefaultValue;
    }
}

public class DataParameterDoubleToolSettingControl : DataParameterNumberDraggerToolSettingControl {
    public new DataParameterDoubleToolSetting? ToolSetting => (DataParameterDoubleToolSetting?) base.ToolSetting;

    protected override double SettingValue {
        get => this.ToolSetting!.Value;
        set => this.ToolSetting!.Value = value;
    }

    protected override void OnConnect() {
        base.OnConnect();
        DataParameterDoubleToolSetting setting = this.ToolSetting!;
        DataParameterDouble param = setting.Parameter;
        this.PART_Dragger.Minimum = param.Minimum;
        this.PART_Dragger.Maximum = param.Maximum;
    }

    protected override void ResetValue() {
        this.ToolSetting!.Value = this.ToolSetting.Parameter.DefaultValue;
    }
}