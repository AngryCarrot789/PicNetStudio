using System;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using PicNetStudio.Avalonia.PicNet.Tools.Settings.DataTransfer;
using PicNetStudio.Avalonia.Utils;

namespace PicNetStudio.Avalonia.PicNet.Tools.Settings.Controls;

public abstract class BaseToolSettingControl : TemplatedControl {
    public static readonly ModelControlRegistry<BaseToolSetting, BaseToolSettingControl> Registry;
    protected bool IsUpdatingPrimaryControl;

    public BaseToolSetting? ToolSetting { get; private set; }
    
    public ToolSettingContainerControl Container { get; private set; }

    public bool IsConnected => this.ToolSetting != null;
    
    protected BaseToolSettingControl() {
    }

    public void Connect(ToolSettingContainerControl container, BaseToolSetting setting) {
        if (this.ToolSetting != null)
            throw new InvalidOperationException("Already connected");
        
        Validate.NotNull(container);
        Validate.NotNull(setting);
        
        this.ToolSetting = setting;
        this.Container = container;
        this.OnConnect();
        setting.Connected += this.OnToolSettingToolConnected;
        setting.Disconnected += this.OnToolSettingToolDisconnected;
        setting.DescriptionChanged += this.OnDescriptionChanged;
        if (setting.Tool != null) {
            this.OnToolConnected();
        }
        
        if (!string.IsNullOrWhiteSpace(setting.Description))
            ToolTip.SetTip(this, setting.Description);
    }
    
    public void Disconnect() {
        if (this.ToolSetting == null)
            throw new InvalidCastException("Already disconnected");
        
        this.ToolSetting.Connected -= this.OnToolSettingToolConnected;
        this.ToolSetting.Disconnected -= this.OnToolSettingToolDisconnected;
        this.ToolSetting.DescriptionChanged -= this.OnDescriptionChanged;
        if (this.ToolSetting.Tool != null) {
            this.OnToolDisconnected();
        }
        
        this.OnDisconnect();
        this.ToolSetting = null;
        this.Container = null;
        ToolTip.SetTip(this, null);
    }

    private void OnDescriptionChanged(BaseToolSetting sender) {
        ToolTip.SetTip(this, string.IsNullOrWhiteSpace(sender.Description) ? null : sender.Description);
    }

    private void OnToolSettingToolConnected(BaseToolSetting sender) {
        this.OnToolConnected();
    }
    
    private void OnToolSettingToolDisconnected(BaseToolSetting sender) {
        this.OnToolDisconnected();
    }

    protected virtual void OnConnect() {
    }

    protected virtual void OnDisconnect() {
    }

    /// <summary>
    /// Invoked when either we become connected and the tool setting already has a tool connected, or the setting has a new tool connected
    /// </summary>
    protected virtual void OnToolConnected() {
        
    }
    
    /// <summary>
    /// Invoked when we are about to become disconnected or when are connected and our tool setting is about to have its tool disconnected
    /// </summary>
    protected virtual void OnToolDisconnected() {
        
    }

    static BaseToolSettingControl() {
        Registry = new ModelControlRegistry<BaseToolSetting, BaseToolSettingControl>();
        Registry.RegisterType<DataParameterFloatToolSetting>(() => new DataParameterFloatToolSettingControl());
        Registry.RegisterType<DataParameterDoubleToolSetting>(() => new DataParameterDoubleToolSettingControl());
        Registry.RegisterType<AutomaticDataParameterFloatToolSetting>(() => new AutomaticDataParameterFloatToolSettingControl());
    }
}