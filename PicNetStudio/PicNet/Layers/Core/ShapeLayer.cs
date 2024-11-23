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

using PicNetStudio.DataTransfer;
using PicNetStudio.Utils;
using PicNetStudio.Utils.Accessing;
using SkiaSharp;

namespace PicNetStudio.PicNet.Layers.Core;

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

    private float width;
    private float height;

    public float Width {
        get => this.width;
        set => DataParameter.SetValueHelper(this, WidthParameter, ref this.width, value);
    }

    public float Height {
        get => this.height;
        set => DataParameter.SetValueHelper(this, HeightParameter, ref this.height, value);
    }  
    
    public BaseSimpleShapeLayer() {
        this.width = WidthParameter.GetDefaultValue(this);
        this.height = HeightParameter.GetDefaultValue(this);
    }

    static BaseSimpleShapeLayer() {
        SerialisationRegistry.Register<BaseSimpleShapeLayer>(0, (layer, data, ctx) => {
            ctx.DeserialiseBaseType(data);
            layer.width = data.GetFloat("Width");
            layer.height = data.GetFloat("Height");
        }, (layer, data, ctx) => {
            ctx.SerialiseBaseType(data);
            data.SetFloat("Width", layer.width);
            data.SetFloat("Height", layer.height);
        });
    }

    protected override void LoadDataIntoClone(BaseLayerTreeObject clone) {
        base.LoadDataIntoClone(clone);
        BaseSimpleShapeLayer layer = (BaseSimpleShapeLayer) clone;
        layer.width = this.width;
        layer.height = this.height;
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