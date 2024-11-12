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
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using PicNetStudio.Avalonia.Bindings;
using PicNetStudio.Avalonia.Utils;

namespace PicNetStudio.Avalonia.PicNet.Layers.Controls;

public class LayerTreeControl : TemplatedControl {
    public static readonly StyledProperty<Canvas?> CanvasProperty = AvaloniaProperty.Register<LayerTreeControl, Canvas?>("Canvas");
    public static readonly StyledProperty<GridLength> ColumnWidth0Property = AvaloniaProperty.Register<LayerTreeControl, GridLength>("ColumnWidth0", new GridLength(1, GridUnitType.Star));
    public static readonly StyledProperty<GridLength> ColumnWidth2Property = AvaloniaProperty.Register<LayerTreeControl, GridLength>("ColumnWidth2", new GridLength(30));
    public static readonly StyledProperty<GridLength> ColumnWidth4Property = AvaloniaProperty.Register<LayerTreeControl, GridLength>("ColumnWidth4", new GridLength(50));

    public Canvas? Canvas {
        get => this.GetValue(CanvasProperty);
        set => this.SetValue(CanvasProperty, value);
    }
    
    public GridLength ColumnWidth0 { get => this.GetValue(ColumnWidth0Property); set => this.SetValue(ColumnWidth0Property, value); }
    public GridLength ColumnWidth2 { get => this.GetValue(ColumnWidth2Property); set => this.SetValue(ColumnWidth2Property, value); }
    public GridLength ColumnWidth4 { get => this.GetValue(ColumnWidth4Property); set => this.SetValue(ColumnWidth4Property, value); }

    private readonly PropertyBinder<Canvas?> canvasBinder;
    private LayerObjectTreeView? PART_TreeView => this.canvasBinder.TargetControl as LayerObjectTreeView;

    public LayerTreeControl() {
        this.canvasBinder = new PropertyBinder<Canvas?>(this, CanvasProperty, LayerObjectTreeView.CanvasProperty);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e) {
        base.OnPointerPressed(e);
        if (e.Handled || this.PART_TreeView == null)
            return;

        List<LayerObjectTreeViewItem> list = this.PART_TreeView.SelectedItems.Cast<LayerObjectTreeViewItem>().ToList();
        for (int i = list.Count - 1; i >= 0; i--) {
            list[i].IsSelected = false;
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        this.canvasBinder.SetTargetControl(e.NameScope.GetTemplateChild<LayerObjectTreeView>("PART_TreeView"));
    }
}