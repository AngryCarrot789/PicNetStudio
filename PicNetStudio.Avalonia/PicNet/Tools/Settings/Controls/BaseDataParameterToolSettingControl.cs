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

using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using PicNetStudio.Avalonia.Bindings;
using PicNetStudio.Avalonia.PicNet.Controls.Dragger;
using PicNetStudio.Avalonia.Utils;
using PicNetStudio.DataTransfer;
using PicNetStudio.PicNet.PropertyEditing.DataTransfer;
using PicNetStudio.PicNet.Tools;
using PicNetStudio.PicNet.Tools.Settings;
using PicNetStudio.PicNet.Tools.Settings.DataTransfer;
using PicNetStudio.Utils;

namespace PicNetStudio.Avalonia.PicNet.Tools.Settings.Controls;

public abstract class BaseDataParameterToolSettingControl : BaseToolSettingControl {
    protected TextBlock? displayNameTextBlock;

    public new BaseDataParameterToolSetting? ToolSetting => (BaseDataParameterToolSetting?) base.ToolSetting;

    protected BaseDataParameterToolSettingControl() {
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        this.displayNameTextBlock = e.NameScope.Find("PART_Label") as TextBlock;
    }

    protected override void OnConnect() {
        base.OnConnect();
        BaseDataParameterToolSetting setting = this.ToolSetting!;
        setting.DisplayNameChanged += this.OnSettingDisplayNameChanged;
        setting.ValueChanged += this.OnSettingValueChanged;
        this.UpdateDisplayName();
        this.OnModelValueChanged();
    }

    protected override void OnDisconnect() {
        base.OnDisconnect();
        BaseDataParameterToolSetting setting = this.ToolSetting!;
        setting.DisplayNameChanged -= this.OnSettingDisplayNameChanged;
        setting.ValueChanged -= this.OnSettingValueChanged;
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
    
    protected override void OnToolConnected() {
        base.OnToolConnected();
        this.OnModelValueChanged();
    }
    
    protected override void OnToolDisconnected() {
        base.OnToolDisconnected();
        this.OnModelValueChanged();
    }
    
    private void OnSettingValueChanged(BaseDataParameterToolSetting sender) => this.OnModelValueChanged();
    
    private void OnSettingDisplayNameChanged(BaseToolSetting tool) => this.UpdateDisplayName();

    private void UpdateDisplayName() {
        if (!this.IsConnected) {
            this.UpdateDisplayNameControl(null, false);
            return;
        }

        string text = this.ToolSetting!.DisplayName;
        this.UpdateDisplayNameControl(string.IsNullOrWhiteSpace(text) ? null : text, true);
    }

    protected virtual void UpdateDisplayNameControl(string? text, bool isVisible) {
        if (this.displayNameTextBlock != null) {
            this.displayNameTextBlock.IsVisible = isVisible;
            this.displayNameTextBlock.Text = text;
        }
    }
}

public abstract class DataParameterNumberDraggerToolSettingControl : BaseDataParameterToolSettingControl {
    protected NumberDragger PART_Dragger;
    private readonly AutoUpdateAndEventPropertyBinder<BaseDataParameterNumberDraggerToolSetting> valueFormatterBinder;

    public new BaseDataParameterNumberDraggerToolSetting? ToolSetting => (BaseDataParameterNumberDraggerToolSetting?) base.ToolSetting;
    
    protected abstract double SettingValue { get; set; }

