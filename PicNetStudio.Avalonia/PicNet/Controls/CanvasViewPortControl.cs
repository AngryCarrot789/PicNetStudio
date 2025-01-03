﻿// 
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

using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Media.Immutable;
using Avalonia.Threading;
using PicNetStudio.Avalonia.Interactivity;
using PicNetStudio.Avalonia.Utils;
using PicNetStudio.Interactivity;
using PicNetStudio.PicNet;
using PicNetStudio.PicNet.Layers;
using PicNetStudio.PicNet.Layers.Core;
using PicNetStudio.PicNet.Tools.Core;
using PicNetStudio.Utils;
using PicNetStudio.Utils.RDA;
using SkiaSharp;
using Canvas = PicNetStudio.PicNet.Canvas;
using Point = Avalonia.Point;
using Size = Avalonia.Size;

namespace PicNetStudio.Avalonia.PicNet.Controls;

/// <summary>
/// A view port that manages the rendering of a canvas
/// </summary>
public class CanvasViewPortControl : TemplatedControl, ICanvasElement {
    public static readonly StyledProperty<Document?> DocumentProperty = AvaloniaProperty.Register<CanvasViewPortControl, Document?>(nameof(Document));
    public static readonly StyledProperty<double> ZoomScaleProperty = FreeMoveViewPortV2.ZoomScaleProperty.AddOwner<CanvasViewPortControl>();

    public static readonly StyledProperty<Editor?> EditorProperty = AvaloniaProperty.Register<CanvasViewPortControl, Editor?>("Editor");

    public Editor? Editor {
        get => this.GetValue(EditorProperty);
        set => this.SetValue(EditorProperty, value);
    }

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

    private readonly DispatcherTimer updateDashStyleOffsetTimer;
    public FreeMoveViewPortV2? PART_FreeMoveViewPort;
    public SKAsyncViewPort? PART_SkiaViewPort;
    private readonly CanvasInputHandler inptHandler;
    private readonly RapidDispatchAction rdaInvalidateRender;
    public TransformationContainer PART_CanvasContainer;

    // tiled background + selection borders stuff
    private const double DashStrokeSize = 8;
    private DashStyle? dashStyle1, dashStyle2;
    private Pen? outlinePen1, outlinePen2;
    private Pen? selPen1, selPen2;
    private bool isProcessingAsyncDrop;
    private Pen? selectionPen;

    public BaseSelection SelectionPreview { get; set; }

    static CanvasViewPortControl() {
        AffectsRender<Image>(DocumentProperty);
        AffectsMeasure<Image>(DocumentProperty);
        DocumentProperty.Changed.AddClassHandler<CanvasViewPortControl, Document?>(OnDocumentChanged);
        EditorProperty.Changed.AddClassHandler<CanvasViewPortControl, Editor?>((o, e) => o.OnEditorChanged(e.OldValue.GetValueOrDefault(), e.NewValue.GetValueOrDefault()));

        DragDrop.DragEnterEvent.AddClassHandler<CanvasViewPortControl>((o, e) => o.OnDragEnter(e));
        DragDrop.DragOverEvent.AddClassHandler<CanvasViewPortControl>((o, e) => o.OnDragOver(e));
        DragDrop.DragLeaveEvent.AddClassHandler<CanvasViewPortControl>((o, e) => o.OnDragLeave(e));
        DragDrop.DropEvent.AddClassHandler<CanvasViewPortControl>((o, e) => o.OnDrop(e));
    }

