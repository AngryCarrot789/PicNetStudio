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

/// <summary>
/// Base class for a <see cref="BaseRasterisedDrawingTool"/> that uses a diameter based tool size
/// </summary>
public abstract class BaseDiameterTool : BaseRasterisedDrawingTool {
    public static readonly DataParameterFloat DiameterDataParameter = DataParameter.Register(new DataParameterFloat(typeof(BaseDiameterTool), nameof(Diameter), 10.0f, 0.5f, 1000f, ValueAccessors.Reflective<float>(typeof(BaseDiameterTool), nameof(diameter))));

    private float diameter = DiameterDataParameter.DefaultValue;

    public float Diameter {
        get => this.diameter;
        set => DataParameter.SetValueHelper(this, DiameterDataParameter, ref this.diameter, value);
    }

    protected BaseDiameterTool() {
        UpdateGap(this);
    }

    static BaseDiameterTool() {
        DiameterDataParameter.ValueChanged += OnDiameterDataParameterOnValueChanged;
        IsGapAutomaticParameter.ValueChanged += OnIsGapAutomaticParameterOnValueChanged;
    }

    private static void OnIsGapAutomaticParameterOnValueChanged(DataParameter parameter, ITransferableData owner) {
        if (owner is BaseDiameterTool tool) {
            UpdateGap(tool);
        }
    }

    private static void OnDiameterDataParameterOnValueChanged(DataParameter parameter, ITransferableData owner) {
        UpdateGap((BaseDiameterTool) owner);
    }

    private static void UpdateGap(BaseDiameterTool tool) {
        if (tool.IsGapAutomatic)
            tool.Gap = tool.Diameter / 8.0F;
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