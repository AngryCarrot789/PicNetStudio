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
using Avalonia.Input;
using PicNetStudio.PicNet;
using PicNetStudio.PicNet.Layers.Core;
using PicNetStudio.PicNet.Tools;
using PicNetStudio.Utils;
using SkiaSharp;

namespace PicNetStudio.Avalonia.PicNet.Controls;

/// <summary>
/// A class used to work around Avalonia's shitty mouse button state tracking
/// systems (click LMB then RMB and then release LMB... no pointer released event is fired)
/// and also handle keyboard inputs and so on
/// </summary>
public class CanvasInputHandler {
    private readonly CanvasViewPortControl control;
    private Point? lastMouseMove;
    private EnumCursorType myCursors;

    /// <summary>
    /// Gets the cursor buttons that are currently pressed
    /// </summary>
    public EnumCursorType CursorButtons => this.myCursors;

    public CanvasInputHandler(CanvasViewPortControl control) {
        this.control = control;
        this.control.PointerPressed += this.OnControlPointerPressed;
        this.control.PointerReleased += this.OnControlPointerReleased;
        this.control.PointerMoved += this.OnPointerPointerMoved;

        this.control.KeyDown += this.OnControlKeyDown;
        this.control.KeyUp += this.OnControlKeyUp;
    }

    private void OnControlKeyDown(object? sender, KeyEventArgs e) {
        if (!this.control.IsFocused)
            this.control.Focus(NavigationMethod.Pointer);

        if (this.control.Document is Document document && document.Editor?.ToolBar.ActiveTool is BaseCanvasTool tool) {
            bool? result = tool.OnCharacterPress(document, (PNKey) e.Key, (ModifierKeys) e.KeyModifiers);
            if (result.HasValue) {
                if (result.Value) {
                    e.Handled = true;
                }
                else {
                    tool.OnCharacterInput(document, (PNKey) e.Key, (ModifierKeys) e.KeyModifiers);
                }
            }
        }
    }

    private void OnControlKeyUp(object? sender, KeyEventArgs e) {
        if (this.control.Document is Document document && document.Editor?.ToolBar.ActiveTool is BaseCanvasTool activeTool) {
            e.Handled = activeTool.OnCharacterRelease(document, (PNKey) e.Key, (ModifierKeys) e.KeyModifiers);
        }
    }

    // Returns old buttons
    private EnumCursorType UpdateButtonForChange(PointerPoint pointer, bool isPress) {
        PointerUpdateKind kind = pointer.Properties.PointerUpdateKind;
        if (isPress && kind >= PointerUpdateKind.LeftButtonReleased && kind <= PointerUpdateKind.XButton2Released) {
            // If it's a press event but for some reason the update kind is a release update, then just ignore it...? who knows
            return this.myCursors;
        }

        return this.UpdateButtons(pointer);
    }

    // Avalonia will put the button whose state change into
    // the move event, so we extract it and update our state
    private EnumCursorType UpdateButtons(PointerPoint pointer) {
        EnumCursorType oldButtons = this.myCursors;
        this.myCursors = this.GetPressedCursors(pointer);
        return oldButtons;
    }

    private void OnControlPointerPressed(object? sender, PointerPressedEventArgs e) {
        PointerPoint pointer = e.GetCurrentPoint(this.control.PART_SkiaViewPort);
        EnumCursorType oldButtons = this.UpdateButtonForChange(pointer, true);
        EnumCursorType newButtons = this.myCursors;
        if (oldButtons == newButtons) {
            // no buttons changed, so do nothing
            return;
        }

        if (this.control.Document is Document document && document.Editor?.ToolBar.ActiveTool is BaseCanvasTool activeTool) {
            Point absPos = pointer.Position;
            Point relPos = this.TranslatePoint(absPos, document);
            if (this.ProcessChangedButtons(relPos, absPos, document, activeTool, e.ClickCount, oldButtons, newButtons, e.KeyModifiers)) {
                e.Pointer.Capture(this.control);
            }
        }
    }

    private void OnControlPointerReleased(object? sender, PointerReleasedEventArgs e) {
        PointerPoint pointer = e.GetCurrentPoint(this.control.PART_SkiaViewPort);
        EnumCursorType oldButtons = this.UpdateButtonForChange(pointer, false);
        EnumCursorType newButtons = this.myCursors;
        if (oldButtons == newButtons) {
            // no buttons changed, so do nothing
            return;
        }
        
        if (ReferenceEquals(e.Pointer.Captured, this.control))
            e.Pointer.Capture(null);

        if (this.control.Document is Document document && document.Editor?.ToolBar.ActiveTool is BaseCanvasTool activeTool) {
            Point absPos = pointer.Position;
            Point relPos = this.TranslatePoint(absPos, document);
            this.ProcessChangedButtons(relPos, absPos, document, activeTool, 1, oldButtons, newButtons, e.KeyModifiers);
        }
    }

