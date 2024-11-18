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

using PicNetStudio.Avalonia.PicNet.Layers.Core;

namespace PicNetStudio.Avalonia.PicNet.Layers;

public class LayerTypeFactory : ReflectiveObjectFactory<BaseLayerTreeObject> {
    public static LayerTypeFactory Instance { get; } = new LayerTypeFactory();

    private LayerTypeFactory() {
        this.RegisterType("l_comp", typeof(CompositionLayer));
        this.RegisterType("l_raster", typeof(RasterLayer));
    }

    public new BaseLayerTreeObject NewInstance(string id) => base.NewInstance(id);
}