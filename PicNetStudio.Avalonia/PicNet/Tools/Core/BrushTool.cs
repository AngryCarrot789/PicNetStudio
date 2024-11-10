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

using Avalonia.Input;
using PicNetStudio.Avalonia.CommandSystem;
using PicNetStudio.Avalonia.DataTransfer;
using PicNetStudio.Avalonia.PicNet.Commands;
using PicNetStudio.Avalonia.Utils.Accessing;
using SkiaSharp;

namespace PicNetStudio.Avalonia.PicNet.Tools.Core;

/// <summary>
/// The brush tool, which lets you draw circular arrangement of pixels on the canvas, with an adjustable diameter
/// </summary>
public class BrushTool : BaseRasterisedDrawingTool {
    public static readonly DataParameterFloat DiameterDataParameter =
        DataParameter.Register(
            new DataParameterFloat(
                typeof(BrushTool),
                nameof(Diameter), 5.0f, 0.5f, 1000f,
                ValueAccessors.Reflective<float>(typeof(BrushTool), nameof(diameter))));

    private float diameter = DiameterDataParameter.DefaultValue;

    public float Diameter {
        get => this.diameter;
        set => DataParameter.SetValueHelper(this, DiameterDataParameter, ref this.diameter, value);
    }

    public override double SpacingFeedback => this.Diameter / 2.0;

    public BrushTool() {
        this.CanDrawSecondaryColour = true;
    }

    public override void DrawPixels(PNBitmap bitmap, Document document, double x, double y, bool isPrimaryColour) {
        if (bitmap.Canvas == null)
            return;

        using SKPaint paint = new SKPaint();
        paint.Color = isPrimaryColour ? document.Editor!.PrimaryColour : document.Editor!.SecondaryColour;
        paint.IsAntialias = true;
        bitmap.Canvas.DrawCircle((float) x, (float) y, this.diameter, paint);
    }

    public override bool? OnCharacterPress(Document document, Key key, KeyModifiers modifiers) {
        if (key == Key.OemOpenBrackets)
            this.Diameter--;
        if (key == Key.OemCloseBrackets)
            this.Diameter++;
        return true;
    }
}

public class IncreaseBrushSizeCommand : ActiveToolCommand<BrushTool> {
    protected override void Execute(Editor editor, Document document, BrushTool tool, CommandEventArgs e) {
        tool.Diameter += 0.5f;
    }
}

public class DecreaseBrushSizeCommand : ActiveToolCommand<BrushTool> {
    protected override void Execute(Editor editor, Document document, BrushTool tool, CommandEventArgs e) {
        tool.Diameter -= 0.5f;
    }
}