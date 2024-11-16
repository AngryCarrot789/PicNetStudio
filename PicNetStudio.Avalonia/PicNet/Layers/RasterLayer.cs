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

using PicNetStudio.Avalonia.DataTransfer;
using PicNetStudio.Avalonia.Utils;
using PicNetStudio.Avalonia.Utils.Accessing;
using SkiaSharp;

namespace PicNetStudio.Avalonia.PicNet.Layers;

public class RasterLayer : BaseVisualLayer {
    public static readonly DataParameterFloat ChannelRParameter = DataParameter.Register(new DataParameterFloat(typeof(RasterLayer), nameof(ChannelR), 1.0f, 0.0f, 1.0f, ValueAccessors.Reflective<float>(typeof(RasterLayer), nameof(channelR))));
    public static readonly DataParameterFloat ChannelGParameter = DataParameter.Register(new DataParameterFloat(typeof(RasterLayer), nameof(ChannelG), 1.0f, 0.0f, 1.0f, ValueAccessors.Reflective<float>(typeof(RasterLayer), nameof(channelG))));
    public static readonly DataParameterFloat ChannelBParameter = DataParameter.Register(new DataParameterFloat(typeof(RasterLayer), nameof(ChannelB), 1.0f, 0.0f, 1.0f, ValueAccessors.Reflective<float>(typeof(RasterLayer), nameof(channelB))));
    public static readonly DataParameterFloat ChannelAParameter = DataParameter.Register(new DataParameterFloat(typeof(RasterLayer), nameof(ChannelA), 1.0f, 0.0f, 1.0f, ValueAccessors.Reflective<float>(typeof(RasterLayer), nameof(channelA))));

    private float channelR = ChannelRParameter.DefaultValue;
    private float channelG = ChannelGParameter.DefaultValue;
    private float channelB = ChannelBParameter.DefaultValue;
    private float channelA = ChannelAParameter.DefaultValue;

    public float ChannelR {
        get => this.channelR;
        set => DataParameter.SetValueHelper(this, ChannelRParameter, ref this.channelR, value);
    }
    
    public float ChannelG {
        get => this.channelG;
        set => DataParameter.SetValueHelper(this, ChannelGParameter, ref this.channelG, value);
    }
    
    public float ChannelB {
        get => this.channelB;
        set => DataParameter.SetValueHelper(this, ChannelRParameter, ref this.channelB, value);
    }
    
    public float ChannelA {
        get => this.channelA;
        set => DataParameter.SetValueHelper(this, ChannelAParameter, ref this.channelA, value);
    }
    
    public PNBitmap Bitmap { get; }

    public RasterLayer() {
        this.Bitmap = new PNBitmap();
        this.UsesCustomOpacityCalculation = true;
    }

    static RasterLayer() {
        SetParameterAffectsRender(ChannelRParameter, ChannelGParameter, ChannelBParameter, ChannelAParameter);
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
            using SKPaint paint = new SKPaint();
            paint.Color = RenderUtils.BlendAlpha(SKColors.White, this.Opacity);
            paint.BlendMode = this.BlendMode;
            if (!DoubleUtils.AreClose(this.ChannelR, 1.0) || !DoubleUtils.AreClose(this.ChannelG, 1.0) || !DoubleUtils.AreClose(this.ChannelB, 1.0) || !DoubleUtils.AreClose(this.ChannelA, 1.0)) {
                float[] matrix = new float[] {
                    this.ChannelR, 0, 0, 0, 0,
                    0, this.ChannelG, 0, 0, 0,
                    0, 0, this.ChannelB, 0, 0,
                    0, 0, 0, this.ChannelA, 0
                };

                paint.ColorFilter = SKColorFilter.CreateColorMatrix(matrix);
            }

            ctx.Canvas.DrawBitmap(this.Bitmap.Bitmap, 0, 0, paint);
        }
    }
}