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

using Avalonia.Input;
using PicNetStudio.Avalonia.DataTransfer;

namespace PicNetStudio.Avalonia.PicNet.Tools;

/// <summary>
/// The base class for all tools that can be used on the canvas
/// <para>
/// A tool can accept character inputs by the user pressing a key, with a passed in state
/// </para>
/// </summary>
public abstract class BaseCanvasTool : ITransferableData {
    public TransferableData TransferableData { get; }

    protected BaseCanvasTool() {
        this.TransferableData = new TransferableData(this);
    }
    
    // TODO: cursors

    /// <summary>
    /// Invoked when the user pressed the cursor or pressed down on their touch screen
    /// </summary>
    /// <param name="document"></param>
    /// <param name="x">The X position where the event occurred (may be between 2 pixels)</param>
    /// <param name="y">The Y position where the event occurred (may be between 2 pixels)</param>
    /// <param name="count">
    ///     The number of times the pointer was pressed in quick succession.
    ///     Typically not used on touch screens. A press and release event will occur
    ///     before this becomes 2, and a timeout resets it back to 1 (typically ~250ms)
    /// </param>
    /// <param name="cursor">The type of cursor that was involved</param>
    /// <param name="modifiers"></param>
    public virtual bool OnCursorPressed(Document document, double x, double y, int count, EnumCursorType cursor, KeyModifiers modifiers) {
        return false;
    }

    /// <summary>
    /// Invoked when the user releases their cursor or stopped pressing their touch screen
    /// </summary>
    /// <param name="document"></param>
    /// <param name="x">The X position where the event occurred (may be between 2 pixels)</param>
    /// <param name="y">The Y position where the event occurred (may be between 2 pixels)</param>
    /// <param name="cursor">The type of cursor that was involved</param>
    /// <param name="modifiers"></param>
    public virtual bool OnCursorReleased(Document document, double x, double y, EnumCursorType cursor, KeyModifiers modifiers) {
        return false;
    }

    /// <summary>
    /// Invoked when the user moves their cursor. It may be pressed or released
    /// </summary>
    /// <param name="document"></param>
    /// <param name="x">The X position where the event occurred (may be between 2 pixels)</param>
    /// <param name="y">The Y position where the event occurred (may be between 2 pixels)</param>
    /// <param name="cursorMask">All of the cursor buttons pressed during the mouse movement event (bitmask)</param>
    public virtual bool OnCursorMoved(Document document, double x, double y, EnumCursorType cursorMask) {
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
    public virtual bool? OnCharacterPress(Document document, Key key, KeyModifiers modifiers) {
        return null;
    }

    public virtual void OnCharacterInput(Document document, Key key, KeyModifiers modifiers) {
    }

    public virtual bool OnCharacterRelease(Document document, Key key, KeyModifiers modifiers) {
        return false;
    }
}