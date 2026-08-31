using System.Windows;

namespace Asteroids
{
    /// <summary>
    /// Interaction logic for WinnerWindow.xaml
    /// </summary>
    public partial class WinnerWindow1 : Window
    {
        public WinnerWindow1(int score, int level, bool highScore)
        {
            InitializeComponent();
			Score.Text = $"You scored {score}";
			if (level > 1) Level.Text = $"You completed level {level-1}";
			if (highScore) High.Text = $"Your best score";
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
