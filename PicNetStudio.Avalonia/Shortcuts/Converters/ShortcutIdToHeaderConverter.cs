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
using PicNetStudio.Avalonia.Shortcuts.Managing;

namespace PicNetStudio.Avalonia.Shortcuts.Converters;

public class ShortcutIdToHeaderConverter : IValueConverter {
    public static ShortcutIdToHeaderConverter Instance { get; } = new ShortcutIdToHeaderConverter();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
        if (value is string path) {
            return ShortcutIdToHeader(path, null, out string gesture) ? gesture : AvaloniaProperty.UnsetValue;
        }

        throw new Exception("Value is not a shortcut string");
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        throw new NotImplementedException();
    }

    public static bool ShortcutIdToHeader(string path, string fallback, out string header) {
        GroupedShortcut shortcut = ShortcutManager.Instance.FindShortcutByPath(path);
        if (shortcut == null) {
            return (header = fallback) != null;
        }

        // This could probably go in the guinness world records
        header = shortcut.DisplayName ?? shortcut.Name ?? shortcut.FullPath ?? shortcut.CommandId ?? fallback ?? shortcut.Shortcut.ToString();
        return true;
    }
}