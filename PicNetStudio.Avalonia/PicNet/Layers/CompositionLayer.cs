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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using PicNetStudio.Avalonia.PicNet.Layers.Core;
using PicNetStudio.Avalonia.Utils.Collections.Observable;

namespace PicNetStudio.Avalonia.PicNet.Layers;

/// <summary>
/// A composition layer contains its own layer hierarchy which can be rendered like a raster layer
/// </summary>
public class CompositionLayer : BaseVisualLayer {
    private readonly SuspendableObservableList<BaseLayerTreeObject> layers;

    public ReadOnlyObservableList<BaseLayerTreeObject> Layers { get; }

    /// <summary>
    /// Helper to check if this layer is the root "folder" in a canvas. Must also check that <see cref="BaseLayerTreeObject.Canvas"/> is non-null too for a full check
    /// </summary>
    public bool IsRootLayer => this.ParentLayer == null;

    protected bool isHierarchyVisualInvalid = true;
    internal readonly PNBitmap cachedVisualHierarchy;

    public CompositionLayer() {
        this.layers = new SuspendableObservableList<BaseLayerTreeObject>();
        this.Layers = new ReadOnlyObservableList<BaseLayerTreeObject>(this.layers);
        this.cachedVisualHierarchy = new PNBitmap();
    }

    public override bool IsHitTest(double x, double y) {
        foreach (BaseLayerTreeObject layer in this.layers) {
            if (layer.IsHitTest(x, y)) {
                return true;
            }
        }
        
        return false;
    }

    protected override void LoadDataIntoClone(BaseLayerTreeObject clone) {
        base.LoadDataIntoClone(clone);
        CompositionLayer compositionLayer = (CompositionLayer) clone;
        foreach (BaseLayerTreeObject child in this.layers) {
            compositionLayer.AddLayer(child.Clone());
        }
    }

    public void AddLayer(BaseLayerTreeObject layer) => this.InsertLayer(this.layers.Count, layer);

    public void InsertLayer(int index, BaseLayerTreeObject layer) {
        if (index < 0)
            throw new ArgumentOutOfRangeException(nameof(index), "Negative indices not allowed");
        if (index > this.layers.Count)
            throw new ArgumentOutOfRangeException(nameof(index), $"Index is beyond the range of this list: {index} > count({this.layers.Count})");
        
        if (layer == null)
            throw new ArgumentNullException(nameof(layer), "Cannot add a null layer");
        if (layer.ParentLayer == this)
            throw new InvalidOperationException("Layer already exists in this layer. It must be removed first");
        if (layer.ParentLayer != null)
            throw new InvalidOperationException("Layer already exists in another container. It must be removed first");
        
        // Update before insertion since it fires an event, IndexInParent would be -1 in handlers
        for (int i = this.layers.Count - 1; i >= index; i--) {
            InternalSetIndexInParent(this.layers[i], i + 1);
        }

        using (this.layers.SuspendEvents()) {
            this.layers.Insert(index, layer);
            InternalOnAddedToLayer(index, layer, this);
        }

        this.InvalidateVisual();
    }
    
    public void AddLayers(IEnumerable<BaseLayerTreeObject> layers) {
        foreach (BaseLayerTreeObject layer in layers) {
            this.AddLayer(layer);
        }
    }

    public void InsertLayers(int index, IEnumerable<BaseLayerTreeObject> layers) {
        if (index < 0)
            throw new ArgumentOutOfRangeException(nameof(index), "Negative indices not allowed");
        if (index > this.layers.Count)
            throw new ArgumentOutOfRangeException(nameof(index), $"Index is beyond the range of this list: {index} > count({this.layers.Count})");
        
        List<BaseLayerTreeObject> list = layers.ToList();
        int newCount = list.Count;
        if (newCount == 0)
            return;
        
        if (newCount == 1) {
            this.InsertLayer(index, list[0]);
            return;
        }
        
        int nextLayerIndex = index;
        foreach (BaseLayerTreeObject layer in list) {
            if (layer == null)
                throw new ArgumentNullException(nameof(layer), "Cannot add a null layer");
            if (layer.ParentLayer == this)
                throw new InvalidOperationException("Layer already exists in this layer. It must be removed first");
            if (layer.ParentLayer != null)
                throw new InvalidOperationException("Layer already exists in another container. It must be removed first");
            InternalSetIndexInParent(layer, nextLayerIndex++);
        }

        Debug.Assert(nextLayerIndex == index + newCount);

        // 0,1,2,3,4,5
        // insert a,b,c at 2
        // 0,1,a,b,c,2,3,4,5

        for (int i = this.layers.Count - 1; i >= index; i--) {
            InternalSetIndexInParent(this.layers[i], i + newCount);
        }

        using (this.layers.SuspendEvents()) {
            this.layers.InsertRange(index, list);
            foreach (BaseLayerTreeObject layer in list) {
                InternalOnAddedToLayer(index++, layer, this);
            }
        }

        this.InvalidateVisual();
    }

