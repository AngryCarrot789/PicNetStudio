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

public class RenderContext {
    /// <summary>
    /// The surface used to draw things
    /// </summary>
    public SKSurface Surface { get; }

    /// <summary>
    /// Our <see cref="Surface"/>'s canvas
    /// </summary>
    public SKCanvas Canvas { get; }

    public Canvas MyCanvas { get; }

    public RenderContext(Canvas myCanvas, SKSurface surface) {
        this.Surface = surface;
        this.Canvas = surface.Canvas;
        this.MyCanvas = myCanvas;
    }
}