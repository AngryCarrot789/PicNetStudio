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

using PicNetStudio.Avalonia.DataTransfer;

namespace PicNetStudio.Avalonia.PicNet.Layers;

public delegate void BaseLayerCanvasChangedEventHandler(BaseLayerTreeObject layerTreeObject, Canvas? oldCanvas, Canvas? newCanvas);

public delegate void BaseLayerTreeObjectParentLayerChangedEventHandler(BaseLayerTreeObject sender, CompositeLayer? oldParentLayer, CompositeLayer? newParentLayer);

public delegate void BaseLayerTreeObjectEventHandler(BaseLayerTreeObject sender);

/// <summary>
/// The base class for an object in the layer hierarchy for a canvas
/// </summary>
public abstract class BaseLayerTreeObject : ITransferableData, IDisplayName {
    private CompositeLayer? parentLayer;
    private string displayName = "Layer Object";

    /// <summary>
    /// Gets the canvas this layer currently exists in
    /// </summary>
    public Canvas? Canvas { get; private set; }

    /// <summary>
    /// Gets or sets the composition layer that is a direct parent to this layer
    /// </summary>
    public CompositeLayer? ParentLayer => this.parentLayer;

    public string DisplayName {
        get => this.displayName;
        set {
            if (this.displayName == value)
                return;

            string oldName = this.displayName;
            this.displayName = value;
            this.DisplayNameChanged?.Invoke(this, oldName, value);
        }
    }
    
    public TransferableData TransferableData { get; }

    public event BaseLayerCanvasChangedEventHandler? CanvasChanged;
    public event BaseLayerTreeObjectParentLayerChangedEventHandler? ParentLayerChanged;
    public event DisplayNameChangedEventHandler? DisplayNameChanged;

    protected BaseLayerTreeObject() {
        this.TransferableData = new TransferableData(this);
    }

    /// <summary>
    /// Sets the canvas reference for only the specific layer, not its children
    /// </summary>
    /// <param name="layer"></param>
    /// <param name="canvas"></param>
    internal static void InternalSetCanvasUnsafe(BaseLayerTreeObject layer, Canvas? canvas) {
        Canvas? oldCanvas = layer.Canvas;
        layer.Canvas = canvas;
        layer.CanvasChanged?.Invoke(layer, oldCanvas, canvas);
    }

    /// <summary>
    /// Sets the canvas reference for the entire layer's hierarchy
    /// </summary>
    internal static void InternalSetCanvasInTree(BaseLayerTreeObject layer, Canvas? canvas) {
        InternalSetCanvasUnsafe(layer, canvas);
        if (layer is CompositeLayer composite) {
            foreach (BaseLayerTreeObject item in composite.Items) {
                InternalSetCanvasInTree(item, canvas);
            }
        }
    }

    internal static void InternalOnAddedToCompositionLayer(BaseLayerTreeObject layer, CompositeLayer? parent) {
        InternalOnTrackTimelineChanged(layer, null, parent);
    }

    internal static void InternalOnRemovedFromCompositionLayer(BaseLayerTreeObject layer, CompositeLayer? parent) {
        InternalOnTrackTimelineChanged(layer, parent, null);
    }

    internal static void InternalOnTrackTimelineChanged(BaseLayerTreeObject layer, CompositeLayer? oldParent, CompositeLayer? newParent) {
        layer.Canvas = newParent?.Canvas;
        layer.parentLayer = newParent;
        layer.ParentLayerChanged?.Invoke(layer, oldParent, newParent);
    }
}