    public CanvasViewPortControl() {
        this.Loaded += this.OnLoaded;
        this.inptHandler = new CanvasInputHandler(this);
        this.rdaInvalidateRender = new RapidDispatchAction(this.RenderCanvas, "RDAInvalidateRender");
        DragDrop.SetAllowDrop(this, true);

        this.updateDashStyleOffsetTimer = new DispatcherTimer(TimeSpan.FromSeconds(0.1d), DispatcherPriority.Background, (sender, args) => {
            if (this.dashStyle1 == null || this.dashStyle2 == null) {
                return;
            }

            if (this.Document is Document document && document.Canvas.SelectionRegion is RectangleSelection) {
                this.dashStyle1.Offset = (this.dashStyle1.Offset + 1) % DashStrokeSize;
                this.dashStyle2.Offset = (this.dashStyle2.Offset + 1) % DashStrokeSize;

                // We aren't rendering the canvas at all, just re-drawing. The view port will
                // retain the last render in its writeable bitmap, so this isn't very expensive.
                this.PART_SkiaViewPort!.InvalidateVisual();
            }
        });
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        e.NameScope.GetTemplateChild("PART_FreeMoveViewPort", out this.PART_FreeMoveViewPort);
        e.NameScope.GetTemplateChild("PART_SkiaViewPort", out this.PART_SkiaViewPort);
        e.NameScope.GetTemplateChild("PART_CanvasContainer", out this.PART_CanvasContainer);

        // nearest neighbour
        RenderOptions.SetBitmapInterpolationMode(this.PART_SkiaViewPort, BitmapInterpolationMode.None);
        RenderOptions.SetEdgeMode(this.PART_SkiaViewPort, EdgeMode.Aliased);
        this.PART_SkiaViewPort.PreRenderExtension += this.OnEndRenderViewPort;
        this.PART_SkiaViewPort.PostRenderExtension += this.OnPostRenderViewPort;

        this.PART_FreeMoveViewPort.Setup(this);
        if (this.Document is Document document) {
            this.UpdateViewPortSize(document.Canvas.Size);
        }
    }

    protected override void OnLoaded(RoutedEventArgs e) {
        base.OnLoaded(e);
        this.updateDashStyleOffsetTimer.Start();
    }

    protected override void OnUnloaded(RoutedEventArgs e) {
        base.OnUnloaded(e);
        this.updateDashStyleOffsetTimer.Stop();
    }

    private void OnEndRenderViewPort(SKAsyncViewPort sender, DrawingContext ctx, Size size, Point minatureOffset) {
        // Not sure how render-intensive DrawingBrush is, especially with GeometryDrawing
        // But since it's not drawing actual Visuals, just geometry, it should be lightning fast.
        using (this.PushInverseScale(ctx, out double scale)) {
            ctx.DrawRectangle(TiledBrush.TiledTransparencyBrush8, null, new Rect(default, size * scale));
        }
    }

