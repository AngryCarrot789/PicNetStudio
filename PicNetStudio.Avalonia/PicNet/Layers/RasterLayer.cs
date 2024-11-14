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

using SkiaSharp;

namespace PicNetStudio.Avalonia.PicNet.Layers;

public class RasterLayer : BaseVisualLayer {
    public PNBitmap Bitmap { get; }

    public RasterLayer() {
        this.Bitmap = new PNBitmap();
        this.UsesCustomOpacityCalculation = true;
    }

    protected override void LoadDataIntoClone(BaseLayerTreeObject clone) {
        base.LoadDataIntoClone(clone);

        RasterLayer raster = (RasterLayer) clone;
        if (this.Bitmap.HasPixels) {
            if (!raster.Bitmap.HasPixels || raster.Bitmap.Size != this.Bitmap.Size)
                raster.Bitmap.InitialiseBitmap(this.Bitmap.Size);
            raster.Bitmap.Paste(this.Bitmap);
        }
    }

    public override void RenderLayer(ref RenderContext ctx) {
        if (this.Bitmap.HasPixels) {
            SKColor colour = RenderUtils.BlendAlpha(SKColors.White, this.Opacity);
            using SKPaint paint = new SKPaint();
            paint.Color = colour;
            paint.BlendMode = this.BlendMode;
            ctx.Canvas.DrawBitmap(this.Bitmap.Bitmap, 0, 0, paint);
        }
    }
}