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

namespace PicNetStudio.PicNet.PropertyEditing;

public enum HandlerCountMode {
    /// <summary>
    /// Any number of handlers are acceptable (more than 0)
    /// </summary>
    Any,

    /// <summary>
    /// Only a single handler is acceptable (only 1)
    /// </summary>
    Single,

    /// <summary>
    /// Only multiple handlers are acceptable (more than 1)
    /// </summary>
    Multi
}