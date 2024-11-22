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
using Avalonia.Input;
using PicNetStudio.Avalonia.DataTransfer;
using PicNetStudio.Avalonia.PicNet.Layers.Core;
using PicNetStudio.Avalonia.Utils;
using PicNetStudio.Avalonia.Utils.Accessing;

namespace PicNetStudio.Avalonia.PicNet.Tools;

/// <summary>
/// The base class for a tool that draws onto a rasterised layer when the mouse is pressed and dragged around
/// </summary>
public abstract class BaseRasterisedDrawingTool : BaseDrawingTool {
    // best result is Diameter / 8
    public static readonly DataParameterFloat GapParameter = DataParameter.Register(new DataParameterFloat(typeof(BaseRasterisedDrawingTool), nameof(Gap), 2.0F, 0.5F, 100.0F, ValueAccessors.Reflective<float>(typeof(BaseRasterisedDrawingTool), nameof(gap))));
    public static readonly DataParameterBool IsGapAutomaticParameter = DataParameter.Register(new DataParameterBool(typeof(BaseRasterisedDrawingTool), nameof(IsGapAutomatic), true, ValueAccessors.Reflective<bool>(typeof(BaseRasterisedDrawingTool), nameof(isGapAutomatic))));

    private bool keepDrawing;
    private Point lastDragPos;
    private float gap = GapParameter.DefaultValue;
    private bool isGapAutomatic = IsGapAutomaticParameter.DefaultValue;

    /// <summary>
    /// Gets or sets a property indicating this drawing tool allows drawing with both primary and secondary cursor inputs (left and right mouse)
    /// </summary>
    protected bool CanDrawSecondaryColour { get; set; }

    /// <summary>
    /// Gets or sets whether to bypass automatic clipping at a canvas-level when drawing with this
    /// tool. Setting this to true means the tool will implement its own clip processing
    /// </summary>
    protected bool BypassClipping { get; set; }

    public float Gap {
        get => this.gap;
        set => DataParameter.SetValueHelper(this, GapParameter, ref this.gap, value);
    }

    public bool IsGapAutomatic {
        get => this.isGapAutomatic;
        set => DataParameter.SetValueHelper(this, IsGapAutomaticParameter, ref this.isGapAutomatic, value);
    }
    
    protected BaseRasterisedDrawingTool() {
    }

    /// <summary>
    /// Performs a draw operation at the specific pixels
    /// </summary>
    /// <param name="bitmap">The bitmap to draw into</param>
    /// <param name="document">The document whose canvas is being drawn into</param>
    /// <param name="x">The X position. May require post-processing for inter-pixel drawing</param>
    /// <param name="y">The Y position. May require post-processing for inter-pixel drawing</param>
    /// <param name="isPrimaryColour">
    ///     True when drawing with primary cursor, false when <see cref="CanDrawSecondaryColour"/> is true and the secondary cursor is pressed
    /// </param>
    public abstract void DrawPixels(PNBitmap bitmap, Document document, double x, double y, bool isPrimaryColour);

    public override bool OnCursorPressed(Document document, double x, double y, int count, EnumCursorType cursor, KeyModifiers modifiers) {
        if (base.OnCursorPressed(document, x, y, count, cursor, modifiers))
            return true;

        // Only allow without modifiers pressed to allow the canvas to be moved around with ALT+LMB
        if ((cursor != EnumCursorType.Primary && cursor != EnumCursorType.Secondary) || modifiers != KeyModifiers.None)
            return false;

        if (cursor == EnumCursorType.Secondary && !this.CanDrawSecondaryColour)
            return false;

        this.keepDrawing = false;
        bool ret = DrawEventV2(this, document, x, y, cursor == EnumCursorType.Primary);
        return ret;
    }

    public override bool OnCursorReleased(Document document, double x, double y, EnumCursorType cursor, KeyModifiers modifiers) {
        if (cursor == EnumCursorType.Primary)
            this.keepDrawing = false;
        return base.OnCursorReleased(document, x, y, cursor, modifiers);
    }

    public override bool OnCursorMoved(Document document, double x, double y, EnumCursorType cursorMask) {
        if (base.OnCursorMoved(document, x, y, cursorMask))
            return true;

        // return if not primary cursor and this brush cannot use secondary, or, it can draw secondary but it wasn't pressed
        if ((cursorMask & EnumCursorType.Primary) == 0 && (!this.CanDrawSecondaryColour || (cursorMask & EnumCursorType.Secondary) == 0))
            return false;

        return DrawEventV2(this, document, x, y, (cursorMask & EnumCursorType.Primary) != 0);
    }

    public static bool DrawEventV2(BaseRasterisedDrawingTool tool, Document document, double x, double y, bool isPrimaryCursor) {
        if (!(document.Canvas.ActiveLayerTreeObject is RasterLayer bitmapLayer)) {
            return false;
        }

        PNBitmap bitmap = bitmapLayer.Bitmap;
        if (!bitmap.IsInitialised) {
            return false;
        }
        
        BaseSelection? selection = document.Canvas.SelectionRegion;
        bool useClip = selection != null && !tool.BypassClipping;
        if (useClip) {
            selection!.ApplyClip(bitmap);
        }

        if (tool.keepDrawing) {
            Point lastPos = tool.lastDragPos;
            double dx = x - lastPos.X;
            double dy = y - lastPos.Y;
            double distance = Math.Sqrt(dx * dx + dy * dy);
            double gap = tool.Gap;
            if (distance < gap) {
                return true;
            }

            double remainingDistance = distance;
            double curX = lastPos.X;
            double curY = lastPos.Y;
            double dirX = dx / distance;
            double dirY = dy / distance;
            while (DoubleUtils.GreaterThanOrClose(remainingDistance, gap)) {
                curX += dirX * gap;
                curY += dirY * gap;
                tool.DrawPixels(bitmap, document, curX, curY, isPrimaryCursor);
                remainingDistance -= gap;
            }

            goto EndFirstDraw;
        }

        tool.keepDrawing = true;
        tool.DrawPixels(bitmap, document, x, y, isPrimaryCursor);
        EndFirstDraw:
        
        if (useClip) {
            selection!.FinishClip(bitmap);
        }

        bitmapLayer.InvalidateVisual();
        tool.lastDragPos = new Point(x, y);
        return true;
    }
}