    public bool RemoveLayer(BaseLayerTreeObject layer) {
        if (!ReferenceEquals(layer.ParentLayer, this))
            return false;
        this.RemoveLayerAt(InternalIndexInParent(layer));
        
        Debug.Assert(layer.ParentLayer != this, "Layer parent not updated, still ourself");
        Debug.Assert(layer.ParentLayer == null, "Layer parent not updated to null");
        return true;
    }

    public void RemoveLayerAt(int index) {
        BaseLayerTreeObject layer = this.layers[index];
        InternalOnPreRemoveFromOwner(layer);
        for (int i = this.layers.Count - 1; i > index /* not >= since we remove one at index */; i--) {
            InternalSetIndexInParent(this.layers[i], i - 1);
        }
        
        using (this.layers.SuspendEvents()) {
            this.layers.RemoveAt(index);
            InternalOnRemovedFromLayer(layer, this);
        }

        this.InvalidateVisual();
    }

    public override void InvalidateVisual() {
        this.isHierarchyVisualInvalid = true;
        base.InvalidateVisual();
    }

    public void RemoveLayers(IEnumerable<BaseLayerTreeObject> layers) {
        foreach (BaseLayerTreeObject layer in layers) {
            this.RemoveLayer(layer);
        }
    }

    public override void RenderLayer(ref RenderContext ctx) {
        Debugger.Break(); // should not be rendered directly
        LayerRenderer.RenderLayer(ref ctx, this);
    }

    public int IndexOf(BaseLayerTreeObject layer) {
        return ReferenceEquals(layer.ParentLayer, this) ? InternalIndexInParent(layer) : -1;
    }

    public bool Contains(BaseLayerTreeObject layer) {
        return this.IndexOf(layer) != -1;
    }

    /// <summary>
    /// Returns true when the given item is equal to one of the parents in our hierarchy
    /// </summary>
    /// <param name="item">The item to check if it's a hierarchical parent relative to this layer</param>
    /// <param name="startAtThis">
    /// When true, this method returns true when the item is equal to the current instance
    /// </param>
    /// <returns>See summary</returns>
    public bool IsParentInHierarchy(CompositionLayer? item, bool startAtThis = true) {
        for (CompositionLayer? parent = (startAtThis ? this : this.ParentLayer); item != null; item = item.ParentLayer) {
            if (ReferenceEquals(parent, item)) {
                return true;
            }
        }

        return false;
    }

    public int LowestIndexOf(IEnumerable<BaseLayerTreeObject> items) {
        int minIndex = int.MaxValue;
        foreach (BaseLayerTreeObject layer in items) {
            int index = this.IndexOf(layer);
            if (index != -1) {
                minIndex = Math.Min(minIndex, index);
            }
        }

        return minIndex == int.MaxValue ? -1 : minIndex;
    }

    protected override void InvalidateTransformationMatrix() {
        foreach (BaseLayerTreeObject layer in this.layers) {
            InternalInvalidateTransformationMatrixFromParent(layer as BaseVisualLayer);
        }
        
        base.InvalidateTransformationMatrix();
    }

    public static CompositionLayer InternalCreateCanvasRoot(Canvas canvas) {
        CompositionLayer layer = new CompositionLayer() {Name = "<<ROOT>>"};
        InternalSetCanvas(layer, canvas);
        return layer;
    }

    internal static bool InternalGetAndResetVisualInvalidState(CompositionLayer layer) {
        bool isInvalid = layer.isHierarchyVisualInvalid;
        layer.isHierarchyVisualInvalid = false;
        return isInvalid;
    }
}