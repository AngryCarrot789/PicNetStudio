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

using PicNetStudio.DataTransfer;
using PicNetStudio.Utils;
using PicNetStudio.Utils.Accessing;
using SkiaSharp;

namespace PicNetStudio.PicNet.Layers.Core;

/// <summary>
/// A pixel/image layer, which supports pixels being drawn directly into it
/// </summary>
public class RasterLayer : BaseVisualLayer {
    public static readonly DataParameterFloat ChannelRParameter = DataParameter.Register(new DataParameterFloat(typeof(RasterLayer), nameof(ChannelR), 1.0f, 0.0f, 1.0f, ValueAccessors.Reflective<float>(typeof(RasterLayer), nameof(channelR))));
    public static readonly DataParameterFloat ChannelGParameter = DataParameter.Register(new DataParameterFloat(typeof(RasterLayer), nameof(ChannelG), 1.0f, 0.0f, 1.0f, ValueAccessors.Reflective<float>(typeof(RasterLayer), nameof(channelG))));
    public static readonly DataParameterFloat ChannelBParameter = DataParameter.Register(new DataParameterFloat(typeof(RasterLayer), nameof(ChannelB), 1.0f, 0.0f, 1.0f, ValueAccessors.Reflective<float>(typeof(RasterLayer), nameof(channelB))));
    public static readonly DataParameterFloat ChannelAParameter = DataParameter.Register(new DataParameterFloat(typeof(RasterLayer), nameof(ChannelA), 1.0f, 0.0f, 1.0f, ValueAccessors.Reflective<float>(typeof(RasterLayer), nameof(channelA))));

    private float channelR;
    private float channelG;
    private float channelB;
    private float channelA;

    // TODO: Make channels an effect
    // and maybe use internal effect detection to improve performance;
    // RenderLayer uses the channels in the paint when drawing the bitmap

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
        this.channelR = ChannelRParameter.GetDefaultValue(this);
        this.channelG = ChannelGParameter.GetDefaultValue(this);
        this.channelB = ChannelBParameter.GetDefaultValue(this);
        this.channelA = ChannelAParameter.GetDefaultValue(this);
        this.Bitmap = new PNBitmap();
        this.UsesCustomOpacityCalculation = true;
    }

    private readonly struct UnmanagedBmpInfo {
        public readonly int width, height;
        public readonly SKColorType colourType;
        public readonly SKAlphaType alphaType;

        public SKImageInfo AsSkia => new SKImageInfo(this.width, this.height, this.colourType, this.alphaType);
        
        public UnmanagedBmpInfo(SKImageInfo info) {
            this.width = info.Width;
            this.height = info.Height;
            this.colourType = info.ColorType;
            this.alphaType = info.AlphaType;
        }
    }

    static RasterLayer() {
        SetParameterAffectsRender(ChannelRParameter, ChannelGParameter, ChannelBParameter, ChannelAParameter);
        SerialisationRegistry.Register<RasterLayer>(0, (layer, data, ctx) => {
            ctx.DeserialiseBaseType(data);
            layer.channelR = data.GetFloat("ChannelR");
            layer.channelG = data.GetFloat("ChannelG");
            layer.channelB = data.GetFloat("ChannelB");
            layer.channelA = data.GetFloat("ChannelA");
            if (data.TryGetStruct("BmpInfo", out UnmanagedBmpInfo bmpInfo) && data.TryGetByteArray("BmpData", out byte[]? array)) {
                SKBitmap bmp = new SKBitmap(bmpInfo.AsSkia);
                unsafe {
                    fixed (byte* address = &new ReadOnlySpan<byte>(array).GetPinnableReference()) {
                        using SKPixmap map = new SKPixmap(bmp.Info, (IntPtr) address);
                        bmp.InstallPixels(map);
                    }
                }
                
                layer.Bitmap.InitialiseUsingBitmap(bmp);
            }
        }, (layer, data, ctx) => {
            ctx.SerialiseBaseType(data);
            data.SetFloat("ChannelR", layer.channelR);
            data.SetFloat("ChannelG", layer.channelG);
            data.SetFloat("ChannelB", layer.channelB);
            data.SetFloat("ChannelA", layer.channelA);
            if (layer.Bitmap.IsInitialised) {
                data.SetByteArray("BmpData", layer.Bitmap.Bitmap!.Bytes);
                data.SetStruct("BmpInfo", new UnmanagedBmpInfo(layer.Bitmap.Bitmap.Info));
            }
        });
    }

    public override bool IsHitTest(double x, double y) {
        if (!this.Bitmap.IsInitialised)
            return false;

        int pixel = this.Bitmap.PixelAt(Maths.Floor(x), Maths.Floor(y));
        return pixel != 0;
    }

    protected override void LoadDataIntoClone(BaseLayerTreeObject clone) {
        base.LoadDataIntoClone(clone);

        RasterLayer raster = (RasterLayer) clone;
        if (this.Bitmap.IsInitialised) {
            if (!raster.Bitmap.IsInitialised || raster.Bitmap.Size != this.Bitmap.Size)
                raster.Bitmap.InitialiseBitmap(this.Bitmap.Size);
            raster.Bitmap.Paste(this.Bitmap);
        }
        
        raster.channelR = this.channelR;
        raster.channelG = this.channelG;
        raster.channelB = this.channelB;
        raster.channelA = this.channelA;
    }

    public override void RenderLayer(ref RenderContext ctx) {
        if (this.Bitmap.IsInitialised) {
            using SKPaint paint = new SKPaint();
            paint.Color = RenderUtils.BlendAlpha(SKColors.White, this.Opacity);
            paint.BlendMode = this.BlendMode;
            float r = this.channelR, g = this.channelG, b = this.channelB, a = this.channelA;
            if (!DoubleUtils.AreClose(r, 1.0) || !DoubleUtils.AreClose(g, 1.0) || !DoubleUtils.AreClose(b, 1.0) || !DoubleUtils.AreClose(a, 1.0)) {
                float[] matrix = new float[] {
                    r, 0, 0, 0, 0,
                    0, g, 0, 0, 0,
                    0, 0, b, 0, 0,
                    0, 0, 0, a, 0
                };

                paint.ColorFilter = SKColorFilter.CreateColorMatrix(matrix);
            }

            ctx.Canvas.DrawBitmap(this.Bitmap.Bitmap, 0, 0, paint);
        }
    }
}