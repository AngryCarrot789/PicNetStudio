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

namespace PicNetStudio.Avalonia.PicNet.Layers;

public delegate void BaseLayerTreeObjectParentLayerChangedEventHandler(BaseLayerTreeObject sender, CompositeLayer? oldParentLayer, CompositeLayer? newParentLayer);
public delegate void BaseLayerTreeObjectCanvasChangedEventHandler(BaseLayerTreeObject sender, Canvas? oldCanvas, Canvas? newCanvas);
public delegate void BaseLayerTreeObjectEventHandler(BaseLayerTreeObject sender);
public delegate void BaseLayerTreeObjectNameChangedEventHandler(BaseLayerTreeObject sender);

/// <summary>
/// The base class for an object in the layer hierarchy for a canvas
/// </summary>
public abstract class BaseLayerTreeObject : ITransferableData {
    private CompositeLayer? parentLayer;
    private string name = "Layer Object";

    /// <summary>
    /// Gets the canvas this layer currently exists in
    /// </summary>
    public Canvas? Canvas { get; private set; }

    /// <summary>
    /// Gets or sets the composition layer that is a direct parent to this layer
    /// </summary>
    public CompositeLayer? ParentLayer => this.parentLayer;

    public TransferableData TransferableData { get; }

    public string Name {
        get => this.name;
        set {
            if (this.name == value)
                return;

            this.name = value;
            this.NameChanged?.Invoke(this);
        }
    }

    /// <summary>
    /// Gets the container object that this layer is currently placed in
    /// </summary>
    public ILayerContainer? ParentContainer {
        get {
            if (this.parentLayer != null)
                return this.parentLayer;
            return this.Canvas;
        }
    }

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
    protected virtual void OnParentChanged(ILayerContainer? oldParent, ILayerContainer? newParent) {
    }

    /// <summary>
    /// Invoked when one of our hierarchical parents is added to or removed from a parent object as a child to a layer as a child. 
    /// This method is just for clarity and most likely isn't needed
    /// <para>    /// <param name="newParent">The origin's new parent (non-null when adding or moving)</param>

