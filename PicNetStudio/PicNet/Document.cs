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

using PicNetStudio.DataTransfer;
using PicNetStudio.Utils.Accessing;
using PicNetStudio.Utils.RBC;

namespace PicNetStudio.PicNet;

public delegate void EditorChangedEventHandler(Document document, Editor? oldEditor, Editor? newEditor);

/// <summary>
/// Represents an document which contains the canvas, file information, etc.
/// </summary>
public class Document : ITransferableData {
    public static readonly DataParameterString FilePathParameter = DataParameter.Register(new DataParameterString(typeof(Document), nameof(FilePath), null, ValueAccessors.Reflective<string?>(typeof(Document), nameof(filePath))));

    private string? filePath = FilePathParameter.DefaultValue;

    public string? FilePath {
        get => this.filePath;
        set => DataParameter.SetValueHelper(this, FilePathParameter, ref this.filePath, value);
    }
    
    /// <summary>
    /// The editor that this document exists in
    /// </summary>
    public Editor? Editor { get; private set; }

    /// <summary>
    /// An event fired when our editor changes. This will happen when a canvas is added to, removed from or moved between editor
    /// </summary>
    public event EditorChangedEventHandler? EditorChanged;

    public Canvas Canvas { get; }

    public TransferableData TransferableData { get; }
    
    public Document() {
        this.TransferableData = new TransferableData(this);
        this.Canvas = new Canvas(this);
    }

    public void SaveTo(string path, bool updateFilePath) {
        if (string.IsNullOrWhiteSpace(path))
            throw new InvalidOperationException("Invalid file path");

        if (updateFilePath)
            this.FilePath = path;

        RBEDictionary dictionary = new RBEDictionary();
        this.WriteProjectData(dictionary);

        try {
            RBEUtils.WriteToFilePacked(dictionary, path);
        }
        catch (Exception e) {
            throw new IOException("Failed to write RBE data to file", e);
        }
    }

    public void ReadFrom(string path, bool updateFilePath) {
        if (string.IsNullOrWhiteSpace(path))
            throw new InvalidOperationException("Invalid file path");

        if (updateFilePath)
            this.FilePath = path;

        RBEDictionary dictionary;
        try {
            dictionary = (RBEDictionary) RBEUtils.ReadFromFilePacked(path);
        }
        catch (Exception e) {
            throw new IOException("Failed to read RBE data from file", e);
        }
        
        this.ReadProjectData(dictionary);
    }

    private void WriteProjectData(RBEDictionary data) {
        try {
            this.Canvas.WriteToRBE(data.CreateDictionary("Canvas"));
        }
        catch (Exception e) {
            throw new Exception("Failed to serialise project data", e);
        }
    }
    
    private void ReadProjectData(RBEDictionary data) {
        try {
            this.Canvas.ReadFromRBE(data.GetDictionary("Canvas"));
        }
        catch (Exception e) {
            throw new Exception("Failed to deserialise project data", e);
        }
    }
    
    internal static void InternalSetEditor(Document document, Editor? editor) {
        Editor? oldEditor = document.Editor;
        if (ReferenceEquals(oldEditor, editor))
            return;

        document.Editor = editor;
        document.EditorChanged?.Invoke(document, oldEditor, editor);
    }
}