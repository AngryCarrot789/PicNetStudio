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

using System;
using PicNetStudio.Avalonia.PicNet.Tools;

namespace PicNetStudio.Avalonia.PicNet.Toolbars;

public delegate void BaseActivatableToolBarItemIsActiveChangedEventHandler(SingleToolBarItem sender);

public class SingleToolBarItem : BaseToolBarItem, IToolBarItem {
    public bool IsActive { get; private set; }

    public BaseCanvasTool Tool { get; }

    public event BaseActivatableToolBarItemIsActiveChangedEventHandler? IsActiveChanged;

    public SingleToolBarItem(BaseCanvasTool tool) {
        this.Tool = tool ?? throw new ArgumentNullException(nameof(tool));
    }

    /// <summary>
    /// Helper method to activate this toolbar item
    /// </summary>
    public void Activate() {
        if (this.ToolBar == null)
            return;
        this.ToolBar.ActiveToolItem = this;
    }

    internal static void InternalSetIsActive(SingleToolBarItem toolBarItem, bool isActive) {
        if (isActive == toolBarItem.IsActive)
            return;

        toolBarItem.IsActive = isActive;
        toolBarItem.IsActiveChanged?.Invoke(toolBarItem);
    }
}