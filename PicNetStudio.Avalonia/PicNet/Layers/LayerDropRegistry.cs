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

using PicNetStudio.Avalonia.Interactivity;

namespace PicNetStudio.Avalonia.PicNet.Layers;

public static class LayerDropRegistry {
    public static DragDropRegistry<BaseLayerTreeObject> DropRegistry { get; }

    /// <summary>
    /// The drag-drop identifier for a resource drag-drop
    /// </summary>
    public const string DropTypeText = "PicNetLayer_DropType";
    
    static LayerDropRegistry() {
        DropRegistry = new DragDropRegistry<BaseLayerTreeObject>();

        // Drop into composition layer is handled manually since we need
        // to handle inter-layer drop to move layers around. See BaseLayerTreeViewItem
        // DropRegistry.Register<CompositionLayer, List<BaseLayerTreeObject>>((target, items, dropType, c) => {
        //     return CanDropItems(target, items, dropType, false);
        // }, (folder, layerList, dropType, c) => {
        //     
        //     return Task.CompletedTask;
        // });
    }
}