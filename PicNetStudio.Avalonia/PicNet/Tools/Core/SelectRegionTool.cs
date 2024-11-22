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

namespace PicNetStudio.Avalonia.PicNet.Tools.Core;

/// <summary>
/// A tool which crates a selection region in the canvas
/// </summary>
public class SelectRegionTool : BaseCanvasTool {
    private Point clickPos;

    public SelectRegionTool() {
    }

    public override bool OnCursorPressed(Document document, SKPointD pos, SKPointD absPos, int count, EnumCursorType cursor, KeyModifiers modifiers) {
        if (cursor != EnumCursorType.Primary || modifiers != KeyModifiers.None) {
            return false;
        }

        this.clickPos = new Point(pos.X, pos.Y);
        document.Canvas.SelectionRegion = CreateSelection(this.clickPos, new Point(pos.X, pos.Y));
        return true;
    }

    public override bool OnCursorReleased(Document document, SKPointD pos, SKPointD absPos, EnumCursorType cursor, KeyModifiers modifiers) {
        if (cursor == EnumCursorType.Secondary) {
            document.Canvas.SelectionRegion = null;
        }
        
        return true;
    }

    public override bool OnCursorMoved(Document document, SKPointD pos, SKPointD absPos, EnumCursorType cursorMask) {
        if ((cursorMask & EnumCursorType.Primary) != 0) {
            document.Canvas.SelectionRegion = CreateSelection(this.clickPos, new Point(pos.X, pos.Y));
        }
        
        return true;
    }

    private static RectangleSelection CreateSelection(Point a, Point b) {
        Point min = Min(a, b);
        Point max = Max(a, b);

        int x1 = (int) Math.Floor(min.X), x2 = (int) Math.Ceiling(max.X);
        int y1 = (int) Math.Floor(min.Y), y2 = (int) Math.Ceiling(max.Y);
        return new RectangleSelection(x1, y1, x2, y2);
    }

    private static Point Min(Point a, Point b) => new(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y));
    private static Point Max(Point a, Point b) => new(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y));
}