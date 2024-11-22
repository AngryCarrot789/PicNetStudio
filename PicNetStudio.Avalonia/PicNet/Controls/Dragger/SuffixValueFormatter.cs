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
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with PicNetStudio. If not, see <https://www.gnu.org/licenses/>.
// 

using System;

namespace PicNetStudio.Avalonia.PicNet.Controls.Dragger;

/// <summary>
/// A value formatter that converts a unit value (0.0 to 1.0) into a percentage (0 to 100%) with an optional decimal precision
/// </summary>
public class SuffixValueFormatter : IValueFormatter {
    private int nonEditingRoundedPlaces;
    private int editingRoundedPlaces;
    private string? suffix;

    public int NonEditingRoundedPlaces {
        get => this.nonEditingRoundedPlaces;
        set {
            value = Math.Max(value, 0);
            if (this.nonEditingRoundedPlaces == value)
                return;

            this.nonEditingRoundedPlaces = value;
            this.InvalidateFormat?.Invoke(this, EventArgs.Empty);
        }
    }

    public int EditingRoundedPlaces {
        get => this.editingRoundedPlaces;
        set {
            value = Math.Max(value, 0);
            if (this.editingRoundedPlaces == value)
                return;

            this.editingRoundedPlaces = value;
            this.InvalidateFormat?.Invoke(this, EventArgs.Empty);
        }
    }

    public string? Suffix {
        get => this.suffix;
        set {
            if (this.suffix == value)
                return;
            this.suffix = value;
            this.InvalidateFormat?.Invoke(this, EventArgs.Empty);
        }
    }

    public event EventHandler? InvalidateFormat;

    public SuffixValueFormatter(string? suffix = null, int nonEditingRoundedPlaces = 2, int editingRoundedPlaces = 6) {
        this.suffix = suffix;
        this.nonEditingRoundedPlaces = nonEditingRoundedPlaces;
        this.editingRoundedPlaces = editingRoundedPlaces;
    }

    public string ToString(double value, bool isEditing) {
        return value.ToString("F" + (isEditing ? this.editingRoundedPlaces : this.nonEditingRoundedPlaces)) + (this.suffix ?? "");
    }

    public bool TryConvertToDouble(string format, out double value) {
        int parseLength = string.IsNullOrEmpty(this.suffix) ? format.Length : (format.Length - this.suffix.Length);
        if (parseLength < 1) {
            value = default;
            return false;
        }
        
        return double.TryParse(format.AsSpan(0, parseLength), out value);
    }

    public static SuffixValueFormatter Parse(string input) {
        if (string.IsNullOrWhiteSpace(input))
            throw new ArgumentException("Input is null, empty or whitespaces only", nameof(input));

        string[] parts = input.Split(',');
        if (parts.Length != 3)
            throw new ArgumentException("Missing 3 parts split by ',' character between the non-editing and editing rounded places", nameof(input));

        if (!int.TryParse(parts[0], out int nonEditingPlaces))
            throw new ArgumentException($"Invalid integer for non-editing part '{parts[0]}'", nameof(input));

        if (!int.TryParse(parts[1], out int editingPlaces))
            throw new ArgumentException($"Invalid integer for non-editing part '{parts[1]}'", nameof(input));

        return new SuffixValueFormatter(parts[2], nonEditingPlaces, editingPlaces);
    }
}