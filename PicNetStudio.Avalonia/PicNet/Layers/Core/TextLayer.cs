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

using System;
using System.Collections.Specialized;
using System.Numerics;
using PicNetStudio.Avalonia.DataTransfer;
using PicNetStudio.Avalonia.Utils.Accessing;
using SkiaSharp;

namespace PicNetStudio.Avalonia.PicNet.Layers.Core;

/// <summary>
/// A layer which draws text
/// </summary>
public class TextLayer : BaseVisualLayer {
    public static readonly DataParameterString TextParameter = DataParameter.Register(new DataParameterString(typeof(TextLayer), nameof(Text), null, ValueAccessors.Reflective<string?>(typeof(TextLayer), nameof(text))));
    public static readonly DataParameterDouble FontSizeParameter = DataParameter.Register(new DataParameterDouble(typeof(TextLayer), nameof(FontSize), 40.0, ValueAccessors.Reflective<double>(typeof(TextLayer), nameof(fontSize))));
    public static readonly DataParameterString FontFamilyParameter = DataParameter.Register(new DataParameterString(typeof(TextLayer), nameof(FontFamily), "Consolas", ValueAccessors.Reflective<string?>(typeof(TextLayer), nameof(fontFamily))));
    public static readonly DataParameterDouble BorderThicknessParameter = DataParameter.Register(new DataParameterDouble(typeof(TextLayer), nameof(BorderThickness), 1.0D, ValueAccessors.Reflective<double>(typeof(TextLayer), nameof(borderThickness))));
    public static readonly DataParameterFloat SkewXParameter = DataParameter.Register(new DataParameterFloat(typeof(TextLayer), nameof(SkewX), 0.0F, ValueAccessors.Reflective<float>(typeof(TextLayer), nameof(skewX))));
    public static readonly DataParameterBool IsAntiAliasedParameter = DataParameter.Register(new DataParameterBool(typeof(TextLayer), nameof(IsAntiAliased), true, ValueAccessors.Reflective<bool>(typeof(TextLayer), nameof(isAntiAliased))));
    public static readonly DataParameterDouble LineHeightMultiplierParameter = DataParameter.Register(new DataParameterDouble(typeof(TextLayer), nameof(LineHeightMultiplier), 1.0, ValueAccessors.Reflective<double>(typeof(TextLayer), nameof(lineHeightMultiplier))));
    
    private string? text;
    private double fontSize;
    private string? fontFamily;
    private double borderThickness;
    private float skewX;
    private bool isAntiAliased;
    private double lineHeightMultiplier;
    private BitVector32 clipProps;
    private SKTextBlob?[]? TextBlobs;
    private Vector2 TextBlobBoundingBox;
    // TODO: colours for automation and implement UI for colour data params
    private SKColor foreground;
    private SKColor border;

    // TODO: rich text, not a plain string.
    // Maybe we make a custom layer that uses Avalonia UI
    // elements, so we could use AvalonEdit? !!!
    public string? Text {
        get => this.text;
        set => DataParameter.SetValueHelper(this, TextParameter, ref this.text, value);
    }
    
    public double FontSize {
        get => this.fontSize;
        set => DataParameter.SetValueHelper(this, FontSizeParameter, ref this.fontSize, value);
    }

    public string FontFamily {
        get => this.fontFamily ?? "Consolas";
        set => DataParameter.SetValueHelper(this, FontFamilyParameter, ref this.fontFamily, value);
    }

    public double BorderThickness {
        get => this.borderThickness;
        set => DataParameter.SetValueHelper(this, BorderThicknessParameter, ref this.borderThickness, value);
    }

    public float SkewX {
        get => this.skewX;
        set => DataParameter.SetValueHelper(this, SkewXParameter, ref this.skewX, value);
    }

    public bool IsAntiAliased {
        get => this.isAntiAliased;
        set => DataParameter.SetValueHelper(this, IsAntiAliasedParameter, ref this.isAntiAliased, value);
    }
    
    public double LineHeightMultiplier {
        get => this.lineHeightMultiplier;
        set => DataParameter.SetValueHelper(this, LineHeightMultiplierParameter, ref this.lineHeightMultiplier, value);
    }

    public SKPaint? GeneratedPaint { get; private set; }

    public SKFont? GeneratedFont { get; private set; }

    public TextLayer() {
        this.text = TextParameter.DefaultValue;
        this.fontSize = FontSizeParameter.GetDefaultValue(this);
        this.fontFamily = FontFamilyParameter.DefaultValue;
        this.borderThickness = BorderThicknessParameter.GetDefaultValue(this);
        this.skewX = SkewXParameter.GetDefaultValue(this);
        this.isAntiAliased = IsAntiAliasedParameter.GetDefaultValue(this);
        this.lineHeightMultiplier = LineHeightMultiplierParameter.GetDefaultValue(this);
        this.clipProps = new BitVector32();
        this.foreground = SKColors.Black;
        this.border = SKColors.DarkGray;
    }

    protected override void LoadDataIntoClone(BaseLayerTreeObject clone) {
        base.LoadDataIntoClone(clone);
        TextLayer layer = (TextLayer) clone;
        layer.text = this.text;
        layer.clipProps = this.clipProps;
    }

