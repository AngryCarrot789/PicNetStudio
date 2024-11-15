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
using PicNetStudio.Avalonia.PicNet.Layers;
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
            this.BaseLayerObjectGroup = new SimplePropertyEditorGroup(typeof(BaseLayerTreeObject)) {
                DisplayName = "Layer", IsExpanded = true
            };

            this.BaseLayerObjectGroup.AddItem(new LayerNamePropertyEditorSlot());
            this.BaseLayerObjectGroup.AddItem(new DataParameterBooleanPropertyEditorSlot(BaseVisualLayer.IsRenderVisibleParameter, typeof(BaseVisualLayer), "Is Render Visible"));
            this.BaseLayerObjectGroup.AddItem(new DataParameterBooleanPropertyEditorSlot(BaseVisualLayer.IsExportVisibleParameter, typeof(BaseVisualLayer), "Is Export Visible"));
            this.BaseLayerObjectGroup.AddItem(new DataParameterBlendModePropertyEditorSlot(BaseVisualLayer.BlendModeParameter, typeof(BaseVisualLayer)));
            this.BaseLayerObjectGroup.AddItem(new DataParameterFloatPropertyEditorSlot(BaseVisualLayer.OpacityParameter, typeof(BaseVisualLayer), "Opacity", DragStepProfile.UnitOne));
        }

        this.Root.AddItem(this.BaseLayerObjectGroup);
    }

    public void UpdateSelectedLayerSelection(IEnumerable<BaseLayerTreeObject>? selection) {
        (this.delayedUpdate ??= new RapidDispatchAction(() => {
            if (selection == null) {
                this.BaseLayerObjectGroup.ClearHierarchy();
            }
            else {
                this.BaseLayerObjectGroup.SetupHierarchyState(new List<object>(selection));
            }
        })).InvokeAsync();
    }
}