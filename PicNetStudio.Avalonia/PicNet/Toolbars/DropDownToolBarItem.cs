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

using PicNetStudio.Avalonia.PicNet.Toolbars.ToolItems;
using PicNetStudio.Avalonia.PicNet.Tools;
using PicNetStudio.Avalonia.Utils.Collections.Observable;
using PicNetStudio.Avalonia.Utils.Collections.ObservableEx;

namespace PicNetStudio.Avalonia.PicNet.Toolbars;

/// <summary>
/// A toolbar item that contains a collection of possible tools to select from.
/// Clicking the button shows a drop down menu and the user can select a tool
/// </summary>
public class DropDownToolBarItem : BaseToolBarItem {
    public ObservableList<SingleDropDownToolBarItem> Items { get; }

    public DropDownToolBarItem() {
        this.Items = new ObservableList<SingleDropDownToolBarItem>();
        ObservableItemProcessor.MakeSimple(this.Items, (x) => x.DropDownToolBarItem = this, (x) => x.DropDownToolBarItem = null);
    }
}

public class SingleDropDownToolBarItem : SingleToolBarItem {
    public DropDownToolBarItem? DropDownToolBarItem { get; internal set; }

    public SingleDropDownToolBarItem(BaseCanvasTool tool) : base(tool) {
    }
}