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

using PicNetStudio.Avalonia.DataTransfer;
using PicNetStudio.Avalonia.Utils.Accessing;
using SkiaSharp;

namespace PicNetStudio.Avalonia.PicNet.Layers;

/// <summary>
/// The base class for standard visual layers, e.g. raster layers, vector layers, etc.
/// </summary>
public abstract class BaseVisualLayer : BaseLayerTreeObject {
    public static readonly DataParameterFloat OpacityParameter = DataParameter.Register(new DataParameterFloat(typeof(BaseVisualLayer), nameof(Opacity), 1.0f, 0.0f, 1.0f, ValueAccessors.Reflective<float>(typeof(BaseVisualLayer), nameof(opacity))));
    public static readonly DataParameterBool IsRenderVisibleParameter = DataParameter.Register(new DataParameterBool(typeof(BaseVisualLayer), nameof(IsVisible), true, ValueAccessors.Reflective<bool>(typeof(BaseVisualLayer), nameof(isVisible))));
    public static readonly DataParameterBool IsExportVisibleParameter = DataParameter.Register(new DataParameterBool(typeof(BaseVisualLayer), nameof(IsExportVisible), true, ValueAccessors.Reflective<bool>(typeof(BaseVisualLayer), nameof(isExportVisible))));
    public static readonly DataParameter<SKBlendMode> BlendModeParameter = DataParameter.Register(new DataParameter<SKBlendMode>(typeof(BaseVisualLayer), nameof(BlendMode), SKBlendMode.SrcOver, ValueAccessors.Reflective<SKBlendMode>(typeof(BaseVisualLayer), nameof(blendMode))));

    private float opacity = OpacityParameter.DefaultValue;
    private bool isVisible = IsRenderVisibleParameter.DefaultValue;
    private bool isExportVisible = IsExportVisibleParameter.DefaultValue;
    private SKBlendMode blendMode = BlendModeParameter.DefaultValue;

    /// <summary>
    /// Gets or sets the opacity of this layer
    /// </summary>
    public float Opacity {
        get => this.opacity;
        set => DataParameter.SetValueHelper(this, OpacityParameter, ref this.opacity, value);
    }

    /// <summary>
    /// Gets or sets if this layer is visible when rendering in the editor
    /// </summary>
    public bool IsVisible {
        get => this.isVisible;
        set => DataParameter.SetValueHelper(this, IsRenderVisibleParameter, ref this.isVisible, value);
    }

    /// <summary>
    /// Gets or sets if this layer is visible when exporting the file
    /// </summary>
    public bool IsExportVisible {
        get => this.isExportVisible;
        set => DataParameter.SetValueHelper(this, IsExportVisibleParameter, ref this.isExportVisible, value);
    }

    /// <summary>
    /// Gets or sets the blending mode used for rendering. This is a blend between the currently rendered composition
    /// </summary>
    public SKBlendMode BlendMode {
        get => this.blendMode;
        set => DataParameter.SetValueHelper(this, BlendModeParameter, ref this.blendMode, value);
    }

    /// <summary>
    /// Returns true if this layer type handles its own opacity calculations in order for a more
    /// efficient render. Returns false if it should be handled automatically using an offscreen buffer
    /// </summary>
    public bool UsesCustomOpacityCalculation { get; protected set; }

    protected BaseVisualLayer() {
    }

    static BaseVisualLayer() {
        DataParameter.AddMultipleHandlers(OnDataParameterChangedToInvalidateVisual, OpacityParameter, IsRenderVisibleParameter, BlendModeParameter);
    }

    protected static void OnDataParameterChangedToInvalidateVisual(DataParameter parameter, ITransferableData owner) {
        ((BaseVisualLayer) owner).Canvas?.RaiseRenderInvalidated();
    }

    /// <summary>
    /// Draws this layer
    /// </summary>
    /// <param name="ctx">Render context</param>
    public abstract void RenderLayer(ref RenderContext ctx);
}