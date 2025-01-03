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
using PicNetStudio.Avalonia.Shortcuts.Inputs;

namespace PicNetStudio.Avalonia.Shortcuts.Managing;

/// <summary>
/// An input state has a property called <see cref="IsActive"/> which can be activated, deactivated or toggled by the user
/// </summary>
public class GroupedInputState : IGroupedObject {
    private IInputStroke activationStroke;
    private IInputStroke deactivationStroke;
    private bool? isPressAndRelease;
    private bool? isToggleBehaviour;

    public ShortcutManager Manager => this.Parent?.Manager;

    public ShortcutGroup Parent { get; }

    public string Name { get; }

    public string FullPath { get; }

    /// <summary>
    /// Gets or sets the <see cref="InputStateManager"/> that manages this input state
    /// </summary>
    public InputStateManager StateManager { get; set; }

    /// <summary>
    /// The state of this input state
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// The input stroke that activates this key state (as in, sets <see cref="IsActive"/> to true)
    /// </summary>
    /// <exception cref="ArgumentNullException">Value cannot be null</exception>
    public IInputStroke ActivationStroke {
        get => this.activationStroke;
        set {
            this.activationStroke = value ?? throw new ArgumentNullException(nameof(value), "Activation stroke cannot be null");
            this.isPressAndRelease = null;
        }
    }

    /// <summary>
    /// The input stroke that deactivates this key state (as in, sets <see cref="IsActive"/> to false)
    /// </summary>
    /// <exception cref="ArgumentNullException">Value cannot be null</exception>
    public IInputStroke DeactivationStroke {
        get => this.deactivationStroke;
        set {
            this.deactivationStroke = value ?? throw new ArgumentNullException(nameof(value), "Activation stroke cannot be null");
            this.isPressAndRelease = null;
        }
    }

    /// <summary>
    /// Whether this input state's activation and deactivation is caused by the same key being
    /// pressed then released. This is a special case used by the <see cref="InputStateManager"/>
    /// </summary>
    /// <value>See above</value>
    public bool IsInputPressAndRelease {
        get {
            if (!this.isPressAndRelease.HasValue) {
                if (this.activationStroke is KeyStroke a && this.deactivationStroke is KeyStroke b) {
                    this.isPressAndRelease = a.EqualsExceptRelease(b);
                }
                else {
                    this.isPressAndRelease = false;
                }
            }

            return this.isPressAndRelease.Value;
        }
    }

    /// <summary>
    /// Whether this input state's activation and deactivation stroke are equal, meaning it behaves like a toggle state
    /// </summary>
    public bool IsUsingToggleBehaviour {
        get {
            if (!this.isToggleBehaviour.HasValue)
                this.isToggleBehaviour = this.activationStroke.Equals(this.deactivationStroke);
            return this.isToggleBehaviour.Value;
        }
    }

    public GroupedInputState(ShortcutGroup group, string name, IInputStroke activationStroke, IInputStroke deactivationStroke) {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be null, empty, or consist of only whitespaces");
        this.Parent = group ?? throw new ArgumentNullException(nameof(group), "Collection cannot be null");
        this.Name = name;
        this.FullPath = group.GetPathForName(name);
        this.ActivationStroke = activationStroke;
        this.DeactivationStroke = deactivationStroke;
    }

    /// <summary>
    /// Called when this input state was activated. Does nothing when already active
    /// </summary>
    /// <param name="shortcutProcessor"></param>
    /// <returns>A task to await for activation</returns>
    public void OnActivated(ShortcutInputProcessor inputProcessor) {
        if (this.IsActive) {
            throw new Exception("Already active; cannot activate again");
        }

        this.IsActive = true;
        inputProcessor.OnInputStateActivated(this);
    }

    /// <summary>
    /// Called when this state was deactivated. Does nothing when already inactve
    /// </summary>
    /// <param name="shortcutProcessor"></param>
    /// <returns>A task to await for activation</returns>
    public void OnDeactivated(ShortcutInputProcessor inputProcessor) {
        if (!this.IsActive) {
            throw new Exception("Not active; cannot deactivate again");
        }

        this.IsActive = false;
        inputProcessor.OnInputStateDeactivated(this);
    }

    public override string ToString() {
        return $"{nameof(GroupedInputState)} ({this.FullPath}: {(this.IsActive ? "pressed" : "released")} [{this.activationStroke}, {this.deactivationStroke}])";
    }
}