using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace PicNetStudio.Avalonia.Services.Messages.Classic;

public delegate void MessageBoxWindowResultChangedEventHandler(MessageBoxWindow sender);

public partial class MessageBoxWindow : Window {
    public static readonly StyledProperty<string?> HeaderMessageProperty = AvaloniaProperty.Register<MessageBoxWindow, string?>("HeaderMessage");
    public static readonly StyledProperty<string?> MessageProperty = AvaloniaProperty.Register<MessageBoxWindow, string?>("Message");

    private MessageBoxResult result;
    
    public string? HeaderMessage {
        get => this.GetValue(HeaderMessageProperty);
        set => this.SetValue(HeaderMessageProperty, value);
    }

    public string? Message {
        get => this.GetValue(MessageProperty);
        set => this.SetValue(MessageProperty, value);
    }

    public MessageBoxResult Result {
        get => this.result;
        private set {
            if (this.result == value)
                return;

            this.result = value;
            this.ResultChanged?.Invoke(this);
        }
    }

    public event MessageBoxWindowResultChangedEventHandler? ResultChanged;

    public MessageBoxWindow() {
        this.InitializeComponent();
    }
    
    static MessageBoxWindow() {
        HeaderMessageProperty.Changed.AddClassHandler<MessageBoxWindow, string?>((o, e) => {
            o.PART_MessageHeader.IsVisible = !string.IsNullOrWhiteSpace(e.GetNewValue<string?>());
        });
    }
}