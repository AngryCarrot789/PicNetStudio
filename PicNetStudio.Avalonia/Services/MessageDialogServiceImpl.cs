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

using System.Threading.Tasks;
using Avalonia.Controls;
using PicNetStudio.Services.Messaging;
using PicNetStudio.Utils;
using MessageBoxDialog = PicNetStudio.Avalonia.Services.Messages.Controls.MessageBoxDialog;

namespace PicNetStudio.Avalonia.Services;

public class MessageDialogServiceImpl : IMessageDialogService {
    public Task<MessageBoxResult> ShowMessage(string caption, string message, MessageBoxButton buttons = MessageBoxButton.OK) {
        MessageBoxInfo info = new MessageBoxInfo(caption, message) { Buttons = buttons };
        info.SetDefaultButtonText();
        return this.ShowMessage(info);
    }

    public Task<MessageBoxResult> ShowMessage(string caption, string header, string message, MessageBoxButton buttons = MessageBoxButton.OK) {
        MessageBoxInfo info = new MessageBoxInfo(caption, header, message) { Buttons = buttons };
        info.SetDefaultButtonText();
        return this.ShowMessage(info);
    }

    public async Task<MessageBoxResult> ShowMessage(MessageBoxInfo info) {
        Validate.NotNull(info);

        if (App.RZApplicationImpl.TryGetActiveWindow(out Window? window)) {
            MessageBoxDialog dialog = new MessageBoxDialog {
                MessageBoxData = info
            };

            MessageBoxResult? result = await dialog.ShowDialog<MessageBoxResult?>(window);
            return result ?? MessageBoxResult.None;
        }

        return MessageBoxResult.None;
    }
}