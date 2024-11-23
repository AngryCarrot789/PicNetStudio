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

using PicNetStudio.PicNet.Layers.Core;
using PicNetStudio.Utils;
using SkiaSharp;

namespace PicNetStudio.PicNet;

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

    public readonly RenderVisibilityFlag VisibilityFlag;

    /// <summary>
    /// True when invalidating the entire canvas, False when just
    /// re-drawing the root level layers and using cached bitmaps.
    /// This is just a hint and may be ignored
    /// </summary>
    public readonly bool FullInvalidateHint;
    
    public RenderContext(Canvas myCanvas, SKSurface surface, RenderVisibilityFlag visibilityFlag, bool fullInvalidateHint) {
        this.FullInvalidateHint = fullInvalidateHint;
        this.Surface = surface;
        this.Canvas = surface.Canvas;
        this.MyCanvas = myCanvas;
        this.VisibilityFlag = visibilityFlag;
    }

    public bool IsLayerVisibleToRender(BaseVisualLayer layer) {
        switch (this.VisibilityFlag) {
            case RenderVisibilityFlag.ExportOnly when !layer.IsExportVisible:
            case RenderVisibilityFlag.PreviewOnly when !layer.IsVisible:
                return false;
            default: return !DoubleUtils.AreClose(layer.Opacity, 0.0);
        }
    }
}

public enum RenderVisibilityFlag {
    /// <summary>
    /// Only renders layers that are preview-visible (<see cref="BaseVisualLayer.IsVisible"/>)
    /// </summary>
    PreviewOnly = 0,
    /// <summary>
    /// Only renders layers that are export-visible (<see cref="BaseVisualLayer.IsExportVisible"/>)
    /// </summary>
    ExportOnly = 1,
    /// <summary>
    /// Force renders layers regardless of the visibility. Opacity is not ignored, so a 0 opacity will make it invisible 
    /// </summary>
    Ignored = 2
}