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

using PicNetStudio.Avalonia.PicNet.Layers;

namespace PicNetStudio.Avalonia.PicNet;

public delegate void EditorChangedEventHandler(Document document, Editor? oldEditor, Editor? newEditor);

/// <summary>
/// Represents an document which contains the canvas, file information, etc.
/// </summary>
public class Document {
    private BaseLayerTreeObject? activeLayerTreeObject;

    /// <summary>
    /// The editor that this document exists in
    /// </summary>
    public Editor? Editor { get; private set; }

    /// <summary>
    /// An event fired when our editor changes. This will happen when a canvas is added to, removed from or moved between editor
    /// </summary>
    public event EditorChangedEventHandler? EditorChanged;

    public Canvas Canvas { get; }

    /// <summary>
    /// Gets this canvas' layer selection manager, which stores which layers are selected
    /// </summary>
    public SimpleSelectionManager<BaseLayerTreeObject> LayerSelectionManager { get; }

    public Document() {
        this.Canvas = new Canvas(this);
    }

    internal static void InternalSetEditor(Document document, Editor? editor) {
        Editor? oldEditor = document.Editor;
        if (ReferenceEquals(oldEditor, editor))
            return;

        document.Editor = editor;
        document.EditorChanged?.Invoke(document, oldEditor, editor);
    }
}