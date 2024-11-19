using System.Threading.Tasks;
using Avalonia;
using PicNetStudio.Avalonia.CommandSystem;
using PicNetStudio.Avalonia.PicNet.Layers;
using PicNetStudio.Avalonia.PicNet.Layers.Core;
using PicNetStudio.Avalonia.Services.Messages;
using SkiaSharp;

namespace PicNetStudio.Avalonia.PicNet.Commands;

public class RasteriseLayerCommand : AsyncDocumentCommand {
    protected override async Task Execute(Editor editor, Document document, CommandEventArgs e) {
        if (document.Canvas.ActiveLayerTreeObject is BaseVisualLayer layer && layer.ParentLayer != null) {
            if (layer is RasterLayer)
                return;

            MessageBoxInfo info = new MessageBoxInfo("Rasterise layer", "Are you sure you want to convert this layer into a raster/pixel layer?") {Buttons = MessageBoxButton.OKCancel, YesOkText = "Yes"};
            if (await IoC.MessageService.ShowMessage(info) != MessageBoxResult.OK) {
                return;
            }
            
            PixelSize size = document.Canvas.Size;
            PNBitmap bitmap = new PNBitmap();
            bitmap.InitialiseBitmap(size);

            using SKPixmap pixmap = new SKPixmap(bitmap.Bitmap!.Info, bitmap.ColourData);
            using SKSurface skSurface = SKSurface.Create(pixmap);
            RenderContext args = new RenderContext(document.Canvas, skSurface, false, true);
            LayerRenderer.RenderLayer(ref args, layer);
            // using SKImage image = skSurface.Snapshot();

            CompositionLayer? parent = layer.ParentLayer;
            int index = parent.IndexOf(layer);
            parent.RemoveLayerAt(index);

            RasterLayer newLayer = new RasterLayer();
            newLayer.Bitmap.InitialiseBitmap(bitmap);
            
            parent.InsertLayer(index, newLayer);
        }
    }
}