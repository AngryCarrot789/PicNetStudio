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

using System;
using PicNetStudio.Avalonia.DataTransfer;
using PicNetStudio.Avalonia.Utils.Accessing;

namespace PicNetStudio.Avalonia.Services.Messages;

/// <summary>
/// A class for a basic message box class with a maximum of 3 buttons; Yes/OK, No and Cancel
/// </summary>
public class MessageBoxData : ITransferableData {
    public static readonly DataParameterString CaptionParameter = DataParameter.Register(new DataParameterString(typeof(MessageBoxData), nameof(Caption), "A message here", ValueAccessors.Reflective<string?>(typeof(MessageBoxData), nameof(caption))));
    public static readonly DataParameterString HeaderParameter = DataParameter.Register(new DataParameterString(typeof(MessageBoxData), nameof(Header), null, ValueAccessors.Reflective<string?>(typeof(MessageBoxData), nameof(header))));
    public static readonly DataParameterString MessageParameter = DataParameter.Register(new DataParameterString(typeof(MessageBoxData), nameof(Message), "Message", ValueAccessors.Reflective<string?>(typeof(MessageBoxData), nameof(message))));
    public static readonly DataParameterString YesOkTextParameter = DataParameter.Register(new DataParameterString(typeof(MessageBoxData), nameof(YesOkText), "OK", ValueAccessors.Reflective<string?>(typeof(MessageBoxData), nameof(yesOkText))));
    public static readonly DataParameterString NoTextParameter = DataParameter.Register(new DataParameterString(typeof(MessageBoxData), nameof(NoText), "No", ValueAccessors.Reflective<string?>(typeof(MessageBoxData), nameof(noText))));
    public static readonly DataParameterString CancelTextParameter = DataParameter.Register(new DataParameterString(typeof(MessageBoxData), nameof(CancelText), "Cancel", ValueAccessors.Reflective<string?>(typeof(MessageBoxData), nameof(cancelText))));

    private string? caption = CaptionParameter.DefaultValue;
    private string? header = HeaderParameter.DefaultValue;
    private string? message = MessageParameter.DefaultValue;
    private string? yesOkText = YesOkTextParameter.DefaultValue;
    private string? noText = NoTextParameter.DefaultValue;
    private string? cancelText = CancelTextParameter.DefaultValue;
    private MessageBoxButton buttons;

    public string? Caption {
        get => this.caption;
        set => DataParameter.SetValueHelper(this, CaptionParameter, ref this.caption, value);
    }

    public string? Header {
        get => this.header;
        set => DataParameter.SetValueHelper(this, HeaderParameter, ref this.header, value);
    }

    public string? Message {
        get => this.message;
        set => DataParameter.SetValueHelper(this, MessageParameter, ref this.message, value);
    }

    public string? YesOkText {
        get => this.yesOkText;
        set => DataParameter.SetValueHelper(this, YesOkTextParameter, ref this.yesOkText, value);
    }

    public string? NoText {
        get => this.noText;
        set => DataParameter.SetValueHelper(this, NoTextParameter, ref this.noText, value);
    }

    public string? CancelText {
        get => this.cancelText;
        set => DataParameter.SetValueHelper(this, CancelTextParameter, ref this.cancelText, value);
    }

    /// <summary>
    /// Gets or sets which buttons are shown
    /// </summary>
    public MessageBoxButton Buttons {
        get => this.buttons;
        set {
            if (this.buttons == value)
                return;

            this.buttons = value;
            this.ButtonsChanged?.Invoke(this);
        }
    }

    public MessageBoxResult DefaultButton { get; init; }

    // MessageBoxData.buttons -- remove this line
    public delegate void MessageBoxDataButtonsChangedEventHandler(MessageBoxData sender);

    public event MessageBoxDataButtonsChangedEventHandler? ButtonsChanged;

    public TransferableData TransferableData { get; }

    public MessageBoxData() {
        this.TransferableData = new TransferableData(this);
    }

    public MessageBoxData(string? message) : this() {
        this.message = message;
    }

    public MessageBoxData(string? caption, string? message) : this() {
        this.caption = caption;
        this.message = message;
    }

    public MessageBoxData(string? caption, string? header, string? message) : this() {
        this.caption = caption;
        this.header = header;
        this.message = message;
    }

    public void SetDefaultButtonText() {
        switch (this.buttons) {
            case MessageBoxButton.OK:
                this.YesOkText = "OK";
                break;
            case MessageBoxButton.OKCancel: 
                this.YesOkText = "OK";
                this.CancelText = "Cancel";
                break;
            case MessageBoxButton.YesNoCancel: 
                this.YesOkText = "Yes";
                this.NoText = "No";
                this.CancelText = "Cancel";
                break;
            case MessageBoxButton.YesNo:
                this.YesOkText = "Yes";
                this.NoText = "No";
                break;
            default: throw new ArgumentOutOfRangeException();
        }
    }
}