using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using Avalonia;
using PicNetStudio.Avalonia.CommandSystem;
using PicNetStudio.Avalonia.Interactivity.Contexts;
using PicNetStudio.Avalonia.PicNet.Layers;
using PicNetStudio.Avalonia.PicNet.Layers.Core;
using SkiaSharp;

namespace PicNetStudio.Avalonia.PicNet.Commands;

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

    public static PixelSize? Max(IEnumerable<PixelSize> sources) {
        using (IEnumerator<PixelSize> enumerator = sources.GetEnumerator()) {
            if (!enumerator.MoveNext())
                return null;

            PixelSize size = enumerator.Current;
            int w = size.Width, h = size.Height;
            while (enumerator.MoveNext()) {
                PixelSize next = enumerator.Current;
                if (next.Width > w)
                    w = next.Width;
                if (next.Height > h)
                    h = next.Height;
            }

            return new PixelSize(w, h);
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
        
        PixelSize size = Max(items.Select(x => x is RasterLayer raster ? raster.Bitmap.Size : x.Canvas!.Size)) ?? default;
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

public class MergeSelectionIntoRasterCommandUsage : SelectionBasedCommandUsage {
    public MergeSelectionIntoRasterCommandUsage() : base("command.layertree.MergeSelectionIntoRaster") {
    }
}