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

namespace PicNetStudio.Avalonia.PicNet;

public readonly struct RenderContext {
    /// <summary>
    /// The surface used to draw things
    /// </summary>
    public readonly SKSurface Surface;

    /// <summary>
    /// Our <see cref="Surface"/>'s canvas
    /// </summary>
    public readonly SKCanvas Canvas;

    public readonly Canvas MyCanvas;

    public readonly bool IsExporting;

    /// <summary>
    /// True when invalidating the entire canvas, False when just
    /// re-drawing the root level layers and using cached bitmaps.
    /// This is just a hint and may be ignored
    /// </summary>
    public readonly bool FullInvalidateHint;
    
    public RenderContext(Canvas myCanvas, SKSurface surface, bool isExport, bool fullInvalidateHint) {
        this.FullInvalidateHint = fullInvalidateHint;
        this.Surface = surface;
        this.Canvas = surface.Canvas;
        this.MyCanvas = myCanvas;
        this.IsExporting = isExport;
    }
}