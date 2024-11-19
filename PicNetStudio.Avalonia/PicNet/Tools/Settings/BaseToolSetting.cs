using System;

namespace PicNetStudio.Avalonia.PicNet.Tools.Settings;

public delegate void BaseToolSettingEventHandler(BaseToolSetting sender);

/// <summary>
/// The base class for a setting model that is used to modify a property of a tool
/// </summary>
public class BaseToolSetting {
    public BaseCanvasTool? ActiveTool { get; private set; }

    public event BaseToolSettingEventHandler? Connected;
    public event BaseToolSettingEventHandler? Disconnected;

    public BaseToolSetting() {
    }

    /// <summary>
    /// Connects this setting to the given tool
    /// </summary>
    /// <param name="tool">The new tool</param>
    public void Connect(BaseCanvasTool tool) {
        if (this.ActiveTool != null)
            throw new InvalidOperationException("Already connected to a tool");
        
        this.ActiveTool = tool;
        this.OnConnected();
        this.Connected?.Invoke(this);
    }

    /// <summary>
    /// Disconnects our active tool from this setting
    /// </summary>
    public void Disconnect() {
        this.OnDisconnected();
        this.Disconnected?.Invoke(this);
        this.ActiveTool = null;
    }

    protected virtual void OnConnected() {
    }

    protected virtual void OnDisconnected() {
    }
}