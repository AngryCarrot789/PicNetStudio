// 
// Copyright (c) 2024-2024 REghZy
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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using PicNetStudio.Avalonia.Utils;

namespace PicNetStudio.Avalonia.DataTransfer;

/// <summary>
/// A class which helps manage data properties relative to a specific instance of an owner
/// object. This class manages firing value changed events and processing data property flags,
/// which is why it is important that the setter methods of the data properties are not called directly
/// </summary>
public sealed class TransferableData {
    private Dictionary<int, ParameterData>? paramData;

    public ITransferableData Owner { get; }

    /// <summary>
    /// An event fired when any parameter's value changes relative to our owner
    /// </summary>
    public event DataParameterValueChangedEventHandler? ValueChanged;

    public TransferableData(ITransferableData owner) {
        this.Owner = owner ?? throw new ArgumentNullException(nameof(owner));
    }

    /// <summary>
    /// Adds an event handler for when the parameter changes for the specific owner. The handler type
    /// be either the default <see cref="DataParameterValueChangedEventHandler"/> delegate type, or it
    /// must match the specific parameter type's value change handler otherwise an exception will be
    /// thrown when the value changes hence why this method is unsafe
    /// </summary>
    /// <param name="parameter">The parameter that must change for the handler to be invoked</param>
    /// <param name="owner">The owner whose value for the given parameter must change for the handler to be invoked</param>
    /// <param name="handler">The handler to be invoked when the value of the parameter on the specific owner changes</param>
    internal static void InternalAddHandlerUnsafe(DataParameter parameter, TransferableData owner, Delegate handler) {
        owner.GetParamData(parameter).AddValueChangedHandler(handler);
    }

    internal static void InternalRemoveHandlerUnsafe(DataParameter parameter, TransferableData owner, Delegate handler) {
        if (owner.TryGetParameterData(parameter, out ParameterData? data)) {
            data.RemoveValueChangedHandler(handler);
        }
    }

    public bool IsValueChanging(DataParameter parameter) {
        return this.TryGetParameterData(parameter, out ParameterData? data) && data.isValueChanging;
    }

    public bool IsParameterValid(DataParameter parameter) {
        return parameter.OwnerType.IsInstanceOfType(this.Owner);
    }

    public void ValidateParameter(DataParameter parameter) {
        if (!this.IsParameterValid(parameter))
            throw new ArgumentException("Invalid parameter key for this automation data: " + parameter.GlobalKey + ". The owner types are incompatible");
    }

    private bool TryGetParameterData(DataParameter parameter, [NotNullWhen(true)] out ParameterData? data) {
        if (parameter == null)
            throw new ArgumentNullException(nameof(parameter), "Parameter cannot be null");
        if (this.paramData != null && this.paramData.TryGetValue(parameter.GlobalIndex, out data))
            return true;
        this.ValidateParameter(parameter);
        data = null;
        return false;
    }

    private ParameterData GetParamData(DataParameter parameter) {
        if (parameter == null)
            throw new ArgumentNullException(nameof(parameter), "Parameter cannot be null");

        ParameterData? data;
        if (this.paramData == null)
            this.paramData = new Dictionary<int, ParameterData>();
        else if (this.paramData.TryGetValue(parameter.GlobalIndex, out data))
            return data;
        this.ValidateParameter(parameter);
        this.paramData[parameter.GlobalIndex] = data = new ParameterData();
        return data;
    }

    private class ParameterData {
        public bool isValueChanging;
        private readonly List<Delegate> handlerList = new List<Delegate>();

        public void OnValueChanged(DataParameter parameter, ITransferableData owner) {
            lock (this.handlerList) {
                foreach (Delegate handler in this.handlerList) {
                    DataParameter.InternalInvokeValueChangeHandler(parameter, owner, handler);
                }
            }
        }

        public void AddValueChangedHandler(Delegate handler) {
            lock (this.handlerList) {
                this.handlerList.TryAdd(handler);
            }
        }

        public void RemoveValueChangedHandler(Delegate handler) {
            lock (this) {
                this.handlerList.Remove(handler);
            }
        }
    }

    internal static void InternalBeginValueChange(DataParameter parameter, ITransferableData owner) {
        ParameterData internalData = owner.TransferableData.GetParamData(parameter);
        if (internalData.isValueChanging) {
            throw new InvalidOperationException("Value is already changing. This exception is thrown, as the alternative is most likely a stack overflow exception");
        }

        internalData.isValueChanging = true;
    }

    internal static void InternalEndValueChange(DataParameter parameter, ITransferableData owner) {
        TransferableData data = owner.TransferableData;
        ParameterData internalData = data.GetParamData(parameter);
        try {
            internalData.OnValueChanged(parameter, owner);
            data.ValueChanged?.Invoke(parameter, owner);
            DataParameter.InternalOnParameterValueChanged(parameter, owner);
        }
        finally {
            internalData.isValueChanging = false;
        }
    }
}