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
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using PicNetStudio.Avalonia.PicNet;
using PicNetStudio.Avalonia.PicNet.Layers;
using Canvas = PicNetStudio.Avalonia.PicNet.Canvas;

namespace PicNetStudio.Avalonia;

public partial class App : Application {
    public static Canvas DummyCanvas { get; } = new Canvas(new Document());

    static App() {
        DummyCanvas.Size = new PixelSize(300, 150);
        DummyCanvas.AddLayer(new RasterLayer() {Name = "Raster 1"});
        DummyCanvas.AddLayer(new CompositeLayer() {Name = "Composite 1"});
        // (DummyCanvas.Layers[0] as RasterLayer).Bitmap.InitialiseBitmap(DummyCanvas.Size);

        ((CompositeLayer) DummyCanvas.Layers[1]).AddLayer(new RasterLayer() {Name = "Raster 2 in composite"});
    }
    
    public App() {
    }

    public override void Initialize() {
        AvaloniaXamlLoader.Load(this);
        RZApplication.InternalPreInititalise(new RZApplicationImpl(this));
    }

    public override void OnFrameworkInitializationCompleted() {
        RZApplication.InternalInititalise();

        if (this.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
            desktop.MainWindow = new EditorWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private class RZApplicationImpl : RZApplication {
        public override IDispatcher Dispatcher { get; }

        public RZApplicationImpl(App app) : base(app) {
            this.Dispatcher = new DispatcherImpl(global::Avalonia.Threading.Dispatcher.UIThread);
        }

        private class DispatcherImpl : IDispatcher {
            private readonly Dispatcher dispatcher;

            public DispatcherImpl(Dispatcher dispatcher) {
                this.dispatcher = dispatcher;
            }

            public bool CheckAccess() {
                return this.dispatcher.CheckAccess();
            }

            public void VerifyAccess() {
                this.dispatcher.VerifyAccess();
            }

            public void Invoke(Action action, DispatchPriority priority) {
                if (priority == DispatchPriority.Send && this.dispatcher.CheckAccess()) {
                    action();
                }
                else {
                    this.dispatcher.Invoke(action, ToAvaloniaPriority(priority));
                }
            }

            public void Invoke<T>(Action<T> action, T parameter, DispatchPriority priority) {
                if (priority == DispatchPriority.Send && this.dispatcher.CheckAccess()) {
                    action(parameter);
                }
                else {
                    this.dispatcher.Post(x => action((T) x!), parameter, ToAvaloniaPriority(priority));
                }
            }

            public T Invoke<T>(Func<T> function, DispatchPriority priority) {
                if (priority == DispatchPriority.Send && this.dispatcher.CheckAccess())
                    return function();
                return this.dispatcher.Invoke(function, ToAvaloniaPriority(priority));
            }

            public Task InvokeAsync(Action action, DispatchPriority priority, CancellationToken token = default) {
                return this.dispatcher.InvokeAsync(action, ToAvaloniaPriority(priority), token).GetTask();
            }

            public Task<T> InvokeAsync<T>(Func<T> function, DispatchPriority priority, CancellationToken token = default) {
                return this.dispatcher.InvokeAsync(function, ToAvaloniaPriority(priority), token).GetTask();
            }

            public void Post(Action action, DispatchPriority priority = DispatchPriority.Default) {
                this.dispatcher.Post(action, ToAvaloniaPriority(priority));
            }

            private static DispatcherPriority ToAvaloniaPriority(DispatchPriority priority) {
                return Unsafe.As<DispatchPriority, DispatcherPriority>(ref priority);
            }
        }
    }
}