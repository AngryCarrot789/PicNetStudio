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

using Avalonia;
using Avalonia.Controls;
using PicNetStudio.Avalonia.Utils;
using PicNetStudio.Avalonia.Utils.Collections.Observable;

namespace PicNetStudio.Avalonia.PicNet.Toolbars.Controls;

/// <summary>
/// A list of toolbar items
/// </summary>
public class ToolBarItemListBox : ItemsControl {
    public static readonly StyledProperty<EditorToolBar> EditorToolBarProperty = AvaloniaProperty.Register<ToolBarItemListBox, EditorToolBar>("EditorToolBar");
    private ObservableItemProcessorIndexing<BaseToolBarItem>? itemProcessor;

    public EditorToolBar EditorToolBar {
        get => this.GetValue(EditorToolBarProperty);
        set => this.SetValue(EditorToolBarProperty, value);
    }

    public ToolBarItemListBox() {
    }

    static ToolBarItemListBox() {
        EditorToolBarProperty.Changed.AddClassHandler<ToolBarItemListBox, EditorToolBar>((o, e) => o.OnEditorToolBarChanged(e));
    }

    private void OnEditorToolBarChanged(AvaloniaPropertyChangedEventArgs<EditorToolBar> e) {
        this.itemProcessor?.Dispose();
        for (int i = this.Items.Count - 1; i >= 0; i--) {
            this.RemoveResourceInternal(i);
        }

        if (e.TryGetNewValue(out EditorToolBar? newToolBar)) {
            this.itemProcessor = ObservableItemProcessor.MakeIndexable(newToolBar.Items, this.OnToolBarItemAddedEvent, this.OnToolBarItemRemovedEvent, this.OnToolBarItemMovedEvent);
            int i = 0;
            foreach (BaseToolBarItem resource in newToolBar.Items) {
                this.InsertResourceInternal(resource, i++);
            }
        }
    }

    private void OnToolBarItemAddedEvent(object sender, int index, BaseToolBarItem item) {
        this.InsertResourceInternal(item, index);
    }

    private void OnToolBarItemRemovedEvent(object sender, int index, BaseToolBarItem item) {
        this.RemoveResourceInternal(index);
    }

    private void OnToolBarItemMovedEvent(object sender, int oldIndex, int newIndex, BaseToolBarItem item) {
        ToolBarItemControl control = (ToolBarItemControl) this.Items[oldIndex]!;
        // control.OnIndexMoving(oldIndex, newIndex);
        this.Items.RemoveAt(oldIndex);
        this.Items.Insert(newIndex, control);
        // control.OnIndexMoved(oldIndex, newIndex);
        this.InvalidateMeasure();
    }

    private void InsertResourceInternal(BaseToolBarItem item, int index) {
        ToolBarItemControl control = new ToolBarItemControl();
        control.OnAddingToList(this, item);
        this.Items.Insert(index, control);
        control.OnAddedToList();
        control.InvalidateMeasure();
        this.InvalidateMeasure();
    }

    private void RemoveResourceInternal(int index) {
        ToolBarItemControl control = (ToolBarItemControl) this.Items[index]!;
        control.OnRemovingFromList();
        this.Items.RemoveAt(index);
        control.OnRemovedFromList();
        this.InvalidateMeasure();
    }
}