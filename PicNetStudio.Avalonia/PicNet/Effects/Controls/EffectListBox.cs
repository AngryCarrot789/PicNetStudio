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
using Avalonia.Controls;
using PicNetStudio.Avalonia.Utils;
using PicNetStudio.PicNet.Layers;

namespace PicNetStudio.Avalonia.PicNet.Effects.Controls;

/// <summary>
/// A list of layer state modifier controls
/// </summary>
public class EffectListBox : ItemsControl {
    public static readonly StyledProperty<BaseLayerTreeObject?> LayerObjectProperty = AvaloniaProperty.Register<EffectListBox, BaseLayerTreeObject?>("LayerObject");

    public BaseLayerTreeObject? LayerObject {
        get => this.GetValue(LayerObjectProperty);
        set => this.SetValue(LayerObjectProperty, value);
    }

    public EffectListBox() {
    }

    static EffectListBox() {
        LayerObjectProperty.Changed.AddClassHandler<EffectListBox, BaseLayerTreeObject?>((o, e) => o.OnLayerChanged(e));
    }

    private void OnLayerChanged(AvaloniaPropertyChangedEventArgs<BaseLayerTreeObject?> e) {
        if (e.TryGetOldValue(out BaseLayerTreeObject? oldLayer)) {
            for (int i = this.Items.Count - 1; i >= 0; i--) {
                BaseEffectListBoxItem control = (BaseEffectListBoxItem) this.Items[i]!;
                control.OnRemovingFromList();
                this.Items.RemoveAt(i);
                control.OnRemovedFromList();
                this.InvalidateMeasure();
            }
        }

        if (e.TryGetNewValue(out BaseLayerTreeObject? newLayer)) {
            List<BaseEffectListBoxItem> list = BaseEffectListBoxItem.ProvieEffectListBoxItems(newLayer);

            int i = 0;
            foreach (BaseEffectListBoxItem control in list) {
                int index = i++;
                control.OnAddingToList(this, newLayer);
                this.Items.Insert(index, control);
                control.OnAddedToList();
                control.InvalidateMeasure();
                this.InvalidateMeasure();
            }
        }
    }
}