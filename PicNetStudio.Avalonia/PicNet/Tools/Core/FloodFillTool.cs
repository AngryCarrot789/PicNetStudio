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
using System.Collections.Generic;
using Avalonia.Input;
using PicNetStudio.Avalonia.PicNet.Layers.Core;
using SkiaSharp;

namespace PicNetStudio.Avalonia.PicNet.Tools.Core;

/// <summary>
/// The flood fill tool
/// </summary>
public class FloodFillTool : BaseDrawingTool {
    public FloodFillTool() {
    }
    
    public override bool OnCursorPressed(Document document, double x, double y, int count, EnumCursorType cursor, KeyModifiers modifiers) {
        if (base.OnCursorPressed(document, x, y, count, cursor, modifiers))
            return true;

        if ((cursor != EnumCursorType.Primary && cursor != EnumCursorType.Secondary) || modifiers != KeyModifiers.None)
            return false;

        if (document.Canvas.ActiveLayerTreeObject is not RasterLayer bitmapLayer)
            return false;

        SKColor replaceColour = cursor == EnumCursorType.Primary ? document.Editor!.PrimaryColour : document.Editor!.SecondaryColour;
        DrawPixels(bitmapLayer.Bitmap, (int) Math.Floor(x), (int) Math.Floor(y), (uint) replaceColour, document.Canvas.SelectionRegion);
        
        bitmapLayer.InvalidateVisual();
        return true;
    }

    public static unsafe void DrawPixels(PNBitmap bitmap, int fillX, int fillY, uint bgraReplace, BaseSelection? selection) {
        if (bitmap.Canvas == null)
            return;

        // ExtFloodFill is better maybe???
        RectangleSelection? rectangle = selection as RectangleSelection;
        int nBmpW = bitmap.Bitmap!.Width;
        int nBmpH = bitmap.Bitmap!.Height;
        int minX = 0, minY = 0, maxX = nBmpW, maxY = nBmpH;
        if (rectangle != null) {
            minX = rectangle.Min.X;
            minY = rectangle.Min.Y;
            maxX = rectangle.Max.X;
            maxY = rectangle.Max.Y;
        }
        
        if (fillX < minX || fillX >= maxX || fillY < minY || fillY >= maxY)
            return;
        
        uint* colourData = (uint*) bitmap.Bitmap!.GetPixels();
        uint bgraTarget = *(colourData + (fillY * nBmpW + fillX));
        if (bgraTarget == bgraReplace)
            return;

        // Could maybe optimise this by making ScanNeighbour prioritise
        // certain directions, then maybe cache parts of the pointer offsets? eh
        // (specifically the multiplication p.Y * width)
        // Not sure if a ternary checking the last p.Y is faster than an integer multiplication though
        
        Queue<SKPointI> queue = new Queue<SKPointI>(2048);
        queue.Enqueue(new SKPointI(fillX, fillY));
        while (queue.Count > 0) {
            SKPointI p = queue.Dequeue();
            if (p.X < minX || p.X >= maxX || p.Y < minY || p.Y >= maxY)
                continue;
            
            uint* pColour = colourData + (p.Y * nBmpW + p.X);
            if (*pColour == bgraTarget) {
                *pColour = bgraReplace;
                ScanNeighbours(p.X, p.Y, maxX, maxY, queue);
            }
        }
    }

    // Probably need to replace 0s and -1s with minX/minY. But it still works anyway even with selection so :-)
    private static void ScanNeighbours(int pX, int pY, int nBmpW, int nBmpH, Queue<SKPointI> queue) {
        if (pX > 0 && pX <= nBmpW && pY > 0 && pY <= nBmpH)
            queue.Enqueue(new SKPointI(pX - 1, pY - 1));
        if (pX > 0 && pX <= nBmpW && pY >= 0 && pY < nBmpH)
            queue.Enqueue(new SKPointI(pX - 1, pY));
        if (pX > 0 && pX <= nBmpW && pY >= -1 && pY + 1 < nBmpH)
            queue.Enqueue(new SKPointI(pX - 1, pY + 1));
        if (pX >= 0 && pX < nBmpW && pY > 0 && pY <= nBmpH)
            queue.Enqueue(new SKPointI(pX, pY - 1));
        if (pX >= 0 && pX < nBmpW && pY >= -1 && pY + 1 < nBmpH)
            queue.Enqueue(new SKPointI(pX, pY + 1));
        if (pX >= -1 && pX + 1 < nBmpW && pY > 0 && pY <= nBmpH)
            queue.Enqueue(new SKPointI(pX + 1, pY - 1));
        if (pX >= -1 && pX + 1 < nBmpW && pY >= 0 && pY < nBmpH)
            queue.Enqueue(new SKPointI(pX + 1, pY));
        if (pX >= -1 && pX + 1 < nBmpW && pY >= -1 && pY + 1 < nBmpH)
            queue.Enqueue(new SKPointI(pX + 1, pY + 1));
    }
}