    /// Even if the old/new parent object is a canvas, this is invoked before <see cref="OnAttachedToCanvas"/> or <see cref="OnDetachedFromCanvas"/>
    /// </para>
    /// <para>
    /// The origin's new parent will equal its <see cref="ParentContainer"/>
    /// </para>
    /// </summary>
    /// <param name="origin">The parent that was actually added. It may equal this layer's parent layer</param>
    /// <param name="oldParent">The origin's previous parent (non-null when removing or moving)</param>
    protected virtual void OnHierarchicalParentChanged(BaseLayerTreeObject origin, ILayerContainer? originOldParent) {
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

    internal void DeselectRecursive() {
        if (this.Canvas != null && this.Canvas.LayerSelectionManager.Selection.Count > 0) {
            List<BaseLayerTreeObject> list = new List<BaseLayerTreeObject>();
            CollectTreeInternal(this, list);
            this.Canvas.LayerSelectionManager.Unselect(list);
        }
    }

    internal static void CollectTreeInternal(BaseLayerTreeObject layer, List<BaseLayerTreeObject> list) {
        list.Add(layer);
        if (layer is CompositeLayer) {
            foreach (BaseLayerTreeObject child in ((CompositeLayer) layer).Layers) {
                CollectTreeInternal(child, list);
            }
        }
    }

    internal static void InternalOnPreRemoveFromOwner(BaseLayerTreeObject layer) {
        layer.DeselectRecursive();
    }

    #region fucking nightmare fuel

    // User deleted top-level layer
    internal static void InternalOnAddedAsTopLevelLayer(BaseLayerTreeObject layer, Canvas canvas) {
        layer.Canvas = canvas;
        layer.OnAttachedToCanvas(layer);
        layer.CanvasChanged?.Invoke(layer, null, canvas);
        if (layer is CompositeLayer asComposition) {
            foreach (BaseLayerTreeObject child in asComposition.Layers) {
                child.Canvas = canvas;
                RecurseChildren(child, layer);
            }
        }

        // Not passing the new canvas in this recurse function as it saves stack memory ^-^

        return;

        static void RecurseChildren(BaseLayerTreeObject child, BaseLayerTreeObject origin) {
            child.OnAttachedToCanvas(origin);
            child.CanvasChanged?.Invoke(child, null, child.Canvas);
            if (child is CompositeLayer asComposition) {
                foreach (BaseLayerTreeObject nextChild in asComposition.Layers) {
                    nextChild.Canvas = child.Canvas;
                    RecurseChildren(nextChild, origin);
                }
            }
        }
    }

    // User deleted top-level layer
    internal static void InternalOnRemovedAsTopLevelLayer(BaseLayerTreeObject layer, Canvas oldCanvas) {
        layer.Canvas = null;
        layer.OnDetachedFromCanvas(layer, oldCanvas);
        layer.CanvasChanged?.Invoke(layer, oldCanvas, null);
        if (layer is CompositeLayer asComposition) {
            foreach (BaseLayerTreeObject child in asComposition.Layers) {
                child.Canvas = null;
                RecurseChildren(child, layer, oldCanvas);
            }
        }

        return;

        static void RecurseChildren(BaseLayerTreeObject child, BaseLayerTreeObject origin, Canvas originOldCanvas) {
            child.OnDetachedFromCanvas(origin, originOldCanvas);
            child.CanvasChanged?.Invoke(child, originOldCanvas, null);
            if (child is CompositeLayer asComposition) {
                foreach (BaseLayerTreeObject nextChild in asComposition.Layers) {
                    nextChild.Canvas = null;
                    RecurseChildren(nextChild, origin, originOldCanvas);
                }
            }
        }
    }

    // User added some layer into composition layer
    internal static void InternalOnAddedToLayer(BaseLayerTreeObject layer, CompositeLayer newParent) {
        Debug.Assert(layer.ParentLayer == null, "Did not expect layer to be in a composition layer when adding it to another");
        Debug.Assert(layer.Canvas == null, "Did not expect layer to be in a canvas when adding to a composition layer");

        layer.parentLayer = newParent;
        layer.OnParentChanged(null, newParent);
        layer.ParentLayerChanged?.Invoke(layer, null, newParent);

        if (newParent.Canvas != null) {
            layer.Canvas = newParent.Canvas;
            layer.OnAttachedToCanvas(layer);
            layer.CanvasChanged?.Invoke(layer, null, layer.Canvas);
        }

        if (layer is CompositeLayer asComposition) {
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

            if (child is CompositeLayer childAsComposition) {
                foreach (BaseLayerTreeObject nextChild in childAsComposition.Layers) {
                    RecurseChildren(nextChild, origin);
                }
            }
        }
    }

    // User deleted some layer from composition layer
    internal static void InternalOnRemovedFromLayer(BaseLayerTreeObject layer, CompositeLayer oldParent) {
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

        if (layer is CompositeLayer asComposition) {
            foreach (BaseLayerTreeObject child in asComposition.Layers) {
                RecurseChildren(child, layer, oldParent, oldCanvas);
            }
        }

        return;

        static void RecurseChildren(BaseLayerTreeObject child, BaseLayerTreeObject origin, CompositeLayer originOldParent, Canvas? oldCanvas) {
            // Detach from canvas first, then notify hierarchical parent removed from layer
            if (child.Canvas != null) {
                Debug.Assert(oldCanvas == child.Canvas, "Expected oldCanvas and our canvas to match");

                child.Canvas = null;
                child.OnDetachedFromCanvas(origin, oldCanvas);
                child.CanvasChanged?.Invoke(child, oldCanvas, null);
            }

            child.OnHierarchicalParentChanged(origin, originOldParent);
            if (child is CompositeLayer layer) {
                foreach (BaseLayerTreeObject nextChild in layer.Layers) {
                    RecurseChildren(nextChild, origin, originOldParent, oldCanvas);
                }
            }
        }
    }

    #endregion

    public static bool CheckHaveParentsAndAllMatch(SelectionManager<BaseLayerTreeObject> manager, [NotNullWhen(true)] out ILayerContainer? sameParent) {
        using (IEnumerator<BaseLayerTreeObject> enumerator = manager.Selection.GetEnumerator()) {
            if (!enumerator.MoveNext())
                throw new InvalidOperationException("Expected items to contain at least 1 item");
            
            if ((sameParent = enumerator.Current.ParentContainer) == null)
                return false;

            while (enumerator.MoveNext()) {
                if (!ReferenceEquals(enumerator.Current.ParentContainer, sameParent)) {
                    return false;
                }
            }

            return true;
        }
    }
}