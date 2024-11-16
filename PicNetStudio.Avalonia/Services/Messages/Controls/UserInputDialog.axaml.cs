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

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using PicNetStudio.Avalonia.Bindings;
using PicNetStudio.Avalonia.PicNet;
using PicNetStudio.Avalonia.Themes.Controls;

namespace PicNetStudio.Avalonia.Services.Messages.Controls;

public partial class UserInputDialog : WindowEx {
    public static readonly SingleUserInputData DummySingleInput = new SingleUserInputData("Text Input Here") {Message = "A primary message here", ConfirmText = "Confirm", CancelText = "Cancel", Caption = "The caption here", Label = "The label here"};
    public static readonly DoubleUserInputData DummyDoubleInput = new DoubleUserInputData("Text A Here", "Text B Here") {Message = "A primary message here", ConfirmText = "Confirm", CancelText = "Cancel", Caption = "The caption here", LabelA = "Label A Here:", LabelB = "Label B Here:"};
    
    public static readonly ModelControlRegistry<UserInputData, Control> Registry;
    
    public static readonly StyledProperty<UserInputData?> UserInputDataProperty = AvaloniaProperty.Register<UserInputDialog, UserInputData?>("UserInputData");

    public UserInputData? UserInputData {
        get => this.GetValue(UserInputDataProperty);
        set => this.SetValue(UserInputDataProperty, value);
    }

    /// <summary>
    /// Gets the dialog result for this user input dialog
    /// </summary>
    public bool? DialogResult { get; private set; }

    private readonly DataParameterPropertyBinder<UserInputData> captionBinder = new DataParameterPropertyBinder<UserInputData>(TitleProperty, UserInputData.CaptionParameter);
    private readonly DataParameterPropertyBinder<UserInputData> messageBinder = new DataParameterPropertyBinder<UserInputData>(TextBlock.TextProperty, UserInputData.MessageParameter);
    private readonly DataParameterPropertyBinder<UserInputData> confirmTextBinder = new DataParameterPropertyBinder<UserInputData>(ContentProperty, UserInputData.ConfirmTextParameter);
    private readonly DataParameterPropertyBinder<UserInputData> cancelTextBinder = new DataParameterPropertyBinder<UserInputData>(ContentProperty, UserInputData.CancelTextParameter);

    public UserInputDialog() {
        this.InitializeComponent();
        this.captionBinder.AttachControl(this);
        this.messageBinder.AttachControl(this.PART_Message);
        this.confirmTextBinder.AttachControl(this.PART_ConfirmButton);
        this.cancelTextBinder.AttachControl(this.PART_CancelButton);
        this.PART_Message.PropertyChanged += this.OnMessageTextBlockPropertyChanged;
        
        this.PART_ConfirmButton.Click += this.OnConfirmButtonClicked;
        this.PART_CancelButton.Click += this.OnCancelButtonClicked;
    }

    static UserInputDialog() {
        Registry = new ModelControlRegistry<UserInputData, Control>();
        Registry.RegisterType<SingleUserInputData>((x) => new SingleUserInputControl());
        Registry.RegisterType<DoubleUserInputData>((x) => new DoubleUserInputControl());
        
        UserInputDataProperty.Changed.AddClassHandler<UserInputDialog, UserInputData?>((o, e) => o.OnUserInputDataChanged(e.OldValue.GetValueOrDefault(), e.NewValue.GetValueOrDefault()));
    }
    
    private void OnMessageTextBlockPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e) {
        if (e.Property == TextBlock.TextProperty) {
            this.PART_MessageContainer.IsVisible = !string.IsNullOrWhiteSpace(e.GetNewValue<string?>());
        }
    }

    protected override void OnLoaded(RoutedEventArgs e) {
        base.OnLoaded(e);
        this.PART_DockPanelRoot.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        Size size = this.PART_DockPanelRoot.DesiredSize;
        if (size.Width > 300.0) {
            this.Width = size.Width;
        }
        
        const double TitleBarHeight = 32;
        const double AdditionalPadding = 0;
        this.Height = size.Height + TitleBarHeight + AdditionalPadding;
    }

    private void OnConfirmButtonClicked(object? sender, RoutedEventArgs e) {
        this.TryCloseDialog(true);
    }
    
    private void OnCancelButtonClicked(object? sender, RoutedEventArgs e) {
        this.TryCloseDialog(false);
    }

    private void OnUserInputDataChanged(UserInputData? oldData, UserInputData? newData) {
        if (oldData != null) {
            (this.PART_InputFieldContent.Content as IUserInputContent)?.Disconnect();
        }

        // Create this first just in case there's a problem with no registrations
        Control? control = newData != null ? Registry.NewInstance(newData) : null;
        
        this.captionBinder.SwitchModel(newData);
        this.messageBinder.SwitchModel(newData);
        this.confirmTextBinder.SwitchModel(newData);
        this.cancelTextBinder.SwitchModel(newData);
        if (control != null) {
            this.PART_InputFieldContent.Content = control;
            control.InvalidateMeasure();
            (control as IUserInputContent)?.Connect(this, newData!);
        }

        this.InvalidateConfirmButton();
        Dispatcher.UIThread.InvokeAsync(() => (this.PART_InputFieldContent.Content as IUserInputContent)?.FocusPrimaryInput(), DispatcherPriority.Loaded);
    }

    /// <summary>
    /// Updates the confirm button's enabled state
    /// </summary>
    public void InvalidateConfirmButton() {
        this.PART_ConfirmButton.IsEnabled = this.UserInputData?.CanDialogClose() ?? false;
    }

    /// <summary>
    /// Tries to close the dialog
    /// </summary>
    /// <param name="result">The dialog result wanted</param>
    /// <returns>True if the dialog was closed, false if it could not be closed due to a validation error or other error</returns>
    public bool TryCloseDialog(bool result) {
        if (result) {
            UserInputData? data = this.UserInputData;
            if (data == null || !data.CanDialogClose()) {
                return false;
            }

            this.Close(this.DialogResult = true);
            return true;
        }
        else {
            this.Close(this.DialogResult = false);
            return true;
        }
    }
}