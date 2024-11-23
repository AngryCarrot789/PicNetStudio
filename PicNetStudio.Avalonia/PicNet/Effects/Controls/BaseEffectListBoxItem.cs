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

using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls.Primitives;
using PicNetStudio.Avalonia.Utils;
using PicNetStudio.PicNet.Effects;
using PicNetStudio.PicNet.Layers;

namespace PicNetStudio.Avalonia.PicNet.Effects.Controls;

/// <summary>
/// The base class for controls that are placed in the state modifier column in the layer tree. Most layers only have one; the visibility option
/// </summary>
public abstract class BaseEffectListBoxItem : TemplatedControl {
    /// <summary>
    /// Maps an effect type to a list of controls that are supported
    /// </summary>
    public static readonly ModelControlRegistry<BaseLayerEffect, BaseEffectListBoxItem> Registry;

    public EffectListBox ListBox { get; private set; }

    public BaseLayerTreeObject Layer { get; private set; }

    protected BaseEffectListBoxItem() {
    }

    static BaseEffectListBoxItem() {
        // TODO: maybe make a model system instead of layer->state mod control, so layer->layer state mod model->control
        Registry = new ModelControlRegistry<BaseLayerEffect, BaseEffectListBoxItem>();
        Registry.RegisterType<ColourChannelLayerEffect>((x) => new ColourChannelEffectListBoxItem());
    }

    protected virtual void OnConnected() {
    }

    protected virtual void OnDisconnected() {
    }

    public static List<BaseEffectListBoxItem> ProvieEffectListBoxItems(BaseLayerTreeObject layer) {
        List<BaseEffectListBoxItem> list = new List<BaseEffectListBoxItem>();
        if (!layer.HasEffects)
            return list;
        
        foreach (BaseLayerEffect effect in layer.Effects) {
            list.Add(Registry.NewInstance(effect));
        }
        
        return list;
    }

    public void OnAddingToList(EffectListBox listBox, BaseLayerTreeObject layer) {
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