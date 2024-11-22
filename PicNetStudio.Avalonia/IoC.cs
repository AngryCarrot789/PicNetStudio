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

using PicNetStudio.Avalonia.Services;
using PicNetStudio.Avalonia.Services.Files;
using PicNetStudio.Avalonia.Services.Messages;
using PicNetStudio.Avalonia.Tasks;

namespace PicNetStudio.Avalonia;

public static class IoC {
    /// <summary>
    /// Gets the application's message dialog service, for showing messages to the user
    /// </summary>
    public static IMessageDialogService MessageService => RZApplication.Instance.Services.GetService<IMessageDialogService>();

    /// <summary>
    /// Gets the application's user input dialog service, for querying basic inputs from the user
    /// </summary>
    public static IUserInputDialogService UserInputService => RZApplication.Instance.Services.GetService<IUserInputDialogService>();

    /// <summary>
    /// Gets the application's file picking service, for picking files and directories to open/save
    /// </summary>
    public static IFilePickDialogService FilePickService => RZApplication.Instance.Services.GetService<IFilePickDialogService>();
    
    public static IColourPickerService ColourPickerService => RZApplication.Instance.Services.GetService<IColourPickerService>();

    /// <summary>
    /// Gets the application's task manager
    /// </summary>
    public static TaskManager TaskManager => RZApplication.Instance.Services.GetService<TaskManager>();
}