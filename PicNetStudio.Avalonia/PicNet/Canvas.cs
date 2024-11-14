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

using System.Collections.Generic;
using System.Linq;
using Avalonia;
using PicNetStudio.Avalonia.PicNet.Layers;
using SkiaSharp;

namespace PicNetStudio.Avalonia.PicNet;

public delegate void CanvasRenderInvalidatedEventHandler(Canvas canvas);

public delegate void CanvasLayerIndexChangedEventHandler(Canvas canvas, BaseLayerTreeObject layer, int oldIndex, int newIndex);

public delegate void CanvasActiveLayerChangedEventHandler(Canvas canvas, BaseLayerTreeObject? oldActiveLayerTreeObject, BaseLayerTreeObject? newActiveLayerTreeObject);

public delegate void CanvasSizeChangedEventHandler(Canvas canvas, PixelSize oldSize, PixelSize newSize);

/// <summary>
/// Represents the canvas for a document. This contains layer information among other data
/// </summary>
public class Canvas {
    private PixelSize size;
    private BaseLayerTreeObject? activeLayerTreeObject;

    /// <summary>
    /// The document that owns this canvas
    /// </summary>
    public Document Document { get; }

    /// <summary>
    /// Gets the folder that stores this canvas' layer hierarchy
    /// </summary>
    public CompositionLayer RootComposition { get; }

    /// <summary>
    /// Gets this canvas' layer selection manager, which stores which layers are selected
    /// </summary>
    public SelectionManager<BaseLayerTreeObject> LayerSelectionManager { get; }

    public BaseLayerTreeObject? ActiveLayerTreeObject {
        get => this.activeLayerTreeObject;
        set {
            BaseLayerTreeObject? oldActiveLayerTreeObject = this.activeLayerTreeObject;
            if (oldActiveLayerTreeObject == value)
                return;

            this.activeLayerTreeObject = value;
            this.ActiveLayerChanged?.Invoke(this, oldActiveLayerTreeObject, value);
            if (value != null)
                this.LayerSelectionManager.Select(value);
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
        this.RootComposition = CompositionLayer.InternalCreateCanvasRoot(this);
        this.LayerSelectionManager = new SelectionManager<BaseLayerTreeObject>();
        this.LayerSelectionManager.SelectionChanged += this.OnSelectionChanged;
        this.size = new PixelSize(500, 500);
    }

    private void OnSelectionChanged(SelectionManager<BaseLayerTreeObject> sender, IList<BaseLayerTreeObject>? olditems, IList<BaseLayerTreeObject>? newitems) {
        this.ActiveLayerTreeObject = sender.Selection.Count == 1 ? sender.Selection.FirstOrDefault() : null;
    }

    // QtBitmapEditor -> Project::paintEvent
    public void Render(SKSurface surface) {
        RenderContext args = new RenderContext(this, surface, false);
        LayerRenderer.RenderCanvas(ref args);
    }

    public void RaiseRenderInvalidated() {
        this.RenderInvalidated?.Invoke(this);
    }
}