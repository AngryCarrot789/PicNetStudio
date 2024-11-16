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

using PicNetStudio.Avalonia.PicNet.Tools;
using PicNetStudio.Avalonia.PicNet.Tools.Core;
using PicNetStudio.Avalonia.Utils.Collections.Observable;

namespace PicNetStudio.Avalonia.PicNet.Toolbars;

public delegate void EditorToolBarActiveToolItemChangedEventHandler(EditorToolBar sender, SingleToolBarItem? oldActiveToolItem, SingleToolBarItem? newActiveToolItem);

/// <summary>
/// Manages all of the tools available
/// </summary>
public class EditorToolBar {
    private readonly ObservableList<BaseToolBarItem> items;
    private SingleToolBarItem? activeToolItem;

    public Editor Editor { get; }

    // TODO: future project, map location key to sub-toolbar-lists for customisable GUI

    public ReadOnlyObservableList<BaseToolBarItem> Items { get; }

    public BaseCanvasTool? ActiveTool => this.activeToolItem?.Tool;

    public SingleToolBarItem? ActiveToolItem {
        get => this.activeToolItem;
        set {
            SingleToolBarItem? oldActiveToolItem = this.activeToolItem;
            if (oldActiveToolItem == value)
                return;

            if (oldActiveToolItem != null && oldActiveToolItem.IsActive)
                SingleToolBarItem.InternalSetIsActive(oldActiveToolItem, false);

            this.activeToolItem = value;
            this.ActiveToolItemChanged?.Invoke(this, oldActiveToolItem, value);

            if (value != null)
                SingleToolBarItem.InternalSetIsActive(value, true);
        }
    }

    public event EditorToolBarActiveToolItemChangedEventHandler? ActiveToolItemChanged;

    public EditorToolBar(Editor editor) {
        this.Editor = editor;
        this.items = new ObservableList<BaseToolBarItem>();
        this.Items = new ReadOnlyObservableList<BaseToolBarItem>(this.items);
        ObservableItemProcessor.MakeSimple(this.Items, (x) => BaseToolBarItem.InternalSetToolBar(x, this), (x) => BaseToolBarItem.InternalSetToolBar(x, null));

        // TODO: Factories?
        // RZApplication.Instance.Services.GetService<>()

        this.items.Add(new SingleToolBarItem(new BrushTool()));
        this.items.Add(new SingleToolBarItem(new PencilTool()));
        this.items.Add(new SingleToolBarItem(new FloodFillTool()));

        this.ActiveToolItem = (SingleToolBarItem?) this.items[0];
    }
}