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

namespace PicNetStudio.Avalonia.PicNet.PropertyEditing.BasicEditors;

public class DisplayNamePropertyEditorSlot : PropertyEditorSlot {
    public IEnumerable<IDisplayName> DisplayNameHandlers => this.Handlers.Cast<IDisplayName>();

    public IDisplayName SingleSelection => (IDisplayName) this.Handlers[0];

    public string DisplayName { get; private set; }

    public override bool IsSelectable => true;

    public event PropertyEditorSlotEventHandler DisplayNameChanged;
    private bool isProcessingValueChange;

    public DisplayNamePropertyEditorSlot() : base(typeof(IDisplayName)) {
    }

    public void SetValue(string value) {
        this.isProcessingValueChange = true;

        this.DisplayName = value;
        for (int i = 0, c = this.Handlers.Count; i < c; i++) {
            IDisplayName clip = (IDisplayName) this.Handlers[i];
            clip.DisplayName = value;
        }

        this.DisplayNameChanged?.Invoke(this);
        this.isProcessingValueChange = false;
    }

    protected override void OnHandlersLoaded() {
        base.OnHandlersLoaded();
        if (this.Handlers.Count == 1) {
            this.SingleSelection.DisplayNameChanged += this.OnClipDisplayNameChanged;
        }

        this.RequeryOpacityFromHandlers();
    }

    protected override void OnClearingHandlers() {
        base.OnClearingHandlers();
        if (this.Handlers.Count == 1) {
            this.SingleSelection.DisplayNameChanged -= this.OnClipDisplayNameChanged;
        }
    }

    public void RequeryOpacityFromHandlers() {
        this.DisplayName = GetEqualValue(this.Handlers, x => ((IDisplayName) x).DisplayName, out string d) ? d : "<different values>";
        this.DisplayNameChanged?.Invoke(this);
    }

    private void OnClipDisplayNameChanged(IDisplayName sender, string oldName, string newName) {
        if (this.isProcessingValueChange)
            return;
        if (this.DisplayName != sender.DisplayName) {
            this.DisplayName = sender.DisplayName;
            this.DisplayNameChanged?.Invoke(this);
        }
    }
}