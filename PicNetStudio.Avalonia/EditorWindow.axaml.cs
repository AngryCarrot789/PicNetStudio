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

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using PicNetStudio.Avalonia.Interactivity.Contexts;
using PicNetStudio.Avalonia.PicNet;
using PicNetStudio.Avalonia.PicNet.Layers;
using PicNetStudio.Avalonia.PicNet.PropertyEditing;
using PicNetStudio.Avalonia.Themes.Controls;
using SkiaSharp;

namespace PicNetStudio.Avalonia;

public partial class EditorWindow : WindowEx {
    public Editor Editor { get; }

    private readonly ContextData contextData;

    public EditorWindow() {
        this.InitializeComponent();

        this.Editor = new Editor();
        this.Editor.ActiveDocumentChanged += this.OnActiveDocumentChanged;
        DataManager.SetContextData(this, this.contextData = new ContextData().Set(DataKeys.TopLevelHostKey, this).Set(DataKeys.EditorKey, this.Editor));

        this.PART_ToolBar.EditorToolBar = this.Editor.ToolBar;

        Document document = new Document();
        document.Canvas.Size = new PixelSize(800, 400);


        void MakeLayer(SKColor fill, string name, CompositeLayer? parent = null) {
            RasterLayer layer = new RasterLayer() {
                Name = name
            };
            layer.Bitmap.InitialiseBitmap(document.Canvas.Size);
            layer.Bitmap.Canvas!.Clear(fill);

            if (parent != null) {
                parent.AddLayer(layer);
            }
            else {
                document.Canvas.AddLayer(layer);
            }
        }

        CompositeLayer MakeCompLayer(string name) {
            CompositeLayer layer = new CompositeLayer() {
                Name = name
            };
            document.Canvas.AddLayer(layer);
            return layer;
        }
        
        MakeLayer(SKColors.Transparent, "Layer 1");
        MakeLayer(SKColors.Transparent, "Layer 2");
        
        CompositeLayer comp = MakeCompLayer("A composition layer");
        MakeLayer(SKColors.Transparent, "Layer 1 in comp", comp);
        MakeLayer(SKColors.Transparent, "Layer 2 in comp", comp);
        
        MakeLayer(SKColors.Transparent, "Layer 4");
        MakeLayer(SKColors.White, "Background Base Layer");

        this.Editor.AddDocument(document);
    }

    protected override void OnLoaded(RoutedEventArgs e) {
        base.OnLoaded(e);
        this.ThePropertyEditor.ApplyStyling();
        this.ThePropertyEditor.ApplyTemplate();
        this.ThePropertyEditor.PropertyEditor = PicNetPropertyEditor.Instance;
        this.PART_ColourPicker.Color = Color.FromUInt32((uint) SKColors.DodgerBlue);
    }

    private void OnActiveDocumentChanged(Editor sender, Document? oldactivedocument, Document? newactivedocument) {
        this.PART_Canvas.Document = newactivedocument;
        this.PART_LayerTreeControl.Canvas = newactivedocument?.Canvas;

        this.contextData.Set(DataKeys.DocumentKey, newactivedocument);
        DataManager.InvalidateInheritedContext(this);
    }

    private void OnColourPickerColourChanged(object? sender, ColorChangedEventArgs e) {
        this.Editor.PrimaryColour = new SKColor(e.NewColor.ToUInt32());
    }
}