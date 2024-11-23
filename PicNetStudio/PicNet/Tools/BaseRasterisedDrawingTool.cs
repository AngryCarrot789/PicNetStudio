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

using PicNetStudio.DataTransfer;
using PicNetStudio.PicNet.Layers.Core;
using PicNetStudio.Utils;
using PicNetStudio.Utils.Accessing;

namespace PicNetStudio.PicNet.Tools;

/// <summary>
/// The base class for a tool that draws onto a rasterised layer when the mouse is pressed and dragged around
/// </summary>
public abstract class BaseRasterisedDrawingTool : BaseDrawingTool {
    // best result is Diameter / 8
    public static readonly DataParameterFloat GapParameter = DataParameter.Register(new DataParameterFloat(typeof(BaseRasterisedDrawingTool), nameof(Gap), 2.0F, 0.5F, 100.0F, ValueAccessors.Reflective<float>(typeof(BaseRasterisedDrawingTool), nameof(gap))));
    public static readonly DataParameterBool IsGapAutomaticParameter = DataParameter.Register(new DataParameterBool(typeof(BaseRasterisedDrawingTool), nameof(IsGapAutomatic), true, ValueAccessors.Reflective<bool>(typeof(BaseRasterisedDrawingTool), nameof(isGapAutomatic))));

    private bool keepDrawing;
    private SKPointD lastDragPos;
    private float gap;
    private bool isGapAutomatic;

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
        this.gap = GapParameter.GetDefaultValue(this);
        this.isGapAutomatic = IsGapAutomaticParameter.GetDefaultValue(this);
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

    public override bool OnCursorPressed(Document document, SKPointD relPos, SKPointD absPos, int count, EnumCursorType cursor, ModifierKeys modifiers) {
        if (base.OnCursorPressed(document, relPos, absPos, count, cursor, modifiers))
            return true;

        // Only allow without modifiers pressed to allow the canvas to be moved around with ALT+LMB
        if ((cursor != EnumCursorType.Primary && cursor != EnumCursorType.Secondary) || modifiers != ModifierKeys.None)
            return false;

        if (cursor == EnumCursorType.Secondary && !this.CanDrawSecondaryColour)
            return false;

        this.keepDrawing = false;
        return DrawEventV2(this, document, relPos.X, relPos.Y, cursor == EnumCursorType.Primary);
    }

    public override bool OnCursorReleased(Document document, SKPointD relPos, SKPointD absPos, EnumCursorType cursor, ModifierKeys modifiers) {
        if (cursor == EnumCursorType.Primary)
            this.keepDrawing = false;
        return base.OnCursorReleased(document, relPos, absPos, cursor, modifiers);
    }

    public override bool OnCursorMoved(Document document, SKPointD relPos, SKPointD absPos, EnumCursorType cursorMask) {
        if (base.OnCursorMoved(document, relPos, absPos, cursorMask))
            return true;

        // return if not primary cursor and this brush cannot use secondary, or, it can draw secondary but it wasn't pressed
        if ((cursorMask & EnumCursorType.Primary) == 0 && (!this.CanDrawSecondaryColour || (cursorMask & EnumCursorType.Secondary) == 0))
            return false;

        return DrawEventV2(this, document, relPos.X, relPos.Y, (cursorMask & EnumCursorType.Primary) != 0);
    }

    public static bool DrawEventV2(BaseRasterisedDrawingTool tool, Document document, double x, double y, bool isPrimaryCursor) {
        if (!(document.Canvas.ActiveLayerTreeObject is RasterLayer bitmapLayer)) {
            return false;
        }

        PNBitmap bitmap = bitmapLayer.Bitmap;
        if (!bitmap.IsInitialised) {
            return false;
        }

        int saveCount = bitmap.Canvas.Save();
        BaseSelection? selection = document.Canvas.SelectionRegion;
        bool useClip = selection != null && !tool.BypassClipping;
        if (useClip) {
            bitmap.Canvas.SetMatrix(bitmap.Canvas.TotalMatrix.PostConcat(bitmapLayer.AbsoluteInverseTransformationMatrix));
            selection!.ApplyClip(bitmap);
            bitmap.Canvas.SetMatrix(bitmap.Canvas.TotalMatrix.PreConcat(bitmapLayer.AbsoluteTransformationMatrix));
        }

        Exception? error = null;
        try {
            if (tool.keepDrawing) {
                SKPointD lastPos = tool.lastDragPos;
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
            }
            else {
                tool.keepDrawing = true;
                tool.DrawPixels(bitmap, document, x, y, isPrimaryCursor);
            }
        }
        catch (Exception e) {
            error = e;
        }
        finally {
            if (useClip) {
                try {
                    selection!.FinishClip(bitmap);
                }
                catch (Exception e) {
                    error = error == null ? e : new AggregateException("Errors occurred while drawing tool", error, new Exception("Exception while finishing selection clip"));
                }
            }   
            
            bitmap.Canvas.RestoreToCount(saveCount);
        }

        tool.lastDragPos = new SKPointD(x, y);
        if (error != null)
            throw error; // dunno why i'm throwing this after setting lastDragPos, not like it matters since it will crash the app...

        bitmapLayer.InvalidateVisual();
        return true;
    }
}