// 
// Copyright (c) 2023-2024 REghZy
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
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using PicNetStudio.Avalonia.Utils;

namespace PicNetStudio.Avalonia.PicNet.Controls.Dragger;

public class NumberDragger : RangeBase {
    public static readonly StyledProperty<double> TinyChangeProperty = AvaloniaProperty.Register<NumberDragger, double>("TinyChange", 0.1);
    public static readonly StyledProperty<double> NormalChangeProperty = AvaloniaProperty.Register<NumberDragger, double>("NormalChange", 1.0);
    public static readonly StyledProperty<DragDirection> DragDirectionProperty = AvaloniaProperty.Register<NumberDragger, DragDirection>("DragDirection", DragDirection.LeftDecrRightIncr);
    public static readonly StyledProperty<bool> LockCursorOnDragProperty = AvaloniaProperty.Register<NumberDragger, bool>("LockCursorOnDrag", true);
    public static readonly StyledProperty<int> NonFormattedRoundedPlacesProperty = AvaloniaProperty.Register<NumberDragger, int>("NonFormattedRoundedPlaces", 2);
    public static readonly StyledProperty<int> NonFormattedRoundedPlacesForEditProperty = AvaloniaProperty.Register<NumberDragger, int>("NonFormattedRoundedPlacesForEdit", 6);
    public static readonly StyledProperty<IValueFormatter?> ValueFormatterProperty = AvaloniaProperty.Register<NumberDragger, IValueFormatter?>("ValueFormatter");
    public static readonly StyledProperty<TextAlignment> TextAlignmentProperty = TextBlock.TextAlignmentProperty.AddOwner<NumberDragger>();

    private TextBlock? PART_TextBlock;
    private TextBox? PART_TextBox;
    private Point lastClickPos, lastMouseMove;
    private int dragState; // 0 = default, 1 = standby, 2 = active
    private bool isEditing;

    public double NormalChange {
        get => this.GetValue(NormalChangeProperty);
        set => this.SetValue(NormalChangeProperty, value);
    }
    
    public double TinyChange {
        get => this.GetValue(TinyChangeProperty);
        set => this.SetValue(TinyChangeProperty, value);
    }

    public DragDirection DragDirection {
        get => this.GetValue(DragDirectionProperty);
        set => this.SetValue(DragDirectionProperty, value);
    }
    
    public bool LockCursorOnDrag {
        get => this.GetValue(LockCursorOnDragProperty);
        set => this.SetValue(LockCursorOnDragProperty, value);
    }

    /// <summary>
    /// Gets or sets the number of rounded places to use when rounding the value for preview in
    /// the control when not editing. This value is ignored when a <see cref="ValueFormatter"/> is present
    /// </summary>
    public int NonFormattedRoundedPlaces {
        get => this.GetValue(NonFormattedRoundedPlacesProperty);
        set => this.SetValue(NonFormattedRoundedPlacesProperty, value);
    }

    public int NonFormattedRoundedPlacesForEdit {
        get => this.GetValue(NonFormattedRoundedPlacesForEditProperty);
        set => this.SetValue(NonFormattedRoundedPlacesForEditProperty, value);
    }
    
    public IValueFormatter? ValueFormatter {
        get => this.GetValue(ValueFormatterProperty);
        set => this.SetValue(ValueFormatterProperty, value);
    }

    public TextAlignment TextAlignment {
        get => this.GetValue(TextAlignmentProperty);
        set => this.SetValue(TextAlignmentProperty, value);
    }

    public bool EditState {
        get => this.isEditing;
        set {
            if (this.isEditing == value)
                return;

            this.isEditing = value;
            this.UpdateTextControlVisibility();
            this.UpdateTextBlockAndBox();
            if (value && this.PART_TextBox != null) {
                BugFix.TextBox_FocusSelectAll(this.PART_TextBox);
            }
        }
    }
    
    public NumberDragger() {
        ValueProperty.Changed.AddClassHandler<NumberDragger, double>((o, e) => o.OnValueChanged(e.OldValue.GetValueOrDefault(), e.NewValue.GetValueOrDefault()));
    }

    private string GetValueToString(bool isEditing) => this.GetValueToString(this.Value, isEditing);

    private string GetValueToString(double value, bool isEditing) {
        if (this.ValueFormatter is IValueFormatter formatter) {
            return formatter.ToString(value, isEditing);
        }
        else {
            int roundedPlaces = isEditing ? this.NonFormattedRoundedPlacesForEdit : this.NonFormattedRoundedPlaces;
            return value.ToString("F" + Math.Max(roundedPlaces, 0));
        }
    }

    private void UpdateTextBlockAndBox() {
        string? textBlock = null;
        if (this.PART_TextBlock != null)
            this.PART_TextBlock.Text = textBlock = this.GetValueToString(false);
        if (this.PART_TextBox != null)
            this.PART_TextBox.Text = this.isEditing ? this.GetValueToString(true) : (textBlock ?? this.GetValueToString(false));
    }
    
    private void OnValueChanged(double oldValue, double newValue) => this.UpdateTextBlockAndBox();

    protected override void OnLoaded(RoutedEventArgs e) {
        base.OnLoaded(e);
        this.UpdateCursor();
        this.UpdateTextControlVisibility();
        this.UpdateTextBlockAndBox();
    }

