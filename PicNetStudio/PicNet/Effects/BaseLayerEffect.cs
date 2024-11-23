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

using System.Diagnostics;
using PicNetStudio.DataTransfer;
using PicNetStudio.PicNet.Layers;
using PicNetStudio.Utils.RBC;

namespace PicNetStudio.PicNet.Effects;

public delegate void EffectOwnerChangedEventHandler(BaseLayerEffect sender, BaseLayerTreeObject? oldLayer, BaseLayerTreeObject? newLayer);

/// <summary>
/// The base class for all effects applicable to a layer
/// </summary>
public abstract class BaseLayerEffect : ITransferableData {
    // IEditorUI contains ILayerTree which contains all ILayerTreeNodeItem
    // ILayerTreeNode contains a list of ILayerEffectUI for each effect in the list box
    // Need to map ILayerEffectUI to the PropertyEditorGroup controls so that we can scroll it into view in the property editor

    /// <summary>
    /// Gets the layer that this effect is applied to
    /// </summary>
    public BaseLayerTreeObject? Layer { get; private set; }

    public TransferableData TransferableData { get; }

    public string FactoryId => EffectFactory.Instance.GetId(this.GetType());

    /// <summary>
    /// An event fired when our <see cref="ParentLayer"/> property changes.
    /// If the new parent is attached to a canvas, our canvas will be updates
    /// after this event is fired (see <see cref="CanvasChanged"/>)
    /// </summary>
    public event EffectOwnerChangedEventHandler? LayerChanged;

    protected virtual bool AllowMultipleInstancesPerLayer => true;

    protected BaseLayerEffect() {
        this.TransferableData = new TransferableData(this);
    }

    public static void WriteSerialisedWithIdList(BaseLayerTreeObject owner, RBEList list) {
        foreach (BaseLayerEffect effect in owner.Effects) {
            if (!(effect.FactoryId is string id))
                throw new Exception("Unknown clip type: " + effect.GetType());
            RBEDictionary dictionary = list.AddDictionary();
            dictionary.SetString(nameof(FactoryId), id);
            effect.WriteToRBE(dictionary.CreateDictionary("Data"));
        }
    }

    public static void ReadSerialisedWithIdList(BaseLayerTreeObject owner, RBEList list) {
        foreach (RBEDictionary dictionary in list.Cast<RBEDictionary>()) {
            string? factoryId = dictionary.GetString(nameof(FactoryId));
            if (factoryId == null)
                throw new Exception("Invalid factory ID");
            
            BaseLayerEffect effect = EffectFactory.Instance.NewInstance(factoryId);
            effect.ReadFromRBE(dictionary.GetDictionary("Data"));
            owner.AddEffect(effect);
        }
    }

    /// <summary>
    /// Invoked when this layer is added to, removed from or moved between layers. Our <see cref="Layer"/> property is replaced prior to this call
    /// </summary>
    /// <param name="oldLayer">The previous layer (non-null when removing or moving)</param>
    /// <param name="newLayer">The new layer (non-null when adding or moving)</param>
    protected void OnLayerChanged(BaseLayerTreeObject? oldLayer, BaseLayerTreeObject? newLayer) {
        
    }

    public virtual void WriteToRBE(RBEDictionary data) {
        
    }

    public virtual void ReadFromRBE(RBEDictionary data) {
        
    }

    internal static void InternalOnAddedToLayer(BaseLayerEffect effect, BaseLayerTreeObject newLayer) {
        Debug.Assert(effect.Layer == null, "Did not expect layer to be in a composition layer when adding it to another");

        effect.Layer = newLayer;
        effect.OnLayerChanged(null, newLayer);
        effect.LayerChanged?.Invoke(effect, null, newLayer);
    }

    // User deleted some layer from composition layer
    internal static void InternalOnRemovedFromLayer(BaseLayerEffect effect, BaseLayerTreeObject oldLayer) {
        Debug.Assert(effect.Layer != null, "Did not expect effect to not have a parent while trying to remove");
        effect.Layer = null;
        effect.OnLayerChanged(oldLayer, null);
        effect.LayerChanged?.Invoke(effect, oldLayer, null);
    }
}