using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using PicNetStudio.Avalonia.PicNet.Toolbars;
using PicNetStudio.Avalonia.PicNet.Toolbars.Controls;
using PicNetStudio.Avalonia.Utils;
using PicNetStudio.Avalonia.Utils.Collections.Observable;

namespace PicNetStudio.Avalonia.PicNet.Layers.StateMods.Controls;

/// <summary>
/// A list of layer state modifier controls
/// </summary>
public class LayerStateModifierListBox : ItemsControl {
    public static readonly StyledProperty<BaseLayerTreeObject?> LayerObjectProperty = AvaloniaProperty.Register<LayerStateModifierListBox, BaseLayerTreeObject?>("LayerObject");

    public BaseLayerTreeObject? LayerObject {
        get => this.GetValue(LayerObjectProperty);
        set => this.SetValue(LayerObjectProperty, value);
    }
    
    public LayerStateModifierListBox() {
    }

    static LayerStateModifierListBox() {
        LayerObjectProperty.Changed.AddClassHandler<LayerStateModifierListBox, BaseLayerTreeObject?>((o, e) => o.OnLayerChanged(e));
    }

    private void OnLayerChanged(AvaloniaPropertyChangedEventArgs<BaseLayerTreeObject?> e) {
        if (e.TryGetOldValue(out BaseLayerTreeObject? oldLayer)) {
            for (int i = this.Items.Count - 1; i >= 0; i--) {
                BaseLayerStateModifierControl control = (BaseLayerStateModifierControl) this.Items[i]!;
                control.OnRemovingFromList();
                this.Items.RemoveAt(i);
                control.OnRemovedFromList();
                this.InvalidateMeasure();
            }
        }

        if (e.TryGetNewValue(out BaseLayerTreeObject? newLayer)) {
            List<BaseLayerStateModifierControl> list = BaseLayerStateModifierControl.ProvideStateModifiers(newLayer);
            
            int i = 0;
            foreach (BaseLayerStateModifierControl control in list) {
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