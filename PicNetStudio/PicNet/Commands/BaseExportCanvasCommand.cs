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
using PicNetStudio.Utils;
using SkiaSharp;

namespace PicNetStudio.PicNet.Commands;

public abstract class BaseExportCanvasCommand : DocumentCommand {
    public abstract SKEncodedImageFormat Format { get; }

    protected BaseExportCanvasCommand() {
    }

    public abstract void AcceptPixels(SKData data, CommandEventArgs commandEventArgs);

    protected override void Execute(Editor editor, Document document, CommandEventArgs e) {
        if (this.CanExecute(e) != Executability.Valid)
            return;
        
        PixSize size = document.Canvas.Size;
        PNBitmap bitmap = new PNBitmap();
        bitmap.InitialiseBitmap(size);

        using SKPixmap pixmap = new SKPixmap(bitmap.Bitmap!.Info, bitmap.ColourData);
        using SKSurface skSurface = SKSurface.Create(pixmap);
        RenderContext args = new RenderContext(document.Canvas, skSurface, RenderVisibilityFlag.ExportOnly, true);
        LayerRenderer.RenderCanvas(ref args);
        using SKImage image = skSurface.Snapshot();

        using SKData data = image.Encode(this.Format, 80);
        this.AcceptPixels(data, e);
    }
}