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
using System.Collections;
using System.Linq;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using PicNetStudio.Avalonia.Bindings;
using PicNetStudio.Avalonia.PicNet.Layers.Controls.Compact;
using PicNetStudio.Avalonia.PicNet.Layers.Controls.Enlarged;
using PicNetStudio.Avalonia.Utils;

namespace PicNetStudio.Avalonia.PicNet.Layers.Controls;

/// <summary>
/// A control that manages the view of a canvas' layer hierarchy, and also processes the different selectable view modes
/// </summary>
public class LayerTreeControl : TemplatedControl {
    public static readonly StyledProperty<Canvas?> CanvasProperty = AvaloniaProperty.Register<LayerTreeControl, Canvas?>("Canvas");
    public static readonly StyledProperty<LayerTreeViewMode> ViewModeProperty = AvaloniaProperty.Register<LayerTreeControl, LayerTreeViewMode>("ViewMode");

    public Canvas? Canvas {
        get => this.GetValue(CanvasProperty);
        set => this.SetValue(CanvasProperty, value);
    }

    public LayerTreeViewMode ViewMode {
        get => this.GetValue(ViewModeProperty);
        set => this.SetValue(ViewModeProperty, value);
    }

    private readonly PropertyBinder<Canvas?> compactCanvasBinder;
    private readonly PropertyBinder<Canvas?> enlargedCanvasBinder;
    private readonly TreeViewSelectionManager selectionManager;
    public CompactLayerTreeView? PART_CompactTreeView => this.compactCanvasBinder.TargetControl as CompactLayerTreeView;
    public EnlargedLayerTreeView? PART_EnlargedTreeView => this.enlargedCanvasBinder.TargetControl as EnlargedLayerTreeView;

    public ISelectionManager<BaseLayerTreeObject> SelectionManager => this.selectionManager;

    public event EventHandler? ViewModelChanged;

    public LayerTreeControl() {
        this.compactCanvasBinder = new PropertyBinder<Canvas?>(this, CanvasProperty, BaseLayerTreeView.CanvasProperty);
        this.enlargedCanvasBinder = new PropertyBinder<Canvas?>(this, CanvasProperty, BaseLayerTreeView.CanvasProperty);
        this.selectionManager = new TreeViewSelectionManager();
    }

    static LayerTreeControl() {
        ViewModeProperty.Changed.AddClassHandler<LayerTreeControl, LayerTreeViewMode>((o, e) => o.OnViewModeChanged(e.OldValue.GetValueOrDefault(), e.NewValue.GetValueOrDefault()));
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e) {
        base.OnPointerPressed(e);
        if (e.Handled) {
            return;
        }

        IList? itemList;
        switch (this.ViewMode) {
            case LayerTreeViewMode.Compact:
                itemList = this.PART_CompactTreeView?.SelectedItems;
                break;
            case LayerTreeViewMode.Enlarged:
                itemList = this.PART_EnlargedTreeView?.SelectedItems;
                break;
            default: throw new ArgumentOutOfRangeException();
        }

        if (itemList != null) {
            itemList.Clear();
        }
    }

    private void OnViewModeChanged(LayerTreeViewMode oldMode, LayerTreeViewMode newMode) {
        this.UpdateTreeVisibility();
        this.ViewModelChanged?.Invoke(this, EventArgs.Empty);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        this.compactCanvasBinder.SetTargetControl(e.NameScope.GetTemplateChild<CompactLayerTreeView>("PART_CompactTreeView"));
        this.enlargedCanvasBinder.SetTargetControl(e.NameScope.GetTemplateChild<EnlargedLayerTreeView>("PART_EnlargedTreeView"));
        this.UpdateTreeVisibility();
    }

    private void UpdateTreeVisibility() {
        switch (this.ViewMode) {
            case LayerTreeViewMode.Compact:
                if (this.PART_CompactTreeView != null)
                    this.PART_CompactTreeView!.IsVisible = true;
                if (this.PART_EnlargedTreeView != null)
                    this.PART_EnlargedTreeView!.IsVisible = false;
                this.selectionManager.Tree = this.PART_CompactTreeView;
                break;
            case LayerTreeViewMode.Enlarged:
                if (this.PART_CompactTreeView != null)
                    this.PART_CompactTreeView!.IsVisible = false;
                if (this.PART_EnlargedTreeView != null)
                    this.PART_EnlargedTreeView!.IsVisible = true;
                this.selectionManager.Tree = this.PART_EnlargedTreeView;
                break;
        }
        
        ((ILightSelectionManager<BaseLayerTreeObject>) this.selectionManager).SelectionChanged += this.OnSelectionChanged;
    }

    private void OnSelectionChanged(ILightSelectionManager<BaseLayerTreeObject> sender) {
        if (this.Canvas is Canvas canvas) {
            canvas.ActiveLayerTreeObject = this.selectionManager.Count == 1 ? this.selectionManager.SelectedItems.First() : null;
        }
    }
}