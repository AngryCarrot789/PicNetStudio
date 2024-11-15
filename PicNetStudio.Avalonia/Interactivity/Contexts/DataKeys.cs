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

using Avalonia.Controls;
using PicNetStudio.Avalonia.PicNet;
using PicNetStudio.Avalonia.PicNet.Layers;
using PicNetStudio.Avalonia.PicNet.Layers.Controls;
using PicNetStudio.Avalonia.PicNet.Toolbars;

namespace PicNetStudio.Avalonia.Interactivity.Contexts;

public static class DataKeys {
    public static readonly DataKey<TopLevel> TopLevelHostKey = DataKey<TopLevel>.Create("TopLevel");
    public static readonly DataKey<Editor> EditorKey = DataKey<Editor>.Create("Editor");
    public static readonly DataKey<Document> DocumentKey = DataKey<Document>.Create("EditorDocument");
    public static readonly DataKey<BaseToolBarItem> ToolBarItemKey = DataKey<BaseToolBarItem>.Create("ToolBarItem");
    public static readonly DataKey<BaseLayerTreeObject> LayerObjectKey = DataKey<BaseLayerTreeObject>.Create("LayerObject");
    public static readonly DataKey<ILayerNodeItem> LayerNodeKey = DataKey<ILayerNodeItem>.Create("LayerNodeObject");
    public static readonly DataKey<ISelectionManager<BaseLayerTreeObject>> LayerSelectionManagerKey = DataKey<ISelectionManager<BaseLayerTreeObject>>.Create("LayerSelectionManager");
}