    private void OnPostRenderViewPort(SKAsyncViewPort sender, DrawingContext ctx, Size size, Point minatureOffset) {
        DrawingContext.PushedState? state = null;
        double scale = 1.0;

        if (this.Document is Document document) {
            if (document.Canvas.ActiveLayerTreeObject is BaseVisualLayer layer && (!(this.Editor?.ToolBar?.ActiveToolItem?.Tool is CursorTool tool) || tool.IsOutlineVisible)) {
                state ??= this.PushInverseScale(ctx, out scale);

                // Cache pen for performance

                SKSize sz = layer.GetSizeForAutomaticOrigins();
                if (DoubleUtils.GreaterThan(sz.Width, 0.0) && DoubleUtils.GreaterThan(sz.Height, 0.0)) {
                    // Map points from 'local' layer space to 'world' canvas space 
                    SKPoint[]? pts = layer.AbsoluteTransformationMatrix.MapPoints(new SKPoint[] {
                        default,
                        new SKPoint(sz.Width, 0),
                        new SKPoint(sz.Width, sz.Height),
                        new SKPoint(0, sz.Height),
                        layer.RotationOrigin,
                        layer.ScaleOrigin,
                        new SKPoint(sz.Width / 2.0F, sz.Height / 2.0F)
                    });

                    // When anti-aliased, floor to lowest pixel. If not, round, since skia rounds by default
                    Func<double, double> func = layer is RasterLayer && RasterLayer.IsAntiAliasedParameter.GetValue(layer) ? Math.Floor : Math.Round;

                    Geometry selRectGeometry = new PolylineGeometry(new List<Point>() {
                        new Point(func(pts[0].X) * scale, func(pts[0].Y) * scale),
                        new Point(func(pts[1].X) * scale, func(pts[1].Y) * scale),
                        new Point(func(pts[2].X) * scale, func(pts[2].Y) * scale),
                        new Point(func(pts[3].X) * scale, func(pts[3].Y) * scale),
                        new Point(func(pts[0].X) * scale, func(pts[0].Y) * scale)
                    }, false);
                    
                    SKMatrix newMat = layer.ParentLayer!.AbsoluteTransformationMatrix.PreConcat(MatrixUtils.CreateTransformationMatrix(layer.Position, new SKPoint(1, 1), layer.Rotation, default, layer.RotationOrigin));
                    SKPoint rOrg = newMat.MapPoint(layer.RotationOrigin);
                    Point cC = new Point(func(pts[6].X) * scale, func(pts[6].Y) * scale);
                    Point cR = new Point(func(rOrg.X) * scale, func(rOrg.Y) * scale);
                    Point cS = new Point(func(pts[5].X) * scale, func(pts[5].Y) * scale);
                    
                    this.selPen1 ??= new Pen(Brushes.Black, 2.0, new ImmutableDashStyle(new double[] { 4, 4 }, 0));
                    this.selPen2 ??= new Pen(Brushes.White, 2.0, new ImmutableDashStyle(new double[] { 4, 4 }, 4 /* start half way */));
                    
                    ctx.DrawGeometry(null, this.selPen1, selRectGeometry);
                    ctx.DrawGeometry(null, this.selPen2, selRectGeometry);

                    Pen pen1 = new Pen(Brushes.Red, 2.0D);
                    Pen pen2 = new Pen(Brushes.DeepSkyBlue, 2.0D);

                    const double crosshairLen = 12.0;
                    ctx.DrawLine(pen1, new Point(cR.X - crosshairLen, cR.Y), new Point(cR.X + crosshairLen, cR.Y));
                    ctx.DrawLine(pen1, new Point(cR.X, cR.Y - crosshairLen), new Point(cR.X, cR.Y + crosshairLen));

                    const double dist = crosshairLen * 0.70710678118; // Math.Sin(Math.PI / 4) * crosshairLen
                    ctx.DrawLine(pen2, new Point(cS.X - dist, cS.Y - dist), new Point(cS.X + dist, cS.Y + dist));
                    ctx.DrawLine(pen2, new Point(cS.X - dist, cS.Y + dist), new Point(cS.X + dist, cS.Y - dist));
                    
                    const double diaInn = 3.0;
                    ctx.DrawEllipse(Brushes.SlateBlue, this.selectionPen ??= new Pen(Brushes.BlueViolet, 2.0), cC, diaInn, diaInn);
                }
            }

            if (document.Canvas.SelectionRegion is RectangleSelection selection) {
                SKRectI r = selection.Rect;
                state ??= this.PushInverseScale(ctx, out scale);
                // Cache pens for performance
                const double thickness = 1.0;
                this.outlinePen1 ??= new Pen(Brushes.Black, thickness, this.dashStyle1 ??= new DashStyle(new double[] { 4, 4 }, 0));
                this.outlinePen2 ??= new Pen(Brushes.White, thickness, this.dashStyle2 ??= new DashStyle(new double[] { 4, 4 }, DashStrokeSize / 2.0 /* start half way */));

                // Can't seem to get minatureOffset to work correctly with this, sometimes the selection border
                // is outside the pixel, sometimes inside, i guess it's just a float rounding problem happening
                Rect theRect = new Rect(r.Left, r.Top, r.Right - r.Left, r.Bottom - r.Top);
                Rect finalRect = theRect * scale;
                ctx.DrawRectangle(null, this.outlinePen2, finalRect);
                ctx.DrawRectangle(null, this.outlinePen1, finalRect);
            }
        }

        state?.Dispose();
    }

