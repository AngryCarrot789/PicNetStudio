using Avalonia.Input;
using PicNetStudio.Avalonia.PicNet.Layers;
using PicNetStudio.Avalonia.PicNet.Layers.Core;
using PicNetStudio.Avalonia.Utils;
using SkiaSharp;

namespace PicNetStudio.Avalonia.PicNet.Tools.Core;

public class PointerTool : BaseCanvasTool {
    private BaseVisualLayer? targetLayer;
    private SKPoint originalPos;
    
    public override bool OnCursorPressed(Document document, SKPointD pos, SKPointD absPos, int count, EnumCursorType cursor, KeyModifiers modifiers) {
        BaseLayerTreeObject? layer = document.Canvas.ActiveLayerTreeObject;
        if (!(layer is BaseVisualLayer visualLayer)) { // || !layer.IsHitTest(x, y)
            return false;
        }

        this.targetLayer = visualLayer;
        this.originalPos = visualLayer.Position - (SKPoint) absPos;
        return true;
    }

    public override bool OnCursorMoved(Document document, SKPointD pos, SKPointD absPos, EnumCursorType cursorMask) {
        if (this.targetLayer == null) {
            return false;
        }

        SKPoint point = this.originalPos + (SKPoint) absPos;
        this.targetLayer.Position = point;
        return true;
    }
}