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

using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Threading;
using PicNetStudio.Avalonia.PicNet.Controls;
using PicNetStudio.Avalonia.PicNet.Layers.Core;
using PicNetStudio.Avalonia.Utils;
using PicNetStudio.Avalonia.Utils.RDA;
using SkiaSharp;

namespace PicNetStudio.Avalonia.PicNet.Layers.Controls.Enlarged;

public abstract class BaseLayerPreviewControl : TemplatedControl {
    public static readonly ModelControlRegistry<BaseLayerTreeObject, BaseLayerPreviewControl> Registry;

    public BaseLayerTreeViewItem? LayerNode { get; private set; }

    public BaseLayerTreeObject? Layer { get; private set; }

    public BaseLayerPreviewControl() {
    }

    static BaseLayerPreviewControl() {
        Registry = new ModelControlRegistry<BaseLayerTreeObject, BaseLayerPreviewControl>();
        Registry.RegisterType<RasterLayer>(() => new RasterLayerPreviewControl());
        Registry.RegisterType<TextLayer>(() => new TextLayerPreviewControl());
    }

    public void Connect(BaseLayerTreeViewItem node, BaseLayerTreeObject layer) {
        this.LayerNode = node;
        this.Layer = layer;
        this.OnConnected();
    }

    public void Disconnect() {
        this.OnDisconnected();
        this.Layer = null;
        this.LayerNode = null;
    }

    protected virtual void OnConnected() {
    }

    protected virtual void OnDisconnected() {
    }
}

public class RasterLayerPreviewControl : BaseLayerPreviewControl {
    private SKAsyncViewPort? PART_ViewPortControl;
    private RateLimitedDispatchAction? rda;

    public RasterLayerPreviewControl() {
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        this.PART_ViewPortControl = e.NameScope.GetTemplateChild<SKAsyncViewPort>("PART_ViewPortControl");
    }

    protected override void OnConnected() {
        base.OnConnected();
        if (this.LayerNode is EnlargedLayerTreeViewItem item) {
            // This is required if the tree is created with Compact view mode before being added to visual tree
            item.Initialized += ItemOnInitialized;
            void ItemOnInitialized(object? sender, EventArgs e) {
                this.rda?.InvokeAsync();
                item.Initialized -= ItemOnInitialized;
            }

            if (this.rda == null)
                this.rda = new RateLimitedDispatchAction(this.DoRenderPreviewAsync, TimeSpan.FromSeconds(0.2));

            ((RasterLayer) this.Layer!).RenderInvalidated += this.OnLayerRenderInvalidated;
            this.rda?.InvokeAsync();
        }
    }

    private Task DoRenderPreviewAsync() {
        return Dispatcher.UIThread.InvokeAsync(this.DoRenderPreview).GetTask();
    }

    private void DoRenderPreview() {
        if (!(this.Layer is RasterLayer layer) || !(this.LayerNode is EnlargedLayerTreeViewItem item)) {
            return;
        }
        
        if (layer.Canvas != null && layer.Bitmap.IsInitialised && this.PART_ViewPortControl != null && this.PART_ViewPortControl!.BeginRender(out SKSurface surface)) {
            surface.Canvas.Clear(SKColor.Empty);
            Size sizeP = this.PART_ViewPortControl.Bounds.Size;
            PixelSize sizeD = layer.Canvas.Size;
            surface.Canvas.Scale((float) (sizeP.Width / sizeD.Width), (float) (sizeP.Height / sizeD.Height));
            
            RenderContext context = new RenderContext(layer.Canvas!, surface, RenderVisibilityFlag.PreviewOnly, true);
            LayerRenderer.RenderLayer(ref context, layer);
            this.PART_ViewPortControl!.EndRender();
        }
    }

    protected override void OnDisconnected() {
        base.OnDisconnected();
        this.rda?.CancelLast();
        this.PART_ViewPortControl?.DisposeBitmaps();
        if (this.LayerNode is EnlargedLayerTreeViewItem) {
            ((RasterLayer) this.Layer!).RenderInvalidated -= this.OnLayerRenderInvalidated;
        }
    }

    private void OnLayerRenderInvalidated(BaseVisualLayer layer) {
        if (this.LayerNode != null && this.LayerNode.IsInitialized && this.LayerNode.IsEffectivelyVisible) {
            this.rda?.InvokeAsync();
        }
    }

    public override void Render(DrawingContext context) {
        base.Render(context);
        context.DrawRectangle(TiledBrush.TiledTransparencyBrush4, null, new Rect(default, this.Bounds.Size));
    }
}

public class TextLayerPreviewControl : BaseLayerPreviewControl {
    public TextLayerPreviewControl() {
    }
}