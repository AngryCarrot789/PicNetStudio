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

public partial class DoubleUserInputControl : UserControl, IUserInputContent {
    private readonly DataParameterPropertyBinder<DoubleUserInputData> labelABinder = new DataParameterPropertyBinder<DoubleUserInputData>(TextBlock.TextProperty, DoubleUserInputData.LabelAParameter);
    private readonly DataParameterPropertyBinder<DoubleUserInputData> labelBBinder = new DataParameterPropertyBinder<DoubleUserInputData>(TextBlock.TextProperty, DoubleUserInputData.LabelBParameter);
    private readonly DataParameterPropertyBinder<DoubleUserInputData> textABinder = new DataParameterPropertyBinder<DoubleUserInputData>(TextBox.TextProperty, DoubleUserInputData.TextAParameter);
    private readonly DataParameterPropertyBinder<DoubleUserInputData> textBBinder = new DataParameterPropertyBinder<DoubleUserInputData>(TextBox.TextProperty, DoubleUserInputData.TextBParameter);
    private UserInputDialog? myDialog;
    private DoubleUserInputData? myData;

    public DoubleUserInputControl() {
        this.InitializeComponent();
        this.labelABinder.AttachControl(this.PART_LabelA);
        this.labelBBinder.AttachControl(this.PART_LabelB);
        this.textABinder.AttachControl(this.PART_TextBoxA);
        this.textBBinder.AttachControl(this.PART_TextBoxB);
        
        this.PART_TextBoxA.KeyDown += this.OnAnyTextFieldKeyDown;
        this.PART_TextBoxB.KeyDown += this.OnAnyTextFieldKeyDown;
    }
    
    private void OnAnyTextFieldKeyDown(object? sender, KeyEventArgs e) {
        if ((e.Key == Key.Escape || e.Key == Key.Enter) && this.myDialog != null) {
            this.myDialog.TryCloseDialog(e.Key != Key.Escape);
        }
    }

    public void Connect(UserInputDialog dialog, UserInputData data) {
        this.myDialog = dialog;
        this.myData = (DoubleUserInputData) data;
        this.labelABinder.AttachModel(this.myData);
        this.labelBBinder.AttachModel(this.myData);
        this.textABinder.AttachModel(this.myData);
        this.textBBinder.AttachModel(this.myData);
        DataParameter.AddMultipleHandlers(this.OnAnyTextChanged, DoubleUserInputData.TextAParameter, DoubleUserInputData.TextBParameter);
        DoubleUserInputData.LabelAParameter.AddValueChangedHandler(this.myData!, this.OnLabelAChanged);
        DoubleUserInputData.LabelBParameter.AddValueChangedHandler(this.myData!, this.OnLabelBChanged);
        this.myData.AllowEmptyTextAChanged += this.OnAllowEmptyTextChanged;
        this.myData.AllowEmptyTextBChanged += this.OnAllowEmptyTextChanged;
        this.UpdateLabelAVisibility();
        this.UpdateLabelBVisibility();
    }

    public void Disconnect() {
        this.labelABinder.DetachModel();
        this.labelBBinder.DetachModel();
        this.textABinder.DetachModel();
        this.textBBinder.DetachModel();
        DataParameter.AddMultipleHandlers(this.OnAnyTextChanged, DoubleUserInputData.TextAParameter, DoubleUserInputData.TextBParameter);
        DoubleUserInputData.LabelAParameter.RemoveValueChangedHandler(this.myData!, this.OnLabelAChanged);
        DoubleUserInputData.LabelBParameter.RemoveValueChangedHandler(this.myData!, this.OnLabelBChanged);
        this.myData!.AllowEmptyTextAChanged -= this.OnAllowEmptyTextChanged;
        this.myData!.AllowEmptyTextBChanged -= this.OnAllowEmptyTextChanged;
        
        this.myDialog = null;
        this.myData = null;
    }
    
    public void FocusPrimaryInput() {
        this.PART_TextBoxA.Focus();
        this.PART_TextBoxA.SelectAll();
    }

    private void UpdateLabelAVisibility() => this.PART_LabelA.IsVisible = !string.IsNullOrWhiteSpace(this.myData!.LabelA);

    private void UpdateLabelBVisibility() => this.PART_LabelA.IsVisible = !string.IsNullOrWhiteSpace(this.myData!.LabelA);

    private void OnLabelAChanged(DataParameter dataParameter, ITransferableData owner) => this.UpdateLabelAVisibility();
    private void OnLabelBChanged(DataParameter dataParameter, ITransferableData owner) => this.UpdateLabelBVisibility();

    private void OnAllowEmptyTextChanged(DoubleUserInputData sender) => this.myDialog!.InvalidateConfirmButton();
    private void OnAnyTextChanged(DataParameter dataParameter, ITransferableData owner) => this.myDialog!.InvalidateConfirmButton();
}