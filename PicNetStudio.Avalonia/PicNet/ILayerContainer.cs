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

using PicNetStudio.Avalonia.PicNet.Layers;
using PicNetStudio.Avalonia.Utils.Collections.Observable;

namespace PicNetStudio.Avalonia.PicNet;

/// <summary>
/// An interface for an object that stores a collection of child layers. This will be a canvas or composition layer
/// </summary>
public interface ILayerContainer {
    ReadOnlyObservableList<BaseLayerTreeObject> Layers { get; }

    void AddLayer(BaseLayerTreeObject layer);
    void InsertLayer(int index, BaseLayerTreeObject layer);
    bool RemoveLayer(BaseLayerTreeObject layer);
    void RemoveLayerAt(int index);
    int IndexOf(BaseLayerTreeObject layer);
}