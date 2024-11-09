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
using Avalonia;
using PicNetStudio.Avalonia.PicNet.Layers;
using SkiaSharp;

namespace PicNetStudio.Avalonia.PicNet;

public delegate void CanvasRenderInvalidatedEventHandler(Canvas canvas);
public delegate void CanvasLayerIndexChangedEventHandler(Canvas canvas, BaseLayer layer, int oldIndex, int newIndex);
public delegate void CanvasActiveLayerChangedEventHandler(Canvas canvas, BaseLayer oldActiveLayer, BaseLayer newActiveLayer);
public delegate void CanvasSizeChangedEventHandler(Canvas canvas, PixelSize oldSize, PixelSize newSize);

/// <summary>
/// Represents the canvas for a document. This contains layer information among other data
/// </summary>
public class Canvas {
    private readonly List<BaseLayer> layers;
    private PixelSize size;
    private BaseLayer activeLayer;

    /// <summary>
    /// The document that owns this canvas
    /// </summary>
    public Document Document { get; }
    
    public IList<BaseLayer> Layers { get; }

    public BaseLayer ActiveLayer {
        get => this.activeLayer;
        set {
            BaseLayer oldActiveLayer = this.activeLayer;
            if (oldActiveLayer == value)
                return;

            this.activeLayer = value;
            this.ActiveLayerChanged?.Invoke(this, oldActiveLayer, value);
        }
    }
    
    public PixelSize Size {
        get => this.size;
        set {
            PixelSize oldSize = this.size;
            if (oldSize == value)
                return;

            this.size = value;
            this.SizeChanged?.Invoke(this, oldSize, value);
        }
    }
    
    /// <summary>
    /// An event fired when the canvas has changed and any UI needs redrawing due to the pixels changing
    /// </summary>
    public event CanvasRenderInvalidatedEventHandler? RenderInvalidated;

    /// <summary>
    /// An event fired when a layer is added, removed or moved
    /// </summary>
    public event CanvasLayerIndexChangedEventHandler? LayerIndexChanged;
    
    /// <summary>
    /// An event fired when the active layer changes
    /// </summary>
    public event CanvasActiveLayerChangedEventHandler? ActiveLayerChanged;
    
    /// <summary>
    /// An event fired when the size of the canvas changes, e.g. from cropping
    /// </summary>
    public event CanvasSizeChangedEventHandler? SizeChanged;

    // TODO: two bitmaps containing pre-rendered layers before and after active layer, as to make rendering faster 
    
    public Canvas(Document document) {
        this.Document = document;
        this.layers = new List<BaseLayer>();
        this.Layers = this.layers.AsReadOnly();
        this.size = new PixelSize(500, 500);
    }
    
    // QtBitmapEditor -> Project::paintEvent
    public void Render(SKSurface surface) {
        foreach (BaseLayer layer in this.layers) {
            layer.Render(surface);
        }

        // surface.Canvas.DrawBitmap(pnb.Bitmap, 0, 0);
    }

    public void RaiseRenderInvalidated() {
        this.RenderInvalidated?.Invoke(this);
    }
    
    public void AddLayer(BaseLayer layer) {
        if (layer.Canvas == this)
            throw new InvalidOperationException("Layer already added");

        this.InsertLayer(this.layers.Count, layer);
    }

    public void InsertLayer(int index, BaseLayer layer) {
        if (layer.Canvas == this)
            throw new InvalidOperationException("Layer already added");

        this.layers.Insert(index, layer);
        BaseLayer.InternalSetCanvas(layer, this);
        this.LayerIndexChanged?.Invoke(this, layer, -1, index);

        // Select first layer for now
        if (this.layers.Count == 1)
            this.ActiveLayer = layer;
        
        this.RaiseRenderInvalidated();
    }

    public bool RemoveLayer(BaseLayer layer) {
        int index = this.layers.IndexOf(layer);
        if (index == -1)
            return false;

        this.RemoveLayerInternal(index, layer);
        return true;
    }

    public void RemoveLayerAt(int index) {
        this.RemoveLayerInternal(index, this.layers[index]);
    }

    public void MoveLayer(int oldIndex, int newIndex) {
        this.LayerIndexChanged?.Invoke(this, this.layers[oldIndex], oldIndex, newIndex);
    }

    private void RemoveLayerInternal(int index, BaseLayer layer) {
        if (this.layers.Count == 1) {
            Debug.Assert(index == 0, "Expected removal index to equal 0 when layer count is 1");

            // Un-set active layer as it's being removed
            this.ActiveLayer = null;
        }

        this.layers.RemoveAt(index);
        BaseLayer.InternalSetCanvas(layer, null);
        this.LayerIndexChanged?.Invoke(this, layer, index, -1);
    }
}