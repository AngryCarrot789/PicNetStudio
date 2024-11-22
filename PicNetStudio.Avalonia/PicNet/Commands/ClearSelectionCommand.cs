using PicNetStudio.Avalonia.CommandSystem;

namespace PicNetStudio.Avalonia.PicNet.Commands;

public class ClearSelectionCommand : DocumentCommand {
    protected override Executability CanExecute(Editor editor, Document document, CommandEventArgs e) {
        return document.Canvas.SelectionRegion != null ? Executability.Valid : Executability.ValidButCannotExecute;
    }

    protected override void Execute(Editor editor, Document document, CommandEventArgs e) {
        document.Canvas.SelectionRegion = null;
    }
}