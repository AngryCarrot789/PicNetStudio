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
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using PicNetStudio.Avalonia.Utils;
using PicNetStudio.PicNet.Tools.Settings;
using PicNetStudio.PicNet.Tools.Settings.DataTransfer;
using PicNetStudio.Utils;

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
        Registry.RegisterType<DataParameterLongToolSetting>(() => new DataParameterLongToolSettingControl());
        Registry.RegisterType<AutomaticDataParameterFloatToolSetting>(() => new AutomaticDataParameterFloatToolSettingControl());
    }
}