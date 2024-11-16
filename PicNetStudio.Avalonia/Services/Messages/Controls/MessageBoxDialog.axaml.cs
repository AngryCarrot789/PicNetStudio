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

using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using PicNetStudio.Avalonia.Bindings;
using PicNetStudio.Avalonia.Themes.Controls;

namespace PicNetStudio.Avalonia.Services.Messages.Controls;

public partial class MessageBoxDialog : WindowEx {
    public static readonly StyledProperty<MessageBoxData?> MessageBoxDataProperty = AvaloniaProperty.Register<MessageBoxDialog, MessageBoxData?>("MessageBoxData");

    public MessageBoxData? MessageBoxData {
        get => this.GetValue(MessageBoxDataProperty);
        set => this.SetValue(MessageBoxDataProperty, value);
    }

    private readonly DataParameterPropertyBinder<MessageBoxData> captionBinder = new DataParameterPropertyBinder<MessageBoxData>(TitleProperty, MessageBoxData.CaptionParameter);
    private readonly DataParameterPropertyBinder<MessageBoxData> headerBinder = new DataParameterPropertyBinder<MessageBoxData>(TextBlock.TextProperty, MessageBoxData.HeaderParameter);
    private readonly DataParameterPropertyBinder<MessageBoxData> messageBinder = new DataParameterPropertyBinder<MessageBoxData>(TextBlock.TextProperty, MessageBoxData.MessageParameter);
    private readonly DataParameterPropertyBinder<MessageBoxData> yesOkTextBinder = new DataParameterPropertyBinder<MessageBoxData>(ContentProperty, MessageBoxData.YesOkTextParameter);
    private readonly DataParameterPropertyBinder<MessageBoxData> noTextBinder = new DataParameterPropertyBinder<MessageBoxData>(ContentProperty, MessageBoxData.NoTextParameter);
    private readonly DataParameterPropertyBinder<MessageBoxData> cancelTextBinder = new DataParameterPropertyBinder<MessageBoxData>(ContentProperty, MessageBoxData.CancelTextParameter);

    public MessageBoxDialog() {
        this.InitializeComponent();
        this.captionBinder.AttachControl(this);
        this.headerBinder.AttachControl(this.PART_Header);
        this.messageBinder.AttachControl(this.PART_Message);
        this.yesOkTextBinder.AttachControl(this.PART_YesOkButton);
        this.noTextBinder.AttachControl(this.PART_NoButton);
        this.cancelTextBinder.AttachControl(this.PART_CancelButton);
        this.PART_Header.PropertyChanged += this.OnHeaderTextBlockPropertyChanged;

        this.PART_YesOkButton.Click += this.OnConfirmButtonClicked;
        this.PART_NoButton.Click += this.OnNoButtonClicked;
        this.PART_CancelButton.Click += this.OnCancelButtonClicked;
    }

