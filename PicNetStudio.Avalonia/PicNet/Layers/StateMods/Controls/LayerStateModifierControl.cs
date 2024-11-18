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
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using PicNetStudio.Avalonia.PicNet.Layers.Core;

namespace PicNetStudio.Avalonia.PicNet.Layers.StateMods.Controls;

/// <summary>
/// The base class for controls that are placed in the state modifier column in the layer tree. Most layers only have one; the visibility option
/// </summary>
public abstract class LayerStateModifierControl : TemplatedControl {
    /// <summary>
    /// Maps a layer type to a list of state modifier controls that are supported
    /// </summary>
    public static readonly ModelMultiControlRegistry<BaseLayerTreeObject, LayerStateModifierControl> Registry;

    public class ModelMultiControlRegistry<TModel, TControl> where TControl : Control where TModel : class {
        // Func<LayerStateModifierControl> == Delegate
        private readonly Dictionary<Type, List<Delegate>> constructors;

        public ModelMultiControlRegistry() {
            this.constructors = new Dictionary<Type, List<Delegate>>();
        }

        private List<Delegate> GetListFor(Type typeOfModel) {
            if (!this.constructors.TryGetValue(typeOfModel, out List<Delegate>? list))
                this.constructors[typeOfModel] = list = new List<Delegate>();
            return list;
        }

        public void RegisterType<TSpecificModel>(Func<TSpecificModel, TControl> constructor) where TSpecificModel : TModel {
            // Need to create a Func<TModel, TControl>. cannot use the parameter since generic type is too high so it's
            // incompatible and therefore impossible to use in the NewInstance method, at least without using reflection it is
            this.GetListFor(typeof(TSpecificModel)).Add(new Func<TModel, TControl>(x => constructor((TSpecificModel) x)));
        }

        public void RegisterType<TSpecificModel>(Func<TControl> constructor) where TSpecificModel : TModel {
            this.GetListFor(typeof(TSpecificModel)).Add(constructor);
        }
        
        public List<TControl> ProvideControls(TModel model) {
            List<TControl> stateModifiers = new List<TControl>();
            
            for (Type? type = model.GetType(); type != null; type = type.BaseType) {
                if (this.constructors.TryGetValue(type, out List<Delegate>? list)) {
                    foreach (Delegate constructor in list) {
                        TControl control = constructor is Func<TModel, TControl> biFunc ? biFunc(model) : ((Func<TControl>) constructor)();
                        stateModifiers.Add(control);
                    }
                }
            }

            return stateModifiers;
        }
    }

    public LayerStateModifierListBox ListBox { get; private set; }

    public BaseLayerTreeObject Layer { get; private set; }

    protected LayerStateModifierControl() {
    }

    static LayerStateModifierControl() {
        // TODO: maybe make a model system instead of layer->state mod control, so layer->layer state mod model->control
        Registry = new ModelMultiControlRegistry<BaseLayerTreeObject, LayerStateModifierControl>();
        // BaseVisualLayer supports LayerVisibilityStateModifier
        Registry.RegisterType<BaseVisualLayer>(() => new LayerVisibilityStateControl());
    }

    protected virtual void OnConnected() {
    }

    protected virtual void OnDisconnected() {
    }

    public static List<LayerStateModifierControl> ProvideStateModifiers(BaseLayerTreeObject layer) {
        List<LayerStateModifierControl> stateModifiers = Registry.ProvideControls(layer);
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