    static TextLayer() {
        SetParameterAffectsRender(TextParameter);
        DataParameter.AddMultipleHandlers((_, owner) => ((TextLayer) owner).InvalidateFontData(), FontSizeParameter, FontFamilyParameter, BorderThicknessParameter, SkewXParameter, IsAntiAliasedParameter, LineHeightMultiplierParameter);
        DataParameter.AddMultipleHandlers((_, owner) => ((TextLayer) owner).DisposeText(), TextParameter);
    }

    public void DisposeText() {
        this.TextBlobBoundingBox = new Vector2();
        DisposeTextBlobs(ref this.TextBlobs);
    }

    public override void RenderLayer(ref RenderContext ctx) {
        if (this.GeneratedPaint == null || this.GeneratedFont == null) {
            this.GenerateCachedData();
        }
        
        if (this.TextBlobs == null && !string.IsNullOrEmpty(this.text)) {
            this.TextBlobBoundingBox = new Vector2();
            this.GenerateTextCache();
        }

        SKPaint? paint = this.GeneratedPaint;
        if (this.TextBlobs == null || paint == null) {
            return;
        }

        this.GeneratedFont!.GetFontMetrics(out SKFontMetrics metrics);
        // we can get away with this since we just use numbers and not any 'special'
        // characters with bits below the baseline and whatnot

        double lineHeightAdd = 0.0;
        foreach (SKTextBlob? blob in this.TextBlobs) {
            if (blob != null) {
                // fd.cachedFont.GetFontMetrics(out SKFontMetrics metrics);
                // // we can get away with this since we just use numbers and not any 'special'
                // // characters with bits below the baseline and whatnot
                // SKRect realFinalRenderArea = new SKRect(0, 0, blob.Bounds.Right, blob.Bounds.Bottom - metrics.Ascent - metrics.Descent);
                // rc.Canvas.DrawText(blob, 0, -blob.Bounds.Top - metrics.Descent, paint);
                //
                // // we still need to tell the track the rendering area, otherwise we're copying the entire frame which is
                // // unacceptable. Even though there will most likely be a bunch of transparent padding pixels, it's still better
                // renderArea = rc.TranslateRect(realFinalRenderArea);

                ctx.Canvas.DrawText(blob, 0, (float) (blob.Bounds.Height / 2d + lineHeightAdd), paint);
                lineHeightAdd += this.LineHeightMultiplier;
                // ctx.Canvas.DrawText(blob, 0, -blob.Bounds.Top - metrics.Descent, paint);
            }
        }
    }

    public void GenerateTextCache() {
        if (this.TextBlobs != null || string.IsNullOrEmpty(this.text)) {
            return;
        }

        this.GenerateCachedData();
        if (this.GeneratedFont != null && this.GeneratedPaint != null) {
            this.TextBlobs = CreateTextBlobs(this.text, this.GeneratedPaint, this.GeneratedFont);
            if (this.TextBlobs != null) {
                float w = 0, h = 0;
                foreach (SKTextBlob? blob in this.TextBlobs) {
                    if (blob != null) {
                        SKRect bound = blob.Bounds;
                        w = Math.Max(w, bound.Width);
                        h = Math.Max(h, bound.Height);
                    }
                }
                
                this.TextBlobBoundingBox = new Vector2(w, h);
            }
        }
    }
    
    /// <summary>
    /// Invalidates the cached font and paint information. This is called automatically when any of our properties change
    /// </summary>
    public void InvalidateFontData() {
        this.GeneratedFont?.Dispose();
        this.GeneratedFont = null;
        this.GeneratedPaint?.Dispose();
        this.GeneratedPaint = null;
        this.DisposeText();
        this.InvalidateVisual();
    }

    /// <summary>
    /// Generates our cached font and paint data. This must be called manually after invalidating font data
    /// </summary>
    public void GenerateCachedData() {
        if (this.GeneratedFont == null) {
            SKTypeface typeface = SKTypeface.FromFamilyName(string.IsNullOrEmpty(this.FontFamily) ? "Consolas" : this.FontFamily);
            if (typeface != null) {
                this.GeneratedFont = new SKFont(typeface, (float) this.FontSize, 1f, this.SkewX);
            }
        }

        this.GeneratedPaint ??= new SKPaint() {
            StrokeWidth = (float) this.BorderThickness,
            Color = this.foreground,
            TextAlign = SKTextAlign.Left,
            IsAntialias = this.IsAntiAliased
        };
    }

    public static SKTextBlob[]? CreateTextBlobs(string input, SKPaint paint, SKFont font) {
        return CreateTextBlobs(input, font, paint.TextSize); // * 1.2f
    }

    public static SKTextBlob[]? CreateTextBlobs(string input, SKFont font, float lineHeight) {
        if (string.IsNullOrEmpty(input)) {
            return null;
        }

        string[] lines = input.Split('\n');
        SKTextBlob[] blobs = new SKTextBlob[lines.Length];
        for (int i = 0; i < lines.Length; i++) {
            float y = i * lineHeight;
            blobs[i] = SKTextBlob.Create(lines[i], font, new SKPoint(0, y));
        }

        return blobs;
    }

    public static void DisposeTextBlobs(ref SKTextBlob?[]? blobs) {
        if (blobs == null)
            return;
        foreach (SKTextBlob? blob in blobs)
            blob?.Dispose();
        blobs = null;
    }
}