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

using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using PicNetStudio.Avalonia.CommandSystem;
using PicNetStudio.Avalonia.Interactivity.Contexts;

namespace PicNetStudio.Avalonia.PicNet.Commands;

public class SaveDocumentCommand : AsyncDocumentCommand {
    protected override async Task Execute(Editor editor, Document document, CommandEventArgs e) {
        if (!DataKeys.TopLevelHostKey.TryGetContext(e.ContextData, out TopLevel? topLevel)) {
            return;
        }

        IStorageFile? file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions() {
            Title = "Save document",
            SuggestedFileName = "document.picnet",
            ShowOverwritePrompt = true
        });

        if (file == null) {
            return;
        }

        document.SaveTo(file.Path.AbsolutePath, true);
    }
}

public class OpenDocumentCommand : AsyncCommand {
    protected override async Task ExecuteAsync(CommandEventArgs e) {
        if (!DataKeys.TopLevelHostKey.TryGetContext(e.ContextData, out TopLevel? topLevel)) {
            return;
        }

        if (!DataKeys.EditorKey.TryGetContext(e.ContextData, out Editor? editor)) {
            return;
        }

        IReadOnlyList<IStorageFile>? file = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions() {
            Title = "Open document",
            SuggestedFileName = "document.picnet",
            AllowMultiple = false
        });

        if (file.Count != 1) {
            return;
        }

        Document document = new Document();
        document.ReadFrom(file[0].Path.AbsolutePath, true);
        for (int i = editor.Documents.Count - 1; i >= 0; i--) {
            editor.RemoveDocumentAt(i);
        }
        
        editor.AddDocument(document);
        editor.ActiveDocument = document;
        
        document.Canvas.RaiseRenderInvalidated(true);
    }
}