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

using SkiaSharp;

namespace PicNetStudio.Avalonia.PicNet.Tools.Core;

public class PencilTool : BaseDiameterTool {
    public PencilTool() {
        this.CanDrawSecondaryColour = true;
    }

    public override void DrawPixels(PNBitmap bitmap, Document document, double x, double y, bool isPrimaryColour) {
        if (bitmap.Canvas == null)
            return;

        using SKPaint paint = new SKPaint();
        paint.Color = isPrimaryColour ? document.Editor!.PrimaryColour : document.Editor!.SecondaryColour;

        double diameter = this.Diameter;
        double realX = x - (diameter / 2);
        double realY = y - (diameter / 2);
        
        bitmap.Canvas.DrawRect((float) realX, (float) realY, (float) diameter, (float) diameter, paint);
    }
}