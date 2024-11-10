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
using Avalonia.Input;
using PicNetStudio.Avalonia.Bindings;
using PicNetStudio.Avalonia.Interactivity.Contexts;

namespace PicNetStudio.Avalonia.PicNet.Toolbars.Controls;

public class ToolBarItemControl : ContentControl {
    public static readonly DirectProperty<ToolBarItemControl, bool> IsActiveProperty = AvaloniaProperty.RegisterDirect<ToolBarItemControl, bool>("IsActive", o => o.IsActive, (o, v) => o.IsActive = v);

    private bool _isActive;

    /// <summary>
    /// Gets whether this tool bar's tool is active
    /// </summary>
    public bool IsActive {
        get => this._isActive;
        private set => this.SetAndRaise(IsActiveProperty, ref this._isActive, value);
    }

    public ToolBarItemListBox? ListBox { get; private set; }

    public BaseToolBarItem? ToolBarItem { get; private set; }

    private readonly IBinder<SingleToolBarItem> isActiveBinder = new AutoUpdateAndEventPropertyBinder<SingleToolBarItem>(nameof(SingleToolBarItem.IsActiveChanged), (x) => ((ToolBarItemControl) x.Control).IsActive = x.Model.IsActive, null);

    public ToolBarItemControl() {
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e) {
        base.OnPointerPressed(e);
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            return;
        e.Handled = true;
        this.MakeActiveTool();
    }

    private void MakeActiveTool() {
        if (this.ToolBarItem is SingleToolBarItem singleToolBarItem)
            singleToolBarItem.Activate();
    }

    public void OnAddingToList(ToolBarItemListBox toolBar, BaseToolBarItem item) {
        this.ListBox = toolBar;
        this.ToolBarItem = item;
        ToolBarItemControlContent content = ToolBarItemControlContent.Registry.NewInstance(item);
        this.Content = content;
        content.ApplyStyling();
    }

    public void OnAddedToList() {
        ToolBarItemControlContent content = (ToolBarItemControlContent) this.Content!;
        // content.ApplyTemplate();
        content.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        content.Connect(this);
        DataManager.SetContextData(this, new ContextData().Set(DataKeys.ToolBarItemKey, this.ToolBarItem));

        if (this.ToolBarItem is SingleToolBarItem singleToolBarItem) {
            this.isActiveBinder.Attach(this, singleToolBarItem);
        }
    }

    public void OnRemovingFromList() {
        if (this.ToolBarItem is SingleToolBarItem) {
            this.isActiveBinder.Detach();
        }

        ToolBarItemControlContent content = (ToolBarItemControlContent) this.Content!;
        content.Disconnect();
        this.Content = null;
    }

    public void OnRemovedFromList() {
        this.ListBox = null;
        this.ToolBarItem = null;
        DataManager.ClearContextData(this);
    }
}