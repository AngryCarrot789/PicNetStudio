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
using PicNetStudio.Avalonia.DataTransfer;
using PicNetStudio.Avalonia.Utils.Accessing;

namespace PicNetStudio.Avalonia.Services.Messages;

/// <summary>
/// The base class for a user input dialog's model, which contains generic
/// properties suitable across any type of two-buttoned titlebar and message dialog
/// </summary>
public abstract class UserInputInfo : ITransferableData {
    public static readonly DataParameterString CaptionParameter = DataParameter.Register(new DataParameterString(typeof(UserInputInfo), nameof(Caption), null, ValueAccessors.Reflective<string?>(typeof(UserInputInfo), nameof(caption))));
    public static readonly DataParameterString MessageParameter = DataParameter.Register(new DataParameterString(typeof(UserInputInfo), nameof(Message), null, ValueAccessors.Reflective<string?>(typeof(UserInputInfo), nameof(message))));
    public static readonly DataParameterString ConfirmTextParameter = DataParameter.Register(new DataParameterString(typeof(UserInputInfo), nameof(ConfirmText), "OK", ValueAccessors.Reflective<string?>(typeof(UserInputInfo), nameof(confirmText))));
    public static readonly DataParameterString CancelTextParameter = DataParameter.Register(new DataParameterString(typeof(UserInputInfo), nameof(CancelText), "Cancel", ValueAccessors.Reflective<string?>(typeof(UserInputInfo), nameof(cancelText))));
    
    private string? caption = CaptionParameter.DefaultValue;
    private string? message = MessageParameter.DefaultValue;
    private string? confirmText = ConfirmTextParameter.DefaultValue;
    private string? cancelText = CancelTextParameter.DefaultValue;
    
    public TransferableData TransferableData { get; }

    /// <summary>
    /// Gets or sets the dialog's caption, displayed usually in the titlebar
    /// </summary>
    public string? Caption {
        get => this.caption;
        set => DataParameter.SetValueHelper(this, CaptionParameter, ref this.caption, value);
    }

    /// <summary>
    /// Gets or sets the dialog's message, displayed above the input field(s) at the top of the dialog's content.
    /// This could be some general information about what the fields do or maybe some rules.
    /// See derived classes for properties such as labels or field descriptions, which may be more specific
    /// </summary>
    public string? Message {
        get => this.message;
        set => DataParameter.SetValueHelper(this, MessageParameter, ref this.message, value);
    }
    
    /// <summary>
    /// Gets or sets the text in the confirm button
    /// </summary>
    public string? ConfirmText {
        get => this.confirmText;
        set => DataParameter.SetValueHelper(this, ConfirmTextParameter, ref this.confirmText, value);
    }
    
    /// <summary>
    /// Gets or sets the text in the cancel button
    /// </summary>
    public string? CancelText {
        get => this.cancelText;
        set => DataParameter.SetValueHelper(this, CancelTextParameter, ref this.cancelText, value);
    }
    
    public UserInputInfo() {
        this.TransferableData = new TransferableData(this);
    }

    public abstract bool CanDialogClose();
}

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

public delegate void DoubleUserInputDataEventHandler(DoubleUserInputInfo sender);

public class DoubleUserInputInfo : UserInputInfo {
    public static readonly DataParameterString TextAParameter = DataParameter.Register(new DataParameterString(typeof(DoubleUserInputInfo), nameof(TextA), null, ValueAccessors.Reflective<string?>(typeof(DoubleUserInputInfo), nameof(textA))));
    public static readonly DataParameterString TextBParameter = DataParameter.Register(new DataParameterString(typeof(DoubleUserInputInfo), nameof(TextB), null, ValueAccessors.Reflective<string?>(typeof(DoubleUserInputInfo), nameof(textB))));
    public static readonly DataParameterString LabelAParameter = DataParameter.Register(new DataParameterString(typeof(DoubleUserInputInfo), nameof(LabelA), null, ValueAccessors.Reflective<string?>(typeof(DoubleUserInputInfo), nameof(labelA))));
    public static readonly DataParameterString LabelBParameter = DataParameter.Register(new DataParameterString(typeof(DoubleUserInputInfo), nameof(LabelB), null, ValueAccessors.Reflective<string?>(typeof(DoubleUserInputInfo), nameof(labelB))));

    private string? textA = TextAParameter.DefaultValue;
    private string? textB = TextBParameter.DefaultValue;
    private string? labelA = LabelAParameter.DefaultValue;
    private string? labelB = LabelBParameter.DefaultValue;
    private bool allowEmptyTextA;
    private bool allowEmptyTextB;

    public string? TextA {
        get => this.textA;
        set => DataParameter.SetValueHelper(this, TextAParameter, ref this.textA, value);
    }

    public string? TextB {
        get => this.textB;
        set => DataParameter.SetValueHelper(this, TextBParameter, ref this.textB, value);
    }
    
    public string? LabelA {
        get => this.labelA;
        set => DataParameter.SetValueHelper(this, LabelAParameter, ref this.labelA, value);
    }

    public string? LabelB {
        get => this.labelB;
        set => DataParameter.SetValueHelper(this, LabelBParameter, ref this.labelB, value);
    }

    public bool AllowEmptyTextA {
        get => this.allowEmptyTextA;
        set {
            if (this.allowEmptyTextA == value)
                return;

            this.allowEmptyTextA = value;
            this.AllowEmptyTextAChanged?.Invoke(this);
        }
    }

    public bool AllowEmptyTextB {
        get => this.allowEmptyTextB;
        set {
            if (this.allowEmptyTextB == value)
                return;

            this.allowEmptyTextB = value;
            this.AllowEmptyTextBChanged?.Invoke(this);
        }
    }

    public event DoubleUserInputDataEventHandler? AllowEmptyTextAChanged;
    public event DoubleUserInputDataEventHandler? AllowEmptyTextBChanged;

    /// <summary>
    /// A validation predicate that is invoked when either of our text properties change, and is used to determine if the dialog can close successfully
    /// </summary>
    public Func<string?, string?, bool>? Validate { get; set; }
    
    public DoubleUserInputInfo() {
    }

    public DoubleUserInputInfo(string? textA, string? textB) {
        this.textA = textA;
        this.textB = textB;
    }
    
    public override bool CanDialogClose() {
        if ((string.IsNullOrEmpty(this.TextA) && !this.AllowEmptyTextA) || (string.IsNullOrEmpty(this.TextA) && !this.AllowEmptyTextA))
            return false;

        return this.Validate == null || this.Validate(this.TextA, this.TextB);
    }
}