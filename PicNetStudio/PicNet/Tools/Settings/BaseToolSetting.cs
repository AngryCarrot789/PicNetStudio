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

namespace PicNetStudio.PicNet.Tools.Settings;

public delegate void BaseToolSettingEventHandler(BaseToolSetting sender);

/// <summary>
/// The base class for a setting model that is used to modify a property of a tool.
/// The tool setting classes are basically a replica of the <see cref="PropertyEditing.PropertyEditorSlot"/>
/// classes, except they only accept a single handler being the <see cref="Tool"/>
/// </summary>
public class BaseToolSetting {
    private string displayName;
    private string? description;
    
    /// <summary>
    /// Gets the tool that is currently connected to this setting
    /// </summary>
    public BaseCanvasTool? Tool { get; private set; }

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
    /// Gets or sets a simple text description of what this tool does and what it can be used for 
    /// </summary>
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

    public event BaseToolSettingEventHandler? DisplayNameChanged;
    public event BaseToolSettingEventHandler? DescriptionChanged;

    public BaseToolSetting() {
    }
    
    public BaseToolSetting(string? displayName) {
        this.displayName = displayName;
    }

    /// <summary>
    /// Connects this setting to the given tool
    /// </summary>
    /// <param name="tool">The new tool</param>
    public void Connect(BaseCanvasTool tool) {
        if (this.Tool != null)
            throw new InvalidOperationException("Already connected to a tool");
        
        this.ValidatePreConnection(tool);
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

    protected virtual void ValidatePreConnection(BaseCanvasTool tool) {
        
    }

    protected virtual void OnConnected() {
    }

    protected virtual void OnDisconnected() {
    }
}