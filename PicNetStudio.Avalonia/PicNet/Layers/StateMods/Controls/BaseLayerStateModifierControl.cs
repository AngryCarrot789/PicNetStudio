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
using Avalonia;
using Avalonia.Controls.Primitives;

namespace PicNetStudio.Avalonia.PicNet.Layers.StateMods.Controls;

/// <summary>
/// The base class for controls that are placed in the state modifier column in the layer tree. Most layers only have one; the visibility option
/// </summary>
public abstract class BaseLayerStateModifierControl : TemplatedControl {
    /// <summary>
    /// Maps a layer type to a list of state modifier controls that are supported
    /// </summary>
    public static readonly Dictionary<Type, List<(Type, Func<BaseLayerStateModifierControl>)>> RegisteredStateModifierControls;

    public LayerStateModifierListBox ListBox { get; private set; }
    
    public BaseLayerTreeObject Layer { get; private set; }

    protected BaseLayerStateModifierControl() {
        
    }

    static BaseLayerStateModifierControl() {
        // TODO: maybe make a model system instead of layer->state mod control, so layer->layer state mod model->control
        RegisteredStateModifierControls = new Dictionary<Type, List<(Type, Func<BaseLayerStateModifierControl>)>>();
        // BaseVisualLayer supports LayerVisibilityStateModifier
        RegisteredStateModifierControls[typeof(BaseVisualLayer)] = new List<(Type, Func<BaseLayerStateModifierControl>)>() {
            (typeof(LayerVisibilityStateControl), () => new LayerVisibilityStateControl())
        };
    }

    protected virtual void OnConnected() {
        
    }
    
    protected virtual void OnDisconnected() {
        
    }

    public static List<BaseLayerStateModifierControl> ProvideStateModifiers(BaseLayerTreeObject layer) {
        List<BaseLayerStateModifierControl> stateModifiers = new List<BaseLayerStateModifierControl>();
        for (Type? type = layer.GetType(); type != null; type = type.BaseType) {
            if (RegisteredStateModifierControls.TryGetValue(type, out List<(Type, Func<BaseLayerStateModifierControl>)>? registeredModifiers)) {
                foreach ((Type, Func<BaseLayerStateModifierControl>) tuple in registeredModifiers) {
                    stateModifiers.Add(tuple.Item2());
                }
            }
        }

        stateModifiers.Reverse();
        return stateModifiers;
    }

    public void OnAddingToList(LayerStateModifierListBox listBox, BaseLayerTreeObject layer) {
        this.ListBox = listBox;
        this.Layer = layer;
        this.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
    }

    public void OnAddedToList() {
        this.OnConnected();
    }

    public void OnRemovingFromList() {
        this.OnDisconnected();
        this.Layer = null;
    }

    public void OnRemovedFromList() {
        
    }
}