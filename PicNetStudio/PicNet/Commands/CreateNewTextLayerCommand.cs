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
using PicNetStudio.PicNet.Layers;
using PicNetStudio.PicNet.Layers.Core;

namespace PicNetStudio.PicNet.Commands;

public class CreateNewTextLayerCommand : DocumentCommand {
    protected override Executability CanExecute(Editor editor, Document document, CommandEventArgs e) {
        if (!DataKeys.LayerSelectionManagerKey.TryGetContext(e.ContextData, out ISelectionManager<BaseLayerTreeObject>? selectionManager))
            return Executability.Invalid;
        
        int count = selectionManager.Count;
        return count > 1 ? Executability.ValidButCannotExecute : Executability.Valid;
    }

    protected override void Execute(Editor editor, Document document, CommandEventArgs e) {
        if (!DataKeys.LayerSelectionManagerKey.TryGetContext(e.ContextData, out ISelectionManager<BaseLayerTreeObject>? selectionManager))
            return;
        
        int count = selectionManager.Count;
        if (count > 1) {
            return;
        }

        TextLayer layer = new TextLayer() {
            Text = "The big fox whatever sentence?!"
        };
        
        if (count == 1) {
            BaseLayerTreeObject selection = selectionManager.SelectedItems.First();
            CompositionLayer target = selection.ParentLayer ?? document.Canvas.RootComposition;
            target.InsertLayer(target.IndexOf(selection), layer);
        }
        else {
            document.Canvas.RootComposition.InsertLayer(0, layer);
        }

        selectionManager.SetSelection(layer);
    }
}