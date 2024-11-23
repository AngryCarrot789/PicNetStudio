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

using PicNetStudio.CommandSystem;
using PicNetStudio.PicNet.Commands;
using PicNetStudio.PicNet.Tools.Core;
using PicNetStudio.Tasks;

namespace PicNetStudio;

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

    /// <summary>
    /// Gets the current build version for this application. This accesses <see cref="CurrentVersion"/>, and changes whenever a new change is made to the application (regardless of how small)
    /// </summary>
    public int CurrentBuild => this.CurrentVersion.Build;
    
    protected RZApplication() {
        this.serviceManager = new ServiceManager();
    }

    private void OnPreInitialise() {
    }

    protected virtual void OnInitialise() {
        this.RegisterServices(this.serviceManager);
        this.RegisterCommands(CommandManager.Instance);
    }

    protected virtual void RegisterServices(ServiceManager manager) {
        manager.Register<TaskManager>(new TaskManager());
    }

    protected virtual void RegisterCommands(CommandManager manager) {
        // tools
        manager.Register("command.tool.BaseDiameterTool.IncreaseBrushSize", new IncreaseBaseDiameterToolSizeCommand());
        manager.Register("command.tool.BaseDiameterTool.DecreaseBrushSize", new DecreaseBaseDiameterToolSizeCommand());
        manager.Register("command.generic.OpenDocument", new OpenDocumentCommand());
        manager.Register("command.generic.SaveDocument", new SaveDocumentCommand());
        manager.Register("command.generic.ExportImage", new ExportImageCommand());
        manager.Register("command.generic.ExportCanvasToClipboard", new ExportCanvasToClipboardCommand());
        manager.Register("command.toolbar.SelectCursorTool", new SelectCursorToolCommand());
        manager.Register("command.toolbar.SelectBrushTool", new SelectBrushToolCommand());
        manager.Register("command.toolbar.SelectPencilTool", new SelectPencilToolCommand());
        manager.Register("command.toolbar.SelectFillTool", new SelectFillToolCommand());
        manager.Register("command.toolbar.SelectSelectionTool", new SelectSelectionToolCommand());
        manager.Register("command.toolbar.ClearSelection", new ClearSelectionCommand());
        manager.Register("command.layertree.CreateNewRasterLayer", new CreateNewRasterLayerCommand());
        manager.Register("command.layertree.CreateNewTextLayer", new CreateNewTextLayerCommand());
        manager.Register("command.layertree.DeleteSelectedLayers", new DeleteSelectedLayersCommand());
        manager.Register("command.layertree.GroupSelectionIntoComposition", new GroupSelectionIntoCompositionCommand());
        manager.Register("command.layertree.MergeSelectionIntoRaster", new MergeSelectionIntoRasterCommand());
        manager.Register("command.layertree.item.EditLayerName", new EditLayerNameCommand());
        manager.Register("command.layertree.item.ToggleLayerVisibility", new ToggleLayerVisibilityCommand());
        manager.Register("command.layertree.item.RasteriseLayer", new RasteriseLayerCommand());
    }

    protected static void InternalPreInititalise(RZApplication application) {
        if (application == null)
            throw new ArgumentNullException(nameof(application));

        if (instance != null)
            throw new InvalidOperationException("Cannot re-initialise application");

        instance = application;
    }

    protected static void InternalInititalise() {
        if (instance == null)
            throw new InvalidOperationException("Application has not been pre-initialised yet");
        
        instance.OnInitialise();
    }
}