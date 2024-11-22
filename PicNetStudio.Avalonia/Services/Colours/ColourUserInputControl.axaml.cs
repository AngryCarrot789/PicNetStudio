using Avalonia.Controls;
using Avalonia.Media;
using PicNetStudio.Avalonia.Bindings;
using PicNetStudio.Avalonia.Services.Messages;
using PicNetStudio.Avalonia.Services.Messages.Controls;
using SkiaSharp;

namespace PicNetStudio.Avalonia.Services.Colours;

public partial class ColourUserInputControl : UserControl, IUserInputContent {
    private readonly DataParameterPropertyBinder<ColourUserInputInfo> colourBinder;
    private UserInputDialog myDialog;
    private ColourUserInputInfo myData;

    public ColourUserInputControl() {
        this.InitializeComponent();
        this.colourBinder = new DataParameterPropertyBinder<ColourUserInputInfo>(ColorView.ColorProperty, ColourUserInputInfo.ColourParameter, arg => {
            SKColor c = (SKColor) arg!;
            return new Color(c.Alpha, c.Red, c.Green, c.Blue);
        }, arg => {
            Color c = (Color) arg!;
            return new SKColor(c.R, c.G, c.B, c.A);
        });
        
        this.colourBinder.AttachControl(this.PART_ColorView);
    }

    public void Connect(UserInputDialog dialog, UserInputInfo info) {
        this.myDialog = dialog;
        this.myData = (ColourUserInputInfo) info;
        this.colourBinder.AttachModel(this.myData);
    }

    public void Disconnect() {
        this.colourBinder.DetachModel();
        this.myDialog = null;
        this.myData = null;
    }
    
    public void FocusPrimaryInput() {
        this.PART_ColorView.Focus();
    }
}