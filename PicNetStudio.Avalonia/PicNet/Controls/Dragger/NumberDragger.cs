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
using System.ComponentModel;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using PicNetStudio.Avalonia.PicNet.Controls.Dragger.Expressions;
using PicNetStudio.Avalonia.Utils;

namespace PicNetStudio.Avalonia.PicNet.Controls.Dragger;

[TemplatePart(Name = nameof(PART_HintTextBlock), Type = typeof(TextBlock))]
[TemplatePart(Name = nameof(PART_TextBlock), Type = typeof(TextBlock))]
[TemplatePart(Name = nameof(PART_TextBox), Type = typeof(TextBox))]
public class NumberDragger : RangeBase {
    private bool _isDragging;
    private bool _isEditing;

    public static readonly StyledProperty<double> TinyChangeProperty = AvaloniaProperty.Register<NumberDragger, double>("TinyChange", 0.001d);
    public static readonly StyledProperty<double> MassiveChangeProperty = AvaloniaProperty.Register<NumberDragger, double>("MassiveChange", 5d);

    public static readonly DirectProperty<NumberDragger, bool> IsDraggingProperty = AvaloniaProperty.RegisterDirect<NumberDragger, bool>("IsDragging", o => o.IsDragging, (o, v) => o.IsDragging = v);
    public static readonly StyledProperty<bool?> CompleteEditOnTextBoxLostFocusProperty = AvaloniaProperty.Register<NumberDragger, bool?>("CompleteEditOnTextBoxLostFocus", true);
    public static readonly StyledProperty<Orientation> OrientationProperty = AvaloniaProperty.Register<NumberDragger, Orientation>("Orientation", Orientation.Horizontal);
    public static readonly StyledProperty<HorizontalIncrement> HorizontalIncrementProperty = AvaloniaProperty.Register<NumberDragger, HorizontalIncrement>("HorizontalIncrement", HorizontalIncrement.LeftDecrRightIncr);
    public static readonly StyledProperty<VerticalIncrement> VerticalIncrementProperty = AvaloniaProperty.Register<NumberDragger, VerticalIncrement>("VerticalIncrement", VerticalIncrement.UpDecrDownIncr);

    public static readonly DirectProperty<NumberDragger, bool> IsEditingTextBoxProperty = AvaloniaProperty.RegisterDirect<NumberDragger, bool>("IsEditingTextBox", o => o.OnCoerceIsEditingTextBox(o._isEditing), (o, v) => o._isEditing = v);

    // public static readonly AvaloniaPropertyKey IsEditingTextBoxPropertyKey = AvaloniaProperty.RegisterReadOnly("IsEditingTextBox", typeof(bool), typeof(NumberDragger), new PropertyMetadata(BoolBox.False, (d, e) => ((NumberDragger) d).OnIsEditingTextBoxChanged((bool) e.OldValue, (bool) e.NewValue), (d, v) => ((NumberDragger) d).OnCoerceIsEditingTextBox(v)));
    // public static readonly AvaloniaProperty IsEditingTextBoxProperty = IsEditingTextBoxPropertyKey.AvaloniaProperty;
    public static readonly StyledProperty<int?> RoundedPlacesProperty = AvaloniaProperty.Register<NumberDragger, int?>("RoundedPlaces", null);
    public static readonly StyledProperty<int?> PreviewRoundedPlacesProperty = AvaloniaProperty.Register<NumberDragger, int?>("PreviewRoundedPlaces", 4);
    public static readonly StyledProperty<string> PreviewDisplayTextOverrideProperty = AvaloniaProperty.Register<NumberDragger, string>("PreviewDisplayTextOverride", null);
    public static readonly StyledProperty<string> EditingHintProperty = AvaloniaProperty.Register<NumberDragger, string>("EditingHint", null);
    public static readonly StyledProperty<bool> RestoreValueOnCancelProperty = AvaloniaProperty.Register<NumberDragger, bool>("RestoreValueOnCancel", true);
    public static readonly StyledProperty<IChangeMapper> ChangeMapperProperty = AvaloniaProperty.Register<NumberDragger, IChangeMapper>("ChangeMapper", null);
    public static readonly StyledProperty<IValuePreProcessor> ValuePreProcessorProperty = AvaloniaProperty.Register<NumberDragger, IValuePreProcessor>("ValuePreProcessor", null);
    public static readonly StyledProperty<bool> IsDoubleClickToEditProperty = AvaloniaProperty.Register<NumberDragger, bool>("IsDoubleClickToEdit", false);
    public static readonly StyledProperty<bool?> ForcedReadOnlyStateProperty = AvaloniaProperty.Register<NumberDragger, bool?>("ForcedReadOnlyState", null);
    public static readonly StyledProperty<IValueFormatter> PreviewValueFormatterProperty = AvaloniaProperty.Register<NumberDragger, IValueFormatter>("PreviewValueFormatter", null);

