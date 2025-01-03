﻿// 
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

using PicNetStudio.DataTransfer;
using PicNetStudio.PicNet.Effects;
using PicNetStudio.Utils.Accessing;
using SkiaSharp;

namespace PicNetStudio.PicNet.Layers.Core;

public delegate void BaseVisualLayerRenderInvalidatedEventHandler(BaseVisualLayer layer);

/// <summary>
/// The base class for standard visual layers, e.g. raster layers, vector layers, etc.
/// </summary>
public abstract class BaseVisualLayer : BaseLayerTreeObject {
    public static readonly DataParameterFloat OpacityParameter = DataParameter.Register(new DataParameterFloat(typeof(BaseVisualLayer), nameof(Opacity), 1.0f, 0.0f, 1.0f, ValueAccessors.Reflective<float>(typeof(BaseVisualLayer), nameof(opacity))));
    public static readonly DataParameterBool IsRenderVisibleParameter = DataParameter.Register(new DataParameterBool(typeof(BaseVisualLayer), nameof(IsVisible), true, ValueAccessors.Reflective<bool>(typeof(BaseVisualLayer), nameof(isVisible))));
    public static readonly DataParameterBool IsExportVisibleParameter = DataParameter.Register(new DataParameterBool(typeof(BaseVisualLayer), nameof(IsExportVisible), true, ValueAccessors.Reflective<bool>(typeof(BaseVisualLayer), nameof(isExportVisible))));
    public static readonly DataParameterBool IsSoloParameter = DataParameter.Register(new DataParameterBool(typeof(BaseVisualLayer), nameof(IsSolo), false, ValueAccessors.Reflective<bool>(typeof(BaseVisualLayer), nameof(isSolo))));
    public static readonly DataParameter<SKBlendMode> BlendModeParameter = DataParameter.Register(new DataParameter<SKBlendMode>(typeof(BaseVisualLayer), nameof(BlendMode), SKBlendMode.SrcOver, ValueAccessors.Reflective<SKBlendMode>(typeof(BaseVisualLayer), nameof(blendMode))));

    public static readonly DataParameterPoint PositionParameter = DataParameter.Register(new DataParameterPoint(typeof(BaseVisualLayer), nameof(Position), default, ValueAccessors.Reflective<SKPoint>(typeof(BaseVisualLayer), nameof(position))));
    public static readonly DataParameterPoint ScaleParameter = DataParameter.Register(new DataParameterPoint(typeof(BaseVisualLayer), nameof(Scale), new SKPoint(1.0F, 1.0F), ValueAccessors.Reflective<SKPoint>(typeof(BaseVisualLayer), nameof(scale))));
    public static readonly DataParameterPoint ScaleOriginParameter = DataParameter.Register(new DataParameterPoint(typeof(BaseVisualLayer), nameof(ScaleOrigin), default, ValueAccessors.Reflective<SKPoint>(typeof(BaseVisualLayer), nameof(scaleOrigin))));
    public static readonly DataParameterFloat RotationParameter = DataParameter.Register(new DataParameterFloat(typeof(BaseVisualLayer), nameof(Rotation), default, ValueAccessors.Reflective<float>(typeof(BaseVisualLayer), nameof(rotation))));
    public static readonly DataParameterPoint RotationOriginParameter = DataParameter.Register(new DataParameterPoint(typeof(BaseVisualLayer), nameof(RotationOrigin), default, ValueAccessors.Reflective<SKPoint>(typeof(BaseVisualLayer), nameof(rotationOrigin))));
    public static readonly DataParameterBool IsScaleOriginAutomaticParameter = DataParameter.Register(new DataParameterBool(typeof(BaseVisualLayer), nameof(IsScaleOriginAutomatic), true, ValueAccessors.Reflective<bool>(typeof(BaseVisualLayer), nameof(isScaleOriginAutomatic))));
    public static readonly DataParameterBool IsRotationOriginAutomaticParameter = DataParameter.Register(new DataParameterBool(typeof(BaseVisualLayer), nameof(IsRotationOriginAutomatic), true, ValueAccessors.Reflective<bool>(typeof(BaseVisualLayer), nameof(isRotationOriginAutomatic))));

