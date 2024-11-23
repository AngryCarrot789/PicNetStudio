// 
// Copyright (c) 2024-2024 REghZy
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

using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace PicNetStudio.Avalonia.PicNet.Layers.Enlarged;

/// <summary>
/// A tree view item that represents any type of layer
/// </summary>
public class EnlargedLayerTreeViewItem : BaseLayerTreeViewItem {
    private Border? previewContainer;
    private ContentControl? previewPresenter;
    private BaseLayerPreviewControl? item;

    public EnlargedLayerTreeViewItem() {
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        this.previewContainer = e.NameScope.Find<Border>("PART_PreviewContainer")!;
        this.previewPresenter = e.NameScope.Find<ContentControl>("PART_PreviewPresenter")!;
        this.TryInitialisePreview();
    }

    public override void OnAdded() {
        base.OnAdded();
        this.TryInitialisePreview();
    }

    private void TryInitialisePreview() {
        if (this.item != null || this.previewPresenter == null) {
            return;
        }

        if (this.LayerObject != null && BaseLayerPreviewControl.Registry.TryGetNewInstance(this.LayerObject, out this.item)) {
            this.previewContainer!.IsVisible = true;
            this.previewPresenter.Content = this.item;
            this.item.ApplyStyling();
            this.item.ApplyTemplate();
            // this.item.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            this.item.Connect(this, this.LayerObject);
        }
        else {
            this.previewContainer!.IsVisible = false;
        }
    }

    public override void OnRemoving() {
        base.OnRemoving();
        this.item?.Disconnect();
        this.item = null;
        this.previewPresenter!.Content = null;
    }
}