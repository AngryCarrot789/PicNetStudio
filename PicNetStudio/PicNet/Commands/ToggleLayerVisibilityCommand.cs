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
using PicNetStudio.PicNet.Layers;
using PicNetStudio.PicNet.Layers.Core;
using PicNetStudio.Utils;

namespace PicNetStudio.PicNet.Commands;

public class ToggleLayerVisibilityCommand : Command {
    public override Executability CanExecute(CommandEventArgs e) {
        return DataKeys.LayerSelectionManagerKey.TryGetContext(e.ContextData, out _) ? Executability.Valid : Executability.Invalid;
    }

    protected override void Execute(CommandEventArgs e) {
        if (!DataKeys.LayerSelectionManagerKey.TryGetContext(e.ContextData, out ISelectionManager<BaseLayerTreeObject>? selectionManager)) {
            return;
        }

        int count = selectionManager.Count;
        if (count < 1) {
            return;
        }
        
        List<BaseVisualLayer> layers = selectionManager.SelectedItems.OfType<BaseVisualLayer>().ToList();
        if (layers.Count < 1)
            return;
        
        if (layers.Count == 1) {
            layers[0].IsVisible = !layers[0].IsVisible;
        }
        else {
            // Initially try to get equal value and invert, or if there's differing values, set to false.
            // GetEqualVisibility(layers, out bool visibility) ? (!visibility) : false 
            bool state = CollectionUtils.GetEqualValue(layers, (x) => x.IsVisible, out bool visibility) && !visibility;
            foreach (BaseVisualLayer layer in layers) {
                layer.IsVisible = state;
            }
        }
    }
}