    #region Properties

    /// <summary>
    /// Gets or sets the tiny-change value. This is added or subtracted when CTRL + SHIFT is pressed
    /// </summary>
    public double TinyChange {
        get => this.GetValue(TinyChangeProperty);
        set => this.SetValue(TinyChangeProperty, value);
    }

    /// <summary>
    /// Gets or sets the massive change value. This is added or subtracted when CTRL is pressed
    /// </summary>
    public double MassiveChange {
        get => this.GetValue(MassiveChangeProperty);
        set => this.SetValue(MassiveChangeProperty, value);
    }

    public bool IsDragging {
        get => this._isDragging;
        protected set => this.SetAndRaise(IsDraggingProperty, ref this._isDragging, value);
    }

    public bool? CompleteEditOnTextBoxLostFocus {
        get => this.GetValue(CompleteEditOnTextBoxLostFocusProperty);
        set => this.SetValue(CompleteEditOnTextBoxLostFocusProperty, value.BoxNullable());
    }

    public Orientation Orientation {
        get => this.GetValue(OrientationProperty);
        set => this.SetValue(OrientationProperty, value);
    }

    public HorizontalIncrement HorizontalIncrement {
        get => this.GetValue(HorizontalIncrementProperty);
        set => this.SetValue(HorizontalIncrementProperty, value);
    }

    public VerticalIncrement VerticalIncrement {
        get => this.GetValue(VerticalIncrementProperty);
        set => this.SetValue(VerticalIncrementProperty, value);
    }

    public bool IsEditingTextBox {
        get => this._isEditing;
        protected set => this.SetAndRaise(IsEditingTextBoxProperty, ref this._isEditing, value);
    }

    /// <summary>
    /// The number of digits to round the actual value to. Set to null to disable rounding.
    /// <para>
    /// When <see cref="RangeBase.ValueProperty"/> is bound to non floating point, this value should be ignored
    /// </para>
    /// <para>
    /// However when binding to floating point numbers, this value should ideally be 6 or 7. For doubles,
    /// this should be 14 or 15. This is to combat floating point rounding issues, causing the the
    /// </para>
    /// </summary>
    public int? RoundedPlaces {
        get => this.GetValue(RoundedPlacesProperty);
        set => this.SetValue(RoundedPlacesProperty, value);
    }

    /// <summary>
    /// The number of digits to round the preview value to. Set to null to disable rounding.
    /// <para>
    /// When <see cref="RangeBase.ValueProperty"/> is bound to non floating point, this value should be ignored
    /// </para>
    /// <para>
    /// However when binding to floating point numbers, this value should ideally be 6 or 7. For doubles,
    /// this should be 14 or 15. This is to combat floating point rounding issues, causing the the
    /// </para>
    /// </summary>
    public int? PreviewRoundedPlaces {
        get => this.GetValue(PreviewRoundedPlacesProperty);
        set => this.SetValue(PreviewRoundedPlacesProperty, value);
    }

