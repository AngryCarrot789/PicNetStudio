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

using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using PicNetStudio.Avalonia.PicNet.Tools;
using PicNetStudio.Avalonia.PicNet.Tools.Core;
using PicNetStudio.Avalonia.Utils;

namespace PicNetStudio.Avalonia.PicNet.Toolbars.Controls;

/// <summary>
/// The base control for content placed inside a <see cref="ToolBarItemControl"/>
/// </summary>
public class ToolBarItemControlContent : TemplatedControl {
    public static readonly ModelControlRegistry<BaseToolBarItem, ToolBarItemControlContent> Registry;
    public static readonly ModelControlRegistry<BaseCanvasTool, ToolBarItemControlContentSingleTool> SingleToolSubRegistry;

    public ToolBarItemControl ToolBarItem { get; private set; }

    protected ToolBarItemControlContent() {
    }

    static ToolBarItemControlContent() {
        Registry = new ModelControlRegistry<BaseToolBarItem, ToolBarItemControlContent>();
        SingleToolSubRegistry = new ModelControlRegistry<BaseCanvasTool, ToolBarItemControlContentSingleTool>();

        // This system is really confusing but it seems to work.
        // It's what happens when you explicitly avoid using MVVM :---)

        // Register top level toolbar items
        Registry.RegisterType<SingleToolBarItem>((x) => SingleToolSubRegistry.NewInstance(x.Tool));

        // Register single-tool specific tools
        SingleToolSubRegistry.RegisterType<PointerTool>(() => new ToolBarItemControlContent_PointerTool());
        SingleToolSubRegistry.RegisterType<BrushTool>(() => new ToolBarItemControlContent_BrushTool());
        SingleToolSubRegistry.RegisterType<PencilTool>(() => new ToolBarItemControlContent_PencilTool());
        SingleToolSubRegistry.RegisterType<FloodFillTool>(() => new ToolBarItemControlContent_FloodFillTool());
        SingleToolSubRegistry.RegisterType<SelectRegionTool>(() => new ToolBarItemControlContent_SelectRegionTool());
    }

    public void Connect(ToolBarItemControl item) {
        this.ToolBarItem = item ?? throw new ArgumentNullException(nameof(item));
        this.OnConnected();
    }

    public void Disconnect() {
        this.OnDisconnected();
        this.ToolBarItem = null;
    }

    protected virtual void OnConnected() {
    }

    protected virtual void OnDisconnected() {
    }

    protected override Size MeasureOverride(Size availableSize) {
        base.MeasureOverride(availableSize);
        return default;
    }
}

/// <summary>
/// The base class for a toolbar list box item that activates a single tool
/// </summary>
public abstract class ToolBarItemControlContentSingleTool : ToolBarItemControlContent;

public class ToolBarItemControlContent_PointerTool : ToolBarItemControlContentSingleTool {
    private Path? PART_Path;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        this.PART_Path = e.NameScope.GetTemplateChild<Path>(nameof(this.PART_Path));
    }

    protected override Size ArrangeOverride(Size finalSize) {
        if (this.PART_Path == null)
            return base.ArrangeOverride(finalSize);

        Size size = finalSize.Deflate(this.Padding);
        double sX = size.Width / this.PART_Path.Width, sY = size.Height / this.PART_Path.Height;
        this.PART_Path!.RenderTransform = new ScaleTransform(Math.Min(sX, sY), Math.Min(sX, sY));
        this.PART_Path.Arrange(new Rect(0, 0, finalSize.Width, finalSize.Height).CenterRect(new Rect(0, 0, size.Width, size.Height)));
        return finalSize;
    }
}

public class ToolBarItemControlContent_BrushTool : ToolBarItemControlContentSingleTool {
    private Panel? PART_Panel;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        this.PART_Panel = e.NameScope.GetTemplateChild<Panel>(nameof(this.PART_Panel));
    }

    protected override Size ArrangeOverride(Size finalSize) {
        if (this.PART_Panel == null)
            return base.ArrangeOverride(finalSize);

        Size size = finalSize.Deflate(this.Padding);
        this.PART_Panel!.RenderTransform = new ScaleTransform(size.Width / this.PART_Panel.Width, size.Height / this.PART_Panel.Height);
        this.PART_Panel.Arrange(new Rect(0, 0, finalSize.Width, finalSize.Height).CenterRect(new Rect(0, 0, size.Width, size.Height)));
        return finalSize;
    }
}

public class ToolBarItemControlContent_PencilTool : ToolBarItemControlContentSingleTool {
    private Path? PART_Path;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        this.PART_Path = e.NameScope.GetTemplateChild<Path>(nameof(this.PART_Path));
    }

    protected override Size ArrangeOverride(Size finalSize) {
        if (this.PART_Path == null)
            return base.ArrangeOverride(finalSize);

        Size size = finalSize.Deflate(this.Padding);
        this.PART_Path!.RenderTransform = new ScaleTransform(size.Width / this.PART_Path.Width, size.Height / this.PART_Path.Height);
        this.PART_Path.Arrange(new Rect(0, 0, finalSize.Width, finalSize.Height).CenterRect(new Rect(0, 0, size.Width, size.Height)));
        return finalSize;
    }
}

public class ToolBarItemControlContent_FloodFillTool : ToolBarItemControlContentSingleTool {
    private Panel? PART_Panel;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        this.PART_Panel = e.NameScope.GetTemplateChild<Panel>(nameof(this.PART_Panel));
    }

    protected override Size ArrangeOverride(Size finalSize) {
        if (this.PART_Panel == null)
            return base.ArrangeOverride(finalSize);

        Size size = finalSize.Deflate(this.Padding);
        double sX = size.Width / this.PART_Panel.Width, sY = size.Height / this.PART_Panel.Height;
        this.PART_Panel!.RenderTransform = new ScaleTransform(Math.Min(sX, sY), Math.Min(sX, sY));
        this.PART_Panel.Arrange(new Rect(0, 0, finalSize.Width, finalSize.Height).CenterRect(new Rect(0, 0, size.Width, size.Height)));
        return finalSize;
    }
}

public class ToolBarItemControlContent_SelectRegionTool : ToolBarItemControlContentSingleTool {
    public static readonly StyledProperty<double> DashOffsetProperty = AvaloniaProperty.Register<ToolBarItemControlContent_SelectRegionTool, double>("DashOffset");

    public double DashOffset {
        get => this.GetValue(DashOffsetProperty);
        set => this.SetValue(DashOffsetProperty, value);
    }

    static ToolBarItemControlContent_SelectRegionTool() {
        AffectsRender<ToolBarItemControlContent_SelectRegionTool>(DashOffsetProperty);
    }
    
    private Pen? outlinePen1;

    public override void Render(DrawingContext context) {
        base.Render(context);
        Rect myRect = this.Bounds;
        if (this.BorderBrush is IBrush border) {
            double mW = myRect.Width;
            double mH = myRect.Height;
            double tW = 16, tH = 16;
            Rect rect = new Rect((mW / 2.0) - (tW / 2.0), (mH / 2.0) - (tH / 2.0), tW, tH);
            
            this.outlinePen1 ??= new Pen(border, 2.0, new ImmutableDashStyle(new double[] { 2, 2 }, this.DashOffset));
            // context.DrawRectangle(null, this.outlinePen1, new Rect(myRect.X + (thickness / 2.0), myRect.Y + (thickness / 2.0), myRect.Width - thickness, myRect.Height - thickness));
            context.DrawRectangle(null, this.outlinePen1, rect);
        }
    }
}