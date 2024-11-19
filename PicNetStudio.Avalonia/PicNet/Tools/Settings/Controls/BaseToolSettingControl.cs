using Avalonia.Controls.Primitives;

namespace PicNetStudio.Avalonia.PicNet.Tools.Settings.Controls;

public abstract class BaseToolSettingControl : TemplatedControl {
    public static readonly ModelControlRegistry<BaseToolSetting, BaseToolSettingControl> Registry;
    protected bool IsUpdatingPrimaryControl;

    public BaseToolSetting? ToolSetting { get; private set; }

    public ToolSettingContainerControl Container { get; private set; }

    protected BaseToolSettingControl() {
    }

    public void Connect(ToolSettingContainerControl container, BaseToolSetting setting) {
        this.ToolSetting = setting;
        this.Container = container;
        this.OnConnect();
    }

    public void Disconnect() {
        this.OnDisconnect();
        this.ToolSetting = null;
        this.Container = null;
    }

    protected virtual void OnConnect() {
    }

    protected virtual void OnDisconnect() {
    }

    static BaseToolSettingControl() {
        Registry = new ModelControlRegistry<BaseToolSetting, BaseToolSettingControl>();
        Registry.RegisterType<DataParameterFloatToolSetting>(() => new DataParameterFloatToolSettingControl());
    }
}