    /// <summary>
    /// Gets or sets a value that is displayed while the value preview is active, instead of displaying the
    /// actual value. A text box will still appear if the control is clicked
    /// <para>
    /// This is only displayed when the value is non-null and not an empty string
    /// </para>
    /// </summary>
    public string PreviewDisplayTextOverride {
        get => this.GetValue(PreviewDisplayTextOverrideProperty);
        set => this.SetValue(PreviewDisplayTextOverrideProperty, value);
    }

    /// <summary>
    /// A piece of text to display overtop of the editor text box when manually editing
    /// the value, and there is no text in the text box
    /// </summary>
    public string EditingHint {
        get => (string) this.GetValue(EditingHintProperty);
        set => this.SetValue(EditingHintProperty, value);
    }

    /// <summary>
    /// Whether or not to restore the value property when the drag is cancelled. Default is true
    /// </summary>
    public bool RestoreValueOnCancel {
        get => this.GetValue(RestoreValueOnCancelProperty);
        set => this.SetValue(RestoreValueOnCancelProperty, value.Box());
    }

    public IChangeMapper ChangeMapper {
        get => (IChangeMapper) this.GetValue(ChangeMapperProperty);
        set => this.SetValue(ChangeMapperProperty, value);
    }

    public IValuePreProcessor ValuePreProcessor {
        get => (IValuePreProcessor) this.GetValue(ValuePreProcessorProperty);
        set => this.SetValue(ValuePreProcessorProperty, value);
    }

    public bool IsDoubleClickToEdit {
        get => this.GetValue(IsDoubleClickToEditProperty);
        set => this.SetValue(IsDoubleClickToEditProperty, value.Box());
    }

    public IValueFormatter PreviewValueFormatter {
        get => (IValueFormatter) this.GetValue(PreviewValueFormatterProperty);
        set => this.SetValue(PreviewValueFormatterProperty, value);
    }

    public bool? ForcedReadOnlyState {
        get => this.GetValue(ForcedReadOnlyStateProperty);
        set => this.SetValue(ForcedReadOnlyStateProperty, value.BoxNullable());
    }

    public bool IsValueReadOnly {
        get {
            if (this.GetValue(ForcedReadOnlyStateProperty) is bool forced)
                return forced;

            return false;
            // TODO: not implemented in avalonia
            // Binding binding;
            // BindingExpression expression = this.GetBindingObservable(ValueProperty);
            // if (expression == null || (binding = expression.ParentBinding) == null)
            //     return false;
            // switch (binding.Mode) {
            //     case BindingMode.OneWay:
            //     case BindingMode.OneTime:
            //         return true;
            //     default: return false;
            // }
        }
    }

    #endregion

    public static readonly RoutedEvent<EditStartEventArgs> EditStartedEvent = RoutedEvent.Register<NumberDragger, EditStartEventArgs>(nameof(EditStarted), RoutingStrategies.Bubble);
    public static readonly RoutedEvent<EditStartEventArgs> EditCompletedEvent = RoutedEvent.Register<NumberDragger, EditStartEventArgs>(nameof(EditCompleted), RoutingStrategies.Bubble);

    [Category("Behavior")]
    public event EditStartEventHandler EditStarted {
        add => this.AddHandler(EditStartedEvent, value);
        remove => this.RemoveHandler(EditStartedEvent, value);
    }

    [Category("Behavior")]
    public event EditCompletedEventHandler EditCompleted {
        add => this.AddHandler(EditCompletedEvent, value);
        remove => this.RemoveHandler(EditCompletedEvent, value);
    }

    private TextBlock? PART_HintTextBlock;
    private TextBlock? PART_TextBlock;
    private TextBox? PART_TextBox;
    private Point? lastClickPoint;
    private Point? lastMouseMove;
    private double? previousValue;
    private bool ignoreMouseMove;
    private bool isUpdatingExternalMouse;
    private bool ignoreLostFocus;
    private bool hasCancelled_ignoreMouseUp;

    public NumberDragger() {
        this.Loaded += (s, e) => {
            this.CoerceValue(IsEditingTextBoxProperty);
            this.UpdateText();
            this.UpdateCursor();
            this.RequeryChangeMapper(this.Value);
        };
    }

