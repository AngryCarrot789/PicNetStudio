// 
// Copyright (c) 2023-2024 REghZy
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

using System.Collections.Generic;
using System.Linq;
using PicNetStudio.Avalonia.PicNet.Layers;

namespace PicNetStudio.Avalonia.PicNet.PropertyEditing.Core;

public class LayerNamePropertyEditorSlot : PropertyEditorSlot {
    public IEnumerable<BaseLayerTreeObject> NameHandlers => this.Handlers.Cast<BaseLayerTreeObject>();

    public BaseLayerTreeObject SingleSelection => (BaseLayerTreeObject) this.Handlers[0];

    public string Name { get; private set; }

    public override bool IsSelectable => true;

    public event PropertyEditorSlotEventHandler? NameChanged;
    private bool isProcessingValueChange;

    public LayerNamePropertyEditorSlot() : base(typeof(BaseLayerTreeObject)) {
    }

    public void SetValue(string value) {
        this.isProcessingValueChange = true;

        this.Name = value;
        for (int i = 0, c = this.Handlers.Count; i < c; i++) {
            BaseLayerTreeObject clip = (BaseLayerTreeObject) this.Handlers[i];
            clip.Name = value;
        }

        this.NameChanged?.Invoke(this);
        this.isProcessingValueChange = false;
    }

    protected override void OnHandlersLoaded() {
        base.OnHandlersLoaded();
        if (this.Handlers.Count == 1) {
            this.SingleSelection.NameChanged += this.OnLayerNameChanged;
        }

        this.RequeryOpacityFromHandlers();
    }

    protected override void OnClearingHandlers() {
        base.OnClearingHandlers();
        if (this.Handlers.Count == 1) {
            this.SingleSelection.NameChanged -= this.OnLayerNameChanged;
        }
    }

    public void RequeryOpacityFromHandlers() {
        this.Name = GetEqualValue(this.Handlers, x => ((BaseLayerTreeObject) x).Name, out string d) ? d : "<different values>";
        this.NameChanged?.Invoke(this);
    }

    private void OnLayerNameChanged(BaseLayerTreeObject sender) {
        if (this.isProcessingValueChange)
            return;

        if (this.Name != sender.Name) {
            this.Name = sender.Name;
            this.NameChanged?.Invoke(this);
        }
    }
}