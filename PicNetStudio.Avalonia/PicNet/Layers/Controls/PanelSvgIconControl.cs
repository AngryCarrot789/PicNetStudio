using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using PicNetStudio.Avalonia.PicNet.Toolbars.Controls;
using PicNetStudio.Avalonia.Utils;

namespace PicNetStudio.Avalonia.PicNet.Layers.Controls;

/// <summary>
/// A control that contains a panel named 'PART_Panel' which contains multiple paths
/// </summary>
public class PanelSvgIconControl : TemplatedControl {
    private Panel? PART_Panel;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        this.PART_Panel = e.NameScope.GetTemplateChild<Panel>(nameof(this.PART_Panel));
    }

    protected override Size ArrangeOverride(Size finalSize) {
        return PathHelper.Arrange(this, this.PART_Panel, finalSize, out Size arrange) ? arrange : base.ArrangeOverride(finalSize);
    }
}
