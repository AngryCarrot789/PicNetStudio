﻿// 
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

using PicNetStudio.DataTransfer;
using PicNetStudio.Utils.Accessing;

namespace PicNetStudio.Services.UserInputs;

public delegate void SingleUserInputDataEventHandler(SingleUserInputInfo sender);

public class SingleUserInputInfo : UserInputInfo {
    public static readonly DataParameterString TextParameter = DataParameter.Register(new DataParameterString(typeof(SingleUserInputInfo), nameof(Text), null, ValueAccessors.Reflective<string?>(typeof(SingleUserInputInfo), nameof(text))));
    public static readonly DataParameterString LabelParameter = DataParameter.Register(new DataParameterString(typeof(SingleUserInputInfo), nameof(Label), null, ValueAccessors.Reflective<string?>(typeof(SingleUserInputInfo), nameof(label))));

    private string? text = TextParameter.DefaultValue;
    private string? label = LabelParameter.DefaultValue;
    private bool allowEmptyText;

    public string? Text {
        get => this.text;
        set => DataParameter.SetValueHelper(this, TextParameter, ref this.text, value);
    }
    
    public string? Label {
        get => this.label;
        set => DataParameter.SetValueHelper(this, LabelParameter, ref this.label, value);
    }

    /// <summary>
    /// Gets or sets if the dialog can close successfully with the text property being an empty
    /// string. When this is true and the text is empty, the validation predicate is not called.
    /// However it is called if this value is false
    /// </summary>
    public bool AllowEmptyText {
        get => this.allowEmptyText;
        set {
            if (this.allowEmptyText == value)
                return;

            this.allowEmptyText = value;
            this.AllowEmptyTextChanged?.Invoke(this);
        }
    }

    public event SingleUserInputDataEventHandler? AllowEmptyTextChanged;

    /// <summary>
    /// A validation predicate that is invoked when our text changes, and is used to determine if the dialog can close successfully
    /// </summary>
    public Predicate<string?>? Validate { get; set; }

    public SingleUserInputInfo() {
    }

    public SingleUserInputInfo(string? text) {
        this.text = text;
    }

    public override bool CanDialogClose() {
        if (string.IsNullOrEmpty(this.Text) && !this.AllowEmptyText)
            return false;

        return this.Validate == null || this.Validate(this.Text);
    }
}