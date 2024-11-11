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
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using PicNetStudio.Avalonia.Utils;
using SkiaSharp;

namespace PicNetStudio.Avalonia.PicNet.Controls;

public class CanvasControl : TemplatedControl {
    public static readonly StyledProperty<Document?> DocumentProperty = AvaloniaProperty.Register<CanvasControl, Document?>(nameof(Document));
    public static readonly StyledProperty<double> ZoomScaleProperty = FreeMoveViewPortV2.ZoomScaleProperty.AddOwner<CanvasControl>();
    public static readonly StyledProperty<bool> IsDrawingWithPointerProperty = AvaloniaProperty.Register<CanvasControl, bool>(nameof(IsDrawingWithPointer));

    public Document? Document {
        get => this.GetValue(DocumentProperty);
        set => this.SetValue(DocumentProperty, value);
    }

    public double ZoomScale {
        get => this.GetValue(ZoomScaleProperty);
        set => this.SetValue(ZoomScaleProperty, value);
    }

    public bool IsDrawingWithPointer {
        get => this.GetValue(IsDrawingWithPointerProperty);
        set => this.SetValue(IsDrawingWithPointerProperty, value);
    }

    public FreeMoveViewPortV2? PART_FreeMoveViewPort;
    public SKAsyncViewPort? PART_SkiaViewPort;
    private readonly CanvasInputHandler tracker;

    static CanvasControl() {
        AffectsRender<Image>(DocumentProperty);
        AffectsMeasure<Image>(DocumentProperty);
        DocumentProperty.Changed.AddClassHandler<CanvasControl, Document?>(OnPicNetBitmapChanged);
    }

    public CanvasControl() {
        this.Loaded += this.OnLoaded;
        this.tracker = new CanvasInputHandler(this);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        e.NameScope.GetTemplateChild("PART_FreeMoveViewPort", out this.PART_FreeMoveViewPort);
        e.NameScope.GetTemplateChild("PART_SkiaViewPort", out this.PART_SkiaViewPort);
        if (this.PART_SkiaViewPort != null) {
            // nearest neighbour
            RenderOptions.SetBitmapInterpolationMode(this.PART_SkiaViewPort, BitmapInterpolationMode.None);
            RenderOptions.SetEdgeMode(this.PART_SkiaViewPort, EdgeMode.Aliased);
        }

        if (this.Document is Document document) {
            this.UpdatePnbSize(document.Canvas.Size);
        }
    }

    public void InvalidateRender() {
        if (this.PART_SkiaViewPort != null && this.Document is Document document) {
            if (this.PART_SkiaViewPort.BeginRender(out SKSurface surface)) {
                document.Canvas.Render(surface);
                this.PART_SkiaViewPort.EndRender();
            }
        }
    }

    private void OnLoaded(object? sender, RoutedEventArgs e) {
        this.Loaded -= this.OnLoaded;
        this.InvalidateRender();
    }

    private void OnCanvasRenderInvalidated(Canvas canvas) {
        if (this.Document?.Canvas == canvas) {
            RZApplication.Instance.Dispatcher.InvokeAsync(this.InvalidateRender);
        }
    }

    private static void OnPicNetBitmapChanged(CanvasControl control, AvaloniaPropertyChangedEventArgs<Document?> e) {
        if (e.TryGetOldValue(out Document? oldDocument)) {
            oldDocument.Canvas.SizeChanged -= control.OnResolutionChanged;
            oldDocument.Canvas.RenderInvalidated -= control.OnCanvasRenderInvalidated;
        }

        if (e.TryGetNewValue(out Document? newDocument)) {
            newDocument.Canvas.SizeChanged += control.OnResolutionChanged;
            newDocument.Canvas.RenderInvalidated += control.OnCanvasRenderInvalidated;
            control.UpdatePnbSize(newDocument.Canvas.Size);
        }
    }

    private void OnResolutionChanged(Canvas canvas, PixelSize oldSize, PixelSize newSize) {
        this.UpdatePnbSize(newSize);
    }

    private void UpdatePnbSize(PixelSize size) {
        if (this.PART_SkiaViewPort == null)
            return;

        this.PART_SkiaViewPort.Width = size.Width;
        this.PART_SkiaViewPort.Height = size.Height;
    }
}