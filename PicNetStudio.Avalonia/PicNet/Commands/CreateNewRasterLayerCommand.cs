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

using System.Linq;
using PicNetStudio.Avalonia.CommandSystem;
using PicNetStudio.Avalonia.PicNet.Layers;

namespace PicNetStudio.Avalonia.PicNet.Commands;

public class CreateNewRasterLayerCommand : DocumentCommand {
    protected override Executability CanExecute(Editor editor, Document document, CommandEventArgs e) {
        int count = document.Canvas.LayerSelectionManager.Selection.Count;
        return count > 1 ? Executability.ValidButCannotExecute : Executability.Valid;
    }

    protected override void Execute(Editor editor, Document document, CommandEventArgs e) {
        int count = document.Canvas.LayerSelectionManager.Selection.Count;
        if (count > 1) {
            return;
        }

        RasterLayer raster = new RasterLayer();
        raster.Bitmap.InitialiseBitmap(document.Canvas.Size);

        if (count == 1) {
            BaseLayerTreeObject selection = document.Canvas.LayerSelectionManager.Selection.First();
            CompositionLayer target = selection.ParentLayer ?? document.Canvas.RootComposition;
            target.InsertLayer(target.IndexOf(selection), raster);
        }
        else {
            document.Canvas.RootComposition.InsertLayer(0, raster);
        }

        document.Canvas.LayerSelectionManager.SetSelection(raster);
    }
}

public class CreateNewRasterLayerCommandUsage : SelectionBasedCommandUsage {
    public CreateNewRasterLayerCommandUsage() : base("command.layertree.CreateNewRasterLayer") {
    }
}