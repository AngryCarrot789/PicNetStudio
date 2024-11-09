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
using System.Collections.Generic;
using PicNetStudio.Avalonia.PicNet.Toolbars;
using SkiaSharp;

namespace PicNetStudio.Avalonia.PicNet;

public delegate void EditorDocumentIndexChangedEventHandler(Editor editor, Document document, int oldIndex, int newIndex);
public delegate void EditorActiveDocumentChangedEventHandler(Editor sender, Document? oldActiveDocument, Document? newActiveDocument);
public delegate void EditorEventHandler(Editor sender);

/// <summary>
/// Represents the state of a PixNet Studio editor window. At the moment, only a single editor exists, but in the future, one can open multiple editor windows per app instance and drag-drop document tabs between them.
/// This class contains a list of opened and recently closed documents, recent colours, etc.
/// </summary>
public class Editor {
    private readonly List<Document> documents;
    private SKColor primaryColour;
    private SKColor secondaryColour;
    private Document? activeDocument;
    
    /// <summary>
    /// A read-only list of all of the documents currently open
    /// </summary>
    public IList<Document> Documents { get; }

    /// <summary>
    /// The main toolbar for this editor
    /// </summary>
    public EditorToolBar ToolBar { get; }

    public SKColor PrimaryColour {
        get => this.primaryColour;
        set {
            if (this.primaryColour == value)
                return;

            this.primaryColour = value;
            this.PrimaryColourChanged?.Invoke(this);
        }
    }
    
    public SKColor SecondaryColour {
        get => this.secondaryColour;
        set {
            if (this.secondaryColour == value)
                return;

            this.secondaryColour = value;
            this.PrimaryColourChanged?.Invoke(this);
        }
    }
    
    public Document? ActiveDocument {
        get => this.activeDocument;
        set {
            Document? oldActiveDocument = this.activeDocument;
            if (oldActiveDocument == value)
                return;

            this.activeDocument = value;
            this.ActiveDocumentChanged?.Invoke(this, oldActiveDocument, value);
        }
    }
    
    /// <summary>
    /// An event fired when a document is added, removed or moved
    /// </summary>
    public event EditorDocumentIndexChangedEventHandler? DocumentIndexChanged;
    
    /// <summary>
    /// An event fired when the active document changes
    /// </summary>
    public event EditorActiveDocumentChangedEventHandler? ActiveDocumentChanged;
    
    public event EditorEventHandler? PrimaryColourChanged;
    public event EditorEventHandler? SecondaryColourChanged;

    public Editor() {
        this.primaryColour = SKColors.ForestGreen;
        this.secondaryColour = SKColors.White;
        
        this.documents = new List<Document>();
        this.Documents = this.documents.AsReadOnly();
        this.ToolBar = new EditorToolBar(this);
    }

    public void AddDocument(Document document) {
        if (document.Editor == this)
            throw new InvalidOperationException("Canvas already added");

        this.InsertDocument(this.documents.Count, document);
    }

    public void InsertDocument(int index, Document document) {
        if (document.Editor == this)
            throw new InvalidOperationException("Canvas already added");

        this.documents.Insert(index, document);
        Document.InternalSetEditor(document, this);
        this.DocumentIndexChanged?.Invoke(this, document, -1, index);

        if (this.documents.Count == 1)
            this.ActiveDocument = document;
    }

    public void RemoveDocumentAt(int index) {
        this.RemoveDocumentInternal(index, this.documents[index]);
    }
    
    public void RemoveDocumentInternal(int index, Document document) {
        if (this.documents.Count == 1)
            this.ActiveDocument = null;
        
        this.documents.RemoveAt(index);
        Document.InternalSetEditor(document, null);
        this.DocumentIndexChanged?.Invoke(this, document, index, -1);
    }

    public void MoveDocument(int oldIndex, int newIndex) {
        Document document = this.documents[oldIndex];
        this.DocumentIndexChanged?.Invoke(this, document, oldIndex, newIndex);
    }
}