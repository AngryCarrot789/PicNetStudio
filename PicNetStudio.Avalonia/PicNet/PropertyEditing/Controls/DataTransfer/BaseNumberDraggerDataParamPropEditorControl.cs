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

using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using PicNetStudio.Avalonia.Bindings;
using PicNetStudio.Avalonia.PicNet.Controls.Dragger;
using PicNetStudio.Avalonia.PicNet.PropertyEditing.DataTransfer;
using PicNetStudio.Avalonia.Utils;

namespace PicNetStudio.Avalonia.PicNet.PropertyEditing.Controls.DataTransfer;

public abstract class BaseNumberDraggerDataParamPropEditorControl : BaseDataParameterPropertyEditorControl {
    internal static readonly IImmutableBrush MultipleValuesBrush;
    
    public new DataParameterFormattableNumberPropertyEditorSlot? SlotModel => (DataParameterFormattableNumberPropertyEditorSlot?) base.SlotControl?.Model;
    
    protected NumberDragger dragger;
    protected Button resetButton;
    private readonly AutoUpdateAndEventPropertyBinder<DataParameterFormattableNumberPropertyEditorSlot> valueFormatterBinder;

    protected BaseNumberDraggerDataParamPropEditorControl() {
        this.valueFormatterBinder = new AutoUpdateAndEventPropertyBinder<DataParameterFormattableNumberPropertyEditorSlot>(NumberDragger.ValueFormatterProperty, nameof(DataParameterFormattableNumberPropertyEditorSlot.ValueFormatterChanged), (x) => ((NumberDragger) x.Control).ValueFormatter = x.Model.ValueFormatter, null);
    }

    static BaseNumberDraggerDataParamPropEditorControl() {
        MultipleValuesBrush = new ImmutableSolidColorBrush(Brushes.OrangeRed.Color, 0.7);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        this.dragger = e.NameScope.GetTemplateChild<NumberDragger>("PART_Dragger");
        this.dragger.ValueChanged += (sender, args) => this.OnControlValueChanged();
        this.resetButton = e.NameScope.GetTemplateChild<Button>("PART_ResetValue");
        this.resetButton.Click += OnResetClick;
        this.valueFormatterBinder.AttachControl(this.dragger);
        this.UpdateDraggerMultiValueState();
    }

    private void OnResetClick(object? sender, RoutedEventArgs e) {
        if (this.IsConnected) {
            this.ResetValue();
        }
    }

    protected virtual void ResetValue() {
        
    }

    private void UpdateDraggerMultiValueState() {
        if (!this.IsConnected) {
            return;
        }

        UpdateNumberDragger(this.dragger, this.SlotModel!.HasMultipleValues, this.SlotModel!.HasProcessedMultipleValuesSinceSetup);
    }

    public static void UpdateNumberDragger(NumberDragger dragger, bool hasMultipleValues, bool hasUsedAdditionSinceSetup) {
        // TODO: really need to make a derived NumberDragger specifically for this case,
        // because at the moment, the Value is set to 0 when multiple values are present.
        // It works but meh...

        // Not using hasUsedAdditionSinceSetup because for now, keep
        // override on because otherwise it sticks with the mid-way
        // between the dragger's min and max, which is confusing.
        // Definitely need a derived NumberDragger but it's 11:25 at night :'(
        if (hasMultipleValues /* && !hasUsedAdditionSinceSetup */) {
            dragger.TextPreviewOverride = "<<Multiple Values>>";
        }
        else {
            dragger.TextPreviewOverride = null;
        }
        
        if (hasMultipleValues) {
            dragger.SetCurrentValue(BackgroundProperty, MultipleValuesBrush);
            dragger.SetCurrentValue(ToolTip.TipProperty, "This dragger currently has multiple values present. Modifying this value will change the underlying value for all selected objects");
        }
        else {
            dragger.ClearValue(BackgroundProperty);
            dragger.ClearValue(ToolTip.TipProperty);
        }
    }
    
    protected override void OnCanEditValueChanged(bool canEdit) {
        this.dragger.IsEnabled = canEdit;
    }

    protected override void OnConnected() {
        this.valueFormatterBinder.AttachModel(this.SlotModel!);
        base.OnConnected();
        
        this.SlotModel!.HasMultipleValuesChanged += this.OnHasMultipleValuesChanged;
        this.SlotModel!.HasProcessedMultipleValuesChanged += this.OnHasProcessedMultipleValuesChanged;
        this.UpdateDraggerMultiValueState();
    }

    protected override void OnDisconnected() {
        this.valueFormatterBinder.DetachModel();
        base.OnDisconnected();
        
        this.SlotModel!.HasMultipleValuesChanged -= this.OnHasMultipleValuesChanged;
        this.SlotModel!.HasProcessedMultipleValuesChanged -= this.OnHasProcessedMultipleValuesChanged;
    }
    
    private void OnHasMultipleValuesChanged(DataParameterPropertyEditorSlot sender) {
        this.UpdateDraggerMultiValueState();
    }
    
    private void OnHasProcessedMultipleValuesChanged(DataParameterPropertyEditorSlot sender) {
        this.UpdateDraggerMultiValueState();
    }
}