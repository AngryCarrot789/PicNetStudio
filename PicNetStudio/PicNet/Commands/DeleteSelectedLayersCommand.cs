﻿// 
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
using PicNetStudio.PicNet.Layers;
using PicNetStudio.Services.Messaging;

namespace PicNetStudio.PicNet.Commands;

public class DeleteSelectedLayersCommand : AsyncDocumentCommand {
    protected override Executability CanExecute(Editor editor, Document document, CommandEventArgs e) {
        if (!DataKeys.LayerSelectionManagerKey.TryGetContext(e.ContextData, out ISelectionManager<BaseLayerTreeObject>? selectionManager))
            return Executability.Invalid;
        
        int count = selectionManager.Count;
        return count > 0 ? Executability.Valid : Executability.ValidButCannotExecute;
    }

    protected override async Task Execute(Editor editor, Document document, CommandEventArgs e) {
        if (!DataKeys.LayerSelectionManagerKey.TryGetContext(e.ContextData, out ISelectionManager<BaseLayerTreeObject>? manager))
            return;
        
        List<BaseLayerTreeObject> list = manager.SelectedItems.ToList();
        if (list.Count > 0) {
            MessageBoxInfo message = new MessageBoxInfo("Delete layers", $"Are you sure you want to delete {list.Count} layer{(list.Count == 1 ? "" : "s")}?") {
                Buttons = MessageBoxButton.OKCancel,
                YesOkText = "Delete",
                DefaultButton = MessageBoxResult.OK
            };

            MessageBoxResult result = await IoC.MessageService.ShowMessage(message);
            if (result != MessageBoxResult.OK) {
                return;
            }
            
            if (BaseLayerTreeObject.CheckHaveParentsAndAllMatch(manager, out CompositionLayer? sameParent)) {
                List<int> indices = list.Select(x => sameParent.IndexOf(x)).OrderBy(index => index).ToList();
                int minIndex = indices[0];
                manager.Clear();

                int count = 0;
                foreach (int indexOfLayer in indices) {
                    sameParent.RemoveLayerAt(indexOfLayer - count);
                    count++;
                }

                if (sameParent.Layers.Count > 0) {
                    if (minIndex >= sameParent.Layers.Count) {
                        minIndex = sameParent.Layers.Count - 1;
                    }

                    manager.Select(sameParent.Layers[minIndex]);
                }
                else if (sameParent is BaseLayerTreeObject parentLayer) {
                    manager.Select(parentLayer);
                }
            }
            else {
                manager.Clear();
                foreach (BaseLayerTreeObject layer in list) {
                    layer.ParentLayer?.RemoveLayer(layer);
                }
            }
        }
    }
}