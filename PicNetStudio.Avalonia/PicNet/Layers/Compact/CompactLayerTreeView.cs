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

using Avalonia;
using Avalonia.Controls;

namespace PicNetStudio.Avalonia.PicNet.Layers.Compact;

/// <summary>
/// A tree view which presents a canvas' layer hierarchy in a tree system
/// </summary>
public class CompactLayerTreeView : BaseLayerTreeView {
    public static readonly StyledProperty<GridLength> ColumnWidth0Property = AvaloniaProperty.Register<CompactLayerTreeView, GridLength>("ColumnWidth0", new GridLength(1, GridUnitType.Star));
    public static readonly StyledProperty<GridLength> ColumnWidth2Property = AvaloniaProperty.Register<CompactLayerTreeView, GridLength>("ColumnWidth2", new GridLength(30));
    public static readonly StyledProperty<GridLength> ColumnWidth4Property = AvaloniaProperty.Register<CompactLayerTreeView, GridLength>("ColumnWidth4", new GridLength(50));
    
    public GridLength ColumnWidth0 { get => this.GetValue(ColumnWidth0Property); set => this.SetValue(ColumnWidth0Property, value); }
    public GridLength ColumnWidth2 { get => this.GetValue(ColumnWidth2Property); set => this.SetValue(ColumnWidth2Property, value); }
    public GridLength ColumnWidth4 { get => this.GetValue(ColumnWidth4Property); set => this.SetValue(ColumnWidth4Property, value); }
    
    public CompactLayerTreeView() {
        
    }

    protected override BaseLayerTreeViewItem CreateTreeViewItem() => new CompactLayerTreeViewItem();
}