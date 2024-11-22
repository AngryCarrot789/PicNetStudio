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