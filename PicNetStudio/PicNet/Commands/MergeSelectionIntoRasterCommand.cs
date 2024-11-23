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

using System.Diagnostics;
using PicNetStudio.CommandSystem;
using PicNetStudio.Interactivity.Contexts;
using PicNetStudio.PicNet.Layers;
using PicNetStudio.PicNet.Layers.Core;
using PicNetStudio.Utils;
using SkiaSharp;

namespace PicNetStudio.PicNet.Commands;

public class MergeSelectionIntoRasterCommand : DocumentCommand {
    protected override Executability CanExecute(Editor editor, Document document, CommandEventArgs e) {
        if (!DataKeys.LayerSelectionManagerKey.TryGetContext(e.ContextData, out ISelectionManager<BaseLayerTreeObject>? manager))
            return Executability.Invalid;

        if (manager.Count < 1)
            return Executability.ValidButCannotExecute;
        if (manager.Count == 1)
            return Executability.Valid;

        return BaseLayerTreeObject.CheckHaveParentsAndAllMatch(manager, out _) ? Executability.Valid : Executability.ValidButCannotExecute;
    }

    public static PixSize? Max(IEnumerable<PixSize> sources) {
        using (IEnumerator<PixSize> enumerator = sources.GetEnumerator()) {
            if (!enumerator.MoveNext())
                return null;

            PixSize size = enumerator.Current;
            int w = size.Width, h = size.Height;
            while (enumerator.MoveNext()) {
                PixSize next = enumerator.Current;
                if (next.Width > w)
                    w = next.Width;
                if (next.Height > h)
                    h = next.Height;
            }

            return new PixSize(w, h);
        }
    }

    protected override void Execute(Editor editor, Document document, CommandEventArgs e) {
        if (!DataKeys.LayerSelectionManagerKey.TryGetContext(e.ContextData, out ISelectionManager<BaseLayerTreeObject>? selectionManager))
            return;

        if (selectionManager.Count < 1) {
            return;
        }

        if (!BaseLayerTreeObject.CheckHaveParentsAndAllMatch(selectionManager, out CompositionLayer? parent)) {
            return;
        }

        List<BaseLayerTreeObject> items = selectionManager.SelectedItems.ToList();
        List<int> indices = items.Select(x => parent.IndexOf(x)).OrderBy(index => index).ToList();
        selectionManager.Clear();
        int minIndex = indices[0];
        Debug.Assert(minIndex >= 0, "An item was not in a parent");
        
        PixSize size = Max(items.Select(x => x is RasterLayer raster ? raster.Bitmap.Size : x.Canvas!.Size)) ?? default;
        RasterLayer raster = new RasterLayer() {
            Name = "Merged layer"
        };
        
        raster.Bitmap.InitialiseBitmap(size);

        using SKPixmap pixmap = new SKPixmap(raster.Bitmap.Bitmap!.Info, raster.Bitmap.ColourData);
        using SKSurface skSurface = SKSurface.Create(pixmap);
        RenderContext args = new RenderContext(document.Canvas, skSurface, RenderVisibilityFlag.PreviewOnly, true);

        for (int i = indices.Count - 1; i >= 0; i--) {
            int index = indices[i];
            if (parent.Layers[index] is BaseVisualLayer visual)
                LayerRenderer.RenderLayer(ref args, visual);

            parent.RemoveLayerAt(index);
        }

        parent.InsertLayer(minIndex, raster);
        selectionManager.SetSelection(raster);
    }
}