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
using Avalonia;
using PicNetStudio.Avalonia.PicNet.Layers;
using PicNetStudio.Avalonia.PicNet.Layers.Core;
using PicNetStudio.Avalonia.RBC;
using SkiaSharp;

namespace PicNetStudio.Avalonia.PicNet;

public delegate void CanvasRenderInvalidatedEventHandler(Canvas canvas);

public delegate void CanvasLayerIndexChangedEventHandler(Canvas canvas, BaseLayerTreeObject layer, int oldIndex, int newIndex);

public delegate void CanvasActiveLayerChangedEventHandler(Canvas canvas, BaseLayerTreeObject? oldActiveLayerTreeObject, BaseLayerTreeObject? newActiveLayerTreeObject);

public delegate void CanvasSizeChangedEventHandler(Canvas canvas, PixelSize oldSize, PixelSize newSize);

public delegate void CanvasSelectionRegionChangedEventHandler(Canvas sender);

/// <summary>
/// Represents the canvas for a document. This contains layer information among other data
/// </summary>
public class Canvas {
    private PixelSize size;
    private BaseLayerTreeObject? activeLayerTreeObject;
    private BaseSelection? selectionRegion;
    private bool fullInvalidate;
    private BaseVisualLayer? mySoloLayer;

    /// <summary>
    /// The document that owns this canvas
    /// </summary>
    public Document Document { get; }

    /// <summary>
    /// Gets the folder that stores this canvas' layer hierarchy
    /// </summary>
    public CompositionLayer RootComposition { get; }

    public BaseLayerTreeObject? ActiveLayerTreeObject {
        get => this.activeLayerTreeObject;
        set {
            BaseLayerTreeObject? oldActiveLayerTreeObject = this.activeLayerTreeObject;
            if (oldActiveLayerTreeObject == value)
                return;

            this.activeLayerTreeObject = value;
            this.ActiveLayerChanged?.Invoke(this, oldActiveLayerTreeObject, value);
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

    public BaseSelection? SelectionRegion {
        get => this.selectionRegion;
        set {
            if (this.selectionRegion == value)
                return;

            this.selectionRegion = value;
            this.SelectionRegionChanged?.Invoke(this);
            this.RaiseRenderInvalidated(false);
        }
    }
    
    /// <summary>
    /// An event fired when the canvas has changed and any UI needs redrawing due to the pixels changing
    /// </summary>
    public event CanvasRenderInvalidatedEventHandler? RenderInvalidated;

    /// <summary>
    /// An event fired when the active layer changes
    /// </summary>
    public event CanvasActiveLayerChangedEventHandler? ActiveLayerChanged;

    /// <summary>
    /// An event fired when the size of the canvas changes, e.g. from cropping
    /// </summary>
    public event CanvasSizeChangedEventHandler? SizeChanged;

    /// <summary>
    /// An event fired when our selection region changes
    /// </summary>
    public event CanvasSelectionRegionChangedEventHandler? SelectionRegionChanged;

    /// <summary>
    /// Gets the layer that is the current "solo" layer, that is, the only layer that can be
    /// drawn and every other layer is hidden. This is for the preview only, not export
    /// </summary>
    public BaseVisualLayer? SoloLayer => this.mySoloLayer;
    
    // TODO: two bitmaps containing pre-rendered layers before and after active layer, as to make rendering faster 

    public Canvas(Document document) {
        this.Document = document;
        this.RootComposition = CompositionLayer.InternalCreateCanvasRoot(this);
        this.RootComposition.RenderInvalidated += this.OnRootRenderInvalidated;
        this.size = new PixelSize(500, 500);
    }

    private void OnRootRenderInvalidated(BaseVisualLayer layer) {
        this.RenderInvalidated?.Invoke(this);
    }
    
    public void WriteToRBE(RBEDictionary data) {
        data.SetStruct("CanvasSize", this.size);
        RBEList list = data.CreateList("RootLayers");
        foreach (BaseLayerTreeObject layer in this.RootComposition.Layers) {
            BaseLayerTreeObject.WriteSerialisedWithId(list.AddDictionary(), layer);
        }
    }
    
    public void ReadFromRBE(RBEDictionary data) {
        this.size = data.GetStruct<PixelSize>("CanvasSize");
        RBEList list = data.GetList("RootLayers");
        foreach (RBEDictionary dictionary in list.Cast<RBEDictionary>()) {
            this.RootComposition.AddLayer(BaseLayerTreeObject.ReadSerialisedWithId(dictionary));
        }
    }

    // QtBitmapEditor -> Project::paintEvent
    public void Render(SKSurface surface) {
        RenderContext args = new RenderContext(this, surface, RenderVisibilityFlag.PreviewOnly, this.fullInvalidate);
        LayerRenderer.RenderCanvas(ref args, false);
    }

    public void RaiseRenderInvalidated(bool fullInvalidate = true) {
        if (fullInvalidate) {
            this.RootComposition.InvalidateVisual();
        }
        else {
            this.RenderInvalidated?.Invoke(this);
        }
    }
    
    internal static void InternalSetSoloLayer(Canvas canvas, BaseVisualLayer? layer) {
        BaseVisualLayer? newSoloLayer = layer;
        if (newSoloLayer != null) { // we can set the solo layer to false if we want to clear it
            // Should be an impossible to reach exception unless
            // someone is poking around with the internal methods
            if (!ReferenceEquals(newSoloLayer.Canvas, canvas))
                throw new InvalidOperationException("Layer is not in this canvas");
            
            // Layer set IsSolo to false, so double check everything's in
            // order then make it look like we're clearing the solo layer
            if (!newSoloLayer.IsSolo) {
                if (BaseVisualLayer.IsSoloParameter.IsValueChanging(newSoloLayer))
                    throw new InvalidOperationException("Layer's " + BaseVisualLayer.IsSoloParameter.Name + " property is false but currently changing.");
             
                newSoloLayer = null;
            }
        }
        
        // Set IsSolo to previous solo layer. If the current solo layer is being set
        // to not solo, then don't worry since it's already false at this point
        if (canvas.mySoloLayer != null && !ReferenceEquals(canvas.mySoloLayer, layer)) {
            // assert oldSoloLayer.IsSolo == true maybe?
            canvas.mySoloLayer.IsSolo = false;
        }
        
        // Update solo layer and raise render invalidated
        canvas.mySoloLayer = newSoloLayer;
        canvas.RaiseRenderInvalidated(false); // not full invalidation, since nothing really changed, pixel wise
    }
}