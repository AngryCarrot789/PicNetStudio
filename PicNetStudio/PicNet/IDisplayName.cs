﻿// 
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

namespace PicNetStudio.PicNet;

public delegate void DisplayNameChangedEventHandler(IDisplayName sender, string oldName, string newName);

/// <summary>
/// An generic interface for models with a display name
/// </summary>
public interface IDisplayName {
    /// <summary>
    /// Gets or sets the display name. Setting this fires an event
    /// </summary>
    string DisplayName { get; set; }

    /// <summary>
    /// Fired when our display name property changes
    /// </summary>
    event DisplayNameChangedEventHandler DisplayNameChanged;
}