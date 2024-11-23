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

namespace PicNetStudio.PicNet.Tools.Core;

/// <summary>
/// The flood fill tool
/// </summary>
public class FloodFillTool : BaseDrawingTool {
    public FloodFillTool() {
    }

    public override bool OnCursorPressed(Document document, SKPointD relPos, SKPointD absPos, int count, EnumCursorType cursor, ModifierKeys modifiers) {
        if (base.OnCursorPressed(document, relPos, absPos, count, cursor, modifiers))
            return true;

        if ((cursor != EnumCursorType.Primary && cursor != EnumCursorType.Secondary) || modifiers != ModifierKeys.None)
            return false;

        if (document.Canvas.ActiveLayerTreeObject is RasterLayer bitmapLayer) {
            SKColor replaceColour = cursor == EnumCursorType.Primary ? document.Editor!.PrimaryColour : document.Editor!.SecondaryColour;
            DrawPixels(bitmapLayer, (int) Math.Floor(relPos.X), (int) Math.Floor(relPos.Y), (uint) replaceColour, document.Canvas.SelectionRegion);

            bitmapLayer.InvalidateVisual();
            return true;
        }

        return false;
    }

    public static unsafe void DrawPixels(RasterLayer layer, int fillX, int fillY, uint bgraReplace, BaseSelection? selection) {
        PNBitmap bitmap = layer.Bitmap;
        if (bitmap.Canvas == null)
            return;

        // ExtFloodFill is better maybe???
        RectangleSelection? rectangle = selection as RectangleSelection;
        int saveCount = bitmap.Canvas.Save();

        SKMatrix matrix = layer.AbsoluteInverseTransformationMatrix;
        int nBmpW = bitmap.Bitmap!.Width;
        int nBmpH = bitmap.Bitmap!.Height;
        int minX = 0, minY = 0, maxX = nBmpW, maxY = nBmpH;
        SKPath? selPath = null;
        if (rectangle != null) {
            // use fast mode if possible
            if (matrix.IsIdentity) {
                minX = rectangle.Left;
                minY = rectangle.Top;
                maxX = Math.Min(maxX, rectangle.Right);
                maxY = Math.Min(maxY, rectangle.Bottom);
            }
            else {
                bitmap.Canvas.SetMatrix(bitmap.Canvas.TotalMatrix.PostConcat(matrix));
                selPath = new SKPath();
                selPath.AddRect(new SKRect(rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom));
                selPath.Transform(layer.AbsoluteInverseTransformationMatrix);
            }
        }

        if (fillX < minX || fillY < minY || fillX >= maxX || fillY >= maxY)
            goto End;

        uint* colourData = (uint*) bitmap.Bitmap!.GetPixels();
        uint bgraTarget = *(colourData + (fillY * nBmpW + fillX));
        if (bgraTarget == bgraReplace)
            goto End;

        Queue<SKPointI> queue = new Queue<SKPointI>(2048);
        queue.Enqueue(new SKPointI(fillX, fillY));
        while (queue.Count > 0) {
            SKPointI p = queue.Dequeue();
            if (p.X < minX || p.X >= maxX || p.Y < minY || p.Y >= maxY)
                continue;

            if (selPath != null && !selPath.Contains(p.X, p.Y))
                continue;     
            
            uint* pColour = colourData + (p.Y * nBmpW + p.X);
            if (*pColour == bgraTarget) {
                *pColour = bgraReplace;
                ScanNeighbours(p.X, p.Y, maxX, maxY, queue);
            }
        }

        End:
        bitmap.Canvas.RestoreToCount(saveCount);
        selPath?.Dispose();
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