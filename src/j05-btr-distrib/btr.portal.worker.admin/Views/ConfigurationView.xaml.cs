using System.Windows;
using System.Windows.Controls;

namespace btr.portal.worker.admin.Views
{
    public partial class ConfigurationView : UserControl
    {
        public ConfigurationView()
        {
            InitializeComponent();
        }
    }

    public class IntervalDisplay : UserControl
    {
        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register(nameof(Label), typeof(string), typeof(IntervalDisplay),
                new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty MinutesProperty =
            DependencyProperty.Register(nameof(Minutes), typeof(int), typeof(IntervalDisplay),
                new PropertyMetadata(0));

        public string Label
        {
            get => (string)GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        public int Minutes
        {
            get => (int)GetValue(MinutesProperty);
            set => SetValue(MinutesProperty, value);
        }

        public IntervalDisplay()
        {
            var stackPanel = new StackPanel { Margin = new Thickness(0, 0, 16, 12) };
            var labelText = new TextBlock { FontWeight = FontWeights.SemiBold, FontSize = 12, Margin = new Thickness(0, 0, 0, 4) };
            var minutesText = new TextBlock { FontSize = 24, FontWeight = FontWeights.Bold };

            labelText.SetBinding(TextBlock.TextProperty, new System.Windows.Data.Binding("Label") { Source = this });
            minutesText.SetBinding(TextBlock.TextProperty, new System.Windows.Data.Binding("Minutes") { Source = this, StringFormat = "{0} min" });

            stackPanel.Children.Add(labelText);
            stackPanel.Children.Add(minutesText);
            Content = stackPanel;
        }
    }
}
