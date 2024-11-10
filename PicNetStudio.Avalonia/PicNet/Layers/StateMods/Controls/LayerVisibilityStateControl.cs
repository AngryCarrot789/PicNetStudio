using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using PicNetStudio.Avalonia.Bindings;
using PicNetStudio.Avalonia.Utils;

namespace PicNetStudio.Avalonia.PicNet.Layers.StateMods.Controls;

/// <summary>
/// A layer state modifier control that controls the visibility
/// </summary>
public class LayerVisibilityStateControl : BaseLayerStateModifierControl {
    private ToggleButton PART_ToggleRender;
    private ToggleButton PART_ToggleExport;

    private readonly IBinder<BaseVisualLayer> renderVisibilityBinder = 
        new AutoUpdateDataParameterPropertyBinder<BaseVisualLayer>(
            BaseVisualLayer.IsRenderVisibleParameter, ToggleButton.IsCheckedProperty,
            obj => ((ToggleButton) obj.Control).IsChecked = obj.Model.IsVisible, 
            obj => obj.Model.IsVisible = ((ToggleButton) obj.Control).IsChecked == true);
    
    private readonly IBinder<BaseVisualLayer> exportVisibilityBinder = 
        new AutoUpdateDataParameterPropertyBinder<BaseVisualLayer>(
            BaseVisualLayer.IsExportVisibleParameter, ToggleButton.IsCheckedProperty,
            obj => ((ToggleButton) obj.Control).IsChecked = obj.Model.IsExportVisible, 
            obj => obj.Model.IsExportVisible = ((ToggleButton) obj.Control).IsChecked == true);

    public new BaseVisualLayer Layer => (BaseVisualLayer) base.Layer;
    
    public LayerVisibilityStateControl() {
    }

    protected override void OnConnected() {
        base.OnConnected();
        this.renderVisibilityBinder.AttachModel(this.Layer);
        this.exportVisibilityBinder.AttachModel(this.Layer);
    }

    protected override void OnDisconnected() {
        base.OnDisconnected();
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        this.PART_ToggleRender = e.NameScope.GetTemplateChild<ToggleButton>(nameof(this.PART_ToggleRender));
        this.PART_ToggleExport = e.NameScope.GetTemplateChild<ToggleButton>(nameof(this.PART_ToggleExport));
        
        this.renderVisibilityBinder.AttachControl(this.PART_ToggleRender);
        this.exportVisibilityBinder.AttachControl(this.PART_ToggleExport);
    }
}