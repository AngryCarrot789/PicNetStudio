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

using System.Threading.Tasks;
using PicNetStudio.Avalonia.CommandSystem;
using PicNetStudio.Avalonia.Interactivity.Contexts;
using PicNetStudio.Avalonia.PicNet.Layers;
using PicNetStudio.Avalonia.PicNet.Layers.Controls;
using PicNetStudio.Avalonia.Services.Messages;

namespace PicNetStudio.Avalonia.PicNet.Commands;

public class EditLayerNameCommand : AsyncDocumentCommand {
    protected override Executability CanExecute(Editor editor, Document document, CommandEventArgs e) {
        if (!DataKeys.LayerSelectionManagerKey.TryGetContext(e.ContextData, out ISelectionManager<BaseLayerTreeObject>? selectionManager))
            return Executability.Invalid;

        if (selectionManager.Count != 1)
            return Executability.ValidButCannotExecute;
        
        return e.ContextData.ContainsKey(DataKeys.LayerNodeKey) ? Executability.Valid : Executability.Invalid;
    }

    protected override async Task Execute(Editor editor, Document document, CommandEventArgs e) {
        if (!DataKeys.LayerSelectionManagerKey.TryGetContext(e.ContextData, out ISelectionManager<BaseLayerTreeObject>? selectionManager))
            return;

        if (selectionManager.Count != 1 || !DataKeys.LayerNodeKey.TryGetContext(e.ContextData, out ILayerNodeItem? node) || node.Layer == null)
            return;

        // node.EditNameState = true;

        SingleUserInputData data = new SingleUserInputData(node.Layer.Name) {
            ConfirmText = "Rename", Caption = "Rename this layer", Label = "Input a new name for this layer:", AllowEmptyText = false
        };

        if (await RZApplication.Instance.Services.GetService<IUserInputDialogService>().ShowInputDialogAsync(data) == true) {
            if (node.Layer != null)
                node.Layer.Name = data.Text!;
        }
    }
}