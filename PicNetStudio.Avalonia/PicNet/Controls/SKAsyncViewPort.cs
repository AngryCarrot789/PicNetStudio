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
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Rendering;
using Avalonia.VisualTree;
using SkiaSharp;

namespace PicNetStudio.Avalonia.PicNet.Controls;

public delegate void AsyncViewPortPreRenderExtensionEventHandler(SKAsyncViewPort sender, SKSurface surface);
public delegate void AsyncViewPortRenderExtensionEventHandler(SKAsyncViewPort sender, DrawingContext ctx, Rect bounds);

public class SKAsyncViewPort : Control {
    private WriteableBitmap? bitmap;
    private SKSurface? targetSurface;
    private SKImageInfo skImageInfo;
    private bool ignorePixelScaling;
    private ILockedFramebuffer lockKey;

    public SKPoint FeedbackZoomOrigin;
    public SKPoint FeedbackZoomSize;

    /// <summary>Gets the current canvas size.</summary>
    /// <value />
    /// <remarks>The canvas size may be different to the view size as a result of the current device's pixel density.</remarks>
    public SKSize CanvasSize { get; private set; }

    /// <summary>Gets or sets a value indicating whether the drawing canvas should be resized on high resolution displays.</summary>
    /// <value />
    /// <remarks>By default, when false, the canvas is resized to 1 canvas pixel per display pixel. When true, the canvas is resized to device independent pixels, and then stretched to fill the view. Although performance is improved and all objects are the same size on different display densities, blurring and pixelation may occur.</remarks>
    public bool IgnorePixelScaling {
        get => this.ignorePixelScaling;
        set {
            this.ignorePixelScaling = value;
            this.InvalidateVisual();
        }
    }

    public event AsyncViewPortPreRenderExtensionEventHandler? PreRenderExtension;
    public event AsyncViewPortRenderExtensionEventHandler? PostRenderExtension;

    public SKImageInfo FrameInfo => this.skImageInfo;

    public SKAsyncViewPort() {
    }

    public bool BeginRender(out SKSurface surface) {
        IRenderRoot? source;
        if (this.targetSurface != null || (source = this.GetVisualRoot()) == null) {
            surface = null;
            return false;
        }

        SKSizeI pixelSize = this.CreateSize(out SKSizeI unscaledSize, out double scaleX, out double scaleY, source);
        this.CanvasSize = this.ignorePixelScaling ? unscaledSize : pixelSize;
        if (pixelSize.Width <= 0 || pixelSize.Height <= 0) {
            surface = null;
            return false;
        }

        SKImageInfo frameInfo = new SKImageInfo(pixelSize.Width, pixelSize.Height, SKImageInfo.PlatformColorType, SKAlphaType.Premul);
        this.skImageInfo = frameInfo;

        WriteableBitmap? bmp = this.bitmap;
        if (bmp == null || frameInfo.Width != bmp.PixelSize.Width || frameInfo.Height != bmp.PixelSize.Height) {
            this.bitmap = bmp = new WriteableBitmap(new PixelSize(frameInfo.Width, frameInfo.Height), new Vector(scaleX * 96d, scaleY * 96d));
        }

        this.lockKey = bmp.Lock();
        this.targetSurface = surface = SKSurface.Create(frameInfo, this.lockKey.Address, this.lockKey.RowBytes);
        if (this.ignorePixelScaling) {
            SKCanvas canvas = surface.Canvas;
            canvas.Scale((float) scaleX, (float) scaleY);
            canvas.Save();
        }

        return true;
    }

    public void EndRender(bool invalidateVisual = true) {
        // SKImageInfo info = this.skImageInfo;
        // this.lockKey.AddDirtyRect(new Int32Rect(0, 0, info.Width, info.Height));
        this.PreRenderExtension?.Invoke(this, this.targetSurface!);
        this.lockKey.Dispose();
        if (invalidateVisual)
            this.InvalidateVisual();
        this.targetSurface!.Dispose();
        this.targetSurface = null;
    }

    public override void Render(DrawingContext context) {
        base.Render(context);
        WriteableBitmap? bmp = this.bitmap;
        if (bmp != null) {
            Rect bounds = this.Bounds;
            context.DrawImage(bmp, new Rect(0d, 0d, bounds.Width, bounds.Height));
            this.PostRenderExtension?.Invoke(this, context, bounds);
        }
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e) {
        base.OnSizeChanged(e);
        this.InvalidateVisual();
    }

    private SKSizeI CreateSize(out SKSizeI unscaledSize, out double scaleX, out double scaleY, IRenderRoot source) {
        unscaledSize = SKSizeI.Empty;
        scaleX = 1f;
        scaleY = 1f;

        Size bounds = this.Bounds.Size;
        if (IsPositive(bounds.Width) && IsPositive(bounds.Height)) {
            unscaledSize = new SKSizeI((int) bounds.Width, (int) bounds.Height);
            double transformToDevice = source.RenderScaling;
            scaleX = transformToDevice;
            scaleY = transformToDevice;
            return new SKSizeI((int) (bounds.Width * scaleX), (int) (bounds.Height * scaleY));
        }

        return SKSizeI.Empty;
    }

    private static bool IsPositive(double value) => !double.IsNaN(value) && !double.IsInfinity(value) && value > 0.0;
}