using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using PicNetStudio.Avalonia.DataTransfer;
using PicNetStudio.Avalonia.PicNet.Controls.Dragger;
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
        BaseDataParameterToolSetting tool = this.ToolSetting!;
        tool.Connected += this.OnToolConnected;
        tool.Disconnected += this.OnToolDisconnected;
        tool.DisplayNameChanged += this.OnSlotDisplayNameChanged;
        tool.ValueChanged += this.OnSlotValueChanged;
        tool.ValueChanged += this.OnSlotValueChanged;
        this.displayNameTextBlock!.Text = tool.DisplayName;
        this.OnModelValueChanged();
    }

    protected override void OnDisconnect() {
        base.OnDisconnect();
        BaseDataParameterToolSetting tool = this.ToolSetting!;
        tool.Connected -= this.OnToolConnected;
        tool.Disconnected -= this.OnToolDisconnected;
        tool.DisplayNameChanged -= this.OnSlotDisplayNameChanged;
        tool.ValueChanged -= this.OnSlotValueChanged;
        tool.ValueChanged -= this.OnSlotValueChanged;
        this.OnModelValueChanged();
    }

    private void OnSlotValueChanged(BaseDataParameterToolSetting sender) {
        this.OnModelValueChanged();
    }

    private void OnSlotDisplayNameChanged(BaseDataParameterToolSetting sender) {
        if (this.displayNameTextBlock != null)
            this.displayNameTextBlock.Text = sender.DisplayName;
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
    
    private void OnToolConnected(BaseToolSetting sender) => this.OnModelValueChanged();
    private void OnToolDisconnected(BaseToolSetting sender) => this.OnModelValueChanged();
}

public class DataParameterFloatToolSettingControl : BaseDataParameterToolSettingControl {
    private NumberDragger PART_Dragger;

    public new DataParameterFloatToolSetting? ToolSetting => (DataParameterFloatToolSetting?) base.ToolSetting;

    public DataParameterFloatToolSettingControl() {
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        this.PART_Dragger = e.NameScope.GetTemplateChild<NumberDragger>(nameof(this.PART_Dragger));
        this.PART_Dragger.ValueChanged += (sender, args) => this.OnControlValueChanged();
    }

    protected override void UpdateControlValue() {
        this.PART_Dragger.Value = this.ToolSetting!.Value;
    }

    protected override void UpdateModelValue() {
        this.ToolSetting!.Value = (float) this.PART_Dragger.Value;
    }

    protected override void OnConnect() {
        base.OnConnect();
        DataParameterFloatToolSetting slot = this.ToolSetting!;
        DataParameterFloat param = (DataParameterFloat) slot.Parameter;
        this.PART_Dragger.Minimum = param.Minimum;
        this.PART_Dragger.Maximum = param.Maximum;
    }

    protected void ResetValue() => this.ToolSetting!.Value = ((DataParameterFloat) this.ToolSetting.Parameter).DefaultValue;
}