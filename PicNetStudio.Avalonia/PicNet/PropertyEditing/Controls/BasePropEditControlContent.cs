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
using System.Collections.Generic;
using System.Diagnostics;
using Avalonia.Controls.Primitives;
using PicNetStudio.Avalonia.PicNet.CustomParameters;
using PicNetStudio.Avalonia.PicNet.PropertyEditing.Controls.Core;
using PicNetStudio.Avalonia.PicNet.PropertyEditing.Controls.DataTransfer;
using PicNetStudio.Avalonia.PicNet.PropertyEditing.Controls.DataTransfer.Automatic;
using PicNetStudio.PicNet.CustomParameters;
using PicNetStudio.PicNet.PropertyEditing;
using PicNetStudio.PicNet.PropertyEditing.Core;
using PicNetStudio.PicNet.PropertyEditing.DataTransfer;
using PicNetStudio.PicNet.PropertyEditing.DataTransfer.Automatic;

namespace PicNetStudio.Avalonia.PicNet.PropertyEditing.Controls;

public abstract class BasePropEditControlContent : TemplatedControl {
    private static readonly Dictionary<Type, Func<BasePropEditControlContent>> Constructors;

    public PropertyEditorSlotControl? SlotControl { get; private set; }

    public PropertyEditorSlot? SlotModel => this.SlotControl?.Model;

    public bool IsConnected => this.SlotControl != null;

    protected BasePropEditControlContent() {
    }

    static BasePropEditControlContent() {
        Constructors = new Dictionary<Type, Func<BasePropEditControlContent>>();
        // specific case editors
        RegisterType(typeof(DisplayNamePropertyEditorSlot), () => new DisplayNamePropertyEditorControl());
        RegisterType(typeof(LayerNamePropertyEditorSlot), () => new LayerNamePropertyEditorControl());

        // standard editors
        RegisterType(typeof(DataParameterLongPropertyEditorSlot), () => new DataParameterLongPropertyEditorControl());
        RegisterType(typeof(DataParameterDoublePropertyEditorSlot), () => new DataParameterDoublePropertyEditorControl());
        RegisterType(typeof(DataParameterFloatPropertyEditorSlot), () => new DataParameterFloatPropertyEditorControl());
        RegisterType(typeof(DataParameterBoolPropertyEditorSlot), () => new DataParameterBooleanPropertyEditorControl());
        RegisterType(typeof(DataParameterStringPropertyEditorSlot), () => new DataParameterStringPropertyEditorControl());
        RegisterType(typeof(DataParameterPointPropertyEditorSlot), () => new DataParameterPointPropertyEditorControl());
        RegisterType(typeof(DataParameterBlendModePropertyEditorSlot), () => new DataParameterBlendModePropertyEditorControl());
        
        // automatic editors
        RegisterType(typeof(AutomaticDataParameterFloatPropertyEditorSlot), () => new AutomaticDataParameterFloatPropertyEditorControl());
        RegisterType(typeof(AutomaticDataParameterDoublePropertyEditorSlot), () => new AutomaticDataParameterDoublePropertyEditorControl());
        RegisterType(typeof(AutomaticDataParameterLongPropertyEditorSlot), () => new AutomaticDataParameterLongPropertyEditorControl());
        RegisterType(typeof(AutomaticDataParameterPointPropertyEditorSlot), () => new AutomaticDataParameterPointPropertyEditorControl());
    }

    public static void RegisterType<T>(Type slotType, Func<T> func) where T : BasePropEditControlContent {
        if (!typeof(PropertyEditorSlot).IsAssignableFrom(slotType))
            throw new ArgumentException("Slot type is invalid: cannot assign " + slotType + " to " + typeof(PropertyEditorSlot));
        Constructors.Add(slotType, func);
    }

    public static BasePropEditControlContent NewContentInstance(Type slotType) {
        if (slotType == null) {
            throw new ArgumentNullException(nameof(slotType));
        }

        // Just try to find a base control type. It should be found first try unless I forgot to register a new control type
        bool hasLogged = false;
        for (Type? type = slotType; type != null; type = type.BaseType) {
            if (Constructors.TryGetValue(type, out Func<BasePropEditControlContent>? func)) {
                return func();
            }

            if (!hasLogged) {
                hasLogged = true;
                Debugger.Break();
                Debug.WriteLine("Could not find control for track type on first try. Scanning base types");
            }
        }

        throw new Exception("No such content control for track type: " + slotType.Name);
    }

    public void Connect(PropertyEditorSlotControl slot) {
        this.SlotControl = slot;
        this.OnConnected();
    }

    public void Disconnect() {
        this.OnDisconnected();
        this.SlotControl = null;
    }

    protected abstract void OnConnected();

    protected abstract void OnDisconnected();
}