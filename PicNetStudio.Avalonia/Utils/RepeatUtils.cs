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
using System.Reflection;
using Avalonia.Controls;
using Avalonia.Threading;

namespace PicNetStudio.Avalonia.Utils;

public static class RepeatUtils {
    private static readonly FieldInfo TimerField = typeof(RepeatButton).GetField("_repeatTimer", BindingFlags.Instance | BindingFlags.NonPublic) ?? throw new Exception("Missing _repeatTimer field in RepeatButton");

    public static bool IsTimerRunning(this RepeatButton button) {
        DispatcherTimer timer = (DispatcherTimer) TimerField.GetValue(button);
        return timer != null && timer.IsEnabled;
    }
}