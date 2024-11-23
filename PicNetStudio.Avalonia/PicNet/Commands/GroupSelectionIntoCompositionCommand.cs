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
using PicNetStudio.Avalonia.Interactivity.Contexts;
using PicNetStudio.Avalonia.PicNet.Layers;

namespace PicNetStudio.Avalonia.PicNet.Commands;

public class GroupSelectionIntoCompositionCommand : DocumentCommand {
    protected override Executability CanExecute(Editor editor, Document document, CommandEventArgs e) {
        if (!DataKeys.LayerSelectionManagerKey.TryGetContext(e.ContextData, out ISelectionManager<BaseLayerTreeObject>? manager))
            return Executability.Invalid;
        
        if (manager.Count < 1)
            return Executability.ValidButCannotExecute;
        if (manager.Count == 1)
            return Executability.Valid;

        return BaseLayerTreeObject.CheckHaveParentsAndAllMatch(manager, out _) ? Executability.Valid : Executability.ValidButCannotExecute;
    }

    protected override void Execute(Editor editor, Document document, CommandEventArgs e) {
        if (!DataKeys.LayerSelectionManagerKey.TryGetContext(e.ContextData, out ISelectionManager<BaseLayerTreeObject>? selectionManager))
            return;
        
        if (selectionManager.Count < 1) {
            return;
        }

        CompositionLayer newCompositionLayer;
        CompositionLayer? theParent;
        if (selectionManager.Count == 1) {
            BaseLayerTreeObject theLayer = selectionManager.SelectedItems.First();
            if ((theParent = theLayer.ParentLayer) != null) {
                int index = theParent.IndexOf(theLayer);
                theParent.RemoveLayerAt(index);

                newCompositionLayer = new CompositionLayer();
                newCompositionLayer.AddLayer(theLayer);
                theParent.InsertLayer(index, newCompositionLayer);
            }


            return;
        }

        if (!BaseLayerTreeObject.CheckHaveParentsAndAllMatch(selectionManager, out theParent)) {
            return;
        }

        List<int> indices = selectionManager.SelectedItems.Select(x => theParent.IndexOf(x)).OrderBy(index => index).ToList();
        selectionManager.Clear();
        int minIndex = indices[0];

        List<BaseLayerTreeObject> layers = new List<BaseLayerTreeObject>();
        newCompositionLayer = new CompositionLayer();
        for (int i = indices.Count - 1; i >= 0; i--) {
            int index = indices[i];
            BaseLayerTreeObject theLayer = theParent.Layers[index];
            theParent.RemoveLayerAt(index);
            layers.Add(theLayer);
        }

        layers.Reverse();
        newCompositionLayer.AddLayers(layers);
        
        theParent.InsertLayer(minIndex, newCompositionLayer);
    }
}

public class GroupSelectionIntoCompositionCommandUsage : SelectionBasedCommandUsage {
    public GroupSelectionIntoCompositionCommandUsage() : base("command.layertree.GroupSelectionIntoComposition") {
    }
}