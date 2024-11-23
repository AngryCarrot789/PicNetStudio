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

using PicNetStudio.CommandSystem;
using PicNetStudio.PicNet.Layers;
using PicNetStudio.PicNet.Layers.Core;
using PicNetStudio.Services.Messaging;
using PicNetStudio.Utils;
using SkiaSharp;

namespace PicNetStudio.PicNet.Commands;

public class RasteriseLayerCommand : AsyncDocumentCommand {
    protected override async Task Execute(Editor editor, Document document, CommandEventArgs e) {
        if (document.Canvas.ActiveLayerTreeObject is BaseVisualLayer layer && layer.ParentLayer != null) {
            if (layer is RasterLayer)
                return;

            MessageBoxInfo info = new MessageBoxInfo("Rasterise layer", "Are you sure you want to convert this layer into a raster/pixel layer?") {Buttons = MessageBoxButton.OKCancel, YesOkText = "Yes"};
            if (await IoC.MessageService.ShowMessage(info) != MessageBoxResult.OK) {
                return;
            }
            
            PixSize size = document.Canvas.Size;
            PNBitmap bitmap = new PNBitmap();
            bitmap.InitialiseBitmap(size);

            using SKPixmap pixmap = new SKPixmap(bitmap.Bitmap!.Info, bitmap.ColourData);
            using SKSurface skSurface = SKSurface.Create(pixmap);
            RenderContext args = new RenderContext(document.Canvas, skSurface, RenderVisibilityFlag.Ignored, true);
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