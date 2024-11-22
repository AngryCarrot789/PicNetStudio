using System.Threading.Tasks;
using SkiaSharp;

namespace PicNetStudio.Avalonia.Services;

public interface IColourPickerService {
    Task<SKColor?> PickColourAsync(SKColor? defaultColour);
}