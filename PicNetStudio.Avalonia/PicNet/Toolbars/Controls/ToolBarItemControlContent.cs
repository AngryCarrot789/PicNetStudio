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

using System;
using Avalonia.Controls.Primitives;
using PicNetStudio.Avalonia.PicNet.Tools;
using PicNetStudio.Avalonia.PicNet.Tools.Core;

namespace PicNetStudio.Avalonia.PicNet.Toolbars.Controls;

/// <summary>
/// The base control for content placed inside a <see cref="ToolBarItemControl"/>
/// </summary>
public class ToolBarItemControlContent : TemplatedControl {
    public static readonly ModelControlRegistry<BaseToolBarItem, ToolBarItemControlContent> Registry;
    public static readonly ModelControlRegistry<BaseCanvasTool, ToolBarItemControlContentSingleTool> SingleToolSubRegistry;

    public ToolBarItemControl ToolBarItem { get; private set; }

    protected ToolBarItemControlContent() {
    }

    static ToolBarItemControlContent() {
        Registry = new ModelControlRegistry<BaseToolBarItem, ToolBarItemControlContent>();
        SingleToolSubRegistry = new ModelControlRegistry<BaseCanvasTool, ToolBarItemControlContentSingleTool>();

        // This system is really confusing but it seems to work.
        // It's what happens when you explicitly avoid using MVVM :---)

        // Register top level toolbar items
        Registry.RegisterType<SingleToolBarItem>((x) => SingleToolSubRegistry.NewInstance(x.Tool));

        // Register single-tool specific tools
        SingleToolSubRegistry.RegisterType<BrushTool>(() => new ToolBarItemControlContent_BrushTool());
    }

    public void Connect(ToolBarItemControl item) {
        this.ToolBarItem = item ?? throw new ArgumentNullException(nameof(item));
        this.OnConnected();
    }

    public void Disconnect() {
        this.OnDisconnected();
        this.ToolBarItem = null;
    }

    protected virtual void OnConnected() {
    }

    protected virtual void OnDisconnected() {
    }
}

/// <summary>
/// The base class for a toolbar list box item that activates a single tool
/// </summary>
public abstract class ToolBarItemControlContentSingleTool : ToolBarItemControlContent;

public class ToolBarItemControlContent_BrushTool : ToolBarItemControlContentSingleTool;