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
using PicNetStudio.Utils;
using SkiaSharp;

namespace PicNetStudio.PicNet.Tools;

public delegate void BaseCanvasToolEventHandler(BaseCanvasTool sender);

/// <summary>
/// The base class for all tools that can be used on the canvas
/// <para>
/// A tool can accept character inputs by the user pressing a key, with a passed in state
/// </para>
/// </summary>
public abstract class BaseCanvasTool : ITransferableData {
    private bool hasProcessedCursor;
    
    public TransferableData TransferableData { get; }
    
    public event BaseCanvasToolEventHandler? CursorInvalidated;

    protected BaseCanvasTool() {
        this.TransferableData = new TransferableData(this);
    }

    protected void InvalidateCursor() {
        this.CursorInvalidated?.Invoke(this);
        this.hasProcessedCursor = false;
    }

    // internal Cursor? GetCursor() {
    //     if (this.myCursor != null || this.hasProcessedCursor)
    //         return this.myCursor;
    //     SKImage? img = this.DrawCursor(out SKPoint hotSpot);
    //     this.hasProcessedCursor = true;
    //     if (img == null)
    //         return null;
    //     using SKData? data = img.Encode(SKEncodedImageFormat.Png, 100);
    //     using MemoryStream stream = new MemoryStream(data.ToArray());
    //     this.myCursorBitmap = new Bitmap(stream);
    //     PixelPoint hotspotPoint = new PixelPoint((int)hotSpot.X, (int)hotSpot.Y);
    //     return this.myCursor = new Cursor(this.myCursorBitmap, hotspotPoint);
    // }
    
    /// <summary>
    /// Draws this tool's cursor into the canvas. This method is typically only ever called once per instance
    /// </summary>
    /// <param name="hotSpot"></param>
    /// <returns></returns>
    protected internal virtual SKImage? DrawCursor(out SKPoint hotSpot) {
        hotSpot = default;
        return null;
    }

    /// <summary>
    /// Invoked when the user pressed the cursor or pressed down on their touch screen
    /// </summary>
    /// <param name="document"></param>
    /// <param name="relPos">The position of the cursor, relative to the active layer</param>
    /// <param name="absPos">The position of the cursor relative to the entire canvas</param>
    /// <param name="count">
    ///     The number of times the pointer was pressed in quick succession.
    ///     Typically not used on touch screens. A press and release event will occur
    ///     before this becomes 2, and a timeout resets it back to 1 (typically ~250ms)
    /// </param>
    /// <param name="cursor">The type of cursor that was involved</param>
    /// <param name="modifiers"></param>
    public virtual bool OnCursorPressed(Document document, SKPointD relPos, SKPointD absPos, int count, EnumCursorType cursor, ModifierKeys modifiers) {
        return false;
    }

    /// <summary>
    /// Invoked when the user releases their cursor or stopped pressing their touch screen
    /// </summary>
    /// <param name="document"></param>
    /// <param name="relPos">The position of the cursor, relative to the active layer</param>
    /// <param name="absPos">The position of the cursor relative to the entire canvas</param>
    /// <param name="cursor">The type of cursor that was involved</param>
    /// <param name="modifiers"></param>
    /// <param name="x">The X position where the event occurred (may be between 2 pixels)</param>
    /// <param name="y">The Y position where the event occurred (may be between 2 pixels)</param>
    public virtual bool OnCursorReleased(Document document, SKPointD relPos, SKPointD absPos, EnumCursorType cursor, ModifierKeys modifiers) {
        return false;
    }

    /// <summary>
    /// Invoked when the user moves their cursor. It may be pressed or released
    /// </summary>
    /// <param name="document"></param>
    /// <param name="relPos">The position of the cursor, relative to the active layer</param>
    /// <param name="absPos">The position of the cursor relative to the entire canvas</param>
    /// <param name="cursorMask">All of the cursor buttons pressed during the mouse movement event (bitmask)</param>
    /// <param name="x">The X position where the event occurred (may be between 2 pixels)</param>
    /// <param name="y">The Y position where the event occurred (may be between 2 pixels)</param>
    public virtual bool OnCursorMoved(Document document, SKPointD relPos, SKPointD absPos, EnumCursorType cursorMask) {
        return false;
    }

    /// <summary>
    /// Invoked when the user presses a key down on only their keyboard
    /// </summary>
    /// <param name="document"></param>
    /// <param name="canvasUi"></param>
    /// <param name="key">The pressed key</param>
    /// <param name="modifiers"></param>
    /// <returns>
    /// True when the key press is handled, false when the key press should be
    /// sent to the character input procedure, null when to ignore the key press
    /// </returns>
    public virtual bool? OnCharacterPress(Document document, PNKey key, ModifierKeys modifiers) {
        return null;
    }

    public virtual void OnCharacterInput(Document document, PNKey key, ModifierKeys modifiers) {
    }

    public virtual bool OnCharacterRelease(Document document, PNKey key, ModifierKeys modifiers) {
        return false;
    }
}