    static NumberDragger() {
        // Application.Current.Deactivated += OnApplicationFocusLost;
        ValueProperty.OverrideMetadata<NumberDragger>(new StyledPropertyMetadata<double>(default, BindingMode.Default, (d, e) => ((NumberDragger) d).OnCoerceValue(e)));
        IsDraggingProperty.Changed.AddClassHandler<NumberDragger, bool>((d, e) => d.OnIsDraggingChanged(e.GetOldValue<bool>(), e.GetNewValue<bool>()));
        OrientationProperty.Changed.AddClassHandler<NumberDragger, Orientation>((d, e) => d.OnOrientationChanged(e.GetOldValue<Orientation>(), e.GetNewValue<Orientation>()));
        RoundedPlacesProperty.Changed.AddClassHandler<NumberDragger, int?>((d, e) => d.OnRoundedPlacesChanged(e.GetOldValue<int?>(), e.GetNewValue<int?>()));
        PreviewRoundedPlacesProperty.Changed.AddClassHandler<NumberDragger, int?>((d, e) => d.OnPreviewRoundedPlacesChanged(e.GetOldValue<int?>(), e.GetNewValue<int?>()));
        PreviewDisplayTextOverrideProperty.Changed.AddClassHandler<NumberDragger>((d, e) => d.UpdateText());

        ValueProperty.Changed.AddClassHandler<NumberDragger, double>((o, e) => {
            o.UpdateText();
            o.RequeryChangeMapper(e.GetNewValue<double>());
        });
    }

    // private static void OnApplicationFocusLost(object sender, EventArgs e) {
    //     if (Keyboard.FocusedElement is NumberDragger dragger) {
    //         if (dragger.IsDragging) {
    //             dragger.CancelDrag();
    //         }
    //         else if (dragger.IsEditingTextBox) {
    //             dragger.CancelInputEdit();
    //         }
    //     }
    // }

    private double OnCoerceValue(double value) {
        double src = value;
        double dst = Maths.Clamp(this.GetRoundedValue(src, false, out _), this.Minimum, this.Maximum);
        if (this.ValuePreProcessor is IValuePreProcessor processor) {
            dst = processor.Process(dst, this.Minimum, this.Maximum);
        }

        return Maths.Equals(dst, src, 0.00000000001d) ? dst : value;
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        this.PART_TextBlock = e.NameScope.GetTemplateChild<TextBlock>("PART_TextBlock");
        this.PART_TextBox = e.NameScope.GetTemplateChild<TextBox>("PART_TextBox");
        this.PART_HintTextBlock = e.NameScope.GetTemplateChild<TextBlock>("PART_HintTextBlock");
        this.PART_TextBox.Focusable = true;
        this.PART_TextBox.KeyDown += this.OnTextBoxKeyDown;
        this.PART_TextBox.LostFocus += (s, e) => {
            if (this.IsEditingTextBox && this.CompleteEditOnTextBoxLostFocus is bool complete) {
                if (!complete || !this.TryCompleteEdit()) {
                    this.CancelInputEdit();
                }
            }

            this.IsEditingTextBox = false;
        };

        this.PART_TextBox.TextChanged += (sender, e) => this.UpdateHintVisibility();

        this.CoerceValue(IsEditingTextBoxProperty);
    }

    public double GetRoundedValue(double value, bool isPreview, out int? places) {
        places = this.RoundedPlaces;
        if (places.HasValue) {
            value = Math.Round(value, places.Value);
        }

        if (isPreview) {
            int? preview = this.PreviewRoundedPlaces;
            if (preview.HasValue) {
                value = Math.Round(value, preview.Value);
                places = preview;
            }
        }

        return value;
    }

    protected virtual void OnIsDraggingChanged(bool oldValue, bool newValue) {
    }

