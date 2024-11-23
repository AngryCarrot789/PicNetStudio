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
public class LayerSoloStateControl : LayerStateModifierControl {
    private ToggleButton PART_IsSoloToggle;

    private readonly IBinder<BaseVisualLayer> isSoloBinder =
        new AutoUpdateDataParameterPropertyBinder<BaseVisualLayer>(
            BaseVisualLayer.IsSoloParameter, ToggleButton.IsCheckedProperty,
            obj => ((ToggleButton) obj.Control).IsChecked = obj.Model.IsSolo,
            obj => obj.Model.IsSolo = ((ToggleButton) obj.Control).IsChecked == true);

    public new BaseVisualLayer Layer => (BaseVisualLayer) base.Layer;
    
    public LayerSoloStateControl() {
    }

    protected override void OnConnected() {
        base.OnConnected();
        this.isSoloBinder.AttachModel(this.Layer);
    }

    protected override void OnDisconnected() {
        base.OnDisconnected();
        this.isSoloBinder.DetachModel();
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        this.PART_IsSoloToggle = e.NameScope.GetTemplateChild<ToggleButton>(nameof(this.PART_IsSoloToggle));
        this.isSoloBinder.AttachControl(this.PART_IsSoloToggle);
    }
}