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

using PicNetStudio.CommandSystem;
using PicNetStudio.Interactivity.Contexts;
using SkiaSharp;

namespace PicNetStudio.PicNet.Commands;

public class ExportCanvasToClipboardCommand : BaseExportCanvasCommand {
    public override SKEncodedImageFormat Format => SKEncodedImageFormat.Png;

    protected override Executability CanExecute(Editor editor, Document document, CommandEventArgs e) {
        return e.ContextData.ContainsKey(DataKeys.TopLevelHostKey) ? Executability.Valid : Executability.Invalid;
    }

    public override void AcceptPixels(SKData data, CommandEventArgs e) {
        // IClipboard? clipboard = AvCore.GetService<IClipboard>();
        // if (clipboard == null)
        //     return;
        // 
        // DataObject dataObject = new DataObject();
        // dataObject.Set(NativeDropTypes.Bitmap, data.ToArray());
        // clipboard.SetDataObjectAsync(dataObject);
    }
}