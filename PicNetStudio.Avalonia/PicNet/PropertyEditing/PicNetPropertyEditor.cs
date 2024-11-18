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

using System.Linq;
using PicNetStudio.Avalonia.PicNet.Controls.Dragger;
using PicNetStudio.Avalonia.PicNet.Layers;
using PicNetStudio.Avalonia.PicNet.Layers.Core;
using PicNetStudio.Avalonia.PicNet.Layers.CustomParameters.BlendMode;
using PicNetStudio.Avalonia.PicNet.PropertyEditing.Core;
using PicNetStudio.Avalonia.PicNet.PropertyEditing.DataTransfer;
using PicNetStudio.Avalonia.Utils.RDA;

namespace PicNetStudio.Avalonia.PicNet.PropertyEditing;

/// <summary>
/// A class which stores the video editor's general property editor information
/// </summary>
public class PicNetPropertyEditor : BasePropertyEditor {
    public static PicNetPropertyEditor Instance { get; } = new PicNetPropertyEditor();
    
    public SimplePropertyEditorGroup BaseLayerObjectGroup { get; }

    private RapidDispatchAction? delayedUpdate;
    
    private PicNetPropertyEditor() {
        {
            SimplePropertyEditorGroup layer = this.BaseLayerObjectGroup = new SimplePropertyEditorGroup(typeof(BaseLayerTreeObject)) {
                DisplayName = "Layer", IsExpanded = true
            };

            layer.AddItem(new LayerNamePropertyEditorSlot());
            layer.AddItem(new DataParameterBlendModePropertyEditorSlot(BaseVisualLayer.BlendModeParameter, typeof(BaseVisualLayer)));
            layer.AddItem(new DataParameterFloatPropertyEditorSlot(BaseVisualLayer.OpacityParameter, typeof(BaseVisualLayer), "Opacity", DragStepProfile.UnitOne) {ValueFormatter = UnitToPercentFormatter.Standard });
            
            {
                SimplePropertyEditorGroup transform = new SimplePropertyEditorGroup(typeof(BaseVisualLayer)) { DisplayName = "Transformation" };
                transform.AddItem(new DataParameterPointPropertyEditorSlot(BaseVisualLayer.PositionParameter, typeof(BaseVisualLayer), "Position") {StepProfileX = DragStepProfile.InfPixelRange, StepProfileY = DragStepProfile.InfPixelRange});
                transform.AddItem(new DataParameterPointPropertyEditorSlot(BaseVisualLayer.ScaleParameter, typeof(BaseVisualLayer), "Scale") {StepProfileX = DragStepProfile.UnitOne, StepProfileY = DragStepProfile.UnitOne});
                transform.AddItem(new DataParameterDoublePropertyEditorSlot(BaseVisualLayer.RotationParameter, typeof(BaseVisualLayer), "Rotation", DragStepProfile.Rotation));
                transform.AddItem(new DataParameterPointPropertyEditorSlot(BaseVisualLayer.ScaleOriginParameter, typeof(BaseVisualLayer), "Scale Origin") {StepProfileX = DragStepProfile.InfPixelRange, StepProfileY = DragStepProfile.InfPixelRange});
                transform.AddItem(new DataParameterPointPropertyEditorSlot(BaseVisualLayer.RotationOriginParameter, typeof(BaseVisualLayer), "Rotation Origin") {StepProfileX = DragStepProfile.InfPixelRange, StepProfileY = DragStepProfile.InfPixelRange});
                layer.AddItem(transform);
            }
            
            {
                SimplePropertyEditorGroup channelGroup = new SimplePropertyEditorGroup(typeof(RasterLayer), GroupType.SecondaryExpander) { DisplayName = "Channels" };
                channelGroup.AddItem(new DataParameterFloatPropertyEditorSlot(RasterLayer.ChannelRParameter, typeof(RasterLayer), "Channel R", DragStepProfile.UnitOne) { ValueFormatter = UnitToPercentFormatter.Standard });
                channelGroup.AddItem(new DataParameterFloatPropertyEditorSlot(RasterLayer.ChannelGParameter, typeof(RasterLayer), "Channel G", DragStepProfile.UnitOne) { ValueFormatter = UnitToPercentFormatter.Standard });
                channelGroup.AddItem(new DataParameterFloatPropertyEditorSlot(RasterLayer.ChannelBParameter, typeof(RasterLayer), "Channel B", DragStepProfile.UnitOne) { ValueFormatter = UnitToPercentFormatter.Standard });
                channelGroup.AddItem(new DataParameterFloatPropertyEditorSlot(RasterLayer.ChannelAParameter, typeof(RasterLayer), "Channel A", DragStepProfile.UnitOne) { ValueFormatter = UnitToPercentFormatter.Standard });
                layer.AddItem(channelGroup);
            }

            {
                SimplePropertyEditorGroup textGroup = new SimplePropertyEditorGroup(typeof(TextLayer), GroupType.SecondaryExpander) { DisplayName = "Text" };
                textGroup.AddItem(new DataParameterStringPropertyEditorSlot(TextLayer.FontFamilyParameter, typeof(TextLayer), "Font Family"));
                textGroup.AddItem(new DataParameterDoublePropertyEditorSlot(TextLayer.FontSizeParameter, typeof(TextLayer), "Font Size", DragStepProfile.FontSize));
                textGroup.AddItem(new DataParameterDoublePropertyEditorSlot(TextLayer.BorderThicknessParameter, typeof(TextLayer), "Stroke Width", DragStepProfile.Pixels));
                textGroup.AddItem(new DataParameterFloatPropertyEditorSlot(TextLayer.SkewXParameter, typeof(TextLayer), "Skew X", DragStepProfile.Pixels));
                textGroup.AddItem(new DataParameterBoolPropertyEditorSlot(TextLayer.IsAntiAliasedParameter, typeof(TextLayer), "Anti Alias"));
                textGroup.AddItem(new DataParameterDoublePropertyEditorSlot(TextLayer.LineHeightMultiplierParameter, typeof(TextLayer), "Line Hight Mtplr.", DragStepProfile.Pixels));
                textGroup.AddItem(new DataParameterStringPropertyEditorSlot(TextLayer.TextParameter, typeof(TextLayer), "Font Family") {AnticipatedLineCount = 8});
                layer.AddItem(textGroup);
            }
        }

        this.Root.AddItem(this.BaseLayerObjectGroup);
    }
    
    public void UpdateSelectedLayerSelection(ISelectionManager<BaseLayerTreeObject> selection, bool clear) {
        (this.delayedUpdate ??= new RapidDispatchAction(() => {
            if (clear) {
                this.BaseLayerObjectGroup.ClearHierarchy();
            }
            else {
                this.BaseLayerObjectGroup.SetupHierarchyState(selection.SelectedItems.ToList());
            }
        })).InvokeAsync();
    }
}