    protected virtual void OnOrientationChanged(Orientation oldValue, Orientation newValue) {
        if (this.IsDragging) {
            this.CancelDrag();
        }

        this.IsEditingTextBox = false;
    }

    protected virtual void OnIsEditingTextBoxChanged(bool oldValue, bool newValue) {
        if (newValue && this.IsDragging) {
            this.CancelDrag();
        }

        this.UpdatePreviewVisibilities();
        this.UpdateText();
        if (oldValue != newValue) {
            this.ignoreLostFocus = true;
            try {
                this.PART_TextBox.Focus();
                this.PART_TextBox.SelectAll();
            }
            finally {
                this.ignoreLostFocus = false;
            }
        }

        this.UpdateCursor();
        this.UpdateHintVisibility();
    }

    private bool OnCoerceIsEditingTextBox(bool isEditing) {
        if (this.PART_TextBox == null || this.PART_TextBlock == null) {
            return isEditing;
        }

        this.UpdatePreviewVisibilities();
        return isEditing;
    }

    private void UpdateHintVisibility() {
        if (this.PART_HintTextBlock != null && this.PART_TextBox != null) {
            this.PART_HintTextBlock.IsVisible = string.IsNullOrWhiteSpace(this.PART_TextBox.Text) && this.IsEditingTextBox && !string.IsNullOrEmpty(this.EditingHint);
        }
    }

    private void UpdatePreviewVisibilities() {
        // WONKY CHANGE Visiblility.Hidden to isVisible False
        if (this.IsEditingTextBox) {
            this.PART_TextBox!.IsVisible = true;
            this.PART_TextBlock!.IsVisible = false;
        }
        else {
            this.PART_TextBox!.IsVisible = false;
            this.PART_TextBlock!.IsVisible = true;
        }

        this.PART_TextBox.IsReadOnly = this.IsValueReadOnly;
    }

    public void UpdateCursor() {
        if (this.IsValueReadOnly) {
            if (this.IsEditingTextBox) {
                if (this.PART_TextBlock != null) {
                    this.PART_TextBlock.ClearValue(CursorProperty);
                }
                else {
                    Debug.WriteLine(nameof(this.PART_TextBlock) + " is null?");
                }

                this.ClearValue(CursorProperty);
            }
            else {
                this.Cursor = new Cursor(StandardCursorType.No);
                if (this.PART_TextBlock != null) {
                    this.PART_TextBlock.Cursor = new Cursor(StandardCursorType.No);
                }
                else {
                    Debug.WriteLine(nameof(this.PART_TextBlock) + " is null?");
                }
            }
        }
        else {
            if (this.IsDragging) {
                // hide cursor while dragging
                this.Cursor = new Cursor(StandardCursorType.None);
                if (this.PART_TextBlock != null) {
                    this.PART_TextBlock.ClearValue(CursorProperty);
                }
                else {
                    Debug.WriteLine(nameof(this.PART_TextBlock) + " is null?");
                }
            }
            else {
                if (this.IsEditingTextBox) {
                    if (this.PART_TextBlock != null) {
                        this.PART_TextBlock.ClearValue(CursorProperty);
                    }
                    else {
                        Debug.WriteLine(nameof(this.PART_TextBlock) + " is null?");
                    }

                    this.ClearValue(CursorProperty);
                }
                else {
                    Cursor cursor = this.GetCursorForOrientation();
                    this.Cursor = cursor;
                    if (this.PART_TextBlock != null) {
                        this.PART_TextBlock.Cursor = cursor;
                    }
                    else {
                        Debug.WriteLine(nameof(this.PART_TextBlock) + " is null?");
                    }
                }
            }
        }
    }

    protected virtual void OnRoundedPlacesChanged(int? oldValue, int? newValue) {
        if (newValue != null)
            this.UpdateText();
    }

    protected virtual void OnPreviewRoundedPlacesChanged(int? oldValue, int? newValue) {
        if (newValue != null)
            this.UpdateText();
    }

