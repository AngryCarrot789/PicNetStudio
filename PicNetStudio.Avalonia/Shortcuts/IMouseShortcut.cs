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

using System.Collections.Generic;
using PicNetStudio.Avalonia.Shortcuts.Inputs;
using PicNetStudio.Avalonia.Shortcuts.Usage;

namespace PicNetStudio.Avalonia.Shortcuts;

/// <summary>
/// An interface for shortcuts that accept mouse inputs
/// </summary>
public interface IMouseShortcut : IShortcut {
    /// <summary>
    /// All of the Mouse Strokes that this shortcut contains
    /// </summary>
    IEnumerable<MouseStroke> MouseStrokes { get; }

    /// <summary>
    /// This can be used in order to track the usage of <see cref="IShortcut.InputStrokes"/>. If
    /// the list is empty, then the return value of this function is effectively pointless
    /// </summary>
    /// <returns></returns>
    IMouseShortcutUsage CreateMouseUsage();
}