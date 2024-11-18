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
using PicNetStudio.Avalonia.PicNet.Layers.Core;

namespace PicNetStudio.Avalonia.PicNet.Effects.Controls;

/// <summary>
/// A layer state modifier control that controls the visibility
/// </summary>
public class ColourChannelEffectListBoxItem : BaseEffectListBoxItem {
    public new BaseVisualLayer Layer => (BaseVisualLayer) base.Layer;
    
    public ColourChannelEffectListBoxItem() {
    }

    protected override void OnConnected() {
        base.OnConnected();
    }

    protected override void OnDisconnected() {
        base.OnDisconnected();
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
    }
}