    private void RequeryChangeMapper(double value) {
        if (this.ChangeMapper is IChangeMapper mapper) {
            mapper.OnValueChanged(value, out double t, out double s, out double l, out double m);
            if (!this.TinyChange.Equals(t))
                this.TinyChange = t;
            if (!this.SmallChange.Equals(s))
                this.SmallChange = s;
            if (!this.LargeChange.Equals(l))
                this.LargeChange = l;
            if (!this.MassiveChange.Equals(m))
                this.MassiveChange = m;
        }
    }

    protected void UpdateText() {
        if (this.PART_TextBox == null && this.PART_TextBlock == null) {
            return;
        }

        if (this.IsEditingTextBox) {
            if (this.PART_TextBlock != null)
                this.PART_TextBlock.Text = "";

            if (this.PART_TextBox == null)
                return;
            // don't use preview for text box; only round to RoundedPlaces, if possible
            double value = this.GetRoundedValue(this.Value, false, out int? places);
            this.PART_TextBox.Text = (places.HasValue ? Math.Round(value, places.Value) : value).ToString();
        }
        else {
            // prevents problems where the text box could be very large due
            // to an un-rounded value, affecting the entire control size
            // 0.300000011920929 for example when it should be 0.3
            if (this.PART_TextBox != null)
                this.PART_TextBox.Text = "";

            if (this.PART_TextBlock == null)
                return;
            string text = this.PreviewDisplayTextOverride;
            if (string.IsNullOrEmpty(text)) {
                if (this.PreviewValueFormatter is IValueFormatter formatter) {
                    text = formatter.ToString(this.Value, this.PreviewRoundedPlaces);
                }
                else {
                    double value = this.GetRoundedValue(this.Value, true, out int? places);
                    text = places.HasValue ? value.ToString("F" + places.Value.ToString()) : value.ToString();
                }
            }

            this.PART_TextBlock.Text = text;
        }
    }

