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

using PicNetStudio.PicNet.Layers;
using PicNetStudio.PicNet.Layers.Core;
using PicNetStudio.Utils;
using PicNetStudio.Utils.Collections.Observable;
using SkiaSharp;

namespace PicNetStudio.PicNet;

/// <summary>
/// The layer rendering engine
/// </summary>
public static class LayerRenderer {
    public static void RenderCanvas(ref RenderContext context, bool ignoreSolo = true) {
        BaseVisualLayer? theSoloLayer = context.MyCanvas.SoloLayer;
        BaseVisualLayer targetLayer = ignoreSolo || theSoloLayer == null ? context.MyCanvas.RootComposition : theSoloLayer;
        RenderLayer(ref context, targetLayer);
    }

    private static bool CanDrawCompositionCachedVisual(CompositionLayer layer) {
        bool isInvalid = CompositionLayer.InternalGetAndResetVisualInvalidState(layer);
        if (isInvalid)
            return false;

        PNBitmap pnb = layer.cachedVisualHierarchy;
        return pnb.IsInitialised && layer.Canvas != null && pnb.Size == layer.Canvas.Size;
    }

    /// <summary>
    /// Main method for drawing layers
    /// </summary>
    /// <param name="ctx"></param>
    /// <param name="layer"></param>
    public static void RenderLayer(ref RenderContext ctx, BaseVisualLayer layer) {
        if (layer.Canvas == null || !ctx.IsLayerVisibleToRender(layer)) {
            return;
        }

        // Layer does not want to be rendered so we won't
        if (!layer.OnPrepareRenderLayer(ref ctx)) {
            return;
        }

        int topLevelMatrixRestore = ctx.Canvas.Save();
        ctx.Canvas.SetMatrix(ctx.Canvas.TotalMatrix.PreConcat(layer.TransformationMatrix));
        
        SKPaint? layerPaint = null;
        int restoreIndex;
        if (layer is CompositionLayer compLayer) {
            // TODO: see below for need to fix -- Was i high writing this todo what's broken??
            bool isRootLayer = compLayer.ParentLayer == null, isCacheClean;
            PNBitmap compCache = compLayer.cachedVisualHierarchy;
            if (CompositionLayer.InternalGetAndResetVisualInvalidState(compLayer)) {
                isCacheClean = false;
            }
            else {
                isCacheClean = !isRootLayer && compCache.IsInitialised && compCache.Size == compLayer.Canvas!.Size;
            }

            bool isUsingCustomBlend = compLayer.BlendMode != BaseVisualLayer.BlendModeParameter.DefaultValue;
            bool isOpacityLayerReqd = IsOpacityLayerRequired(compLayer);
            if (!isCacheClean || isOpacityLayerReqd || isUsingCustomBlend) {
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

            if (isCacheClean) {
                // Draw clean cached surface
                ctx.Canvas.DrawBitmap(compCache.Bitmap, 0, 0, null);
            }
            else {
                // Cache is dirty, first ensure cache is initialised
                if (!isRootLayer && (!compCache.IsInitialised || compCache.Size != compLayer.Canvas!.Size)) {
                    compCache.InitialiseBitmap(compLayer.Canvas!.Size);
                }

                ReadOnlyObservableList<BaseLayerTreeObject> layers = compLayer.Layers;
                if (!isRootLayer && compCache.IsInitialised) {
                    // Generate temporary rendering surface to draw the layer into
                    PixSize size = ctx.MyCanvas.Size;
                    SKImageInfo frameInfo = new SKImageInfo(size.Width, size.Height, SKImageInfo.PlatformColorType, SKAlphaType.Premul);
                    
                    using SKSurface subSurface = SKSurface.Create(frameInfo);
                    subSurface.Canvas.Clear(SKColors.Transparent);
                    RenderContext subCtx = new RenderContext(ctx.MyCanvas, subSurface, ctx.VisibilityFlag, ctx.FullInvalidateHint);
                    for (int i = layers.Count - 1; i >= 0; i--) {
                        if (layers[i] is BaseVisualLayer visualLayer)
                            RenderLayer(ref subCtx, visualLayer);
                    }

                    compCache.Canvas!.Clear(SKColors.Transparent);
                    subSurface.Draw(compCache.Canvas, 0, 0, null);   // Draw temp surface into cache
                    ctx.Canvas.DrawBitmap(compCache.Bitmap, 0, 0, null); // Draw cache into ctx rendering surface
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
            layer.isRendering = true;
            layer.RenderLayer(ref ctx);
            layer.isRendering = false;
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