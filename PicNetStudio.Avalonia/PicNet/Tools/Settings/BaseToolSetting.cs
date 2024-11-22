using System;

namespace PicNetStudio.Avalonia.PicNet.Tools.Settings;

public delegate void BaseToolSettingEventHandler(BaseToolSetting sender);

/// <summary>
/// The base class for a setting model that is used to modify a property of a tool.
/// The tool setting classes are basically a replica of the <see cref="PropertyEditing.PropertyEditorSlot"/>
/// classes, except they only accept a single handler being the <see cref="Tool"/>
/// </summary>
public class BaseToolSetting {
    private string? description;
    
    public BaseCanvasTool? Tool { get; private set; }

    public string? Description {
        get => this.description;
        set {
            if (this.description == value)
                return;

            this.description = value;
            this.DescriptionChanged?.Invoke(this);
        }
    }
    
    /// <summary>
    /// Fired when this setting is connected to a tool
    /// </summary>
    public event BaseToolSettingEventHandler? Connected;
    
    /// <summary>
    /// Fired when this setting is disconnected from a tool
    /// </summary>
    public event BaseToolSettingEventHandler? Disconnected;

    public event BaseToolSettingEventHandler? DescriptionChanged;

    public BaseToolSetting() {
    }

    /// <summary>
    /// Connects this setting to the given tool
    /// </summary>
    /// <param name="tool">The new tool</param>
    public void Connect(BaseCanvasTool tool) {
        if (this.Tool != null)
            throw new InvalidOperationException("Already connected to a tool");
        
        this.Tool = tool;
        this.OnConnected();
        this.Connected?.Invoke(this);
    }

    /// <summary>
    /// Disconnects our tool from this setting
    /// </summary>
    public void Disconnect() {
        this.OnDisconnected();
        this.Disconnected?.Invoke(this);
        this.Tool = null;
    }

    protected virtual void OnConnected() {
    }

    protected virtual void OnDisconnected() {
    }
}