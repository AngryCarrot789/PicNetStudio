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
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using PicNetStudio.Avalonia.PicNet.Tools.Settings.Controls;
using PicNetStudio.Avalonia.Utils;
using PicNetStudio.PicNet.Formatting;
using PicNetStudio.PicNet.PropertyEditing.DataTransfer;
using PicNetStudio.PicNet.Tools;
using PicNetStudio.PicNet.Tools.Core;
using PicNetStudio.PicNet.Tools.Settings;
using PicNetStudio.PicNet.Tools.Settings.DataTransfer;

namespace PicNetStudio.Avalonia.PicNet.Tools.Settings;

public class ToolSettingContainerControl : TemplatedControl {
    private readonly Dictionary<Type, List<BaseToolSetting>> ToolTypeToSettings;

    private StackPanel? PART_Panel;

    public ToolSettingContainerControl() {
        this.ToolTypeToSettings = new Dictionary<Type, List<BaseToolSetting>>();

        this.AddSetting<CursorTool>(new DataParameterBoolToolSetting(CursorTool.IsOutlineVisibleParameter, "Show Outline") {
            Description = "Change the diameter of this brush tool"
        });
        this.AddSetting<BrushTool>(new DataParameterFloatToolSetting(BaseDiameterTool.DiameterParameter, "Diameter:", DragStepProfile.SubPixel) {
            ValueFormatter = SuffixValueFormatter.StandardPixels, 
            Description = "Change the diameter of this brush tool"
        });
        this.AddSetting<PencilTool>(new DataParameterFloatToolSetting(BaseDiameterTool.DiameterParameter, "Size:", DragStepProfile.SubPixel) {
            ValueFormatter = SuffixValueFormatter.StandardPixels, 
            Description = "Change the size of this pencil tool"
        });
        this.AddSetting<BaseRasterisedDrawingTool>(new AutomaticDataParameterFloatToolSetting(BaseRasterisedDrawingTool.GapParameter, BaseRasterisedDrawingTool.IsGapAutomaticParameter, "Gap:", DragStepProfile.SubPixel) {
            ValueFormatter = SuffixValueFormatter.StandardPixels, 
            Description = "Change the gap between each brush draw event. Bigger means more space between each draw event" + '\n' + "This is calculated automatically, but can be overridden. Click and type \"auto\" and press enter to make it auto again"
        });
        this.AddSetting<BrushTool>(new DataParameterFloatToolSetting(BrushTool.HardnessParameter, "Hardness:", DragStepProfile.UnitOne) {
            ValueFormatter = UnitToPercentFormatter.Standard, 
            Description = "Change the hardness factor. Smaller means less solid pixels on the outside of the tool pattern"
        });
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        this.PART_Panel = e.NameScope.GetTemplateChild<StackPanel>(nameof(this.PART_Panel));
    }

    public void SetActiveTool(BaseCanvasTool? tool) {
        if (this.PART_Panel == null) {
            return;
        }

        global::Avalonia.Controls.Controls controlList = this.PART_Panel.Children;
        for (int i = controlList.Count - 1; i >= 0; i--) {
            BaseToolSettingControl setting = (BaseToolSettingControl) controlList[i];
            setting.ToolSetting!.Disconnect();
            setting.Disconnect();
            controlList.RemoveAt(i);
        }

        if (tool == null) {
            return;
        }

        List<(BaseToolSettingControl, BaseToolSetting)> controls = new List<(BaseToolSettingControl, BaseToolSetting)>();
        for (Type? type = tool.GetType(); type != null; type = type.BaseType) {
            if (this.ToolTypeToSettings.TryGetValue(type, out List<BaseToolSetting>? list)) {
                foreach (BaseToolSetting setting in list) {
                    BaseToolSettingControl control = BaseToolSettingControl.Registry.NewInstance(setting);
                    controls.Add((control, setting));
                } 
            }   
        }
        
        foreach ((BaseToolSettingControl control, BaseToolSetting setting) tuple in controls) {
            tuple.setting.Connect(tool);
            this.PART_Panel.Children.Add(tuple.control);
            tuple.control.ApplyStyling();
            tuple.control.ApplyTemplate();
            tuple.control.Connect(this, tuple.setting);
        }
    }
    
    public void AddSetting<T>(BaseToolSetting setting) where T : BaseCanvasTool {
        if (!this.ToolTypeToSettings.TryGetValue(typeof(T), out List<BaseToolSetting>? list))
            this.ToolTypeToSettings[typeof(T)] = list = new List<BaseToolSetting>();
        list.Add(setting);
    }
}