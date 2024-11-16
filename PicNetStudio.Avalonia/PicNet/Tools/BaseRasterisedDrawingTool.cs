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
using PicNetStudio.Avalonia.PicNet.Layers;

namespace PicNetStudio.Avalonia.PicNet.Tools;

/// <summary>
/// The base class for a tool that draws onto a rasterised layer when the mouse is pressed and dragged around
/// </summary>
public abstract class BaseRasterisedDrawingTool : BaseDrawingTool {
    private bool keepDrawing;
    private Point lastDragDrawPoint;

    /// <summary>
    /// The recommended spacing interval that should be used to call the
    /// <see cref="DrawPixels"/> function when the user is drag-drawing
    /// (as apposed to a single click). General recommendation is 1.0 for a 1-pixel brush
    /// </summary>
    public abstract double SpacingFeedback { get; }

    /// <summary>
    /// Gets or sets a property indicating this drawing tool allows drawing with both primary and secondary cursor inputs (left and right mouse)
    /// </summary>
    protected bool CanDrawSecondaryColour { get; set; }
    
    protected bool BypassClipping { get; set; }

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
        bool ret = DrawEvent(this, document, x, y, cursor == EnumCursorType.Primary);
        this.keepDrawing = true;
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

        return DrawEvent(this, document, x, y, (cursorMask & EnumCursorType.Primary) != 0);
    }

    public static bool DrawEvent(BaseRasterisedDrawingTool tool, Document document, double x, double y, bool isPrimaryCursor) {
        if (!(document.Canvas.ActiveLayerTreeObject is RasterLayer bitmapLayer)) {
            return false;
        }

        PNBitmap bitmap = bitmapLayer.Bitmap;
        Point mPos = new Point(x, y);
        
        BaseSelection? selection = document.Canvas.SelectionRegion;
        if (selection != null && !tool.BypassClipping) {
            selection.ApplyClip(bitmap);
        }
        
        if (tool.keepDrawing) {
            double distance = GetDistance(tool.lastDragDrawPoint, mPos);

            if (distance > tool.SpacingFeedback) {
                int steps = (int) (distance / tool.SpacingFeedback);
                double deltaX = (mPos.X - tool.lastDragDrawPoint.X) / steps;
                double deltaY = (mPos.Y - tool.lastDragDrawPoint.Y) / steps;

                for (int i = 0; i < steps; i++) {
                    double interpolatedX = tool.lastDragDrawPoint.X + i * deltaX;
                    double interpolatedY = tool.lastDragDrawPoint.Y + i * deltaY;
                    tool.DrawPixels(bitmap, document, interpolatedX, interpolatedY, isPrimaryCursor);
                }
            }
        }

        tool.keepDrawing = true;
        tool.DrawPixels(bitmap, document, mPos.X, mPos.Y, isPrimaryCursor);
        if (selection != null && !tool.BypassClipping) {
            selection.FinishClip(bitmap);
        }
        
        document.Canvas.RaiseRenderInvalidated();
        tool.lastDragDrawPoint = mPos;
        return true;
    }

    private static double GetDistance(Point p1, Point p2) {
        double dx = p2.X - p1.X;
        double dy = p2.Y - p1.Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }
}