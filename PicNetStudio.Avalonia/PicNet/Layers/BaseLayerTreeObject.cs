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
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using PicNetStudio.Avalonia.DataTransfer;
using PicNetStudio.Avalonia.PicNet.Effects;
using PicNetStudio.Avalonia.Utils.Collections.Observable;

namespace PicNetStudio.Avalonia.PicNet.Layers;

public delegate void BaseLayerTreeObjectParentLayerChangedEventHandler(BaseLayerTreeObject sender, CompositionLayer? oldParentLayer, CompositionLayer? newParentLayer);
public delegate void BaseLayerTreeObjectCanvasChangedEventHandler(BaseLayerTreeObject sender, Canvas? oldCanvas, Canvas? newCanvas);
public delegate void BaseLayerTreeObjectEventHandler(BaseLayerTreeObject sender);
public delegate void BaseLayerTreeObjectNameChangedEventHandler(BaseLayerTreeObject sender);

/// <summary>
/// The base class for an object in the layer hierarchy for a canvas
/// </summary>
public abstract class BaseLayerTreeObject : ITransferableData {
    private readonly SuspendableObservableList<BaseLayerEffect> effects;

    private CompositionLayer? parentLayer;
    private string name = "Layer Object";

    // Updated by composition layer add/remove/move operations
    // !! Do not use to check if this layer has a parent or not !!
    // It is 0 by default, and may be invalid if an add/rem/move operations fails
    private int indexInParent;

    /// <summary>
    /// Gets the canvas this layer currently exists in
    /// </summary>
    public Canvas? Canvas { get; private set; }

    /// <summary>
    /// Gets or sets the composition layer that is a direct parent to this layer
    /// </summary>
    public CompositionLayer? ParentLayer => this.parentLayer;

    public ReadOnlyObservableList<BaseLayerEffect> Effects { get; }

    public bool HasEffects => this.effects.Count > 0;

    public TransferableData TransferableData { get; }

    public string Name {
        get => this.name;
        set {
            if (this.name == value)
                return;

            this.name = value ?? "";
            this.NameChanged?.Invoke(this);
        }
    }

    public string FactoryId => LayerTypeFactory.Instance.GetId(this.GetType());

    /// <summary>
    /// An event fired when our <see cref="ParentLayer"/> property changes.
    /// If the new parent is attached to a canvas, our canvas will be updates
    /// after this event is fired (see <see cref="CanvasChanged"/>)
    /// </summary>
    public event BaseLayerTreeObjectParentLayerChangedEventHandler? ParentLayerChanged;

    /// <summary>
    /// An event fired when our <see cref="Canvas"/> property changes due to canvas attachment or detachment.
    /// When caused by layer arrangement changes, this is fired AFTER <see cref="ParentLayerChanged"/> 
    /// </summary>
    public event BaseLayerTreeObjectCanvasChangedEventHandler? CanvasChanged;

    public event BaseLayerTreeObjectNameChangedEventHandler? NameChanged;

    protected BaseLayerTreeObject() {
        this.TransferableData = new TransferableData(this);
        this.effects = new SuspendableObservableList<BaseLayerEffect>();
        this.Effects = new ReadOnlyObservableList<BaseLayerEffect>(this.effects);
    }

    /// <summary>
    /// Creates a new instance with the same data which will be completely unaffected by the current instance
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public BaseLayerTreeObject Clone() {
        string id = this.FactoryId;
        BaseLayerTreeObject clone = LayerTypeFactory.Instance.NewInstance(id);
        if (clone.GetType() != this.GetType())
            throw new Exception("Cloned object type does not match the item type");

        this.LoadDataIntoClone(clone);
        return clone;
    }

    protected virtual void LoadDataIntoClone(BaseLayerTreeObject clone) {
        clone.name = this.name;
    }
    
    public int GetIndexInParent() {
        return this.parentLayer != null ? this.indexInParent : -1;
    }

    /// <summary>
    /// Invoked when this child layer is added to or removed from a parent object as a child.
    /// This method is called before our child layers have their <see cref="OnHierarchicalParentChanged"/> method invoked.
    /// <para>
    /// Even if the old/new parent object is a canvas, this is invoked before <see cref="OnAttachedToCanvas"/> or <see cref="OnDetachedFromCanvas"/>
    /// </para>
    /// </summary>
    /// <param name="oldParent">The previous parent (non-null when removing or moving)</param>
    /// <param name="newParent">The new parent (non-null when adding or moving)</param>
    protected virtual void OnParentChanged(CompositionLayer? oldParent, CompositionLayer? newParent) {
        
    }

