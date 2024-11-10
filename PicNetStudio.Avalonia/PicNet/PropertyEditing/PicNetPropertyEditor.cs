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

using System.Threading;
using PicNetStudio.Avalonia.PicNet.Layers;
using PicNetStudio.Avalonia.PicNet.PropertyEditing.BasicEditors;
using PicNetStudio.Avalonia.PicNet.PropertyEditing.DataTransfer;
using PicNetStudio.Avalonia.Utils;

namespace PicNetStudio.Avalonia.PicNet.PropertyEditing;

/// <summary>
/// A class which stores the video editor's general property editor information
/// </summary>
public class PicNetPropertyEditor : BasePropertyEditor {
    public static PicNetPropertyEditor Instance { get; } = new PicNetPropertyEditor();

    private volatile int isUpdateClipSelectionScheduled;
    private volatile int isUpdateTrackSelectionScheduled;

    public SimplePropertyEditorGroup BaseLayerObjectGroup { get; }

    private PicNetPropertyEditor() {
        {
            this.BaseLayerObjectGroup = new SimplePropertyEditorGroup(typeof(BaseLayerTreeObject)) {
                DisplayName = "Base Layer", IsExpanded = true
            };

            this.BaseLayerObjectGroup.AddItem(new DisplayNamePropertyEditorSlot());
            this.BaseLayerObjectGroup.AddItem(new DataParameterBooleanPropertyEditorSlot(BaseVisualLayer.IsVisibleParameter, typeof(BaseVisualLayer), "Is Visible"));
            this.BaseLayerObjectGroup.AddItem(new DataParameterFloatPropertyEditorSlot(BaseVisualLayer.OpacityParameter, typeof(BaseVisualLayer), "Opacity", DragStepProfile.UnitOne));
        }

        this.Root.AddItem(this.BaseLayerObjectGroup);
    }

    public void UpdateLayerSelection(Editor? editor) {
        if (editor == null || Interlocked.CompareExchange(ref this.isUpdateClipSelectionScheduled, 1, 0) != 0) {
            return;
        }

        RZApplication.Instance.Dispatcher.InvokeAsync(() => {
            Canvas? canvas = editor.ActiveDocument?.Canvas;
            if (canvas == null)
                return;

            try {
                this.UpdateSelectedLayerSelection(canvas);
            }
            finally {
                this.isUpdateClipSelectionScheduled = 0;
            }
        }, DispatchPriority.Background);
    }

    public void UpdateSelectedLayerSelection(Canvas canvas) {
        if (new SingletonList<BaseLayerTreeObject>(canvas.ActiveLayerTreeObject).CollectionEquals(this.BaseLayerObjectGroup.Handlers)) {
            return;
        }

        this.BaseLayerObjectGroup.SetupHierarchyState(new SingletonReadOnlyList<BaseLayerTreeObject>(canvas.ActiveLayerTreeObject));
        // if (selection.Count == 1) {
        // if (true) {
        //     this.BaseLayerObjectGroup.SetupHierarchyState(new SingletonReadOnlyList<BaseLayerTreeObject>(canvas.ActiveLayerTreeObject));
        // }
        // else {
        //     this.BaseLayerObjectGroup.ClearHierarchy();
        // }
    }
}