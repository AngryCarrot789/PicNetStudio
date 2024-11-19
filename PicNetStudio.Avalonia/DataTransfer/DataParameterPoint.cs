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
using PicNetStudio.Avalonia.Utils;
using PicNetStudio.Avalonia.Utils.Accessing;
using SkiaSharp;

namespace PicNetStudio.Avalonia.DataTransfer;

/// <summary>
/// A <see cref="DataParameter{T}"/> that manages an SKPoint, which is two 32-bit single precision floating
/// point numbers (aka, a float). This also has an optional minimum and maximum value range
/// </summary>
public sealed class DataParameterPoint : DataParameter<SKPoint> {
    public static SKPoint SKPointMinValue => new SKPoint(float.MinValue, float.MinValue);
    public static SKPoint SKPointMaxValue => new SKPoint(float.MaxValue, float.MaxValue);
    
    /// <summary>
    /// The minimum value of the parameter. The final effective value may not drop below this
    /// </summary>
    public SKPoint Minimum { get; }

    /// <summary>
    /// The maximum value of the parameter. The final effective value may not exceed this
    /// </summary>
    public SKPoint Maximum { get; }

    private readonly bool hasRangeLimit;

    public DataParameterPoint(Type ownerType, string name, ValueAccessor<SKPoint> accessor, DataParameterFlags flags = DataParameterFlags.None) : this(ownerType, name, default, accessor, flags) {
    }

    public DataParameterPoint(Type ownerType, string name, SKPoint defValue, ValueAccessor<SKPoint> accessor, DataParameterFlags flags = DataParameterFlags.None) : this(ownerType, name, defValue, SKPointMinValue, SKPointMaxValue, accessor, flags) {
    }

    public DataParameterPoint(Type ownerType, string name, SKPoint defValue, SKPoint minValue, SKPoint maxValue, ValueAccessor<SKPoint> accessor, DataParameterFlags flags = DataParameterFlags.None) : base(ownerType, name, defValue, accessor, flags) {
        if (minValue.X > maxValue.X || minValue.Y > maxValue.Y)
            throw new ArgumentException($"All or a part of the Minimum value exceeds the Maximum value: {minValue} > {maxValue}", nameof(minValue));
        if (defValue.X < minValue.X || defValue.Y < minValue.Y || defValue.X > maxValue.X || defValue.Y > maxValue.Y)
            throw new ArgumentOutOfRangeException(nameof(defValue), $"Default value ({defValue}) falls out of range of the min/max values ({minValue}/{maxValue})");
        this.Minimum = minValue;
        this.Maximum = maxValue;
        this.hasRangeLimit = minValue.X > float.MinValue || minValue.Y > float.MinValue || maxValue.X < float.MaxValue || maxValue.Y < float.MaxValue;
    }

    public static SKPoint Clamp(SKPoint point, SKPoint min, SKPoint max) => new SKPoint(Maths.Clamp(point.X, min.X, max.X), Maths.Clamp(point.Y, min.Y, max.Y));
    
    public SKPoint Clamp(SKPoint value) => Clamp(value, this.Minimum, this.Maximum);

    public bool IsValueOutOfRange(SKPoint value) => value.X < this.Minimum.X || value.Y < this.Minimum.Y || value.X > this.Maximum.X || value.Y > this.Maximum.Y;

    public override void SetValue(ITransferableData owner, SKPoint value) {
        if (this.hasRangeLimit) {
            value = Clamp(value, this.Minimum, this.Maximum);
        }

        base.SetValue(owner, value);
    }

    public override void SetObjectValue(ITransferableData owner, object? value) {
        if (this.hasRangeLimit) {
            SKPoint unboxed = (SKPoint) value!;
            SKPoint clamped = Clamp(unboxed, this.Minimum, this.Maximum);
            if (!Maths.Equals(unboxed, clamped))
                value = clamped;
        }

        base.SetObjectValue(owner, value);
    }
}