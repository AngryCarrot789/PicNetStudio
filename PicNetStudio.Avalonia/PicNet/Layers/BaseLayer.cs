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

using SkiaSharp;

namespace PicNetStudio.Avalonia.PicNet.Layers;

public delegate void BaseLayerCanvasChangedEventHandler(BaseLayer layer, Canvas? oldCanvas, Canvas? newCanvas);

/// <summary>
/// The base class for a layer in a canvas
/// </summary>
public abstract class BaseLayer {
    public Canvas? Canvas { get; private set; }

    public event BaseLayerCanvasChangedEventHandler? CanvasChanged;

    protected BaseLayer() {
    }
    
    public abstract void Render(SKSurface surface);

    internal static void InternalSetCanvas(BaseLayer layer, Canvas? canvas) {
        Canvas? oldCanvas = layer.Canvas;
        layer.Canvas = canvas;
        layer.CanvasChanged?.Invoke(layer, oldCanvas, canvas);
    }
}