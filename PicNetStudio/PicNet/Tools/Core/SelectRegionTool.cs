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

using PicNetStudio.Utils;

namespace PicNetStudio.PicNet.Tools.Core;

/// <summary>
/// A tool which crates a selection region in the canvas
/// </summary>
public class SelectRegionTool : BaseCanvasTool {
    private SKPointD? clickPos;

    public SelectRegionTool() {
    }

    public override bool OnCursorPressed(Document document, SKPointD relPos, SKPointD absPos, int count, EnumCursorType cursor, ModifierKeys modifiers) {
        if (cursor == EnumCursorType.Primary && modifiers == ModifierKeys.None) {
            document.Canvas.SelectionRegion = CreateSelection((this.clickPos = new SKPointD(absPos.X, absPos.Y)).Value, new SKPointD(absPos.X, absPos.Y));
            return true;
        }

        return false;
    }

    public override bool OnCursorReleased(Document document, SKPointD relPos, SKPointD absPos, EnumCursorType cursor, ModifierKeys modifiers) {
        switch (cursor) {
            case EnumCursorType.Primary: this.clickPos = null;
                break;
            case EnumCursorType.Secondary: document.Canvas.SelectionRegion = null;
                break;
            default: return false;
        }

        return true;
    }

    public override bool OnCursorMoved(Document document, SKPointD relPos, SKPointD absPos, EnumCursorType cursorMask) {
        if ((cursorMask & EnumCursorType.Primary) != 0 && this.clickPos.HasValue) {
            document.Canvas.SelectionRegion = CreateSelection(this.clickPos.Value, new SKPointD(absPos.X, absPos.Y));
        }
        
        return true;
    }

    private static RectangleSelection CreateSelection(SKPointD a, SKPointD b) {
        SKPointD min = Min(a, b);
        SKPointD max = Max(a, b);

        int x1 = (int) Math.Floor(min.X), x2 = (int) Math.Ceiling(max.X);
        int y1 = (int) Math.Floor(min.Y), y2 = (int) Math.Ceiling(max.Y);
        return new RectangleSelection(x1, y1, x2, y2);
    }

    private static SKPointD Min(SKPointD a, SKPointD b) => new(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y));
    private static SKPointD Max(SKPointD a, SKPointD b) => new(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y));
}