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
using PicNetStudio.Avalonia.DataTransfer;
using PicNetStudio.Avalonia.PicNet.Effects;
using PicNetStudio.Avalonia.Utils.Accessing;
using SkiaSharp;

namespace PicNetStudio.Avalonia.PicNet.Layers.Core;

public delegate void BaseVisualLayerRenderInvalidatedEventHandler(BaseVisualLayer layer);

/// <summary>
/// The base class for standard visual layers, e.g. raster layers, vector layers, etc.
/// </summary>
public abstract class BaseVisualLayer : BaseLayerTreeObject {
    public static readonly DataParameterFloat OpacityParameter = DataParameter.Register(new DataParameterFloat(typeof(BaseVisualLayer), nameof(Opacity), 1.0f, 0.0f, 1.0f, ValueAccessors.Reflective<float>(typeof(BaseVisualLayer), nameof(opacity))));
    public static readonly DataParameterBool IsRenderVisibleParameter = DataParameter.Register(new DataParameterBool(typeof(BaseVisualLayer), nameof(IsVisible), true, ValueAccessors.Reflective<bool>(typeof(BaseVisualLayer), nameof(isVisible))));
    public static readonly DataParameterBool IsExportVisibleParameter = DataParameter.Register(new DataParameterBool(typeof(BaseVisualLayer), nameof(IsExportVisible), true, ValueAccessors.Reflective<bool>(typeof(BaseVisualLayer), nameof(isExportVisible))));
    public static readonly DataParameter<SKBlendMode> BlendModeParameter = DataParameter.Register(new DataParameter<SKBlendMode>(typeof(BaseVisualLayer), nameof(BlendMode), SKBlendMode.SrcOver, ValueAccessors.Reflective<SKBlendMode>(typeof(BaseVisualLayer), nameof(blendMode))));
    
    public static readonly DataParameterPoint PositionParameter = DataParameter.Register(new DataParameterPoint(typeof(BaseVisualLayer), nameof(Position), default, ValueAccessors.Reflective<SKPoint>(typeof(BaseVisualLayer), nameof(position))));
    public static readonly DataParameterPoint ScaleParameter = DataParameter.Register(new DataParameterPoint(typeof(BaseVisualLayer), nameof(Scale), new SKPoint(1.0F, 1.0F), ValueAccessors.Reflective<SKPoint>(typeof(BaseVisualLayer), nameof(scale))));
    public static readonly DataParameterPoint ScaleOriginParameter = DataParameter.Register(new DataParameterPoint(typeof(BaseVisualLayer), nameof(ScaleOrigin), default, ValueAccessors.Reflective<SKPoint>(typeof(BaseVisualLayer), nameof(scaleOrigin))));
    public static readonly DataParameterFloat RotationParameter = DataParameter.Register(new DataParameterFloat(typeof(BaseVisualLayer), nameof(Rotation), default, ValueAccessors.Reflective<float>(typeof(BaseVisualLayer), nameof(rotation))));
    public static readonly DataParameterPoint RotationOriginParameter = DataParameter.Register(new DataParameterPoint(typeof(BaseVisualLayer), nameof(RotationOrigin), default, ValueAccessors.Reflective<SKPoint>(typeof(BaseVisualLayer), nameof(rotationOrigin))));

    private float opacity = OpacityParameter.DefaultValue;
    private bool isVisible = IsRenderVisibleParameter.DefaultValue;
    private bool isExportVisible = IsExportVisibleParameter.DefaultValue;
    private SKBlendMode blendMode = BlendModeParameter.DefaultValue;
    
    private SKPoint position = PositionParameter.DefaultValue;
    private SKPoint scale = ScaleParameter.DefaultValue;
    private SKPoint scaleOrigin = ScaleOriginParameter.DefaultValue;
    private float rotation = RotationParameter.DefaultValue;
    private SKPoint rotationOrigin = RotationOriginParameter.DefaultValue;
    private SKMatrix myTransformationMatrix, myInverseTransformationMatrix;
    private SKMatrix myAbsoluteTransformationMatrix, myAbsoluteInverseTransformationMatrix;
    protected bool isMatrixDirty = true;

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
    
    public SKPoint Position {
        get => this.position;
        set => DataParameter.SetValueHelper(this, PositionParameter, ref this.position, value);
    }
    
    public SKPoint Scale {
        get => this.scale;
        set => DataParameter.SetValueHelper(this, ScaleParameter, ref this.scale, value);
    }
    
    public SKPoint ScaleOrigin {
        get => this.scaleOrigin;
        set => DataParameter.SetValueHelper(this, ScaleOriginParameter, ref this.scaleOrigin, value);
    }
    
    public float Rotation {
        get => this.rotation;
        set => DataParameter.SetValueHelper(this, RotationParameter, ref this.rotation, value);
    }
    
    public SKPoint RotationOrigin {
        get => this.rotationOrigin;
        set => DataParameter.SetValueHelper(this, RotationOriginParameter, ref this.rotationOrigin, value);
    }
    
    /// <summary>
    /// Gets the transformation matrix for the transformation properties in this layer only, not including parent transformations
    /// </summary>
    public SKMatrix TransformationMatrix {
        get {
            if (this.isMatrixDirty)
                this.GenerateMatrices();
            return this.myTransformationMatrix;
        }
    }
    
