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
using PicNetStudio.Avalonia.DataTransfer;

namespace PicNetStudio.Avalonia.PicNet.Tools.Settings.DataTransfer;

public delegate void BaseDataParameterToolSettingEventHandler(BaseDataParameterToolSetting sender);

/// <summary>
/// The base class for a tool setting that uses a single <see cref="DataParameter"/> to get/set the underlying tool value
/// </summary>
public abstract class BaseDataParameterToolSetting : BaseToolSetting {
    /// <summary>
    /// The parameter used to communicate a tool's value
    /// </summary>
    public DataParameter Parameter { get; }

    /// <summary>
    /// An event fired when the value of this tool setting has changed or should be re-queried (as it may be different from an initial state)
    /// </summary>
    public event BaseDataParameterToolSettingEventHandler? ValueChanged;

    protected BaseDataParameterToolSetting(DataParameter parameter, string? displayName = null) : base(displayName) {
        this.Parameter = parameter ?? throw new ArgumentNullException(nameof(parameter));
    }

    protected override void ValidatePreConnection(BaseCanvasTool tool) {
        base.ValidatePreConnection(tool);
        this.Parameter.ValidateOwner(tool);
    }

    protected override void OnConnected() {
        base.OnConnected();
        this.Parameter.AddValueChangedHandler(this.Tool!, this.OnValueChangedForTool);
        this.QueryValueAndRaiseValueChanged();
    }

    protected override void OnDisconnected() {
        base.OnDisconnected();
        this.Parameter.RemoveValueChangedHandler(this.Tool!, this.OnValueChangedForTool);
    }

    private void OnValueChangedForTool(DataParameter parameter, ITransferableData owner) {
        this.QueryValueAndRaiseValueChanged();
    }

    private void QueryValueAndRaiseValueChanged() {
        // QueryValue updates the local field value, so we need to raise ValueChanged
        // afterwards or listening UI controls won't notice the value changing.
        // This is required since the standard Value properties derived classes might use
        // are typically set from the UI controls and are treated as a user changing it,
        // which means it updates the underlying parameter value and issues could occur
        this.QueryValueFromHandlers();
        this.OnValueChanged();
    }

    /// <summary>
    /// Queries the effective value of the parameter from our tool and sets it as
    /// the local value (so that it does not update our <see cref="Parameter"/>'s value)
    /// </summary>
    protected abstract void QueryValueFromHandlers();

    /// <summary>
    /// Raises the value changed event, which allows the tool setting UI controls to re-query
    /// the effective value of the parameter and present them in the UI elements' value(s)
    /// </summary>
    protected void OnValueChanged() {
        this.ValueChanged?.Invoke(this);
    }
}
