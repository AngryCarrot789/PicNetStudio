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

namespace PicNetStudio.Tasks;

/// <summary>
/// An implementation of <see cref="EmptyActivityProgress"/> that does nothing (no events, get/set values return default values, etc.)
/// </summary>
public class EmptyActivityProgress : IActivityProgress {
    public static readonly IActivityProgress Instance = new EmptyActivityProgress();

    bool IActivityProgress.IsIndeterminate { get => default; set { } }
    double IActivityProgress.TotalCompletion { get => default; set { } }
    string IActivityProgress.HeaderText { get => default; set { } }
    string IActivityProgress.Text { get => default; set { } }

    event ActivityProgressEventHandler IActivityProgress.IsIndeterminateChanged { add { } remove { } }
    event ActivityProgressEventHandler IActivityProgress.CompletionValueChanged { add { } remove { } }
    event ActivityProgressEventHandler IActivityProgress.HeaderTextChanged { add { } remove { } }
    event ActivityProgressEventHandler IActivityProgress.TextChanged { add { } remove { } }

    private int stackCount; // used to track possible bugs

    public EmptyActivityProgress() { }

    PopDispose IActivityProgress.PushCompletionRange(double min, double max) {
        ++this.stackCount;
        return new PopDispose(this);
    }

    void IActivityProgress.PopCompletionRange() {
        if (this.stackCount == 0)
            throw new InvalidOperationException("Cannot pop completion range: stack was empty!");
        --this.stackCount;
    }

    void IActivityProgress.OnProgress(double value) { }
}