    private void OnPointerPointerMoved(object? sender, PointerEventArgs e) {
        PointerPoint pointer = e.GetCurrentPoint(this.control.PART_SkiaViewPort);
        EnumCursorType oldButtons = this.UpdateButtons(pointer);
        EnumCursorType newButtons = this.myCursors;

        if (!(this.control.Document is Document document)) {
            return;
        }
        
        Point absPos = pointer.Position;
        if (document.Editor?.ToolBar.ActiveTool is BaseCanvasTool activeTool) {
            Point relPos = this.TranslatePoint(absPos, document);
            // We also need to do a final check on the last mouse move position,
            // because Avalonia receives WM_MOVE even though the mouse didn't actually move.
            // To reproduce: Click LMB then RMB then release RMB, and with the lastMouseMove check
            // removed you will hit a breakpoint on OnCursorMoved
            if ((oldButtons == newButtons) && newButtons != EnumCursorType.None && (!this.lastMouseMove.HasValue || !this.lastMouseMove.Value.Equals(relPos))) {
                // Debug.WriteLine("Cursor Moved with [" + newButtons + "] pressed");
                e.Handled = activeTool.OnCursorMoved(document, new SKPointD(relPos.X, relPos.Y), new SKPointD(absPos.X, absPos.Y), newButtons);
            }
            else {
                e.Handled = this.ProcessChangedButtons(relPos, absPos, document, activeTool, 1, oldButtons, newButtons, e.KeyModifiers);
            }

            this.lastMouseMove = relPos;
        }
        else {
            this.lastMouseMove = absPos;
        }
    }

    private Point TranslatePoint(Point pos, Document document) {
        if (document.Canvas.ActiveLayerTreeObject is BaseVisualLayer visualLayer) {
            SKPoint mapped = visualLayer.AbsoluteInverseTransformationMatrix.MapPoint(new SKPoint((float) pos.X, (float) pos.Y));
            return new Point(mapped.X, mapped.Y);
        }

        return pos;
    }

    private bool ProcessChangedButtons(Point relPos, Point absPos, Document document, BaseCanvasTool activeTool, int clickCount, EnumCursorType oldButtons, EnumCursorType newButtons, KeyModifiers modifiers) {
        bool handled = false;
        EnumCursorType addedFlags = (oldButtons ^ newButtons) & newButtons;
        foreach (EnumCursorType type in GetEnumerableFlagSet(addedFlags)) {
            // Debug.WriteLine("Cursor pressed: " + type);
            handled |= activeTool.OnCursorPressed(document, new SKPointD(relPos.X, relPos.Y), new SKPointD(absPos.X, absPos.Y), clickCount, type, (ModifierKeys) modifiers);
        }

        EnumCursorType removedFlags = (oldButtons ^ newButtons) & oldButtons;
        foreach (EnumCursorType type in GetEnumerableFlagSet(removedFlags)) {
            // Debug.WriteLine("Cursor released: " + type);
            handled |= activeTool.OnCursorReleased(document, new SKPointD(relPos.X, relPos.Y), new SKPointD(absPos.X, absPos.Y), type, (ModifierKeys) modifiers);
        }

        return handled;
    }

    public static IEnumerable<EnumCursorType> GetEnumerableFlagSet(EnumCursorType flags) {
        if ((flags & EnumCursorType.Primary) != 0)
            yield return EnumCursorType.Primary;
        if ((flags & EnumCursorType.Secondary) != 0)
            yield return EnumCursorType.Secondary;
        if ((flags & EnumCursorType.Middle) != 0)
            yield return EnumCursorType.Middle;
        if ((flags & EnumCursorType.XButton1) != 0)
            yield return EnumCursorType.XButton1;
        if ((flags & EnumCursorType.XButton2) != 0)
            yield return EnumCursorType.XButton2;
    }

    public EnumCursorType GetPressedCursors(PointerPoint point) {
        EnumCursorType cursors = EnumCursorType.None;
        PointerPointProperties props = point.Properties;
        if (props.IsLeftButtonPressed)
            cursors |= EnumCursorType.Primary;
        if (props.IsMiddleButtonPressed)
            cursors |= EnumCursorType.Middle;
        if (props.IsRightButtonPressed)
            cursors |= EnumCursorType.Secondary;
        if (props.IsXButton1Pressed)
            cursors |= EnumCursorType.XButton1;
        if (props.IsXButton2Pressed)
            cursors |= EnumCursorType.XButton2;
        return cursors;
    }
}