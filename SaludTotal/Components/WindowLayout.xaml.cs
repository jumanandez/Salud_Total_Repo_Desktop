using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SaludTotal.Components
{
    /// <summary>
    /// Lógica de interacción para WindowLayout.xaml
    /// </summary>
    public partial class WindowLayout : UserControl
    {

        public Brush HeaderBackgroundColor
        {
            get { return (Brush)GetValue(HeaderBackgroundColorProperty); }
            set { SetValue(HeaderBackgroundColorProperty, value); }
        }

        public Brush HeaderForegroundColor
        {
            get { return (Brush)GetValue(HeaderForegroundColorProperty); }
            set { SetValue(HeaderForegroundColorProperty, value); }
        }

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty HeaderBackgroundColorProperty =
        DependencyProperty.Register("HeaderBackgroundColor",
        typeof(Brush),
        typeof(WindowLayout),
        new PropertyMetadata(Brushes.Transparent));

        public static readonly DependencyProperty HeaderForegroundColorProperty =
        DependencyProperty.Register("HeaderForegroundColor",
            typeof(Brush),
            typeof(WindowLayout),
            new PropertyMetadata(Brushes.Black));

        public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register("Title",
        typeof(string),
        typeof(WindowLayout),
        new PropertyMetadata("Placeholder"));

        public WindowLayout()
        {
            InitializeComponent();
        }
    }
}
