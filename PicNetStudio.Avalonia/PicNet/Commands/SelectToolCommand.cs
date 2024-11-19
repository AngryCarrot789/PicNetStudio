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

using PicNetStudio.Avalonia.CommandSystem;
using PicNetStudio.Avalonia.PicNet.Toolbars;
using PicNetStudio.Avalonia.PicNet.Tools;
using PicNetStudio.Avalonia.PicNet.Tools.Core;

namespace PicNetStudio.Avalonia.PicNet.Commands;

public abstract class SelectToolCommand<TTool> : DocumentCommand where TTool : BaseCanvasTool {
    protected override void Execute(Editor editor, Document document, CommandEventArgs e) {
        foreach (BaseToolBarItem item in editor.ToolBar.Items) {
            if (item is IToolBarItem toolBarItem && toolBarItem.Tool is TTool) {
                toolBarItem.Activate();
                return;
            }
        }
    }
}

public class SelectBrushToolCommand : SelectToolCommand<BrushTool>;
public class SelectPencilToolCommand : SelectToolCommand<PencilTool>;
public class SelectFillToolCommand : SelectToolCommand<FloodFillTool>;
public class SelectSelectionToolCommand : SelectToolCommand<SelectRegionTool>;