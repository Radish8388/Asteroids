using System.Windows;

namespace Asteroids
{
    /// <summary>
    /// Interaction logic for WinnerWindow.xaml
    /// </summary>
    public partial class WinnerWindow3 : Window
    {
        public WinnerWindow3(int score, int level, bool highScore)
        {
            InitializeComponent();
			Score.Text = $"You scored {score}";
			if (highScore) High.Text = $"Your best score";
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
