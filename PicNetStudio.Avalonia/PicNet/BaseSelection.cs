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

namespace PicNetStudio.Avalonia.PicNet;

public abstract class BaseSelection {
    public abstract void ApplyClip(PNBitmap bitmap);

    public abstract void FinishClip(PNBitmap bitmap);
}

public class RectangleSelection : BaseSelection {
    /// <summary>
    /// The minimum point
    /// </summary>
    public SKPointI Min { get; }
    
    /// <summary>
    /// The maximum point
    /// </summary>
    public SKPointI Max { get; }

    public SKRectI Rect => new SKRectI(this.Min.X, this.Min.Y, this.Max.X, this.Max.Y);

    public int Width => this.Max.X - this.Min.X;
    
    public int Height => this.Max.Y - this.Min.Y;

    public RectangleSelection(SKPointI min, SKPointI max) {
        this.Min = min;
        this.Max = max;
    }

    public override void ApplyClip(PNBitmap bitmap) {
        bitmap.Canvas!.Save();
        bitmap.Canvas!.ClipRect(this.Rect);
    }

    public override void FinishClip(PNBitmap bitmap) {
        bitmap.Canvas!.Restore();
    }
}