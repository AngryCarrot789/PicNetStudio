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
using PicNetStudio.Interactivity.Contexts;
using PicNetStudio.Utils;
using SkiaSharp;

namespace PicNetStudio.PicNet.Commands;

public class ExportImageCommand : AsyncCommand {
    protected override async Task ExecuteAsync(CommandEventArgs e) {
        if (!DataKeys.DocumentKey.TryGetContext(e.ContextData, out Document? document)) {
            if (!DataKeys.EditorKey.TryGetContext(e.ContextData, out Editor? editor) || (document = editor.ActiveDocument) == null) {
                return;
            }
        }

        string? file = await IoC.FilePickService.SaveFile("Quick export picture", null, "picture.png");
        if (file == null) {
            return;
        }

        PixSize size = document.Canvas.Size;
        PNBitmap bitmap = new PNBitmap();
        bitmap.InitialiseBitmap(size);

        using SKPixmap pixmap = new SKPixmap(bitmap.Bitmap!.Info, bitmap.ColourData);
        using SKSurface skSurface = SKSurface.Create(pixmap);
        RenderContext args = new RenderContext(document.Canvas, skSurface, RenderVisibilityFlag.ExportOnly, true);
        LayerRenderer.RenderCanvas(ref args);
        using SKImage image = skSurface.Snapshot();

        SKEncodedImageFormat type = SKEncodedImageFormat.Png;
        string fileName = file.ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(fileName))
            return;

        string extension = Path.GetExtension(fileName.ToUpperInvariant());
        if (!string.IsNullOrEmpty(extension)) {
            switch (extension) {
                case ".BMP":
                    type = SKEncodedImageFormat.Bmp;
                    break;
                case ".GIF":
                    type = SKEncodedImageFormat.Gif;
                    break;
                case ".ICO":
                    type = SKEncodedImageFormat.Ico;
                    break;
                case ".JPEG":
                    type = SKEncodedImageFormat.Jpeg;
                    break;
                case ".PNG":
                    type = SKEncodedImageFormat.Png;
                    break;
                case ".WBMP":
                    type = SKEncodedImageFormat.Wbmp;
                    break;
                case ".WEBP":
                    type = SKEncodedImageFormat.Webp;
                    break;
                case ".PKM":
                    type = SKEncodedImageFormat.Pkm;
                    break;
                case ".KTX":
                    type = SKEncodedImageFormat.Ktx;
                    break;
                case ".ASTC":
                    type = SKEncodedImageFormat.Astc;
                    break;
                case ".DNG":
                    type = SKEncodedImageFormat.Dng;
                    break;
                case ".HEIF":
                    type = SKEncodedImageFormat.Heif;
                    break;
                case ".AVIF":
                    type = SKEncodedImageFormat.Avif;
                    break;
            }
        }

        using SKData data = image.Encode(type, 80);
        await using Stream stream = File.OpenWrite(file);

        // save the data to a stream
        data.SaveTo(stream);
    }
}