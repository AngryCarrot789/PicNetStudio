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

using Avalonia;
using PicNetStudio.Avalonia.PicNet.Layers;
using PicNetStudio.Avalonia.PicNet.Layers.Core;
using PicNetStudio.Avalonia.Utils;
using PicNetStudio.Avalonia.Utils.Collections.Observable;
using SkiaSharp;

namespace PicNetStudio.Avalonia.PicNet;

/// <summary>
/// The layer rendering engine
/// </summary>
public static class LayerRenderer {
    public static void RenderCanvas(ref RenderContext context) {
        RenderLayer(ref context, context.MyCanvas.RootComposition);
    }

    private static bool CanDrawCompositionCachedVisual(CompositionLayer layer) {
        bool isInvalid = CompositionLayer.InternalGetAndResetVisualInvalidState(layer);
        if (isInvalid)
            return false;

        PNBitmap pnb = layer.cachedVisualHierarchy;
        return pnb.IsInitialised && layer.Canvas != null && pnb.Size == layer.Canvas.Size;
    }

    private static SKSurface? TempSubSurface;
    private static SKImageInfo TempSubSurfaceInfo;

    /// <summary>
    /// Main method for drawing layers
    /// </summary>
    /// <param name="ctx"></param>
    /// <param name="layer"></param>
    public static void RenderLayer(ref RenderContext ctx, BaseVisualLayer layer) {
        if ((ctx.IsExporting && !layer.IsExportVisible) || (!ctx.IsExporting && !layer.IsVisible) || DoubleUtils.AreClose(layer.Opacity, 0.0)) {
            return;
        }

        int topLevelMatrixRestore = ctx.Canvas.Save();
        ctx.Canvas.SetMatrix(ctx.Canvas.TotalMatrix.PreConcat(layer.TransformationMatrix));
        
        SKPaint? layerPaint = null;
        int restoreIndex;
        if (layer is CompositionLayer compLayer) {
            // TODO: see below for need to fix -- Was i high writing this todo what's broken??
            bool isCacheDirty;
            PNBitmap cache = compLayer.cachedVisualHierarchy;
            if (CompositionLayer.InternalGetAndResetVisualInvalidState(compLayer)) {
                isCacheDirty = true;
            }
            else {
                isCacheDirty = !cache.IsInitialised || compLayer.Canvas == null || cache.Size != compLayer.Canvas.Size;
            }

            bool isUsingCustomBlend = compLayer.BlendMode != BaseVisualLayer.BlendModeParameter.DefaultValue;
            bool isOpacityLayerReqd = IsOpacityLayerRequired(compLayer);
            if (isCacheDirty || isOpacityLayerReqd || isUsingCustomBlend) {
                layerPaint = new SKPaint();
                if (isOpacityLayerReqd)
                    layerPaint.Color = new SKColor(255, 255, 255, RenderUtils.DoubleToByte255(compLayer.Opacity));
                if (isUsingCustomBlend)
                    layerPaint.BlendMode = compLayer.BlendMode;
                restoreIndex = ctx.Canvas.SaveLayer(layerPaint);
            }
            else {
                restoreIndex = ctx.Canvas.Save();
            }

            if (!isCacheDirty) {
                // Draw clean cached surface
                ctx.Canvas.DrawBitmap(cache.Bitmap, 0, 0, null);
            }
            else {
                // Cache is dirty, first ensure cache is initialised
                bool canDrawIntoCache = compLayer.ParentLayer != null && compLayer.Canvas != null;
                if (canDrawIntoCache && (!cache.IsInitialised || cache.Size != compLayer.Canvas!.Size)) {
                    cache.InitialiseBitmap(compLayer.Canvas!.Size);
                }

                ReadOnlyObservableList<BaseLayerTreeObject> layers = compLayer.Layers;
                if (canDrawIntoCache && cache.IsInitialised) {
                    // Generate temporary rendering surface to draw the layer into
                    PixelSize size = ctx.MyCanvas.Size;
                    SKImageInfo frameInfo = new SKImageInfo(size.Width, size.Height, SKImageInfo.PlatformColorType, SKAlphaType.Premul);
                    if (TempSubSurface == null || TempSubSurfaceInfo != frameInfo) {
                        TempSubSurface?.Dispose();
                        TempSubSurface = SKSurface.Create(frameInfo);
                    }

                    TempSubSurface.Canvas.Clear(SKColor.Empty);
                    RenderContext subCtx = new RenderContext(ctx.MyCanvas, TempSubSurface, ctx.IsExporting, ctx.FullInvalidateHint);
                    for (int i = layers.Count - 1; i >= 0; i--) {
                        if (layers[i] is BaseVisualLayer visualLayer)
                            RenderLayer(ref subCtx, visualLayer);
                    }

                    cache.Canvas!.Clear(SKColor.Empty);
                    TempSubSurface.Draw(cache.Canvas, 0, 0, null);   // Draw temp surface into cache
                    ctx.Canvas.DrawBitmap(cache.Bitmap, 0, 0, null); // Draw cache into ctx rendering surface
                }
                else {
                    // Render layer without cache writing. Used for the root layer container
                    for (int i = layers.Count - 1; i >= 0; i--) {
                        if (layers[i] is BaseVisualLayer visualLayer)
                            RenderLayer(ref ctx, visualLayer);
                    }
                }
            }
            
            ctx.Canvas.RestoreToCount(restoreIndex);
            layerPaint?.Dispose();
        }
        else {
            restoreIndex = BeginOpacitySection(ctx.Canvas, layer, out layerPaint);
            layer.RenderLayer(ref ctx);
            EndOpacitySection(ctx.Canvas, restoreIndex, layerPaint);
        }
        
        ctx.Canvas.RestoreToCount(topLevelMatrixRestore);
    }

    /// <summary>
    /// Pushes an opacity layer onto the canvas
    /// </summary>
    /// <returns>The restore count</returns>
    public static int BeginOpacitySection(SKCanvas canvas, BaseVisualLayer layer, out SKPaint? paint) {
        if (IsOpacityLayerRequired(layer)) {
            return canvas.SaveLayer(paint = new SKPaint {
                Color = new SKColor(255, 255, 255, RenderUtils.DoubleToByte255(layer.Opacity))
            });
        }
        else {
            // check greater than just in case...
            paint = null;
            return canvas.Save();
        }
    }

    /// <summary>
    /// Pops the opacity layer
    /// </summary>
    public static void EndOpacitySection(SKCanvas canvas, int count, SKPaint? paint) {
        canvas.RestoreToCount(count);
        paint?.Dispose();
    }

    public static bool IsOpacityLayerRequired(BaseVisualLayer layer) {
        return !layer.UsesCustomOpacityCalculation && layer.Opacity < 1.0;
    }
}