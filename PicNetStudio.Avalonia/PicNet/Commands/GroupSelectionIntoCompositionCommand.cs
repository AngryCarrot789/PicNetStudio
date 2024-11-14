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

public class GroupSelectionIntoCompositionCommand : DocumentCommand {
    protected override Executability CanExecute(Editor editor, Document document, CommandEventArgs e) {
        IReadOnlySet<BaseLayerTreeObject> selection = document.Canvas.LayerSelectionManager.Selection;
        if (selection.Count < 1) {
            return Executability.ValidButCannotExecute;
        }

        if (selection.Count == 1) {
            return Executability.Valid;
        }

        return BaseLayerTreeObject.CheckHaveParentsAndAllMatch(document.Canvas.LayerSelectionManager, out _) ? Executability.Valid : Executability.ValidButCannotExecute;
    }

    protected override void Execute(Editor editor, Document document, CommandEventArgs e) {
        SelectionManager<BaseLayerTreeObject> selectionManager = document.Canvas.LayerSelectionManager;
        IReadOnlySet<BaseLayerTreeObject> selection = selectionManager.Selection;
        if (selection.Count < 1) {
            return;
        }

        CompositionLayer newCompositionLayer;
        CompositionLayer? theParent;
        if (selection.Count == 1) {
            BaseLayerTreeObject theLayer = selection.First();
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

        List<int> indices = selection.Select(x => theParent.IndexOf(x)).OrderBy(index => index).ToList();
        selectionManager.Clear();
        int minIndex = indices[0];

        int count = 0;
        newCompositionLayer = new CompositionLayer();
        foreach (int indexOfLayer in indices) {
            int actualIndex = indexOfLayer - count;
            BaseLayerTreeObject theLayer = theParent.Layers[actualIndex];
            theParent.RemoveLayerAt(actualIndex);
            newCompositionLayer.AddLayer(theLayer);
            count++;
        }

        theParent.InsertLayer(minIndex, newCompositionLayer);
    }
}

public class GroupSelectionIntoCompositionCommandUsage : SelectionBasedCommandUsage {
    public GroupSelectionIntoCompositionCommandUsage() : base("command.layertree.GroupSelectionIntoComposition") {
    }
}