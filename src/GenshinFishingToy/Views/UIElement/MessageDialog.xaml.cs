using ModernWpf.Controls;
using System.Windows;

namespace GenshinFishingToy.Views;

public partial class MessageDialog : ContentDialog
{
    public string Message
    {
        get => (string)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }
    public static readonly DependencyProperty MessageProperty = DependencyProperty.Register("Message", typeof(string), typeof(MessageDialog), new PropertyMetadata(null!));

    public object MessageContent
    {
        get => GetValue(MessageContentProperty);
        set => SetValue(MessageContentProperty, value);
    }
    public static readonly DependencyProperty MessageContentProperty = DependencyProperty.Register("MessageContent", typeof(object), typeof(MessageDialog), new PropertyMetadata(null!));

    public MessageDialog(string title, string message, object? messageContent = null)
    {
        Message = message;
        MessageContent = messageContent!;
        DataContext = this;
        InitializeComponent();
        Title = title;
    }
}
