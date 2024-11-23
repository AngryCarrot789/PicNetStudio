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

using Avalonia.Controls.Primitives;
using PicNetStudio.Avalonia.Bindings;
using PicNetStudio.Avalonia.Utils;
using PicNetStudio.PicNet.Layers.Core;

namespace PicNetStudio.Avalonia.PicNet.Layers.StateMods;

/// <summary>
/// A layer state modifier control that controls the visibility
/// </summary>
public class LayerVisibilityStateControl : LayerStateModifierControl {
    private ToggleButton PART_ToggleRender;
    private ToggleButton PART_ToggleExport;

    private readonly IBinder<BaseVisualLayer> renderVisibilityBinder =
        new AutoUpdateDataParameterPropertyBinder<BaseVisualLayer>(
            BaseVisualLayer.IsRenderVisibleParameter, ToggleButton.IsCheckedProperty,
            obj => ((ToggleButton) obj.Control).IsChecked = obj.Model.IsVisible,
            obj => obj.Model.IsVisible = ((ToggleButton) obj.Control).IsChecked == true);
    
    private readonly IBinder<BaseVisualLayer> exportVisibilityBinder =
        new AutoUpdateDataParameterPropertyBinder<BaseVisualLayer>(
            BaseVisualLayer.IsExportVisibleParameter, ToggleButton.IsCheckedProperty,
            obj => ((ToggleButton) obj.Control).IsChecked = obj.Model.IsExportVisible,
            obj => obj.Model.IsExportVisible = ((ToggleButton) obj.Control).IsChecked == true);

    public new BaseVisualLayer Layer => (BaseVisualLayer) base.Layer;
    
    public LayerVisibilityStateControl() {
    }

    protected override void OnConnected() {
        base.OnConnected();
        this.renderVisibilityBinder.AttachModel(this.Layer);
        this.exportVisibilityBinder.AttachModel(this.Layer);
    }

    protected override void OnDisconnected() {
        base.OnDisconnected();
        this.renderVisibilityBinder.DetachModel();
        this.exportVisibilityBinder.DetachModel();
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        this.PART_ToggleRender = e.NameScope.GetTemplateChild<ToggleButton>(nameof(this.PART_ToggleRender));
        this.PART_ToggleExport = e.NameScope.GetTemplateChild<ToggleButton>(nameof(this.PART_ToggleExport));

        this.renderVisibilityBinder.AttachControl(this.PART_ToggleRender);
        this.exportVisibilityBinder.AttachControl(this.PART_ToggleExport);
    }
}