    protected override void OnPointerExited(PointerEventArgs e) {
        base.OnPointerExited(e);
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed && this.IsDragging) {
            this.CompleteDrag();
        }
    }

    private bool? canActivateInputEdit;

    protected override void OnPointerPressed(PointerPressedEventArgs e) {
        if (e.GetCurrentPoint(this).Properties.PointerUpdateKind != PointerUpdateKind.LeftButtonPressed) {
            base.OnPointerPressed(e);
            return;
        }

        if (!this.IsDragging && !this.IsValueReadOnly) {
            e.Handled = true;
            this.Focus();

            this.lastMouseMove = this.lastClickPoint = e.GetPosition(this);
            if (this.IsDoubleClickToEdit && e.ClickCount < 2) {
                this.canActivateInputEdit = false;
            }
            else {
                this.canActivateInputEdit = true;
                this.ignoreMouseMove = true;
                try {
                    this.capturedPointer = e.Pointer;
                    e.Pointer.Capture(this);
                }
                finally {
                    this.ignoreMouseMove = false;
                }

                this.UpdateCursor();
            }
        }

        base.OnPointerPressed(e);
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e) {
        if (e.GetCurrentPoint(this).Properties.PointerUpdateKind != PointerUpdateKind.LeftButtonPressed) {
            base.OnPointerReleased(e);
            return;
        }

        e.Handled = true;
        if (this.IsDragging) {
            this.CompleteDrag();
        }
        else if (this.hasCancelled_ignoreMouseUp) {
            this.hasCancelled_ignoreMouseUp = false;
        }
        else if ((!this.canActivateInputEdit.HasValue || this.canActivateInputEdit.Value) && this.IsPointerOver && !this.IsValueReadOnly) {
            if (ReferenceEquals(e.Pointer.Captured, this)) {
                this.capturedPointer = e.Pointer;
                e.Pointer.Capture(null);
            }

            this.OnBeginInputEdit();
        }

        this.canActivateInputEdit = false;

        base.OnPointerReleased(e);
    }

    protected override void OnPointerMoved(PointerEventArgs e) {
        base.OnPointerMoved(e);
        if (this.ignoreMouseMove) {
            return;
        }

        if (this.isUpdatingExternalMouse) {
            return;
        }

        if (!(this.lastClickPoint is Point lastClick)) {
            return;
        }

        // System.Windows.Forms.Cursor

        PointerPoint pointer = e.GetCurrentPoint(this);
        if (!pointer.Properties.IsLeftButtonPressed) {
            if (this.IsDragging) {
                this.CompleteDrag();
            }

            return;
        }
        else if (!this.IsEnabled || this.IsValueReadOnly) {
            // saves a bit of performance by processing these here
            return;
        }

        if (!(this.lastMouseMove is Point lastPos)) {
            return;
        }

        bool wrap = false;
        Point mpos = e.GetPosition(this);
        if (!this.IsDragging) {
            if (Math.Abs(mpos.X - lastClick.X) < 5d && Math.Abs(mpos.Y - lastClick.Y) < 5d) {
                return;
            }

            this.BeginMouseDrag(e.Pointer);
        }

        if (!this.IsDragging) {
            return;
        }

        if (this.IsEditingTextBox) {
            Debug.WriteLine("IsEditingTextBox and IsDragging were both true");
            this.IsEditingTextBox = false;
        }

        double change;
        Orientation orientation = this.Orientation;
        switch (orientation) {
            case Orientation.Horizontal: {
                change = mpos.X - lastPos.X;
                break;
            }
            case Orientation.Vertical: {
                change = mpos.Y - lastPos.Y;
                break;
            }
            default: {
                throw new Exception("Invalid orientation: " + orientation);
            }
        }

        bool isShiftDown = (e.KeyModifiers & KeyModifiers.Shift) != 0;
        bool isCtrlDown = (e.KeyModifiers & KeyModifiers.Control) != 0;

        if (isShiftDown) {
            if (isCtrlDown) {
                change *= this.TinyChange;
            }
            else {
                change *= this.SmallChange;
            }
        }
        else if (isCtrlDown) {
            change *= this.MassiveChange;
        }
        else {
            change *= this.LargeChange;
        }

        double newValue;
        if ((orientation == Orientation.Horizontal && this.HorizontalIncrement == HorizontalIncrement.LeftDecrRightIncr) ||
            (orientation == Orientation.Vertical && this.VerticalIncrement == VerticalIncrement.UpDecrDownIncr)) {
            newValue = this.Value + change;
        }
        else {
            newValue = this.Value - change;
        }

        double roundedValue = Maths.Clamp(this.GetRoundedValue(newValue, false, out _), this.Minimum, this.Maximum);
        if (Maths.Equals(this.GetRoundedValue(this.Value, false, out _), roundedValue)) {
            return;
        }

        this.Value = roundedValue;
        this.lastMouseMove = this.lastClickPoint;
        this.isUpdatingExternalMouse = true;
        try {
            PixelPoint sp = this.PointToScreen(lastClick);
            CursorUtils.SetCursorPos(sp.X, sp.Y);
        }
        finally {
            this.isUpdatingExternalMouse = false;
        }
    }

    protected override void OnKeyDown(KeyEventArgs e) {
        base.OnKeyDown(e);
        if (!e.Handled) {
            if (this.IsDragging) {
                if (e.Key == Key.Escape) {
                    this.hasCancelled_ignoreMouseUp = true;
                    e.Handled = true;
                    this.CancelInputEdit();
                    if (this.IsDragging) {
                        this.CancelDrag();
                    }

                    this.IsEditingTextBox = false;
                }
            }
            // If the user previously edited another NumberDragger, then once they complete/cancel an edit, WPF
            // auto-focused that number dragger. Then they can press tab to navigate nearby draggers, and they can
            // edit them by just clicking a key. Massive convenience feature, saves having to use the mouse as much
            else if (this.CanEnableAutoEdit(e.Key) && !this.IsValueReadOnly && (this.IsKeyboardFocusWithin || this.IsFocused)) {
                this.OnBeginInputEdit();
            }
        }
    }

    private bool CanEnableAutoEdit(Key k) {
        return k >= Key.D0 && k <= Key.D9 || k == Key.Enter;
    }

    private void OnTextBoxKeyDown(object sender, KeyEventArgs e) {
        if (!e.Handled && !this.IsDragging && this.IsEditingTextBox) {
            if ((e.Key == Key.Enter || e.Key == Key.Escape)) {
                if (e.Key != Key.Enter || !this.TryCompleteEdit()) {
                    this.CancelInputEdit();
                }

                e.Handled = true;
            }
        }
    }

    protected override void OnLostFocus(RoutedEventArgs e) {
        base.OnLostFocus(e);
        if (!this.ignoreLostFocus) {
            if (this.IsDragging) {
                this.CancelDrag();
            }

            this.IsEditingTextBox = false;
        }
    }

    public void OnBeginInputEdit() {
        this.IsEditingTextBox = true;
        this.UpdateCursor();
    }

    public bool TryCompleteEdit() {
        if (this.IsValueReadOnly) {
            return false;
        }

        if (double.TryParse(this.PART_TextBox.Text, out double value)) {
            this.CompleteInputEdit(value);
            return true;
        }

        using (ComplexNumericExpression.ExpressionState state = ComplexNumericExpression.DefaultParser.PushState()) {
            state.SetVariable("value", this.Value);
            try {
                value = state.Expression.Parse(this.PART_TextBox.Text!);
            }
            catch {
                return false;
            }

            this.CompleteInputEdit(value);
            return true;
        }
    }

    public void CompleteInputEdit(double value) {
        this.IsEditingTextBox = false;
        // TODO: figure out "trimmed" out part (due to rounding) and use that to figure out if the value is actually different
        this.Value = value;
    }

    public void CancelInputEdit() {
        this.IsEditingTextBox = false;
    }

    public void BeginMouseDrag(IPointer pointer) {
        this.IsEditingTextBox = false;
        this.previousValue = this.Value;
        this.Focus();
        this.capturedPointer = pointer;
        pointer.Capture(this);
        this.IsDragging = true;
        this.UpdateCursor();

        bool fail = true;
        try {
            this.RaiseEvent(new EditStartEventArgs());
            fail = false;
        }
        finally {
            if (fail) {
                this.CancelDrag();
            }
        }
    }

    public void CompleteDrag() {
        if (this.IsDragging) {
            this.ProcessDragCompletion(false);
            this.previousValue = null;
        }
    }

    public void CancelDrag() {
        if (this.IsDragging) {
            this.ProcessDragCompletion(true);
            if (this.previousValue is double oldVal) {
                this.previousValue = null;
                if (this.RestoreValueOnCancel) {
                    this.Value = oldVal;
                }
            }
        }
    }

    private IPointer? capturedPointer;

    private void ProcessDragCompletion(bool cancelled) {
        if (this.capturedPointer != null && ReferenceEquals(this.capturedPointer.Captured, this))
            this.capturedPointer.Capture(null);

        this.capturedPointer = null;
        this.IsDragging = false;

        this.lastMouseMove = null;
        if (this.lastClickPoint is Point point) {
            this.isUpdatingExternalMouse = true;
            try {
                PixelPoint p = this.PointToScreen(point);
                CursorUtils.SetCursorPos(p.X, p.Y);
            }
            finally {
                this.isUpdatingExternalMouse = false;
            }
        }

        this.lastClickPoint = null;
        this.UpdateCursor();

        this.RaiseEvent(new EditCompletedEventArgs(cancelled));
    }

    private Cursor GetCursorForOrientation() {
        Cursor cursor;
        switch (this.Orientation) {
            case Orientation.Horizontal:
                cursor = new Cursor(StandardCursorType.SizeWestEast);
                break;
            case Orientation.Vertical:
                cursor = new Cursor(StandardCursorType.SizeNorthSouth);
                break;
            default:
                cursor = new Cursor(StandardCursorType.Arrow);
                break;
        }

        return cursor;
    }
}