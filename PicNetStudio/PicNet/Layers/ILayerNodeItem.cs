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

namespace PicNetStudio.PicNet.Layers;

/// <summary>
/// An interface that represents the state of a node in a layer tree control
/// </summary>
public interface ILayerNodeItem {
    /// <summary>
    /// Gets the layer model for this node
    /// </summary>
    BaseLayerTreeObject? Layer { get; }

    /// <summary>
    /// Gets or sets if this item is selected
    /// </summary>
    bool IsSelected { get; set; }

    /// <summary>
    /// Gets or sets the editing name state, which when true will show a text box to
    /// edit the name and when false just shows plain text
    /// </summary>
    bool EditNameState { get; set; }
}