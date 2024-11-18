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
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using PicNetStudio.Avalonia.Utils;

namespace PicNetStudio.Avalonia.PicNet.Layers.Controls;

public class LayerViewModelToIsToggleConverter : IValueConverter {
    public static LayerViewModelToIsToggleConverter Instance { get; } = new LayerViewModelToIsToggleConverter();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
        if (value == AvaloniaProperty.UnsetValue)
            return AvaloniaProperty.UnsetValue;

        if (!(value is LayerTreeViewMode srcViewMode))
            throw new ArgumentException("Value is not a view mode enum", nameof(value));
        if (!(parameter is LayerTreeViewMode matchViewMode))
            throw new ArgumentException("Parameter is not a view mode enum", nameof(value));

        return (srcViewMode == matchViewMode).Box();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
        throw new NotImplementedException();
    }
}