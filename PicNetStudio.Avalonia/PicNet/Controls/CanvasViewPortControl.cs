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
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using PicNetStudio.Avalonia.PicNet.Layers;
using PicNetStudio.Avalonia.PicNet.Layers.Core;
using PicNetStudio.Avalonia.Utils;
using PicNetStudio.Avalonia.Utils.RDA;
using SkiaSharp;

namespace PicNetStudio.Avalonia.PicNet.Controls;

/// <summary>
/// A view port that manages the rendering of a canvas
/// </summary>
public class CanvasViewPortControl : TemplatedControl, ICanvasElement {
    public static readonly StyledProperty<Document?> DocumentProperty = AvaloniaProperty.Register<CanvasViewPortControl, Document?>(nameof(Document));
    public static readonly StyledProperty<double> ZoomScaleProperty = FreeMoveViewPortV2.ZoomScaleProperty.AddOwner<CanvasViewPortControl>();

    /// <summary>
    /// Gets or sets the document that this canvas control will draw and watch the states of
    /// </summary>
    public Document? Document {
        get => this.GetValue(DocumentProperty);
        set => this.SetValue(DocumentProperty, value);
    }

    /// <summary>
    /// Gets or sets the zoom factor. Does not reset translation
    /// </summary>
    public double ZoomScale {
        get => this.GetValue(ZoomScaleProperty);
        set => this.SetValue(ZoomScaleProperty, value);
    }

    public FreeMoveViewPortV2? PART_FreeMoveViewPort;
    public SKAsyncViewPort? PART_SkiaViewPort;
    private readonly CanvasInputHandler inptHandler;
    private readonly RapidDispatchAction rdaInvalidateRender;
    public TransformationContainer PART_CanvasContainer;

    static CanvasViewPortControl() {
        AffectsRender<Image>(DocumentProperty);
        AffectsMeasure<Image>(DocumentProperty);
        DocumentProperty.Changed.AddClassHandler<CanvasViewPortControl, Document?>(OnDocumentChanged);
    }

    public CanvasViewPortControl() {
        this.Loaded += this.OnLoaded;
        this.inptHandler = new CanvasInputHandler(this);
        this.rdaInvalidateRender = new RapidDispatchAction(this.RenderCanvas, "RDAInvalidateRender");
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        e.NameScope.GetTemplateChild("PART_FreeMoveViewPort", out this.PART_FreeMoveViewPort);
        e.NameScope.GetTemplateChild("PART_SkiaViewPort", out this.PART_SkiaViewPort);
        e.NameScope.GetTemplateChild("PART_CanvasContainer", out this.PART_CanvasContainer);
        
        // nearest neighbour
        RenderOptions.SetBitmapInterpolationMode(this.PART_SkiaViewPort, BitmapInterpolationMode.None);
        RenderOptions.SetEdgeMode(this.PART_SkiaViewPort, EdgeMode.Aliased);
        this.PART_SkiaViewPort.PostRenderExtension += this.OnPostRenderViewPort;

        this.PART_FreeMoveViewPort.Setup(this);
        if (this.Document is Document document) {
            this.UpdateViewPortSize(document.Canvas.Size);
        }
    }

    private void OnPostRenderViewPort(SKAsyncViewPort sender, DrawingContext ctx, Rect bounds) {
        if (this.Document is Document document && document.Canvas.SelectionRegion is RectangleSelection selection) {
            SKRectI rect = selection.Rect;
            ctx.DrawRectangle(null, new Pen(Brushes.Red, 1.0), new Rect(new Point(rect.Left - 0.5, rect.Top - 0.5), new Point(rect.Right + 0.5, rect.Bottom + 0.5)));
        }
    }

    public void RenderCanvas() {
        if (this.PART_SkiaViewPort != null && this.Document is Document document) {
            if (this.PART_SkiaViewPort.BeginRender(out SKSurface surface)) {
                surface.Canvas.Clear(SKColor.Empty);
                document.Canvas.Render(surface);
                this.PART_SkiaViewPort.EndRender();
            }
        }
    }

    private void OnLoaded(object? sender, RoutedEventArgs e) {
        this.Loaded -= this.OnLoaded;
        this.RenderCanvas();
    }

    private void OnCanvasRenderInvalidated(Canvas canvas) {
        if (this.Document?.Canvas == canvas) {
            this.rdaInvalidateRender.InvokeAsync();
        }
    }

    private static void OnDocumentChanged(CanvasViewPortControl control, AvaloniaPropertyChangedEventArgs<Document?> e) {
        if (e.TryGetOldValue(out Document? oldDocument)) {
            oldDocument.Canvas.SizeChanged -= control.OnResolutionChanged;
            oldDocument.Canvas.RenderInvalidated -= control.OnCanvasRenderInvalidated;
            oldDocument.Canvas.ActiveLayerChanged -= control.OnActiveLayerChanged;
        }

        if (e.TryGetNewValue(out Document? newDocument)) {
            newDocument.Canvas.SizeChanged += control.OnResolutionChanged;
            newDocument.Canvas.RenderInvalidated += control.OnCanvasRenderInvalidated;
            newDocument.Canvas.ActiveLayerChanged += control.OnActiveLayerChanged;
            control.UpdateViewPortSize(newDocument.Canvas.Size);
            control.UpdateCursorForActiveLayer(newDocument.Canvas.ActiveLayerTreeObject);
        }
        else {
            control.UpdateViewPortSize(new PixelSize(1, 1));
        }
    }

    private void OnActiveLayerChanged(Canvas canvas, BaseLayerTreeObject? oldactivelayertreeobject, BaseLayerTreeObject? newactivelayertreeobject) {
        this.UpdateCursorForActiveLayer(newactivelayertreeobject);
    }

    private void UpdateCursorForActiveLayer(BaseLayerTreeObject? layer) {
        this.Cursor = new Cursor(!(layer is RasterLayer) ? StandardCursorType.No : StandardCursorType.Cross);
    }

    private void OnResolutionChanged(Canvas canvas, PixelSize oldSize, PixelSize newSize) {
        this.UpdateViewPortSize(newSize);
    }

    private void UpdateViewPortSize(PixelSize size) {
        if (this.PART_SkiaViewPort == null)
            return;

        this.PART_SkiaViewPort.Width = size.Width;
        this.PART_SkiaViewPort.Height = size.Height;
    }

    public BaseSelection SelectionPreview { get; set; }
}