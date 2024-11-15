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
using System.IO;
using System.Threading.Tasks;
using PicNetStudio.Avalonia.CommandSystem;
using PicNetStudio.Avalonia.PicNet.Commands;
using PicNetStudio.Avalonia.PicNet.Tools.Core;
using PicNetStudio.Avalonia.Services.Files;
using PicNetStudio.Avalonia.Services.Messages;
using PicNetStudio.Avalonia.Shortcuts.Avalonia;
using PicNetStudio.Avalonia.Tasks;
using PicNetStudio.Avalonia.Utils;

namespace PicNetStudio.Avalonia;

public abstract class RZApplication {
    private static RZApplication? instance;

    public static RZApplication Instance {
        get {
            if (instance == null)
                throw new InvalidOperationException("Application not initialised yet");

            return instance;
        }
    }

    private readonly ServiceManager serviceManager;

    public IServiceManager Services => this.serviceManager;

    /// <summary>
    /// Gets the main application thread
    /// </summary>
    public abstract IDispatcher Dispatcher { get; }

    /// <summary>
    /// Gets the avalonia application
    /// </summary>
    public App App { get; }

    /// <summary>
    /// Gets the current version of the application. This value does not change during runtime.
    /// <para>The <see cref="Version.Major"/> property is used to represent a backwards-compatibility breaking change to the application (e.g. removal or a change in operating behaviour of a core feature)</para>
    /// <para>The <see cref="Version.Minor"/> property is used to represent a significant but non-breaking change (e.g. new feature that does not affect existing features, or a UI change)</para>
    /// <para>The <see cref="Version.Revision"/> property is used to represent any change to the code</para>
    /// <para>The <see cref="Version.Build"/> property is represents the current build, e.g. if a revision is made but then reverted, there are 2 builds in that</para>
    /// <para>
    /// 'for next update' meaning the number is incremented when there's a push to the github, as this is
    /// easiest to track. Many different changes can count as one update
    /// </para>
    /// </summary>
    public Version CurrentVersion { get; } = new Version(1, 0, 0, 0);

    protected RZApplication(App app) {
        this.App = app ?? throw new ArgumentNullException(nameof(app));
        this.serviceManager = new ServiceManager();
    }

    private void OnPreInitialise() {
    }

    private void OnInitialise() {
        this.serviceManager.Register<IMessageDialogService>(new DummyMessageDialogService());
        this.serviceManager.Register<IUserInputDialogService>(new DummyUserInputDialogService());
        this.serviceManager.Register<IFilePickDialogService>(new DummyFilePickDialogService());
        this.serviceManager.Register<TaskManager>(new TaskManager());

        this.RegisterCommands(CommandManager.Instance);

        string keymapFilePath = Path.GetFullPath(@"Keymap.xml");
        if (File.Exists(keymapFilePath)) {
            try {
                using (FileStream stream = File.OpenRead(keymapFilePath)) {
                    AvaloniaShortcutManager.AvaloniaInstance.DeserialiseRoot(stream);
                }
            }
            catch (Exception ex) {
                IoC.MessageService.ShowMessage("Keymap", "Failed to read keymap file" + keymapFilePath, ex.GetToString());
            }
        }
        else {
            IoC.MessageService.ShowMessage("Keymap", "Keymap file does not exist at " + keymapFilePath);
        }
    }

    private void RegisterCommands(CommandManager manager) {
        // tools
        manager.Register("command.tool.BaseDiameterTool.IncreaseBrushSize", new IncreaseBaseDiameterToolSizeCommand());
        manager.Register("command.tool.BaseDiameterTool.DecreaseBrushSize", new DecreaseBaseDiameterToolSizeCommand());
        manager.Register("command.generic.ExportImage", new ExportImageCommand());
        manager.Register("command.generic.ExportCanvasToClipboard", new ExportCanvasToClipboardCommand());
        manager.Register("command.layertree.CreateNewRasterLayer", new CreateNewRasterLayerCommand());
        manager.Register("command.layertree.DeleteSelectedLayers", new DeleteSelectedLayersCommand());
        manager.Register("command.layertree.GroupSelectionIntoComposition", new GroupSelectionIntoCompositionCommand());
    }

    internal static void InternalPreInititalise(RZApplication application) {
        if (application == null)
            throw new ArgumentNullException(nameof(application));

        if (instance != null)
            throw new InvalidOperationException("Cannot re-initialise application");

        instance = application;
        AvCore.OnApplicationInitialised();
    }

    internal static void InternalInititalise() {
        if (instance == null)
            throw new InvalidOperationException("Application has not been pre-initialised yet");

        AvCore.OnFrameworkInitialised();
        UIInputManager.Init();
        instance.OnInitialise();
    }

    private class DummyMessageDialogService : IMessageDialogService {
        public Task<MessageBoxResult> ShowMessage(string caption, string message, MessageBoxButton buttons = MessageBoxButton.OK) {
            return default;
        }

        public Task<MessageBoxResult> ShowMessage(string caption, string header, string message, MessageBoxButton buttons = MessageBoxButton.OK) {
            return default;
        }
    }

    private class DummyUserInputDialogService : IUserInputDialogService {
        public string ShowSingleInputDialog(string caption, string message, string defaultInput = null, Predicate<string> validate = null, bool allowEmptyString = false) {
            return null;
        }
    }

    private class DummyFilePickDialogService : IFilePickDialogService {
        public string OpenFile(string message, string? filter = null, string? initialDirectory = null) {
            return null;
        }

        public string[] OpenMultipleFiles(string message, string? filter = null, string? initialDirectory = null) {
            return null;
        }

        public string SaveFile(string message, string? filter = null, string? initialFilePath = null) {
            return null;
        }
    }
}