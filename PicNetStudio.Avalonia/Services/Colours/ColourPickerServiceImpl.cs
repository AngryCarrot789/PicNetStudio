using System.Threading.Tasks;
using SkiaSharp;

namespace PicNetStudio.Avalonia.Services.Colours;

public class ColourPickerServiceImpl : IColourPickerService {
    public async Task<SKColor?> PickColourAsync(SKColor? defaultColour) {
        ColourUserInputInfo info = new ColourUserInputInfo() {
            Colour = defaultColour ?? SKColors.Black
        };

        return await ShowAsync(info) == true ? info.Colour : default(SKColor?);
    }

    private static Task<bool?> ShowAsync(ColourUserInputInfo info) {
        return InputDialogServiceImpl.ShowDialogAsync(info);
    }
}