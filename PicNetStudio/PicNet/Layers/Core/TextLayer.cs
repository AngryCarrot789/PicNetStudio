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

using System.Collections.Specialized;
using PicNetStudio.DataTransfer;
using PicNetStudio.Utils.Accessing;
using SkiaSharp;

namespace PicNetStudio.PicNet.Layers.Core;

/// <summary>
/// A layer which draws text
/// </summary>
public class TextLayer : BaseVisualLayer {
    public static readonly DataParameterString TextParameter = DataParameter.Register(new DataParameterString(typeof(TextLayer), nameof(Text), null, ValueAccessors.Reflective<string?>(typeof(TextLayer), nameof(text))));
    public static readonly DataParameterFloat FontSizeParameter = DataParameter.Register(new DataParameterFloat(typeof(TextLayer), nameof(FontSize), 40.0F, ValueAccessors.Reflective<float>(typeof(TextLayer), nameof(fontSize))));
    public static readonly DataParameterString FontFamilyParameter = DataParameter.Register(new DataParameterString(typeof(TextLayer), nameof(FontFamily), "Consolas", ValueAccessors.Reflective<string?>(typeof(TextLayer), nameof(fontFamily))));
    public static readonly DataParameterDouble BorderThicknessParameter = DataParameter.Register(new DataParameterDouble(typeof(TextLayer), nameof(BorderThickness), 1.0D, ValueAccessors.Reflective<double>(typeof(TextLayer), nameof(borderThickness))));
    public static readonly DataParameterFloat SkewXParameter = DataParameter.Register(new DataParameterFloat(typeof(TextLayer), nameof(SkewX), 0.0F, ValueAccessors.Reflective<float>(typeof(TextLayer), nameof(skewX))));
    public static readonly DataParameterBool IsAntiAliasedParameter = DataParameter.Register(new DataParameterBool(typeof(TextLayer), nameof(IsAntiAliased), true, ValueAccessors.Reflective<bool>(typeof(TextLayer), nameof(isAntiAliased))));
    public static readonly DataParameterFloat LineSpacingParameter = DataParameter.Register(new DataParameterFloat(typeof(TextLayer), nameof(LineSpacing), 0.0F, ValueAccessors.Reflective<float>(typeof(TextLayer), nameof(lineSpacing))));
    
    private string? text;
    private float fontSize;
    private string? fontFamily;
    private double borderThickness;
    private float skewX;
    private bool isAntiAliased;
    private float lineSpacing;
    private BitVector32 clipProps;
    private SKTextBlob?[]? TextBlobs;
    private SKSize TextBlobBoundingBox;
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
    