    /// <summary>
    /// Invoked when one of our hierarchical parents is added to or removed from a parent object as a child to a layer as a child. 
    /// This method is just for clarity and most likely isn't needed
    /// <para>    /// <param name="newParent">The origin's new parent (non-null when adding or moving)</param>
    /// Even if the old/new parent object is a canvas, this is invoked before <see cref="OnAttachedToCanvas"/> or <see cref="OnDetachedFromCanvas"/>
    /// </para>
    /// <para>
    /// The origin's new parent will equal its <see cref="ParentLayer"/>
    /// </para>
    /// </summary>
    /// <param name="origin">The parent that was actually added. It may equal this layer's parent layer</param>
    /// <param name="oldParent">The origin's previous parent (non-null when removing or moving)</param>
    protected virtual void OnHierarchicalParentChanged(BaseLayerTreeObject origin, CompositionLayer? originOldParent) {
        
    }

    /// <summary>
    /// Invoked when this layer is added to a canvas. This can be fired by either this layer being added to
    /// a composition layer which exists in a canvas, or when we are added as a top-level layer in a canvas
    /// <para>
    /// This is invoked BEFORE <see cref="CanvasChanged"/>
    /// </para>
    /// </summary>
    /// <param name="origin">
    /// The layer that directly caused the canvas to become attached (either by being added as a top-level layer
    /// or being added into a composition layer that exists in a canvas). May equal the current instance
    /// </param>
    protected virtual void OnAttachedToCanvas(BaseLayerTreeObject origin) {
        
    }

    /// <summary>
    /// Invoked when this layer is removed from a canvas. This can be fired by either this layer being removed from
    /// a composition layer which exists in a canvas, or when we are removed as a top-level layer from a canvas.
    /// <para>
    /// This is invoked BEFORE <see cref="CanvasChanged"/>
    /// </para>
    /// </summary>
    /// <param name="origin">
    /// The layer that directly caused the canvas to become detached (either by being removed as a top-level layer
    /// or being removed from a container layer that exists in a canvas). May equal the current instance
    /// </param>
    /// <param name="oldCanvas">The canvas that we previously existed in</param>
    protected virtual void OnDetachedFromCanvas(BaseLayerTreeObject origin, Canvas oldCanvas) {
        
    }

    protected virtual void OnEffectAdded(int index, BaseLayerEffect effect) {
        
    }

    protected virtual void OnEffectRemoved(int index, BaseLayerEffect effect) {
        
    }
    
    public void AddEffect(BaseLayerEffect effect) => this.InsertEffect(this.effects.Count, effect);

    public void InsertEffect(int index, BaseLayerEffect effect) {
        if (index < 0)
            throw new ArgumentOutOfRangeException(nameof(index), "Negative indices not allowed");
        if (index > this.effects.Count)
            throw new ArgumentOutOfRangeException(nameof(index), $"Index is beyond the range of this list: {index} > count({this.effects.Count})");

        if (effect == null)
            throw new ArgumentNullException(nameof(effect), "Cannot add a null effect");
        if (effect.Layer == this)
            throw new InvalidOperationException("Effect already exists in this effect. It must be removed first");
        if (effect.Layer != null)
            throw new InvalidOperationException("Effect already exists in another container. It must be removed first");

        using (this.effects.SuspendEvents()) {
            this.effects.Insert(index, effect);
            BaseLayerEffect.InternalOnAddedToLayer(effect, this);
        }

        this.OnEffectAdded(index, effect);
    }

    public bool RemoveEffect(BaseLayerEffect effect) {
        int index = this.effects.IndexOf(effect);
        if (index == -1)
            return false;
        this.RemoveEffectAt(index);
        return true;
    }

    public void RemoveEffectAt(int index) {
        BaseLayerEffect effect = this.effects[index];
        using (this.effects.SuspendEvents()) {
            this.effects.RemoveAt(index);
            BaseLayerEffect.InternalOnRemovedFromLayer(effect, this);
        }
        
        this.OnEffectRemoved(index, effect);
    }

    public override string ToString() {
        return $"{this.GetType().Name}({this.Name})";
    }

    public static bool CheckHaveParentsAndAllMatch(ISelectionManager<BaseLayerTreeObject> manager, [NotNullWhen(true)] out CompositionLayer? sameParent) {
        using (IEnumerator<BaseLayerTreeObject> enumerator = manager.SelectedItems.GetEnumerator()) {
            if (!enumerator.MoveNext())
                throw new InvalidOperationException("Expected items to contain at least 1 item");

            if ((sameParent = enumerator.Current.ParentLayer) == null)
                return false;

            while (enumerator.MoveNext()) {
                if (!ReferenceEquals(enumerator.Current.ParentLayer, sameParent)) {
                    return false;
                }
            }

            return true;
        }
    }

