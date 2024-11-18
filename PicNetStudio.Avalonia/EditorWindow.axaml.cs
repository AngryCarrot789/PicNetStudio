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

using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using PicNetStudio.Avalonia.Interactivity.Contexts;
using PicNetStudio.Avalonia.PicNet;
using PicNetStudio.Avalonia.PicNet.Effects;
using PicNetStudio.Avalonia.PicNet.Layers;
using PicNetStudio.Avalonia.PicNet.Layers.Controls;
using PicNetStudio.Avalonia.PicNet.Layers.Core;
using PicNetStudio.Avalonia.PicNet.PropertyEditing;
using PicNetStudio.Avalonia.Themes.Controls;
using SkiaSharp;

namespace PicNetStudio.Avalonia;

public partial class EditorWindow : WindowEx {
    public Editor Editor { get; private set; }

    private readonly ContextData contextData;

    public EditorWindow() {
        this.InitializeComponent();
        DataManager.SetContextData(this, this.contextData = new ContextData().Set(DataKeys.TopLevelHostKey, this));
    }

    protected override void OnLoaded(RoutedEventArgs e) {
        base.OnLoaded(e);
        this.ThePropertyEditor.ApplyStyling();
        this.ThePropertyEditor.ApplyTemplate();
        this.ThePropertyEditor.PropertyEditor = PicNetPropertyEditor.Instance;
        this.PART_ColourPicker.Color = Color.FromUInt32((uint) SKColors.DodgerBlue);
        DataManager.SetContextData(this, this.contextData.Set(DataKeys.LayerSelectionManagerKey, this.PART_LayerTreeControl.SelectionManager));

        this.Editor = new Editor();
        this.Editor.ActiveDocumentChanged += this.OnActiveDocumentChanged;
        DataManager.InvalidateInheritedContext(this);

        this.PART_ToolBar.EditorToolBar = this.Editor.ToolBar;

        Document document = new Document();
        document.Canvas.Size = new PixelSize(800, 400);


        RasterLayer MakeLayer(SKColor fill, string name, CompositionLayer? parent = null) {
            RasterLayer layer = new RasterLayer() {
                Name = name
            };

            layer.Bitmap.InitialiseBitmap(document.Canvas.Size);
            layer.Bitmap.Canvas!.Clear(fill);

            (parent ?? document.Canvas.RootComposition).AddLayer(layer);
            return layer;
        }
        
        TextLayer MakeTextLayer(string text, string name, CompositionLayer? parent = null) {
            TextLayer layer = new TextLayer() {
                Name = name,
                Text = text
            };
            
            (parent ?? document.Canvas.RootComposition).AddLayer(layer);
            return layer;
        }

        CompositionLayer MakeCompLayer(string name) {
            CompositionLayer layer = new CompositionLayer() {
                Name = name
            };
            document.Canvas.RootComposition.AddLayer(layer);
            return layer;
        }

        RasterLayer firstLayer = MakeLayer(SKColors.Transparent, "Layer 1");
        MakeLayer(SKColors.Transparent, "Layer 2");
        MakeTextLayer("Some text here!!!!", "Some Text Layer");

        CompositionLayer comp = MakeCompLayer("A composition layer");
        MakeLayer(SKColors.Transparent, "Layer 1 in comp", comp);
        MakeLayer(SKColors.Transparent, "Layer 2 in comp", comp);

        MakeLayer(SKColors.Transparent, "Layer 4");
        RasterLayer background = MakeLayer(SKColors.White, "Background Base Layer");
        background.AddEffect(new ColourChannelLayerEffect());

        this.Editor.AddDocument(document);
        this.Editor.ActiveDocument = document;

        ISelectionManager<BaseLayerTreeObject> selectionManager = DataKeys.LayerSelectionManagerKey.GetContext(this.contextData)!;
        selectionManager.SelectionChanged += this.OnSelectionChanged;
        selectionManager.SelectionCleared += this.OnSelectionCleared;
        selectionManager.Select(firstLayer);

        Dispatcher.UIThread.InvokeAsync(() => {
            this.PART_Canvas.RenderCanvas();
            Dispatcher.UIThread.InvokeAsync(() => {
                this.PART_Canvas.PART_FreeMoveViewPort!.FitContentToCenter();
            }, DispatcherPriority.Background);
        }, DispatcherPriority.Background);
    }

    private void OnSelectionChanged(ISelectionManager<BaseLayerTreeObject> sender, IList<BaseLayerTreeObject>? olditems, IList<BaseLayerTreeObject>? newitems) {
        PicNetPropertyEditor.Instance.UpdateSelectedLayerSelection(this.PART_LayerTreeControl.SelectionManager, false);
    }

    private void OnSelectionCleared(ISelectionManager<BaseLayerTreeObject> sender) {
        PicNetPropertyEditor.Instance.UpdateSelectedLayerSelection(this.PART_LayerTreeControl.SelectionManager, true);
    }

    private void OnActiveDocumentChanged(Editor sender, Document? oldactivedocument, Document? newactivedocument) {
        this.PART_Canvas.Document = newactivedocument;
        this.PART_LayerTreeControl.Canvas = newactivedocument?.Canvas;

        this.contextData.Set(DataKeys.DocumentKey, newactivedocument);
        if (newactivedocument != null) {
            PicNetPropertyEditor.Instance.UpdateSelectedLayerSelection(this.PART_LayerTreeControl.SelectionManager, false);
        }

        DataManager.InvalidateInheritedContext(this);
    }

    private void OnColourPickerColourChanged(object? sender, ColorChangedEventArgs e) {
        this.Editor.PrimaryColour = new SKColor(e.NewColor.ToUInt32());
    }

    private void SetViewModeClick(object? sender, RoutedEventArgs e) {
        if (((Button?) sender)?.Tag is LayerTreeViewMode mode) {
            if (this.PART_LayerTreeControl.ViewMode == mode) {
                ((ToggleButton) sender).IsChecked = true;
                e.Handled = true;
            }
            else {
                this.PART_LayerTreeControl.ViewMode = mode;
            }
        }
    }
}