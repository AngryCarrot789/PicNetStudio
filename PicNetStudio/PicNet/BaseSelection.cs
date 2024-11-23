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

namespace PicNetStudio.PicNet;

public abstract class BaseSelection {
    // TODO: BeginUnsafeClip, EndUnsafeClip
    // using offscreen buffers, EndUnsafeClip draws it into the PNBitmap in the clipping region
    
    public abstract void ApplyClip(PNBitmap bitmap);

    public abstract void FinishClip(PNBitmap bitmap);
}

public class RectangleSelection : BaseSelection {
    private int counter;
    
    public int Left { get; }
    public int Top { get; }
    public int Right { get; }
    public int Bottom { get; }

    public int Width => this.Right - this.Left;

    public int Height => this.Bottom - this.Top;
    
    public SKRectI Rect => new SKRectI(this.Left, this.Top, this.Right, this.Bottom);

    public RectangleSelection(int left, int top, int right, int bottom) {
        this.Left = left;
        this.Top = top;
        this.Right = right;
        this.Bottom = bottom;
    }

    public override void ApplyClip(PNBitmap bitmap) {
        this.counter = bitmap.Canvas!.Save();
        bitmap.Canvas!.ClipRect(new SKRect(this.Left, this.Top, this.Right, this.Bottom));
    }

    public override void FinishClip(PNBitmap bitmap) {
        bitmap.Canvas!.RestoreToCount(this.counter);
    }
}