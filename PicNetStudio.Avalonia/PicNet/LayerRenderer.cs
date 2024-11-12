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

using PicNetStudio.Avalonia.PicNet.Layers;
using PicNetStudio.Avalonia.Utils;
using PicNetStudio.Avalonia.Utils.Collections.Observable;
using SkiaSharp;

namespace PicNetStudio.Avalonia.PicNet;

/// <summary>
/// The layer rendering engine
/// </summary>
public class LayerRenderer {
    public static LayerRenderer Instance { get; } = new LayerRenderer();

    public LayerRenderer() {
    }

    public void Render(SKSurface target, Canvas canvas, bool isExport = false) {
        target.Canvas.Clear(SKColors.Empty);
        RenderContext ctx = new RenderContext(canvas, target, isExport);

        ReadOnlyObservableList<BaseLayerTreeObject> layers = canvas.Layers;
        for (int i = layers.Count - 1; i >= 0; i--) {
            if (layers[i] is BaseVisualLayer visualLayer)
                this.DrawLayer(ctx, visualLayer);
        }
    }

    protected void DrawLayer(RenderContext ctx, BaseVisualLayer layer) {
        if ((ctx.IsExporting && !layer.IsExportVisible) || (!ctx.IsExporting && !layer.IsVisible) || DoubleUtils.AreClose(layer.Opacity, 0.0)) {
            return;
        }
        
        SKPaint? transparency = null;
        if (layer is CompositeLayer compositeLayer) {
            ReadOnlyObservableList<BaseLayerTreeObject> layers = compositeLayer.Layers;
            for (int i = layers.Count - 1; i >= 0; i--) {
                if (layers[i] is BaseVisualLayer visualLayer)
                    this.DrawLayer(ctx, visualLayer);
            }
        }
        else {
            int clipSaveCount = BeginOpacityLayer(ctx.Canvas, layer, ref transparency);
            layer.RenderLayer(ctx);
            EndOpacityLayer(ctx.Canvas, clipSaveCount, ref transparency);
        }
    }

    public static int BeginOpacityLayer(SKCanvas canvas, BaseVisualLayer layer, ref SKPaint? paint) {
        if (layer.UsesCustomOpacityCalculation || layer.Opacity >= 1.0) {
            // check greater than just in case...
            return canvas.Save();
        }
        else {
            return canvas.SaveLayer(paint ??= new SKPaint {
                Color = new SKColor(255, 255, 255, RenderUtils.DoubleToByte255(layer.Opacity))
            });
        }
    }

    public static void EndOpacityLayer(SKCanvas canvas, int count, ref SKPaint? paint) {
        canvas.RestoreToCount(count);
        if (paint != null) {
            paint.Dispose();
            paint = null;
        }
    }
}