    /// <summary>
    /// A helper method for removing a list of items from their parent containers
    /// </summary>
    /// <param name="list"></param>
    public static void RemoveListFromTree(List<BaseLayerTreeObject> list) {
        foreach (BaseLayerTreeObject layer in list) {
            layer.ParentLayer?.RemoveLayer(layer);
        }
    }

    internal static void InternalOnPreRemoveFromOwner(BaseLayerTreeObject layer) {
    }

    internal static void InternalSetCanvas(BaseLayerTreeObject layer, Canvas canvas) {
        layer.Canvas = canvas;
    }

    // User added some layer into composition layer
    internal static void InternalOnAddedToLayer(int index, BaseLayerTreeObject layer, CompositionLayer newParent) {
        Debug.Assert(layer.ParentLayer == null, "Did not expect layer to be in a composition layer when adding it to another");
        Debug.Assert(layer.Canvas == null, "Did not expect layer to be in a canvas when adding to a composition layer");

        layer.parentLayer = newParent;
        layer.indexInParent = index;
        layer.OnParentChanged(null, newParent);
        layer.ParentLayerChanged?.Invoke(layer, null, newParent);

        if (newParent.Canvas != null) {
            layer.Canvas = newParent.Canvas;
            layer.OnAttachedToCanvas(layer);
            layer.CanvasChanged?.Invoke(layer, null, layer.Canvas);
        }

        if (layer is CompositionLayer asComposition) {
            foreach (BaseLayerTreeObject child in asComposition.Layers) {
                RecurseChildren(child, layer);
            }
        }

        return;

        static void RecurseChildren(BaseLayerTreeObject child, BaseLayerTreeObject origin) {
            child.OnHierarchicalParentChanged(origin, null);
            if (origin.Canvas != null) {
                child.Canvas = origin.Canvas;
                child.OnAttachedToCanvas(origin);
                child.CanvasChanged?.Invoke(child, null, child.Canvas);
            }

            if (child is CompositionLayer childAsComposition) {
                foreach (BaseLayerTreeObject nextChild in childAsComposition.Layers) {
                    RecurseChildren(nextChild, origin);
                }
            }
        }
    }

    // User deleted some layer from composition layer
    internal static void InternalOnRemovedFromLayer(BaseLayerTreeObject layer, CompositionLayer oldParent) {
        Debug.Assert(layer.ParentLayer != null, "Did not expect layer to not be in a composition layer when removing it from another");

        // While child layers are notified of canvas detachment first, should we do the same here???
        Canvas? oldCanvas = layer.Canvas;
        if (oldCanvas != null) {
            layer.Canvas = null;
            layer.OnDetachedFromCanvas(layer, oldCanvas);
            layer.CanvasChanged?.Invoke(layer, oldCanvas, null);
        }

        layer.parentLayer = null;
        layer.OnParentChanged(oldParent, null);
        layer.ParentLayerChanged?.Invoke(layer, oldParent, null);

        if (layer is CompositionLayer asComposition) {
            foreach (BaseLayerTreeObject child in asComposition.Layers) {
                RecurseChildren(child, layer, oldParent, oldCanvas);
            }
        }

        return;

        static void RecurseChildren(BaseLayerTreeObject child, BaseLayerTreeObject origin, CompositionLayer originOldParent, Canvas? oldCanvas) {
            // Detach from canvas first, then notify hierarchical parent removed from layer
            if (child.Canvas != null) {
                Debug.Assert(oldCanvas == child.Canvas, "Expected oldCanvas and our canvas to match");

                child.Canvas = null;
                child.OnDetachedFromCanvas(origin, oldCanvas);
                child.CanvasChanged?.Invoke(child, oldCanvas, null);
            }

            child.OnHierarchicalParentChanged(origin, originOldParent);
            if (child is CompositionLayer layer) {
                foreach (BaseLayerTreeObject nextChild in layer.Layers) {
                    RecurseChildren(nextChild, origin, originOldParent, oldCanvas);
                }
            }
        }
    }

    protected internal static void InternalSetIndexInParent(BaseLayerTreeObject layer, int index) {
        layer.indexInParent = index;
    }

    protected internal static int InternalIndexInParent(BaseLayerTreeObject layer) {
        return layer.indexInParent;
    }
}