// 
// Copyright (c) 2023-2024 REghZy
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
using PicNetStudio.Avalonia.Utils;
using SkiaSharp;

namespace PicNetStudio.Avalonia.PicNet.Tools.Core;

/// <summary>
/// A tool which crates a selection region in the canvas
/// </summary>
public class SelectRegionTool : BaseCanvasTool {
    private PixelPoint lastStartPoint;
    
    public SelectRegionTool() {
    }

    public override bool OnCursorPressed(Document document, double x, double y, int count, EnumCursorType cursor, KeyModifiers modifiers) {
        if (cursor != EnumCursorType.Primary || modifiers != KeyModifiers.None) {
            return false;
        }
        
        this.lastStartPoint = new PixelPoint(Math.Max(0, Maths.Floor(x)), Math.Max(0, Maths.Floor(y)));
        return true;
    }

    public override bool OnCursorReleased(Document document, double x, double y, EnumCursorType cursor, KeyModifiers modifiers) {
        if (cursor == EnumCursorType.Secondary) {
            document.Canvas.SelectionRegion = null;
        }
        
        return true;
    }

    public override bool OnCursorMoved(Document document, double x, double y, EnumCursorType cursorMask) {
        if ((cursorMask & EnumCursorType.Primary) != 0) {
            PixelPoint a = this.lastStartPoint;
            PixelPoint b = new PixelPoint(Math.Max(0, Maths.Ceil(x)), Math.Max(0, Maths.Ceil(y)));
            int width = Math.Abs(b.X - this.lastStartPoint.X);
            int height = Math.Abs(b.Y - this.lastStartPoint.Y);
            if (width != 0 && height != 0) {
                document.Canvas.SelectionRegion = new RectangleSelection(
                    new SKPointI(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y)), 
                    new SKPointI(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y)));
            }
        }
        
        return true;
    }
}