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

using PicNetStudio.Avalonia.CommandSystem;
using PicNetStudio.Avalonia.DataTransfer;
using PicNetStudio.Avalonia.PicNet.Commands;
using PicNetStudio.Avalonia.Utils.Accessing;

namespace PicNetStudio.Avalonia.PicNet.Tools.Core;

public abstract class BaseDiameterTool : BaseRasterisedDrawingTool {
    public static readonly DataParameterFloat DiameterDataParameter =
        DataParameter.Register(
            new DataParameterFloat(
                typeof(BaseDiameterTool),
                nameof(Diameter), 5.0f, 0.5f, 1000f,
                ValueAccessors.Reflective<float>(typeof(BaseDiameterTool), nameof(diameter))));

    private float diameter = DiameterDataParameter.DefaultValue;

    public float Diameter {
        get => this.diameter;
        set => DataParameter.SetValueHelper(this, DiameterDataParameter, ref this.diameter, value);
    }

    public override double SpacingFeedback => this.Diameter / 2.0;

    protected BaseDiameterTool() {
    }
}

public class IncreaseBaseDiameterToolSizeCommand : ActiveToolCommand<BaseDiameterTool> {
    protected override void Execute(Editor editor, Document document, BaseDiameterTool tool, CommandEventArgs e) {
        tool.Diameter += 0.5f;
    }
}

public class DecreaseBaseDiameterToolSizeCommand : ActiveToolCommand<BaseDiameterTool> {
    protected override void Execute(Editor editor, Document document, BaseDiameterTool tool, CommandEventArgs e) {
        tool.Diameter -= 0.5f;
    }
}