    /// <summary>
    /// Gets the absolute transformation matrix, which is a concatenation of all of our parents' matrices and our own
    /// </summary>
    public SKMatrix AbsoluteTransformationMatrix {
        get {
            if (this.isMatrixDirty)
                this.GenerateMatrices();
            return this.myAbsoluteTransformationMatrix;
        }
    }

    /// <summary>
    /// Gets the inverse of our transformation matrix
    /// </summary>
    public SKMatrix InverseTransformationMatrix {
        get {
            if (this.isMatrixDirty)
                this.GenerateMatrices();
            return this.myInverseTransformationMatrix;
        }
    }
    
    /// <summary>
    /// Gets the inverse of our absolute transformation matrix. This can be used to,
    /// for example, map a location on the entire canvas to this layer
    /// </summary>
    public SKMatrix AbsoluteInverseTransformationMatrix {
        get {
            if (this.isMatrixDirty)
                this.GenerateMatrices();
            return this.myAbsoluteInverseTransformationMatrix;
        }
    }

    /// <summary>
    /// Returns true if this layer type handles its own opacity calculations in order for a more
    /// efficient render. Returns false if it should be handled automatically using an offscreen buffer
    /// </summary>
    public bool UsesCustomOpacityCalculation { get; protected set; }

    /// <summary>
    /// An event fired when this layer is marked as "dirty" and requires re-drawing
    /// </summary>
    public event BaseVisualLayerRenderInvalidatedEventHandler? RenderInvalidated;
    
    protected BaseVisualLayer() {
    }

    static BaseVisualLayer() {
        // TODO: Maybe we can optimise with invalidation and re-paint events?
        // Maybe i'm not thinking about it right but we make it so the cached
        // visual never has the composition layer opacity applied; we use it
        // when drawing the cache into a surface. Maybe that's overall slower
        // than re-drawing the composition layer when its opacity changes?
        // Not sure...
        SetParameterAffectsRender(OpacityParameter, IsRenderVisibleParameter, BlendModeParameter);
        
        DataParameter.AddMultipleHandlers((p, o) => ((BaseVisualLayer) o.TransferableData.Owner).InvalidateTransformationMatrix(), PositionParameter, ScaleParameter, ScaleOriginParameter, RotationParameter, RotationOriginParameter);
    }

    /// <summary>
    /// Adds a parameter value changed handler for each of the given parameters
    /// that will invalidate the render state of the current visual layer
    /// <remarks>Must be called from a static constructor, as it adds a static event handler</remarks>
    /// </summary>
    /// <param name="parameters">The parameters that, when changed, invalidate the visual state</param>
    protected static void SetParameterAffectsRender(params DataParameter[] parameters) {
        foreach (DataParameter parameter in parameters) {
            if (!parameter.OwnerType.IsAssignableTo(typeof(BaseVisualLayer)))
                throw new ArgumentException(parameter + "'s owner is not an instance of visual layer");
        }
        
        DataParameter.AddMultipleHandlers(OnDataParameterChangedToInvalidateVisual, parameters);
    }
    
    private void GenerateMatrices() {
        this.myTransformationMatrix = MatrixUtils.CreateTransformationMatrix(this.Position, this.Scale, this.Rotation, this.ScaleOrigin, this.RotationOrigin);
        this.myInverseTransformationMatrix = MatrixUtils.CreateInverseTransformationMatrix(this.Position, this.Scale, this.Rotation, this.ScaleOrigin, this.RotationOrigin);
        this.myAbsoluteTransformationMatrix = this.ParentLayer == null ? this.myTransformationMatrix : this.ParentLayer.AbsoluteTransformationMatrix.PreConcat(this.myTransformationMatrix);
        this.myAbsoluteInverseTransformationMatrix = this.ParentLayer == null ? this.myInverseTransformationMatrix : this.myInverseTransformationMatrix.PreConcat(this.ParentLayer.AbsoluteInverseTransformationMatrix);
        this.isMatrixDirty = false;
    }

    protected override void OnEffectAdded(int index, BaseLayerEffect effect) {
        base.OnEffectAdded(index, effect);
        this.InvalidateVisual();
    }

    protected override void OnEffectRemoved(int index, BaseLayerEffect effect) {
        base.OnEffectRemoved(index, effect);
        this.InvalidateVisual();
    }
    
    /// <summary>
    /// Marks this layer as having an invalid rendered state, meaning that any pre-rendered frame using this layer must be re-rendered
    /// </summary>
    public virtual void InvalidateVisual() {
        this.RenderInvalidated?.Invoke(this);
        this.ParentLayer?.InvalidateVisual();
    }

    protected static void OnDataParameterChangedToInvalidateVisual(DataParameter parameter, ITransferableData owner) {
        ((BaseVisualLayer) owner).InvalidateVisual();
    }

    /// <summary>
    /// Draws this layer
    /// </summary>
    /// <param name="ctx">Render context</param>
    public abstract void RenderLayer(ref RenderContext ctx);

    protected virtual void InvalidateTransformationMatrix() {
        this.isMatrixDirty = true;
        this.InvalidateVisual();
    }

    internal static void InternalInvalidateTransformationMatrixFromParent(BaseVisualLayer? layer) {
        if (layer != null) {
            layer.isMatrixDirty = true;
        }
    }
}