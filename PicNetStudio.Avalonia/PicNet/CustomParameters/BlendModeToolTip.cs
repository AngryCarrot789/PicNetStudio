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
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Primitives;
using PicNetStudio.Avalonia.Utils;
using SkiaSharp;

namespace PicNetStudio.Avalonia.PicNet.CustomParameters;

public class BlendModeToolTip : TemplatedControl {
    private static readonly Dictionary<SKBlendMode, EnumDescriptor> descriptors;

    private struct EnumDescriptor {
        public readonly string EnumName;
        public readonly string Header;
        public readonly string Description;

        public EnumDescriptor() {
        }

        public EnumDescriptor(string enumName, string header, string description) {
            this.EnumName = enumName;
            this.Header = header;
            this.Description = description;
        }
    }

    private bool isTemplatedApplied;
    public TextBlock PART_TextBlock;
    public Run PART_HeaderRun;
    public Run PART_EnumNameRun;
    public Run PART_DescriptionRun;

    public static readonly StyledProperty<SKBlendMode> CurrentBlendModeProperty = AvaloniaProperty.Register<BlendModeToolTip, SKBlendMode>("CurrentBlendMode");

    public SKBlendMode CurrentBlendMode {
        get => this.GetValue(CurrentBlendModeProperty);
        set => this.SetValue(CurrentBlendModeProperty, value);
    }
    
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        this.PART_TextBlock = e.NameScope.GetTemplateChild<TextBlock>(nameof(this.PART_TextBlock));
        this.PART_HeaderRun = e.NameScope.GetTemplateChild<Run>(nameof(this.PART_HeaderRun));
        this.PART_EnumNameRun = e.NameScope.GetTemplateChild<Run>(nameof(this.PART_EnumNameRun));
        this.PART_DescriptionRun = e.NameScope.GetTemplateChild<Run>(nameof(this.PART_DescriptionRun));
        this.isTemplatedApplied = true;
        this.UpdateValues(this.CurrentBlendMode);
    }

    private void UpdateValues(SKBlendMode newBlendMode) {
        if (!this.isTemplatedApplied)
            return;
        
        if (descriptors.TryGetValue(newBlendMode, out EnumDescriptor descriptor)) {
            this.PART_HeaderRun.Text = descriptor.Header;
            this.PART_EnumNameRun.Text = descriptor.EnumName;
            this.PART_DescriptionRun.Text = descriptor.Description;
        }
        else {
            this.PART_HeaderRun.Text = "error";
            this.PART_EnumNameRun.Text = "error";
            this.PART_DescriptionRun.Text = "error";
        }
    }

    public BlendModeToolTip() {
    }

    static BlendModeToolTip() {
        descriptors = new Dictionary<SKBlendMode, EnumDescriptor> {
            [SKBlendMode.Clear]      = new EnumDescriptor("Clear",       "None",                 "No regions are enabled, the area is transparent"),
            [SKBlendMode.Src]        = new EnumDescriptor("Src",         "None/Copy/Replace",    "Draws only the source image, ignoring destination"),
            [SKBlendMode.Dst]        = new EnumDescriptor("Dst",         "None/Keep Background", "Draws only the destination image, ignoring source"),
            [SKBlendMode.SrcOver]    = new EnumDescriptor("SrcOver",     "Normal",               "Source is drawn over destination with transparency support"),
            [SKBlendMode.DstOver]    = new EnumDescriptor("DstOver",     "Behind",               "Destination drawn over source, source shows through if transparent"),
            [SKBlendMode.SrcIn]      = new EnumDescriptor("SrcIn",       "Mask",                 "Only keeps the parts of the source that overlap with the destination"),
            [SKBlendMode.DstIn]      = new EnumDescriptor("DstIn",       "Mask Inverse",         "Only keeps the parts of the destination that overlap with the source"),
            [SKBlendMode.SrcOut]     = new EnumDescriptor("SrcOut",      "Knockout",             "Only keeps the parts of the source outside the destination area"),
            [SKBlendMode.DstOut]     = new EnumDescriptor("DstOut",      "Knockout Inverse",     "Only keeps the parts of the destination outside the source area"),
            [SKBlendMode.SrcATop]    = new EnumDescriptor("SrcATop",     "Source Atop",          "Source drawn on top but only within the destination bounds"),
            [SKBlendMode.DstATop]    = new EnumDescriptor("DstATop",     "Destination Atop",     "Destination drawn on top but only within the source bounds"),
            [SKBlendMode.Xor]        = new EnumDescriptor("Xor",         "Difference",           "Shows the non-overlapping parts of source and destination"),
            [SKBlendMode.Plus]       = new EnumDescriptor("Plus",        "Add",                  "Combines brightness values of source and destination, resulting in a lighter image"),
            [SKBlendMode.Modulate]   = new EnumDescriptor("Modulate",    "Multiply",             "Multiplies pixel values of source and destination, darkening the result"),
            [SKBlendMode.Screen]     = new EnumDescriptor("Screen",      "Screen",               "Inverts both images, multiplies them, then inverts the result, creating a lighter effect"),
            [SKBlendMode.Overlay]    = new EnumDescriptor("Overlay",     "Overlay",              "Combination of Multiply and Screen based on brightness, adding contrast"),
            [SKBlendMode.Darken]     = new EnumDescriptor("Darken",      "Darken",               "Retains the darker pixels of source or destination"),
            [SKBlendMode.Lighten]    = new EnumDescriptor("Lighten",     "Lighten",              "Retains the lighter pixels of source or destination"),
            [SKBlendMode.ColorDodge] = new EnumDescriptor("ColorDodge",  "Color Dodge",          "Brightens the destination based on the source"),
            [SKBlendMode.ColorBurn]  = new EnumDescriptor("ColorBurn",   "Color Burn",           "Darkens the destination based on the source"),
            [SKBlendMode.HardLight]  = new EnumDescriptor("HardLight",   "Hard Light",           "Similar to Overlay but uses the source image as the dominant input"),
            [SKBlendMode.SoftLight]  = new EnumDescriptor("SoftLight",   "Soft Light",           "A softer version of Hard Light, less contrast"),
            [SKBlendMode.Difference] = new EnumDescriptor("Difference",  "Difference",           "Subtracts brightness values, resulting in high contrast with inverted colors"),
            [SKBlendMode.Exclusion]  = new EnumDescriptor("Exclusion",   "Exclusion",            "Similar to Difference but with lower contrast"),
            [SKBlendMode.Multiply]   = new EnumDescriptor("Multiply",    "Multiply",             "Multiplies pixel values, resulting in a darker image"),
            [SKBlendMode.Hue]        = new EnumDescriptor("Hue",         "Hue",                  "Applies the hue of the source to the destination"),
            [SKBlendMode.Saturation] = new EnumDescriptor("Saturation",  "Saturation",           "Applies the saturation of the source to the destination"),
            [SKBlendMode.Color]      = new EnumDescriptor("Color",       "Color",                "Applies both the hue and saturation of the source to the destination"),
            [SKBlendMode.Luminosity] = new EnumDescriptor("Luminosity",  "Luminosity",           "Applies the brightness of the source to the destination")
        };

        CurrentBlendModeProperty.Changed.AddClassHandler<BlendModeToolTip, SKBlendMode>((o, e) => {
            o.UpdateValues(e.NewValue.GetValueOrDefault());
        });
    }
}