    private void OnHeaderTextBlockPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e) {
        if (e.Property == TextBlock.TextProperty) {
            this.PART_MessageContainer.IsVisible = !string.IsNullOrWhiteSpace(e.GetNewValue<string?>());
        }
    }

    static MessageBoxDialog() {
        MessageBoxDataProperty.Changed.AddClassHandler<MessageBoxDialog, MessageBoxData?>((o, e) => o.OnMessageBoxDataChanged(e.OldValue.GetValueOrDefault(), e.NewValue.GetValueOrDefault()));
    }

    protected override void OnLoaded(RoutedEventArgs e) {
        base.OnLoaded(e);
        this.PART_DockPanelRoot.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        Size size = this.PART_DockPanelRoot.DesiredSize;
        if (size.Width > 300.0) {
            this.Width = size.Width;
        }

        const double TitleBarHeight = 32;
        this.Height = Math.Max(size.Height + TitleBarHeight, 125);
    }

    private void OnConfirmButtonClicked(object? sender, RoutedEventArgs e) {
        MessageBoxData? data = this.MessageBoxData;
        if (data == null) {
            return;
        }

        switch (data.Buttons) {
            case MessageBoxButton.OK:
            case MessageBoxButton.OKCancel:
                this.Close(MessageBoxResult.OK);
                return;
            case MessageBoxButton.YesNoCancel:
            case MessageBoxButton.YesNo:
                this.Close(MessageBoxResult.Yes);
                return;
            default:
                this.Close(MessageBoxResult.None);
                return;
        }
    }

    private void OnNoButtonClicked(object? sender, RoutedEventArgs e) {
        MessageBoxData? data = this.MessageBoxData;
        if (data == null) {
            return;
        }

        if ((data.Buttons == MessageBoxButton.YesNo || data.Buttons == MessageBoxButton.YesNoCancel)) {
            this.Close(MessageBoxResult.No);
        }
        else {
            this.Close(MessageBoxResult.None);
        }
    }

    private void OnCancelButtonClicked(object? sender, RoutedEventArgs e) {
        MessageBoxData? data = this.MessageBoxData;
        if (data == null) {
            return;
        }

        if ((data.Buttons == MessageBoxButton.OKCancel || data.Buttons == MessageBoxButton.YesNoCancel)) {
            this.Close(MessageBoxResult.Cancel);
        }
        else {
            this.Close(MessageBoxResult.None);
        }
    }

    private void OnMessageBoxDataChanged(MessageBoxData? oldData, MessageBoxData? newData) {
        if (oldData != null)
            oldData.ButtonsChanged -= this.OnActiveButtonsChanged;
        if (newData != null)
            newData.ButtonsChanged += this.OnActiveButtonsChanged;

        // Create this first just in case there's a problem with no registrations
        this.captionBinder.SwitchModel(newData);
        this.headerBinder.SwitchModel(newData);
        this.messageBinder.SwitchModel(newData);
        this.yesOkTextBinder.SwitchModel(newData);
        this.noTextBinder.SwitchModel(newData);
        this.cancelTextBinder.SwitchModel(newData);
        this.UpdateVisibleButtons();
        if (newData != null) {
            Dispatcher.UIThread.InvokeAsync(() => {
                switch (newData.DefaultButton) {
                    case MessageBoxResult.None: break;
                    case MessageBoxResult.Yes:
                    case MessageBoxResult.OK:
                        if (this.PART_YesOkButton.IsVisible)
                            this.PART_YesOkButton.Focus();
                        break;
                    case MessageBoxResult.Cancel:
                        if (this.PART_CancelButton.IsVisible)
                            this.PART_CancelButton.Focus();
                        break;
                    case MessageBoxResult.No:
                        if (this.PART_NoButton.IsVisible)
                            this.PART_NoButton.Focus();
                        break;
                }
            }, DispatcherPriority.Loaded);
        }
    }

    private void OnActiveButtonsChanged(MessageBoxData sender) {
        this.UpdateVisibleButtons();
    }

    /// <summary>
    /// Updates which buttons are visible based on our message box data's <see cref="Messages.MessageBoxData.Buttons"/> property
    /// </summary>
    public void UpdateVisibleButtons() {
        MessageBoxData? data = this.MessageBoxData;
        if (data == null) {
            return;
        }

        switch (data.Buttons) {
            case MessageBoxButton.OK:
                this.PART_YesOkButton.IsVisible = true;
                this.PART_NoButton.IsVisible = false;
                this.PART_CancelButton.IsVisible = false;
                break;
            case MessageBoxButton.OKCancel:
                this.PART_YesOkButton.IsVisible = true;
                this.PART_NoButton.IsVisible = false;
                this.PART_CancelButton.IsVisible = true;
                break;
            case MessageBoxButton.YesNoCancel:
                this.PART_YesOkButton.IsVisible = true;
                this.PART_NoButton.IsVisible = true;
                this.PART_CancelButton.IsVisible = true;
                break;
            case MessageBoxButton.YesNo:
                this.PART_YesOkButton.IsVisible = true;
                this.PART_NoButton.IsVisible = true;
                this.PART_CancelButton.IsVisible = false;
                break;
            default: throw new ArgumentOutOfRangeException();
        }
    }

    /// <summary>
    /// Tries to close the dialog
    /// </summary>
    /// <param name="result">The dialog result wanted</param>
    /// <returns>True if the dialog was closed, false if it could not be closed due to a validation error or other error</returns>
    public void Close(MessageBoxResult result) {
        base.Close(result);
    }
}