using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SaludTotal.Components
{
    public enum MessageType
    {
        Error,
        Warning,
        Info,
        Success
    }
    public partial class MessageControl : UserControl
    {
        public static readonly DependencyProperty HasMessageProperty =
        DependencyProperty.Register("HasMessage", typeof(bool), typeof(MessageControl),
            new PropertyMetadata(false));

        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register("Message", typeof(string), typeof(MessageControl),
                new PropertyMetadata("Message"));

        public static readonly DependencyProperty MessageTypeProperty =
            DependencyProperty.Register("MessageType", typeof(MessageType), typeof(MessageControl),
                new PropertyMetadata(MessageType.Info));

        public static readonly DependencyProperty ClearMessageProperty =
            DependencyProperty.Register(nameof(ClearMessage), typeof(ICommand), typeof(MessageControl),
                new PropertyMetadata(null));

        public bool HasMessage
        {
            get { return (bool)GetValue(HasMessageProperty); }
            set { SetValue(HasMessageProperty, value); }
        }

        public string Message
        {
            get { return (string)GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }

        public MessageType MessageType
        {
            get { return (MessageType)GetValue(MessageTypeProperty); }
            set { SetValue(MessageTypeProperty, value); }
        }

        public ICommand ClearMessage
        {
            get => (ICommand)GetValue(ClearMessageProperty);
            set => SetValue(ClearMessageProperty, value);
        }

        public MessageControl()
        {
            InitializeComponent();
        }
    }
}
