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

namespace PicNetStudio.Avalonia.PicNet.Tools.Core;

/// <summary>
/// The brush tool, which lets you draw circular arrangement of pixels on the canvas, with an adjustable diameter
/// </summary>
public class BrushTool : BaseDiameterTool {
    public static readonly DataParameterFloat HardnessParameter = DataParameter.Register(new DataParameterFloat(typeof(BrushTool), nameof(Hardness), 0.5F, 0.0F, 1.0F, ValueAccessors.Reflective<float>(typeof(BrushTool), nameof(hardness))));

    private float hardness;
    public float Hardness {
        get => this.hardness;
        set => DataParameter.SetValueHelper(this, HardnessParameter, ref this.hardness, value);
    }

    public BrushTool() {
        this.hardness = HardnessParameter.GetDefaultValue(this);
        this.CanDrawSecondaryColour = true;
    }

    protected override void OnDiameterChanged() {
        base.OnDiameterChanged();
        this.InvalidateCursor();
    }

    protected internal override SKImage DrawCursor(out SKPoint hotSpot) {
        // The problem is, this cursor needs to be scaled to the canvas zoom.
        int diameter = Maths.Ceil(this.Diameter);
        SKImageInfo info = new SKImageInfo(diameter, diameter);
        using (SKSurface surface = SKSurface.Create(info)) {
            int middle = Maths.Floor(this.Diameter / 2.0);
            using SKPaint paint = new SKPaint();
            paint.Color = SKColors.Orange;
            paint.Style = SKPaintStyle.Stroke;
            paint.StrokeWidth = 2.0F;
            surface.Canvas.DrawCircle(middle, middle, this.Diameter / 2.0F, paint);

            hotSpot = new SKPoint(middle, middle);
            return surface.Snapshot();
        }
    }

    public override void DrawPixels(PNBitmap bitmap, Document document, double x, double y, bool isPrimaryColour) {
        SKColor colour = isPrimaryColour ? document.Editor!.PrimaryColour : document.Editor!.SecondaryColour;
        if (DoubleUtils.LessThanOrClose(this.Hardness, 0.9999)) {
            float radius = this.Diameter / 2.0F;

            SKPoint center = new SKPoint((float) x, (float) y);
            using SKShader shader = SKShader.CreateRadialGradient(center, radius, new SKColor[] {
                colour, colour.WithAlpha(0)
            }, new float[] {
                this.Hardness, 1.0f
            }, SKShaderTileMode.Clamp);

            using SKPaint paint = new SKPaint();
            paint.IsAntialias = true;
            paint.Shader = shader;
            bitmap.Canvas!.DrawCircle(center, radius, paint);
        }
        else {
            // Swap to fast method for almost 100% hardness, since they are effectively the same
            using SKPaint paint = new SKPaint();
            paint.Color = colour;
            paint.IsAntialias = true;
            bitmap.Canvas!.DrawCircle((float) x, (float) y, (this.Diameter / 2.0F), paint);
        }
    }
}