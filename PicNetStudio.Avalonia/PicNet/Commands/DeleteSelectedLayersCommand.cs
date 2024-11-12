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

using System.Collections.Generic;
using System.Linq;
using PicNetStudio.Avalonia.CommandSystem;
using PicNetStudio.Avalonia.PicNet.Layers;

namespace PicNetStudio.Avalonia.PicNet.Commands;

public class DeleteSelectedLayersCommand : DocumentCommand {
    protected override Executability CanExecute(Editor editor, Document document, CommandEventArgs e) {
        int count = document.Canvas.LayerSelectionManager.Selection.Count;
        return count > 0 ? Executability.Valid : Executability.ValidButCannotExecute;
    }
    
    protected override void Execute(Editor editor, Document document, CommandEventArgs e) {
        List<BaseLayerTreeObject> list = document.Canvas.LayerSelectionManager.Selection.ToList();
        if (list.Count > 0) {
            document.Canvas.LayerSelectionManager.Clear();
            foreach (BaseLayerTreeObject layer in list) {
                BaseLayerTreeObject.RemoveFromTree(layer);
            }
        }
    }
}

public class DeleteSelectedLayersCommandUsage : SelectionBasedCommandUsage {
    public DeleteSelectedLayersCommandUsage() : base("command.layertree.DeleteSelectedLayers") {
    }
}