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

using PicNetStudio.CommandSystem;
using PicNetStudio.PicNet.Tools;

namespace PicNetStudio.PicNet.Commands;

/// <summary>
/// A base class for commands that run upon the active tool
/// </summary>
public abstract class ActiveToolCommand<T> : DocumentCommand where T : BaseCanvasTool {
    protected override Executability CanExecute(Editor editor, Document document, CommandEventArgs e) {
        if (editor.ToolBar.ActiveTool is T tool)
            return this.CanExecute(editor, document, tool, e);
        return Executability.ValidButCannotExecute;
    }

    protected override void Execute(Editor editor, Document document, CommandEventArgs e) {
        if (editor.ToolBar.ActiveTool is T tool)
            this.Execute(editor, document, tool, e);
    }

    protected virtual Executability CanExecute(Editor editor, Document document, T tool, CommandEventArgs e) => Executability.Valid;
    protected abstract void Execute(Editor editor, Document document, T tool, CommandEventArgs e);
}