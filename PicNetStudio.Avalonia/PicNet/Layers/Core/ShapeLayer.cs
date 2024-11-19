using PicNetStudio.Avalonia.DataTransfer;
using PicNetStudio.Avalonia.Utils;
using PicNetStudio.Avalonia.Utils.Accessing;
using SkiaSharp;

namespace PicNetStudio.Avalonia.PicNet.Layers.Core;

/// <summary>
/// A layer that draws a single shape
/// </summary>
public abstract class BaseShapeLayer : BaseVisualLayer {
    public BaseShapeLayer() {
        this.UsesCustomOpacityCalculation = true;
    }

    public override void RenderLayer(ref RenderContext ctx) {
        using SKPaint paint = new SKPaint();
        paint.Color = RenderUtils.BlendAlpha(SKColors.White, this.Opacity);
        paint.BlendMode = this.BlendMode;
        this.Render(ref ctx, paint);
    }

    protected abstract void Render(ref RenderContext ctx, SKPaint paint);
    
    public enum PrimitiveDrawType {
        Rect, // + RoundRect
        Line,
        Oval, // Ellipse?
        Circle,
        Path,
        Arc
    }
}

public abstract class BaseSimpleShapeLayer : BaseShapeLayer {
    public static readonly DataParameterFloat WidthParameter = DataParameter.Register(new DataParameterFloat(typeof(BaseSimpleShapeLayer), nameof(Width), 0.0F, ValueAccessors.Reflective<float>(typeof(BaseSimpleShapeLayer), nameof(width))));
    public static readonly DataParameterFloat HeightParameter = DataParameter.Register(new DataParameterFloat(typeof(BaseSimpleShapeLayer), nameof(Height), 0.0F, ValueAccessors.Reflective<float>(typeof(BaseSimpleShapeLayer), nameof(height))));

    private float width = WidthParameter.DefaultValue;
    private float height = HeightParameter.DefaultValue;

    public float Width {
        get => this.width;
        set => DataParameter.SetValueHelper(this, WidthParameter, ref this.width, value);
    }

    public float Height {
        get => this.height;
        set => DataParameter.SetValueHelper(this, HeightParameter, ref this.height, value);
    }    
}

public class RectangleShapeLayer : BaseSimpleShapeLayer {
    protected override void Render(ref RenderContext ctx, SKPaint paint) {
        float w = this.Width, h = this.Height;
        if (DoubleUtils.AreClose(w, 0.0) || DoubleUtils.AreClose(h, 0.0))
            return;
        
        ctx.Canvas.DrawRect(0, 0, w, h, paint);
    }
}