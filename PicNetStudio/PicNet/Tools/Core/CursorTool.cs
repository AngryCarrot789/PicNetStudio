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

using PicNetStudio.DataTransfer;
using PicNetStudio.PicNet.Layers;
using PicNetStudio.PicNet.Layers.Core;
using PicNetStudio.Utils;
using PicNetStudio.Utils.Accessing;
using SkiaSharp;

namespace PicNetStudio.PicNet.Tools.Core;

public class CursorTool : BaseCanvasTool {
    public static readonly DataParameterBool AutoSelectLayerParameter = DataParameter.Register(new DataParameterBool(typeof(CursorTool), nameof(AutoSelectLayer), false, ValueAccessors.Reflective<bool>(typeof(CursorTool), nameof(autoSelectLayer))));

    private bool autoSelectLayer;
    private BaseVisualLayer? targetLayer;
    private SKPoint originalPos;
    public bool AutoSelectLayer {
        get => this.autoSelectLayer;
        set => DataParameter.SetValueHelper(this, AutoSelectLayerParameter, ref this.autoSelectLayer, value);
    }

    public CursorTool() {
        this.autoSelectLayer = AutoSelectLayerParameter.GetDefaultValue(this);
    }

    public override bool OnCursorPressed(Document document, SKPointD relPos, SKPointD absPos, int count, EnumCursorType cursor, ModifierKeys modifiers) {
        if (modifiers != ModifierKeys.None || cursor != EnumCursorType.Primary) {
            return false;
        }
        
        BaseLayerTreeObject? layer = document.Canvas.ActiveLayerTreeObject;
        if (!(layer is BaseVisualLayer visualLayer)) { // || !layer.IsHitTest(x, y) // TODO: maybe automatic selection parameter?
            return false;
        }

        this.targetLayer = visualLayer;
        this.originalPos = visualLayer.Position - (SKPoint) absPos;
        return true;
    }

    public override bool OnCursorMoved(Document document, SKPointD relPos, SKPointD absPos, EnumCursorType cursorMask) {
        if (this.targetLayer == null) {
            return false;
        }

        SKPoint point = this.originalPos + (SKPoint) absPos;
        this.targetLayer.Position = point;
        return true;
    }

    public override bool OnCursorReleased(Document document, SKPointD relPos, SKPointD absPos, EnumCursorType cursor, ModifierKeys modifiers) {
        this.targetLayer = null;
        return false;
    }
}