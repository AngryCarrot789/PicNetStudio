using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using PicNetStudio.Avalonia.Bindings;
using PicNetStudio.Avalonia.Utils;

namespace PicNetStudio.Avalonia.PicNet.Layers.Controls;

public class LayerTreeControl : TemplatedControl {
    public static readonly StyledProperty<Canvas?> CanvasProperty = AvaloniaProperty.Register<LayerTreeControl, Canvas?>("Canvas");
    public static readonly StyledProperty<GridLength> ColumnWidth0Property = AvaloniaProperty.Register<LayerTreeControl, GridLength>("ColumnWidth0", new GridLength(1, GridUnitType.Star));
    public static readonly StyledProperty<GridLength> ColumnWidth2Property = AvaloniaProperty.Register<LayerTreeControl, GridLength>("ColumnWidth2", new GridLength(30));
    public static readonly StyledProperty<GridLength> ColumnWidth4Property = AvaloniaProperty.Register<LayerTreeControl, GridLength>("ColumnWidth4", new GridLength(50));

    public Canvas? Canvas {
        get => this.GetValue(CanvasProperty);
        set => this.SetValue(CanvasProperty, value);
    }
    
    public GridLength ColumnWidth0 { get => this.GetValue(ColumnWidth0Property); set => this.SetValue(ColumnWidth0Property, value); }
    public GridLength ColumnWidth2 { get => this.GetValue(ColumnWidth2Property); set => this.SetValue(ColumnWidth2Property, value); }
    public GridLength ColumnWidth4 { get => this.GetValue(ColumnWidth4Property); set => this.SetValue(ColumnWidth4Property, value); }

    private readonly PropertyBinder<Canvas?> canvasBinder;
    private LayerObjectTreeView? PART_TreeView => this.canvasBinder.TargetControl as LayerObjectTreeView;

    public LayerTreeControl() {
        this.canvasBinder = new PropertyBinder<Canvas?>(this, CanvasProperty, LayerObjectTreeView.CanvasProperty);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        this.canvasBinder.SetTargetControl(e.NameScope.GetTemplateChild<LayerObjectTreeView>("PART_TreeView"));
    }
}