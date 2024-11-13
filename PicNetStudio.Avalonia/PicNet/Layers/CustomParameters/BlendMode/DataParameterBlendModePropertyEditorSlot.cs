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
using PicNetStudio.Avalonia.DataTransfer;
using PicNetStudio.Avalonia.PicNet.PropertyEditing.DataTransfer;
using SkiaSharp;

namespace PicNetStudio.Avalonia.PicNet.Layers.CustomParameters.BlendMode;

public class DataParameterBlendModePropertyEditorSlot : DataParameterEnumPropertyEditorSlot<SKBlendMode> {
    public static readonly DataParameterEnumInfo<SKBlendMode> EnumInfo;

    public DataParameterBlendModePropertyEditorSlot(DataParameter<SKBlendMode> parameter, Type applicableType, string displayName = "Blend Mode") : base(parameter, applicableType, displayName, null, EnumInfo) {
    }

    static DataParameterBlendModePropertyEditorSlot() {
        List<SKBlendMode> blendModeList = new List<SKBlendMode>() {
            SKBlendMode.SrcOver,
            SKBlendMode.Multiply,
            SKBlendMode.Src,
            SKBlendMode.SrcIn,
            SKBlendMode.SrcOut,
            SKBlendMode.SrcATop,
            SKBlendMode.Dst,
            SKBlendMode.DstOver,
            SKBlendMode.DstIn,
            SKBlendMode.DstOut,
            SKBlendMode.DstATop,
            SKBlendMode.Clear,
            SKBlendMode.Xor,
            SKBlendMode.Plus,
            SKBlendMode.Modulate,
            SKBlendMode.Screen,
            SKBlendMode.Overlay,
            SKBlendMode.Darken,
            SKBlendMode.Lighten,
            SKBlendMode.ColorDodge,
            SKBlendMode.ColorBurn,
            SKBlendMode.HardLight,
            SKBlendMode.SoftLight,
            SKBlendMode.Difference,
            SKBlendMode.Exclusion,
            SKBlendMode.Hue,
            SKBlendMode.Saturation,
            SKBlendMode.Color,
            SKBlendMode.Luminosity
        };
        
        Dictionary<SKBlendMode, string> translationMap =new Dictionary<SKBlendMode, string> {
            [SKBlendMode.Clear] = "Clear",
            [SKBlendMode.Src] = "Source",
            [SKBlendMode.Dst] = "Destination",
            [SKBlendMode.SrcOver] = "Source Over (Default)",
            [SKBlendMode.DstOver] = "Destination Over",
            [SKBlendMode.SrcIn] = "Source (In)",
            [SKBlendMode.DstIn] = "Destination (In)",
            [SKBlendMode.SrcOut] = "Source (Out)",
            [SKBlendMode.DstOut] = "Destination (Out)",
            [SKBlendMode.SrcATop] = "Source (Atop)",
            [SKBlendMode.DstATop] = "Destination (Atop)",
            [SKBlendMode.Xor] = "Xor",
            [SKBlendMode.Plus] = "Plus",
            [SKBlendMode.Modulate] = "Modulate",
            [SKBlendMode.Screen] = "Screen",
            [SKBlendMode.Overlay] = "Overlay",
            [SKBlendMode.Darken] = "Darken",
            [SKBlendMode.Lighten] = "Lighten",
            [SKBlendMode.ColorDodge] = "Color Dodge",
            [SKBlendMode.ColorBurn] = "Color Burn",
            [SKBlendMode.HardLight] = "Hard Light",
            [SKBlendMode.SoftLight] = "Soft Light",
            [SKBlendMode.Difference] = "Difference",
            [SKBlendMode.Exclusion] = "Exclusion",
            [SKBlendMode.Multiply] = "Multiply",
            [SKBlendMode.Hue] = "Hue",
            [SKBlendMode.Saturation] = "Saturation",
            [SKBlendMode.Color] = "Color",
            [SKBlendMode.Luminosity] = "Luminosity"
        };

        EnumInfo = new DataParameterEnumInfo<SKBlendMode>(blendModeList, translationMap);
    }
}