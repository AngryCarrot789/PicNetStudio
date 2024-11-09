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
using Avalonia;
using Avalonia.Controls;
using PicNetStudio.Avalonia.Interactivity.Contexts;
using PicNetStudio.Avalonia.PicNet;
using PicNetStudio.Avalonia.PicNet.Layers;
using SkiaSharp;

namespace PicNetStudio.Avalonia;

public partial class EditorWindow : Window {
    public Editor Editor { get; }

    private readonly ContextData contextData;
    
    public EditorWindow() {
        this.InitializeComponent();

        this.Editor = new Editor();
        this.Editor.ActiveDocumentChanged += this.OnActiveDocumentChanged;
        DataManager.SetContextData(this, this.contextData = new ContextData().Set(DataKeys.EditorKey, this.Editor));

        this.PART_ToolBar.EditorToolBar = this.Editor.ToolBar;
        
        Document document = new Document();
        document.Canvas.Size = new PixelSize(800, 400);

        BitmapLayer layer = new BitmapLayer();
        document.Canvas.AddLayer(layer);

        layer.Bitmap.InitialiseBitmap(document.Canvas.Size);
        layer.Bitmap.Canvas!.Clear(SKColors.White);
        
        this.Editor.AddDocument(document);
    }

    private void OnActiveDocumentChanged(Editor sender, Document? oldactivedocument, Document? newactivedocument) {
        this.PART_Canvas.Document = newactivedocument;
        
        this.contextData.Set(DataKeys.DocumentKey, newactivedocument);
        DataManager.InvalidateInheritedContext(this);
    }
}