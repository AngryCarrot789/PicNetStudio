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

using Avalonia.Controls;
using Avalonia.Input;
using PicNetStudio.Avalonia.Bindings;
using PicNetStudio.Avalonia.DataTransfer;

namespace PicNetStudio.Avalonia.Services.Messages.Controls;

public partial class SingleUserInputControl : UserControl, IUserInputContent {
    private readonly DataParameterPropertyBinder<SingleUserInputData> labelBinder = new DataParameterPropertyBinder<SingleUserInputData>(TextBlock.TextProperty, SingleUserInputData.LabelParameter);
    private readonly DataParameterPropertyBinder<SingleUserInputData> textBinder = new DataParameterPropertyBinder<SingleUserInputData>(TextBox.TextProperty, SingleUserInputData.TextParameter);
    private UserInputDialog? myDialog;
    private SingleUserInputData? myData;

    public SingleUserInputControl() {
        this.InitializeComponent();
        this.labelBinder.AttachControl(this.PART_Label);
        this.textBinder.AttachControl(this.PART_TextBox);
        
        this.PART_TextBox.KeyDown += this.OnTextFieldKeyDown;
    }

    private void OnTextFieldKeyDown(object? sender, KeyEventArgs e) {
        if ((e.Key == Key.Escape || e.Key == Key.Enter) && this.myDialog != null) {
            this.myDialog.TryCloseDialog(e.Key != Key.Escape);
        }
    }

    public void Connect(UserInputDialog dialog, UserInputData data) {
        this.myDialog = dialog;
        this.myData = (SingleUserInputData) data;
        this.labelBinder.AttachModel(this.myData);
        this.textBinder.AttachModel(this.myData);
        SingleUserInputData.TextParameter.AddValueChangedHandler(data, this.OnTextChanged);
        SingleUserInputData.LabelParameter.AddValueChangedHandler(data, this.OnLabelChanged);
        
        this.myData.AllowEmptyTextChanged += this.OnAllowEmptyTextChanged;
        this.UpdateLabelVisibility();
    }

    public void Disconnect() {
        this.labelBinder.DetachModel();
        this.textBinder.DetachModel();
        SingleUserInputData.TextParameter.RemoveValueChangedHandler(this.myData!, this.OnTextChanged);
        SingleUserInputData.LabelParameter.RemoveValueChangedHandler(this.myData!, this.OnLabelChanged);

        this.myData!.AllowEmptyTextChanged -= this.OnAllowEmptyTextChanged;
        this.myDialog = null;
        this.myData = null;
    }

    public void FocusPrimaryInput() {
        this.PART_TextBox.Focus();
        this.PART_TextBox.SelectAll();
    }

    private void UpdateLabelVisibility() => this.PART_Label.IsVisible = !string.IsNullOrWhiteSpace(this.myData!.Label);

    private void OnLabelChanged(DataParameterString parameter, ITransferableData owner) => this.UpdateLabelVisibility();

    private void OnAllowEmptyTextChanged(SingleUserInputData sender) => this.myDialog!.InvalidateConfirmButton();

    private void OnTextChanged(DataParameterString parameter, ITransferableData owner) => this.myDialog!.InvalidateConfirmButton();
}