    public DataParameterNumberDraggerToolSettingControl() {
        this.valueFormatterBinder = new AutoUpdateAndEventPropertyBinder<BaseDataParameterNumberDraggerToolSetting>(null, nameof(BaseDataParameterNumberDraggerToolSetting.ValueFormatterChanged), (x) => {
            DataParameterNumberDraggerToolSettingControl editor = (DataParameterNumberDraggerToolSettingControl) x.Control;
            editor.PART_Dragger.ValueFormatter = x.Model.ValueFormatter;
            editor.PART_Dragger.ValueFormatter = x.Model.ValueFormatter;
        }, null);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        this.PART_Dragger = e.NameScope.GetTemplateChild<NumberDragger>(nameof(this.PART_Dragger));
        this.PART_Dragger.ValueChanged += (sender, args) => this.OnControlValueChanged();
        this.valueFormatterBinder.AttachControl(this);
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
        this.valueFormatterBinder.AttachModel(this.ToolSetting);
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
public class AutomaticDataParameterFloatToolSettingControl : DataParameterNumberDraggerToolSettingControl {
    public new AutomaticDataParameterFloatToolSetting? ToolSetting => (AutomaticDataParameterFloatToolSetting?) base.ToolSetting;

    protected override double SettingValue {
        get => this.ToolSetting!.Value;
        set => this.ToolSetting!.SetValue((float) value, false);
    }

    public AutomaticDataParameterFloatToolSettingControl() {
    }

    protected override void OnConnect() {
        base.OnConnect();
        AutomaticDataParameterFloatToolSetting setting = this.ToolSetting!;
        DataParameterFloat param = setting.Parameter;
        this.PART_Dragger.Minimum = param.Minimum;
        this.PART_Dragger.Maximum = param.Maximum;
        
        this.PART_Dragger.InvalidInputEntered += this.PartDraggerOnInvalidInputEntered;
    }

    protected override void OnDisconnect() {
        base.OnDisconnect();
        this.PART_Dragger.InvalidInputEntered -= this.PartDraggerOnInvalidInputEntered;
    }

    private void PartDraggerOnInvalidInputEntered(object? sender, InvalidInputEnteredEventArgs e) {
        BaseCanvasTool? tool = this.ToolSetting!.Tool;
        if (tool != null && ("auto".EqualsIgnoreCase(e.Input) || "automatic".EqualsIgnoreCase(e.Input) || "\"auto\"".EqualsIgnoreCase(e.Input))) {
            this.ToolSetting!.IsAutomaticParameter.SetValue(tool, true);
        }
    }

    protected override void OnToolConnected() {
        base.OnToolConnected();
        this.ToolSetting!.IsAutomaticParameter.AddValueChangedHandler(this.ToolSetting!.Tool!, this.OnIsAutomaticChanged);
    }

    protected override void OnToolDisconnected() {
        base.OnToolDisconnected();
        this.ToolSetting!.IsAutomaticParameter.RemoveValueChangedHandler(this.ToolSetting!.Tool!, this.OnIsAutomaticChanged);
    }
    
    private void OnIsAutomaticChanged(DataParameter parameter, ITransferableData owner) {
        this.UpdateTextPreview();
    }

    protected override void UpdateControlValue() {
        base.UpdateControlValue();
        this.UpdateTextPreview();
    }

    private void UpdateTextPreview() {
        if (this.ToolSetting!.IsAutomaticParameter.GetValue(this.ToolSetting!.Tool!)) {
            this.PART_Dragger.FinalPreviewStringFormat = "{0} (Auto)";
        }
        else {
            this.PART_Dragger.FinalPreviewStringFormat = null;
        }
    }

    protected override void ResetValue() {
        if (!this.IsConnected)
            return;

        BaseCanvasTool? tool = this.ToolSetting!.Tool;
        if (tool != null) {
            this.ToolSetting!.IsAutomaticParameter.SetValue(tool, true);
        }
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

public class DataParameterLongToolSettingControl : DataParameterNumberDraggerToolSettingControl {
    public new DataParameterLongToolSetting? ToolSetting => (DataParameterLongToolSetting?) base.ToolSetting;

    protected override double SettingValue {
        get => this.ToolSetting!.Value;
        set => this.ToolSetting!.Value = (long) value;
    }

    protected override void OnConnect() {
        base.OnConnect();
        DataParameterLongToolSetting setting = this.ToolSetting!;
        DataParameterLong param = setting.Parameter;
        this.PART_Dragger.Minimum = param.Minimum;
        this.PART_Dragger.Maximum = param.Maximum;
        this.PART_Dragger.IsIntegerValue = true;
    }

    protected override void ResetValue() {
        this.ToolSetting!.Value = this.ToolSetting.Parameter.DefaultValue;
    }
}

public class DataParameterBoolToolSettingControl : BaseDataParameterToolSettingControl {
    private ToggleButton? PART_CheckBox;
    
    public new DataParameterBoolToolSetting? ToolSetting => (DataParameterBoolToolSetting?) base.ToolSetting;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        this.PART_CheckBox = e.NameScope.GetTemplateChild<ToggleButton>(nameof(this.PART_CheckBox));
        this.PART_CheckBox.IsCheckedChanged += (sender, args) => this.OnControlValueChanged();
    }

    protected override void UpdateControlValue() {
        this.PART_CheckBox!.IsChecked = this.ToolSetting!.Value;
    }

    protected override void UpdateModelValue() {
        this.ToolSetting!.Value = this.PART_CheckBox!.IsChecked == true;
    }

    protected override void UpdateDisplayNameControl(string? text, bool isVisible) {
        if (this.PART_CheckBox != null) {
            this.PART_CheckBox.IsVisible = isVisible;
            this.PART_CheckBox.Content = text;
        }
    }
}