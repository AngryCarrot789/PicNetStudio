using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using PicNetStudio.Avalonia.PicNet.Controls.Dragger;
using PicNetStudio.Avalonia.PicNet.PropertyEditing.DataTransfer;
using PicNetStudio.Avalonia.PicNet.Tools.Core;
using PicNetStudio.Avalonia.PicNet.Tools.Settings.Controls;
using PicNetStudio.Avalonia.PicNet.Tools.Settings.DataTransfer;
using PicNetStudio.Avalonia.Utils;

namespace PicNetStudio.Avalonia.PicNet.Tools.Settings;

public class ToolSettingContainerControl : TemplatedControl {
    private readonly Dictionary<Type, List<BaseToolSetting>> ToolTypeToSettings;

    private StackPanel? PART_Panel;

    public ToolSettingContainerControl() {
        this.ToolTypeToSettings = new Dictionary<Type, List<BaseToolSetting>>();

        this.AddSetting<BrushTool>(new DataParameterFloatToolSetting(BaseDiameterTool.DiameterDataParameter, "Diameter:", DragStepProfile.SubPixel) {
            ValueFormatter = new SuffixValueFormatter("px"), 
            Description = "Change the diameter of this brush tool"
        });
        this.AddSetting<PencilTool>(new DataParameterFloatToolSetting(BaseDiameterTool.DiameterDataParameter, "Size:", DragStepProfile.SubPixel) {
            ValueFormatter = new SuffixValueFormatter("px"), 
            Description = "Change the size of this pencil tool"
        });
        this.AddSetting<BaseRasterisedDrawingTool>(new AutomaticDataParameterFloatToolSetting(BaseRasterisedDrawingTool.GapParameter, BaseRasterisedDrawingTool.IsGapAutomaticParameter, "Gap:", DragStepProfile.SubPixel) {
            ValueFormatter = new SuffixValueFormatter("px"), 
            Description = "Change the gap between each brush draw event. Bigger means more space between each draw event" + '\n' + "This is calculated automatically, but can be overridden. Click and enter \"auto\" and press enter to make it auto again"
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
        
        controls.Reverse();
        foreach ((BaseToolSettingControl control, BaseToolSetting setting) tuple in controls) {
            tuple.setting.Connect(tool);
            this.PART_Panel.Children.Add(tuple.control);
            tuple.control.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            tuple.control.Connect(this, tuple.setting);
            tuple.control.InvalidateMeasure();
        }
    }
    
    public void AddSetting<T>(BaseToolSetting setting) where T : BaseCanvasTool {
        if (!this.ToolTypeToSettings.TryGetValue(typeof(T), out List<BaseToolSetting>? list))
            this.ToolTypeToSettings[typeof(T)] = list = new List<BaseToolSetting>();
        list.Add(setting);
    }
}