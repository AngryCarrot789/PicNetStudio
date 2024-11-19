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

using System;
using PicNetStudio.Avalonia.PicNet.Tools;
using PicNetStudio.Avalonia.Utils.Collections.Observable;

namespace PicNetStudio.Avalonia.PicNet.Toolbars;

public delegate void ActiveToolBarItemChangedEventHandler(ActivatableDropDownToolBarItem sender, SingleDropDownToolBarItem? oldActiveToolBarItem, SingleDropDownToolBarItem? newActiveToolBarItem);

/// <summary>
/// A toolbar item that contains a collection of possible tools to select from.
/// Right clicking the button shows a drop down menu and the user can select a tool
/// </summary>
public class ActivatableDropDownToolBarItem : BaseToolBarItem, IToolBarItem {
    private SingleDropDownToolBarItem? activeToolBarItem;

    public ObservableList<SingleDropDownToolBarItem> Items { get; }

    public SingleDropDownToolBarItem? ActiveToolBarItem {
        get => this.activeToolBarItem;
        set {
            SingleDropDownToolBarItem? oldActiveToolBarItem = this.activeToolBarItem;
            if (oldActiveToolBarItem == value)
                return;

            this.activeToolBarItem = value;
            this.ActiveToolBarItemChanged?.Invoke(this, oldActiveToolBarItem, value);
        }
    }

    public BaseCanvasTool Tool => this.ActiveToolBarItem?.Tool ?? throw new InvalidCastException("No items yet");

    public event ActiveToolBarItemChangedEventHandler? ActiveToolBarItemChanged;

    public ActivatableDropDownToolBarItem() {
        this.Items = new ObservableList<SingleDropDownToolBarItem>();
        ObservableItemProcessor.MakeSimple(this.Items, (x) => {
            x.DropDownToolBarItem = this;
            if (this.Items.Count == 1)
                this.ActiveToolBarItem = x;
        }, (x) => {
            if (this.Items.Count == 0)
                this.ActiveToolBarItem = null;
            x.DropDownToolBarItem = null;
        });
    }
    
    public void Activate() {
        if (this.ToolBar == null || this.activeToolBarItem == null)
            return;
        this.ToolBar.ActiveToolItem = this.activeToolBarItem;
    }
}

public class SingleDropDownToolBarItem : SingleToolBarItem {
    public ActivatableDropDownToolBarItem? DropDownToolBarItem { get; internal set; }

    public SingleDropDownToolBarItem(BaseCanvasTool tool) : base(tool) {
    }
}