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
using System.Diagnostics;
using System.Threading.Tasks;
using PicNetStudio.Avalonia.Interactivity;

namespace PicNetStudio.Avalonia.PicNet.Layers {
    public static class ResourceDropRegistry {
        public static DragDropRegistry<BaseLayerTreeObject> DropRegistry { get; }

        /// <summary>
        /// The drag-drop identifier for a resource drag-drop
        /// </summary>
        public const string ResourceDropType = "PFXResource_DropType";

        static ResourceDropRegistry() {
            DropRegistry = new DragDropRegistry<BaseLayerTreeObject>();

            DropRegistry.Register<CompositionLayer, List<BaseLayerTreeObject>>((target, items, dropType, c) => {
                if (dropType == EnumDropType.None || dropType == EnumDropType.Link) {
                    return EnumDropType.None;
                }

                if (items.Count == 1) {
                    BaseLayerTreeObject item = items[0];
                    if (item is CompositionLayer folder && folder.IsParentInHierarchy(target)) {
                        return EnumDropType.None;
                    }
                    else if (dropType != EnumDropType.Copy) {
                        if (target.Contains(item)) {
                            return EnumDropType.None;
                        }
                    }
                }

                return dropType;
            }, (folder, resources, dropType, c) => {
                if (dropType != EnumDropType.Copy && dropType != EnumDropType.Move) {
                    return Task.CompletedTask;
                }

                List<BaseLayerTreeObject> items = new List<BaseLayerTreeObject>();
                foreach (BaseLayerTreeObject resource in resources) {
                    if (resource is CompositionLayer group && @group.IsParentInHierarchy(folder)) {
                        continue;
                    }

                    // if (dropType == EnumDropType.Copy) {
                    //     BaseLayerTreeObject clone = BaseLayerTreeObject.Clone(resource);
                    //     if (!TextIncrement.GetIncrementableString(folder.IsNameFree, clone.DisplayName, out string name))
                    //         name = clone.DisplayName;
                    //     clone.DisplayName = name;
                    //     folder.AddItem(clone);
                    //     if (clone is ResourceItem)
                    //         items.Add((ResourceItem) clone);
                    // }
                    /* else */ 
                    if (resource.ParentLayer != null) {
                        if (resource.ParentLayer != folder) {
                            // drag dropped a resource into the same folder
                            resource.ParentLayer.MoveItemTo(folder, resource);
                        }
                    }
                    else {
                        Debug.Assert(false, "No parent");
                        // ???
                        // AppLogger.Instance.WriteLine("A resource was dropped with a null parent???");
                    }
                }

                // ResourceLoaderDialog.TryLoadResources(items);

                return Task.CompletedTask;
            });
        }
    }
}