    private float opacity;
    private bool isVisible;
    private bool isExportVisible;
    private bool isSolo;
    private SKBlendMode blendMode;
    private SKPoint position;
    private SKPoint scale;
    private SKPoint scaleOrigin;
    private float rotation;
    private SKPoint rotationOrigin;
    private bool isScaleOriginAutomatic;
    private bool isRotationOriginAutomatic;
    private SKMatrix myTransformationMatrix, myInverseTransformationMatrix;
    private SKMatrix myAbsoluteTransformationMatrix, myAbsoluteInverseTransformationMatrix;
    protected bool isMatrixDirty = true; 
    internal bool isRendering = false;

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
    /// Gets or sets if this layer is the only layer being rendered in the PREVIEW
    /// </summary>
    public bool IsSolo {
        get => this.isSolo;
        set => DataParameter.SetValueHelper(this, IsSoloParameter, ref this.isSolo, value);
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
    
    public bool IsScaleOriginAutomatic {
        get => this.isScaleOriginAutomatic;
        set => DataParameter.SetValueHelper(this, IsScaleOriginAutomaticParameter, ref this.isScaleOriginAutomatic, value);
    }

    public bool IsRotationOriginAutomatic {
        get => this.isRotationOriginAutomatic;
        set => DataParameter.SetValueHelper(this, IsRotationOriginAutomaticParameter, ref this.isRotationOriginAutomatic, value);
    }

    /// <summary>
    /// Gets the transformation matrix for the transformation properties in this layer
    /// only, not including parent transformations. This is our local-to-world matrix
    /// </summary>
    public SKMatrix TransformationMatrix {
        get {
            if (this.isMatrixDirty)
                this.GenerateMatrices();
            return this.myTransformationMatrix;
        }
    }

    /// <summary>
    /// Gets the absolute transformation matrix, which is a concatenation of all of our
    /// parents' matrices and our own. This is our local-to-world matrix
    /// </summary>
    public SKMatrix AbsoluteTransformationMatrix {
        get {
            if (this.isMatrixDirty)
                this.GenerateMatrices();
            return this.myAbsoluteTransformationMatrix;
        }
    }

    /// <summary>
    /// Gets the inverse of our transformation matrix. This is our world-to-local matrix
    /// </summary>
    public SKMatrix InverseTransformationMatrix {
        get {
            if (this.isMatrixDirty)
                this.GenerateMatrices();
            return this.myInverseTransformationMatrix;
        }
    }

    /// <summary>
    /// Gets the inverse of our absolute transformation matrix. This can be used to, for example,
    /// map a location on the entire canvas to this layer. This is our world-to-local matrix
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
        this.opacity = OpacityParameter.GetDefaultValue(this);
        this.isVisible = IsRenderVisibleParameter.GetDefaultValue(this);
        this.isExportVisible = IsExportVisibleParameter.GetDefaultValue(this);
        this.isSolo = IsSoloParameter.GetDefaultValue(this);
        this.blendMode = BlendModeParameter.GetDefaultValue(this);
        this.position = PositionParameter.GetDefaultValue(this);
        this.scale = ScaleParameter.GetDefaultValue(this);
        this.scaleOrigin = ScaleOriginParameter.GetDefaultValue(this);
        this.rotation = RotationParameter.GetDefaultValue(this);
        this.rotationOrigin = RotationOriginParameter.GetDefaultValue(this);
        this.isScaleOriginAutomatic = IsScaleOriginAutomaticParameter.GetDefaultValue(this);
        this.isRotationOriginAutomatic = IsRotationOriginAutomaticParameter.GetDefaultValue(this);
    }

    static BaseVisualLayer() {
        SerialisationRegistry.Register<BaseVisualLayer>(0, (layer, data, ctx) => {
            ctx.DeserialiseBaseType(data);
            layer.opacity = data.GetFloat("Opacity");
            layer.isVisible = data.GetBool("IsVisible");
            layer.isExportVisible = data.GetBool("IsExportVisible");
            layer.blendMode = data.GetStruct<SKBlendMode>("BlendMode");
            layer.position = data.GetStruct<SKPoint>("Position");
            layer.scale = data.GetStruct<SKPoint>("Scale");
            layer.scaleOrigin = data.GetStruct<SKPoint>("ScaleOrigin");
            layer.rotation = data.GetFloat("Rotation");
            layer.rotationOrigin = data.GetStruct<SKPoint>("RotationOrigin");
            layer.isScaleOriginAutomatic = data.GetBool("IsScaleOriginAutomatic");
            layer.isRotationOriginAutomatic = data.GetBool("IsRotationOriginAutomatic");
            layer.isMatrixDirty = true;
        }, (layer, data, ctx) => {
            ctx.SerialiseBaseType(data);
            data.SetFloat("Opacity", layer.opacity);
            data.SetBool("IsVisible", layer.isVisible);
            data.SetBool("IsExportVisible", layer.isExportVisible);
            data.SetStruct("BlendMode", layer.blendMode);
            data.SetStruct("Position", layer.position);
            data.SetStruct("Scale", layer.scale);
            data.SetStruct("ScaleOrigin", layer.scaleOrigin);
            data.SetFloat("Rotation", layer.rotation);
            data.SetStruct("RotationOrigin", layer.rotationOrigin);
            data.SetBool("IsScaleOriginAutomatic", layer.isScaleOriginAutomatic);
            data.SetBool("IsRotationOriginAutomatic", layer.isRotationOriginAutomatic);
        });

        // TODO: Maybe we can optimise with invalidation and re-paint events?
        // Maybe i'm not thinking about it right but we make it so the cached
        // visual never has the composition layer opacity applied; we use it
        // when drawing the cache into a surface. Maybe that's overall slower
        // than re-drawing the composition layer when its opacity changes?
        // Not sure...
        SetParameterAffectsRender(OpacityParameter, IsRenderVisibleParameter, BlendModeParameter);
        
        // Add handlers to properties that affect the transformation matrix
        DataParameter.AddMultipleHandlers(OnMatrixInvalidatingPropertyChanged, PositionParameter, ScaleParameter, ScaleOriginParameter, RotationParameter, RotationOriginParameter);
        
        // Add internal handler for solo system
        IsSoloParameter.PriorityValueChanged += OnIsSoloValueChanged;
        IsScaleOriginAutomaticParameter.PriorityValueChanged += OnIsScaleOriginAutomaticParameterValueChanged;
        IsRotationOriginAutomaticParameter.PriorityValueChanged += OnIsRotationOriginAutomaticParameterValueChanged;
    }