    private DrawingContext.PushedState PushInverseScale(DrawingContext ctx, out double realScale) {
        // The ctx is relative to the fully translated and scaled view port.
        // This means that any drawing into it is scaled according to the zoom,
        // so we need to inverse it to get back to screen pixels.
        // Translations/Sizes have to be multiplied by realScale too, or drawings will
        // be positioned with the view port's translation but screen scale so it'd be all weird
        realScale = this.PART_FreeMoveViewPort?.ZoomScale ?? 1.0;
        double inverseScale = 1 / realScale;
        return ctx.PushTransform(Matrix.CreateScale(inverseScale, inverseScale));
    }

    public void RenderCanvas() {
        if (this.PART_SkiaViewPort != null && this.Document is Document document) {
            if (this.PART_SkiaViewPort.BeginRender(out SKSurface surface)) {
                surface.Canvas.Clear(SKColors.Transparent);
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
            control.UpdateViewPortSize(new PixSize(1, 1));
        }
    }

    private void OnActiveLayerChanged(Canvas canvas, BaseLayerTreeObject? oldactivelayertreeobject, BaseLayerTreeObject? newactivelayertreeobject) {
        this.UpdateCursorForActiveLayer(newactivelayertreeobject);
        this.rdaInvalidateRender.InvokeAsync();
    }

    private void OnEditorChanged(Editor? oldEditor, Editor? newEditor) {
    }

    private void UpdateCursorForActiveLayer(BaseLayerTreeObject? layer) {
        // this.Cursor = new Cursor(!(layer is RasterLayer) ? StandardCursorType.No : StandardCursorType.Cross);
        this.UpdateCursor();
    }

    private void UpdateCursor() {
        if (this.Document is Document document && document.Editor != null) {
            // BaseCanvasTool? tool = document.Editor.ToolBar.ActiveTool;
            // if (tool != null && tool.GetCursor() is Cursor cursor) {
            //     this.Cursor = cursor;
            //     return;
            // }

            this.Cursor = new Cursor(!(document.Canvas.ActiveLayerTreeObject is RasterLayer) ? StandardCursorType.No : StandardCursorType.Cross);
        }
    }

    private void OnResolutionChanged(Canvas canvas, PixSize oldSize, PixSize newSize) {
        this.UpdateViewPortSize(newSize);
    }

    private void UpdateViewPortSize(PixSize size) {
        if (this.PART_SkiaViewPort == null)
            return;

        this.PART_SkiaViewPort.Width = size.Width;
        this.PART_SkiaViewPort.Height = size.Height;
    }

    #region Drag drop

    private void OnDragEnter(DragEventArgs e) {
        this.OnDragOver(e);
    }

    private void OnDragOver(DragEventArgs e) {
        if (!(this.Document is Document document))
            return;

        EnumDropType effects = DropUtils.GetDropAction(e.KeyModifiers, (EnumDropType) e.DragEffects & EnumDropType.Copy);
        e.DragEffects = (DragDropEffects) CanvasDropRegistry.DropRegistry.CanDropNative(document.Canvas, new DataObjectWrapper(e.Data), effects);
    }

    private void OnDragLeave(DragEventArgs e) {
    }

    private async void OnDrop(DragEventArgs e) {
        e.Handled = true;
        if (!(this.Document is Document document))
            return;

        EnumDropType effects = DropUtils.GetDropAction(e.KeyModifiers, (EnumDropType) e.DragEffects & EnumDropType.Copy);
        try {
            this.isProcessingAsyncDrop = true;
            if (!await CanvasDropRegistry.DropRegistry.OnDroppedNative(document.Canvas, new DataObjectWrapper(e.Data), effects)) {
                await IoC.MessageService.ShowMessage("Unknown Data", "Unknown dropped item. Drop files here");
            }
        }
        finally {
            this.isProcessingAsyncDrop = false;
        }
    }

    #endregion
}