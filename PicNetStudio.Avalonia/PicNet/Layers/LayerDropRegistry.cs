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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using PicNetStudio.Avalonia.Interactivity;
using PicNetStudio.Avalonia.Interactivity.Contexts;
using PicNetStudio.Avalonia.PicNet.Layers.Core;
using PicNetStudio.Avalonia.Utils;
using SkiaSharp;

namespace PicNetStudio.Avalonia.PicNet.Layers;

public static class LayerDropRegistry {
    public static readonly DataKey<bool> IsAboveTarget = DataKey<bool>.Create("LayerDropIsAboveTarget");

    public static DragDropRegistry<BaseLayerTreeObject> DropRegistry { get; }

    /// <summary>
    /// The drag-drop identifier for a resource drag-drop
    /// </summary>
    public const string DropTypeText = "PicNetLayer_DropType";

    static LayerDropRegistry() {
        DropRegistry = new DragDropRegistry<BaseLayerTreeObject>();
        DropRegistry.RegisterNative<BaseLayerTreeObject>(NativeDropTypes.Files, CanDrop, OnDropped);

        // Drop into composition layer is handled manually since we need
        // to handle inter-layer drop to move layers around. See BaseLayerTreeViewItem
        // DropRegistry.Register<CompositionLayer, List<BaseLayerTreeObject>>((target, items, dropType, c) => {
        //     return CanDropItems(target, items, dropType, false);
        // }, (folder, layerList, dropType, c) => {
        //     
        //     return Task.CompletedTask;
        // });
    }

    private static EnumDropType CanDrop(BaseLayerTreeObject obj, IDataObjekt objekt, EnumDropType allowedDropType, IContextData arg4) {
        return objekt.GetDataPresent(NativeDropTypes.Files) && arg4.ContainsKey(IsAboveTarget) ? (EnumDropType.Copy) : EnumDropType.None;
    }

    private static async Task OnDropped(BaseLayerTreeObject layer, IDataObjekt objekt, EnumDropType dropType, IContextData context) {
        Canvas? canvas = layer.Canvas;
        if (canvas != null && objekt.GetData(NativeDropTypes.Files) is IEnumerable<IStorageItem> fileEnumerator) {
            if (!IsAboveTarget.TryGetContext(context, out bool isDropAbove)) {
                return;
            }

            List<IStorageItem> files = fileEnumerator.ToList();
            if (files.Count != 1) {
                return;
            }

            RasterLayer newLayer = new RasterLayer();
            SKBitmap? bmp = null;
            try {
                using BufferedStream stream = new BufferedStream(File.OpenRead(files[0].Path.AbsolutePath), 32768);
                bmp = SKBitmap.Decode(stream);
                if (bmp == null) {
                    await IoC.MessageService.ShowMessage("Invalid image", "The file is not a valid image file or could not be decoded");
                    return;
                }
            }
            catch (Exception e) {
                bmp?.Dispose();
                bmp = null;
                await IoC.MessageService.ShowMessage("Error", "An error occurred loading the file", e.GetToString());
            }
            
            newLayer.Bitmap.InitialiseUsingBitmap(bmp);
            CompositionLayer target = layer.ParentLayer ?? canvas.RootComposition;
            int index = target.IndexOf(layer);

            target.InsertLayer(isDropAbove ? index : (index + 1), newLayer);
            if (DataKeys.LayerSelectionManagerKey.TryGetContext(context, out ISelectionManager<BaseLayerTreeObject>? selectionManager)) {
                selectionManager.SetSelection(newLayer);
            }
        }
    }
}