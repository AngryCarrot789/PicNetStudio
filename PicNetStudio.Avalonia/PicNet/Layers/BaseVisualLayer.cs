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

using System.Net.Security;
using PicNetStudio.Avalonia.DataTransfer;
using PicNetStudio.Avalonia.Utils.Accessing;

namespace PicNetStudio.Avalonia.PicNet.Layers;

/// <summary>
/// The base class for standard visual layers, e.g. raster layers, vector layers, etc.
/// </summary>
public abstract class BaseVisualLayer : BaseLayerTreeObject {
    public static readonly DataParameterFloat OpacityParameter = DataParameter.Register(new DataParameterFloat(typeof(BaseVisualLayer), nameof(Opacity), 1.0f, 0.0f, 1.0f, ValueAccessors.Reflective<float>(typeof(BaseVisualLayer), nameof(opacity))));
    public static readonly DataParameterBool IsVisibleParameter = DataParameter.Register(new DataParameterBool(typeof(BaseVisualLayer), nameof(IsVisible), true, ValueAccessors.Reflective<bool>(typeof(BaseVisualLayer), nameof(isVisible))));

    private float opacity = OpacityParameter.DefaultValue;
    private bool isVisible = IsVisibleParameter.DefaultValue;

    public float Opacity {
        get => this.opacity;
        set => DataParameter.SetValueHelper(this, OpacityParameter, ref this.opacity, value);
    }
    
    public bool IsVisible {
        get => this.isVisible;
        set => DataParameter.SetValueHelper(this, IsVisibleParameter, ref this.isVisible, value);
    }

    /// <summary>
    /// Returns true if this layer type handles its own opacity calculations in order for a more
    /// efficient render. Returns false if it should be handled automatically using an offscreen buffer
    /// </summary>
    public bool UsesCustomOpacityCalculation { get; protected set; }
    
    protected BaseVisualLayer() {
    }

    static BaseVisualLayer() {
        DataParameter.AddMultipleHandlers(OnDataParameterChangedToInvalidateVisual, OpacityParameter, IsVisibleParameter);
    }

    protected static void OnDataParameterChangedToInvalidateVisual(DataParameter parameter, ITransferableData owner) {
        ((BaseVisualLayer) owner).Canvas?.RaiseRenderInvalidated();
    }

    /// <summary>
    /// Draws this layer
    /// </summary>
    /// <param name="ctx">Render context</param>
    public abstract void RenderLayer(RenderContext ctx);
}