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
using System.Diagnostics;
using PicNetStudio.Avalonia.Utils.Collections.Observable;

namespace PicNetStudio.Avalonia.PicNet.Layers;

/// <summary>
/// A composition layer contains its own layer hierarchy which can be rendered like a raster layer
/// </summary>
public class CompositeLayer : BaseVisualLayer, ILayerContainer {
    private readonly ObservableList<BaseLayerTreeObject> layers;

    public ReadOnlyObservableList<BaseLayerTreeObject> Layers { get; }

    public CompositeLayer() {
        this.layers = new ObservableList<BaseLayerTreeObject>();
        this.Layers = new ReadOnlyObservableList<BaseLayerTreeObject>(this.layers);
    }

    public void AddLayer(BaseLayerTreeObject layer) => this.InsertLayer(this.layers.Count, layer);

    public void InsertLayer(int index, BaseLayerTreeObject layer) {
        if (layer == null)
            throw new ArgumentNullException(nameof(layer), "Cannot add a null layer");
        if (layer.ParentContainer == this)
            throw new InvalidOperationException("Layer already exists in this layer. It must be removed first");
        if (layer.ParentContainer != null)
            throw new InvalidOperationException("Layer already exists in another container. It must be removed first");
        
        this.layers.Insert(index, layer);
        InternalOnAddedToLayer(layer, this);
        this.Canvas?.RaiseRenderInvalidated();
    }

    public bool RemoveLayer(BaseLayerTreeObject layer) {
        int index = this.layers.IndexOf(layer);
        if (index == -1)
            return false;
        this.RemoveLayerAt(index);
        return true;
    }

    public void RemoveLayerAt(int index) {
        BaseLayerTreeObject layer = this.layers[index];
        InternalOnPreRemoveFromOwner(layer);
        this.layers.RemoveAt(index);
        InternalOnRemovedFromLayer(layer, this);
        this.Canvas?.RaiseRenderInvalidated();
    }

    public void MoveTrackIndex(int oldIndex, int newIndex) {
        if (oldIndex != newIndex) {
            this.layers.Move(oldIndex, newIndex);
            this.Canvas?.RaiseRenderInvalidated();
        }
    }

    public override void RenderLayer(RenderContext ctx) {
        Debugger.Break(); // should not be rendered directly
    }

    public int IndexOf(BaseLayerTreeObject layer) {
        return this.layers.IndexOf(layer);
    }
}