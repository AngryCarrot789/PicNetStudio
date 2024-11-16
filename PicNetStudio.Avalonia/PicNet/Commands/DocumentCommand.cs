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

using System.Threading.Tasks;
using PicNetStudio.Avalonia.CommandSystem;
using PicNetStudio.Avalonia.Interactivity.Contexts;

namespace PicNetStudio.Avalonia.PicNet.Commands;

public abstract class DocumentCommand : Command {
    public sealed override Executability CanExecute(CommandEventArgs e) {
        if (DataKeys.DocumentKey.TryGetContext(e.ContextData, out Document? document) && document.Editor != null) {
            return this.CanExecute(document.Editor, document, e);
        }
        else if (DataKeys.EditorKey.TryGetContext(e.ContextData, out Editor? editor) && editor.ActiveDocument != null) {
            return this.CanExecute(editor, editor.ActiveDocument, e);
        }

        return Executability.Invalid;
    }

    protected sealed override void Execute(CommandEventArgs e) {
        if (DataKeys.DocumentKey.TryGetContext(e.ContextData, out Document? document) && document.Editor != null) {
            this.Execute(document.Editor, document, e);
        }
        else if (DataKeys.EditorKey.TryGetContext(e.ContextData, out Editor? editor) && editor.ActiveDocument != null) {
            this.Execute(editor, editor.ActiveDocument, e);
        }
    }

    protected virtual Executability CanExecute(Editor editor, Document document, CommandEventArgs e) {
        return Executability.Valid;
    }

    protected abstract void Execute(Editor editor, Document document, CommandEventArgs e);
}

public abstract class AsyncDocumentCommand : AsyncCommand {
    protected virtual Executability CanExecute(Editor editor, Document document, CommandEventArgs e) {
        return Executability.Valid;
    }

    protected abstract Task Execute(Editor editor, Document document, CommandEventArgs e);

    protected override Executability CanExecuteOverride(CommandEventArgs e) {
        if (DataKeys.DocumentKey.TryGetContext(e.ContextData, out Document? document) && document.Editor != null) {
            return this.CanExecute(document.Editor, document, e);
        }
        else if (DataKeys.EditorKey.TryGetContext(e.ContextData, out Editor? editor) && editor.ActiveDocument != null) {
            return this.CanExecute(editor, editor.ActiveDocument, e);
        }

        return Executability.Invalid;
    }

    protected override Task ExecuteAsync(CommandEventArgs e) {
        if (DataKeys.DocumentKey.TryGetContext(e.ContextData, out Document? document) && document.Editor != null) {
            return this.Execute(document.Editor, document, e);
        }
        else if (DataKeys.EditorKey.TryGetContext(e.ContextData, out Editor? editor) && editor.ActiveDocument != null) {
            return this.Execute(editor, editor.ActiveDocument, e);
        }
        else {
            return Task.CompletedTask;
        }
    }
}