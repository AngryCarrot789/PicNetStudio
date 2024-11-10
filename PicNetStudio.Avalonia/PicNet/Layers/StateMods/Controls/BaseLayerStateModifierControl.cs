using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls.Primitives;
using PicNetStudio.Avalonia.PicNet.Toolbars;

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