    private void UpdateTextControlVisibility() {
        if (this.PART_TextBlock != null)
            this.PART_TextBlock!.IsVisible = !this.isEditing;
        if (this.PART_TextBox != null)
            this.PART_TextBox!.IsVisible = this.isEditing;
    }
    
    private void UpdateCursor() {
        this.Cursor = new Cursor(this.dragState != 2 ? StandardCursorType.Arrow : StandardCursorType.None);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        this.PART_TextBlock = e.NameScope.GetTemplateChild<TextBlock>(nameof(this.PART_TextBlock));
        this.PART_TextBox = e.NameScope.GetTemplateChild<TextBox>(nameof(this.PART_TextBox));
        if (this.PART_TextBox != null) {
            this.PART_TextBox.KeyDown += this.OnTextInputKeyPress;
            this.PART_TextBox.LostFocus += this.OnTextInputFocusLost;
        }
    }

    private void OnTextInputFocusLost(object? sender, RoutedEventArgs e) {
        this.EditState = false;
    }

    private void OnTextInputKeyPress(object? sender, KeyEventArgs e) {
        if (e.Key == Key.Enter || e.Key == Key.Escape) {
            this.CompleteEdit(e);
        }
    }

    private void CompleteEdit(KeyEventArgs e) {
        string? parseText = this.PART_TextBox!.Text;
        this.EditState = false;
        if (parseText == null || e.Key == Key.Escape) {
            return;
        }

        if (this.ValueFormatter is IValueFormatter formatter) {
            if (formatter.TryConvertToDouble(parseText, out double value)) {
                this.Value = value;
            }
        }
        else if (double.TryParse(parseText, out double newValue)) {
            this.Value = newValue;
        }
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e) {
        base.OnPointerPressed(e);
        e.Handled = true;
        this.dragState = 1;
        this.lastClickPos = this.lastMouseMove = e.GetPosition(this);
        e.Pointer.Capture(this);
        e.PreventGestureRecognition();
        this.UpdateCursor();
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e) {
        base.OnPointerReleased(e);
        e.Handled = true;
        int state = this.dragState;
        this.dragState = 0;
        
        if (state == 1) {
            this.EditState = true;
        }
        else {
            if (ReferenceEquals(e.Pointer.Captured, this))
                e.Pointer.Capture(null);
        }

        this.UpdateCursor();
    }

    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e) {
        base.OnPointerCaptureLost(e);
    }

    protected override void OnKeyDown(KeyEventArgs e) {
        base.OnKeyDown(e);
        if (e.Key == Key.Escape && this.dragState != 0) {
            e.Handled = true;
            this.dragState = 0;
            this.UpdateCursor();
        }
    }

    protected override void OnPointerMoved(PointerEventArgs e) {
        base.OnPointerMoved(e);
        if (this.dragState == 0) {
            return;
        }

        PointerPoint pointer = e.GetCurrentPoint(this);
        if (!pointer.Properties.IsLeftButtonPressed) {
            this.dragState = 0;
            if (ReferenceEquals(e.Pointer.Captured, this))
                e.Pointer.Capture(null);
            this.UpdateCursor();
            return;
        }

        Point point = e.GetPosition(this);
        DragDirection dir = this.DragDirection;
        Point delta = point - this.lastMouseMove;

        if (this.dragState == 1) {
            if (dir == DragDirection.LeftDecrRightIncr || dir == DragDirection.LeftIncrRightDecr) {
                if (!(Math.Abs(delta.X) > 4))
                    return;
            }
            else if (!(Math.Abs(delta.Y) > 4)) {
                return;
            }

            this.dragState = 2;
            this.UpdateCursor();
        }

        bool isShiftDown = (e.KeyModifiers & KeyModifiers.Shift) != 0;
        bool isCtrlDown = (e.KeyModifiers & KeyModifiers.Control) != 0;

        if (isShiftDown) {
            if (isCtrlDown) {
                delta *= this.TinyChange;
            }
            else {
                delta *= this.SmallChange;
            }
        }
        else if (isCtrlDown) {
            delta *= this.LargeChange;
        }
        else {
            delta *= this.NormalChange;
        }

        double oldValue = this.Value, newValue;
        switch (dir) {
            case DragDirection.LeftDecrRightIncr: newValue = oldValue + delta.X; break;
            case DragDirection.LeftIncrRightDecr: newValue = oldValue - delta.X; break;
            case DragDirection.UpDecrDownIncr:    newValue = oldValue + delta.Y; break;
            case DragDirection.UpIncrDownDecr:    newValue = oldValue - delta.Y; break;
            default: throw new ArgumentOutOfRangeException();
        }

        double coercedNewValue = Maths.Clamp(newValue, this.Minimum, this.Maximum);
        if (!DoubleUtils.AreClose(coercedNewValue, oldValue)) {
            this.Value = coercedNewValue;
        }

        if (this.LockCursorOnDrag) {
            PixelPoint sp = this.PointToScreen(this.lastClickPos);
            CursorUtils.SetCursorPos(sp.X, sp.Y);
        }
        else {
            this.lastMouseMove = point;
        }
    }
}