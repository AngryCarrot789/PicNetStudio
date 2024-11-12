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
public class CompositeLayer : BaseVisualLayer {
    private readonly ObservableList<BaseLayerTreeObject> items;

    public ReadOnlyObservableList<BaseLayerTreeObject> Items { get; }

    public CompositeLayer() {
        this.items = new ObservableList<BaseLayerTreeObject>();
        this.Items = new ReadOnlyObservableList<BaseLayerTreeObject>(this.items);
    }

    public void AddLayer(BaseLayerTreeObject layer) => this.InsertLayer(this.items.Count, layer);

    public void InsertLayer(int index, BaseLayerTreeObject layer) {
        if (layer == null)
            throw new ArgumentNullException(nameof(layer), "Cannot add a null layer");
        if (layer.ParentLayer == this)
            throw new ArgumentException("Layer already exists in this timeline. It must be removed first");
        if (layer.ParentLayer != null)
            throw new ArgumentException("Layer already exists in another timeline. It must be removed first");

        this.items.Insert(index, layer);
        InternalOnAddedToLayer(layer, this);
        this.Canvas?.RaiseRenderInvalidated();
    }

    public bool RemoveLayer(BaseLayerTreeObject layer) {
        int index = this.items.IndexOf(layer);
        if (index == -1)
            return false;
        this.RemoveLayerAt(index);
        return true;
    }

    public void RemoveLayerAt(int index) {
        BaseLayerTreeObject layer = this.items[index];
        InternalOnPreRemoveFromOwner(layer);
        this.items.RemoveAt(index);
        InternalOnRemovedFromLayer(layer, this);
        this.Canvas?.RaiseRenderInvalidated();
    }

    public void MoveTrackIndex(int oldIndex, int newIndex) {
        if (oldIndex != newIndex) {
            this.items.Move(oldIndex, newIndex);
            this.Canvas?.RaiseRenderInvalidated();
        }
    }

    public override void RenderLayer(RenderContext ctx) {
        Debugger.Break(); // should not be rendered directly
    }

    public int IndexOf(BaseLayerTreeObject layer) {
        return this.items.IndexOf(layer);
    }
}