    public float FontSize {
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
    
    public float LineSpacing {
        get => this.lineSpacing;
        set => DataParameter.SetValueHelper(this, LineSpacingParameter, ref this.lineSpacing, value);
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
        this.lineSpacing = LineSpacingParameter.GetDefaultValue(this);
        this.clipProps = new BitVector32();
        this.foreground = SKColors.Black;
        this.border = SKColors.DarkGray;
    }

    static TextLayer() {
        SetParameterAffectsRender(TextParameter);
        DataParameter.AddMultipleHandlers((_, owner) => ((TextLayer) owner).InvalidateFontData(), FontSizeParameter, FontFamilyParameter, BorderThicknessParameter, SkewXParameter, IsAntiAliasedParameter, LineSpacingParameter);
        DataParameter.AddMultipleHandlers((_, owner) => ((TextLayer) owner).DisposeText(), TextParameter);
        
        SerialisationRegistry.Register<TextLayer>(0, (layer, data, ctx) => {
            ctx.DeserialiseBaseType(data);
            layer.text = data.GetString("Text", null);
            layer.fontSize = data.GetFloat("FontSize");
            layer.fontFamily = data.GetString("FontFamily", FontFamilyParameter.GetDefaultValue(layer)!);
            layer.borderThickness = data.GetDouble("BorderThickness");
            layer.skewX = data.GetFloat("SkewX");
            layer.isAntiAliased = data.GetBool("IsAntiAliased");
            layer.lineSpacing = data.GetFloat("LineHeightMultiplier");
            layer.clipProps = data.GetStruct<BitVector32>("ClipProps");
            layer.foreground = data.GetStruct<SKColor>("Foreground");
            layer.border = data.GetStruct<SKColor>("Border");
        }, (layer, data, ctx) => {
            ctx.SerialiseBaseType(data);
            data.SetString("Text", layer.text!);
            data.SetDouble("FontSize", layer.fontSize);
            data.SetString("FontFamily", layer.fontFamily!);
            data.SetDouble("BorderThickness", layer.borderThickness);
            data.SetFloat("SkewX", layer.skewX);
            data.SetBool("IsAntiAliased", layer.isAntiAliased);
            data.SetDouble("LineHeightMultiplier", layer.lineSpacing);
            data.SetStruct("ClipProps", layer.clipProps);
            data.SetStruct("Foreground", layer.foreground);
            data.SetStruct("Border", layer.border);
        });
    }

    protected override void LoadDataIntoClone(BaseLayerTreeObject clone) {
        base.LoadDataIntoClone(clone);
        TextLayer layer = (TextLayer) clone;
        layer.text = this.text;
        layer.clipProps = this.clipProps;
        layer.fontSize = this.fontSize;
        layer.fontFamily = this.fontFamily;
        layer.borderThickness = this.borderThickness;
        layer.skewX = this.skewX;
        layer.isAntiAliased = this.isAntiAliased;
        layer.lineSpacing = this.lineSpacing;
        layer.foreground = this.foreground;
        layer.border = this.border;
    }

    public void DisposeText() {
        this.TextBlobBoundingBox = new SKSize();
        DisposeTextBlobs(ref this.TextBlobs);
        this.OnSizeForAutomaticOriginsChanged();
    }

    public override SKSize GetSizeForAutomaticOrigins() {
        return this.TextBlobBoundingBox;
    }

    protected override void OnSizeForAutomaticOriginsChanged() {
        base.OnSizeForAutomaticOriginsChanged();
        this.InvalidateTransformationMatrix();
    }

    public override bool OnPrepareRenderLayer(ref RenderContext ctx) {
        base.OnPrepareRenderLayer(ref ctx);
        if (this.GeneratedPaint == null || this.GeneratedFont == null) {
            this.GenerateCachedData();
        }
        
        if (this.TextBlobs == null && !string.IsNullOrEmpty(this.text)) {
            this.TextBlobBoundingBox = new SKSize();
            this.GenerateTextCache();
        }

        return this.TextBlobs != null && this.GeneratedPaint != null;
    }

    public override void RenderLayer(ref RenderContext ctx) {
        SKPaint? paint = this.GeneratedPaint;
        this.GeneratedFont!.GetFontMetrics(out SKFontMetrics metrics);
        SKTextBlob?[] blobs = this.TextBlobs!;
        float offset = 0.0F;
        for (int i = 0, endIndex = blobs.Length - 1; i <= endIndex; i++) {
            SKTextBlob? blob = blobs[i];
            if (blob != null) {
                float y = -blob.Bounds.Top - metrics.Descent + offset;
                ctx.Canvas.DrawText(blob, 0, y, paint);
                offset += this.FontSize;
                if (i != endIndex)
                    offset += this.lineSpacing;
            }
        }
    }

    public void GenerateTextCache() {
        if (this.TextBlobs != null || string.IsNullOrEmpty(this.text)) {
            return;
        }

        this.GenerateCachedData();
        if (this.GeneratedFont != null && this.GeneratedPaint != null) {
            SKTextBlob?[]? blobs = this.TextBlobs = this.CreateTextBlobs(this.text, this.GeneratedPaint, this.GeneratedFont);
            if (blobs != null) {
                this.GeneratedFont!.GetFontMetrics(out SKFontMetrics metrics);
                float w = 0, h = 0, myLineHeight = 0.0F;
                for (int i = 0, endIndex = blobs.Length - 1; i <= endIndex; i++) {
                    SKTextBlob? blob = blobs[i];
                    if (blob != null) {
                        SKRect bound = blob.Bounds;
                        w = Math.Max(w, bound.Width);
                        
                        float height = Math.Abs(blob.Bounds.Bottom - blob.Bounds.Top) - metrics.Bottom;
                        h += height + myLineHeight;
                        myLineHeight = this.lineSpacing;
                    }
                }

                this.TextBlobBoundingBox = new SKSize(w, h);
                this.OnSizeForAutomaticOriginsChanged();
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

    public SKTextBlob[]? CreateTextBlobs(string input, SKPaint paint, SKFont font) {
        return CreateTextBlobs(input, font, (float) this.LineSpacing); // * 1.2f
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