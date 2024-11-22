using PicNetStudio.Avalonia.DataTransfer;
using PicNetStudio.Avalonia.Services.Messages;
using PicNetStudio.Avalonia.Utils.Accessing;
using SkiaSharp;

namespace PicNetStudio.Avalonia.Services.Colours;

public class ColourUserInputInfo : UserInputInfo {
    public static readonly DataParameter<SKColor> ColourParameter = DataParameter.Register(new DataParameter<SKColor>(typeof(ColourUserInputInfo), nameof(Colour), SKColors.Empty, ValueAccessors.Reflective<SKColor>(typeof(ColourUserInputInfo), nameof(colour))));

    private SKColor colour = ColourParameter.DefaultValue;

    public SKColor Colour {
        get => this.colour;
        set => DataParameter.SetValueHelper(this, ColourParameter, ref this.colour, value);
    }

    public ColourUserInputInfo() {
    }

    public override bool CanDialogClose() {
        return true;
    }
}