    private static void OnIsScaleOriginAutomaticParameterValueChanged(DataParameter parameter, ITransferableData owner) {
        ((BaseVisualLayer) owner).UpdateAutomaticScaleOrigin();
    }
    
    private static void OnIsRotationOriginAutomaticParameterValueChanged(DataParameter parameter, ITransferableData owner) {
        ((BaseVisualLayer) owner).UpdateAutomaticRotationOrigin();
    }

    protected override void OnAttachedToCanvas(BaseLayerTreeObject origin) {
        base.OnAttachedToCanvas(origin);
        this.UpdateAutomaticRotationOrigin();
        this.UpdateAutomaticScaleOrigin();
    }

    protected void UpdateAutomaticScaleOrigin() {
        if (this.IsScaleOriginAutomatic) {
            SKSize size = this.GetSizeForAutomaticOrigins();
            this.ScaleOrigin = new SKPoint(size.Width / 2, size.Height / 2);
        }
    }
    
    protected void UpdateAutomaticRotationOrigin() {
        if (this.IsRotationOriginAutomatic) {
            SKSize size = this.GetSizeForAutomaticOrigins();
            this.RotationOrigin = new SKPoint(size.Width / 2, size.Height / 2);
        }
    }

    public abstract SKSize GetSizeForAutomaticOrigins();

    protected virtual void OnSizeForAutomaticOriginsChanged() {
        this.UpdateAutomaticScaleOrigin();
        this.UpdateAutomaticRotationOrigin();
    }

    private static void OnMatrixInvalidatingPropertyChanged(DataParameter parameter, ITransferableData owner) {
        ((BaseVisualLayer) owner).InvalidateTransformationMatrix();
    }

    // Update for the entire 
    private static void OnIsSoloValueChanged(DataParameter parameter, ITransferableData owner) {
        BaseVisualLayer layer = (BaseVisualLayer) owner;

        // Something set the property while not in a canvas tree. This is basically pointless
        // since IsSolo is set to false when adding/removing a layer, so we can ignore it
        if (layer.Canvas == null) {
            return;
        }

        // The property was set to false so try and clear the canvas' current solo layer
        if (!layer.IsSolo) {
            // Check that the layer whose IsSolo is now false is the actual solo layer.
            // If it isn't then I don't know what happened but it should be fine... hopefully
            if (ReferenceEquals(layer, layer.Canvas.SoloLayer))
                Canvas.InternalSetSoloLayer(layer.Canvas, null); // Clear solo layer
        }
        else {
            // IsSolo set to true so let the canvas deal with the old/new solo layer states
            Canvas.InternalSetSoloLayer(layer.Canvas, layer);
        }
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
    /// Prepare this layer for rendering. This should do things like generate caches that may affect the transformation matrix
    /// </summary>
    /// <param name="ctx">The rendering context</param>
    /// <returns>True when this layer can be rendered. False to stop it from rendering, e.g. objects missing or invalid</returns>
    public virtual bool OnPrepareRenderLayer(ref RenderContext ctx) {
        return true;
    }
    
    /// <summary>
    /// Draws this layer. This should not be called directly, but instead, use <see cref="LayerRenderer"/> since the
    /// layer may not be in a valid state to be drawn unless <see cref="OnPrepareRenderLayer"/> is called, and this
    /// method also may not take into account opacity
    /// </summary>
    /// <param name="ctx">Render context</param>
    public abstract void RenderLayer(ref RenderContext ctx);

    protected virtual void InvalidateTransformationMatrix() {
        if (this.isRendering)
            throw new InvalidOperationException("Attempt to invalidate transformation matrix during render. This is not allowed");
        this.isMatrixDirty = true;
        this.InvalidateVisual();
    }

    internal static void InternalInvalidateTransformationMatrixFromParent(BaseVisualLayer? layer) {
        if (layer != null) {
            if (layer.isRendering)
                throw new InvalidOperationException("Attempt to invalidate transformation matrix during render. This is not allowed");
            layer.isMatrixDirty = true;
        }
    }

    public static void InternalClearIsSolo(BaseVisualLayer? layer) {
        if (layer == null)
            return;
        layer.IsSolo = false;
    }
}