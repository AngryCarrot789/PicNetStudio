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
using PicNetStudio.Avalonia.PicNet.PropertyEditing.Core;
using PicNetStudio.Avalonia.PicNet.PropertyEditing.DataTransfer;
using PicNetStudio.Avalonia.Utils;
using PicNetStudio.Avalonia.Utils.RDA;

namespace PicNetStudio.Avalonia.PicNet.PropertyEditing;

/// <summary>
/// A class which stores the video editor's general property editor information
/// </summary>
public class PicNetPropertyEditor : BasePropertyEditor {
    public static PicNetPropertyEditor Instance { get; } = new PicNetPropertyEditor();

    private RapidDispatchAction? delayedUpdate;
    private Document? activeDocument;
    
    public SimplePropertyEditorGroup BaseLayerObjectGroup { get; }

    private PicNetPropertyEditor() {
        {
            this.BaseLayerObjectGroup = new SimplePropertyEditorGroup(typeof(BaseLayerTreeObject)) {
                DisplayName = "Layer", IsExpanded = true
            };

            this.BaseLayerObjectGroup.AddItem(new LayerNamePropertyEditorSlot());
            this.BaseLayerObjectGroup.AddItem(new DataParameterBooleanPropertyEditorSlot(BaseVisualLayer.IsRenderVisibleParameter, typeof(BaseVisualLayer), "Is Render Visible"));
            this.BaseLayerObjectGroup.AddItem(new DataParameterBooleanPropertyEditorSlot(BaseVisualLayer.IsExportVisibleParameter, typeof(BaseVisualLayer), "Is Export Visible"));
            this.BaseLayerObjectGroup.AddItem(new DataParameterFloatPropertyEditorSlot(BaseVisualLayer.OpacityParameter, typeof(BaseVisualLayer), "Opacity", DragStepProfile.UnitOne));
        }

        this.Root.AddItem(this.BaseLayerObjectGroup);
    }

    public void UpdateSelectedLayerSelection() {
        if (this.activeDocument == null)
            return;
        
        (this.delayedUpdate ??= new RapidDispatchAction(() => {
            if (this.activeDocument != null)
                this.BaseLayerObjectGroup.SetupHierarchyState(new List<object>(this.activeDocument.Canvas.LayerSelectionManager.Selection));
        })).InvokeAsync();
    }

    public void OnActiveDocumentChanged(Document? oldDocument, Document? newDocument) {
        if (oldDocument != null)
            oldDocument.Canvas.LayerSelectionManager.SelectionChanged -= this.LayerSelectionManagerOnSelectionChanged;
        if (newDocument != null)
            newDocument.Canvas.LayerSelectionManager.SelectionChanged += this.LayerSelectionManagerOnSelectionChanged;

        this.activeDocument = newDocument;
        if (newDocument != null)
            this.UpdateSelectedLayerSelection();
    }

    private void LayerSelectionManagerOnSelectionChanged(SelectionManager<BaseLayerTreeObject> sender, IList<BaseLayerTreeObject>? olditems, IList<BaseLayerTreeObject>? newitems) {
        if (this.activeDocument != null)
            this.UpdateSelectedLayerSelection();
    }
}