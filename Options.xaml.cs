using System.Windows;

namespace Asteroids
{
    /// <summary>
    /// Interaction logic for Options.xaml
    /// </summary>
    public partial class Options : Window
    {
        public Options()
        {
            InitializeComponent();
            SoundOn.IsChecked = Properties.Settings.Default.SoundOn;
            SaucerAttack.IsChecked = Properties.Settings.Default.SaucerAttack;
            InvincibleShip.IsChecked = Properties.Settings.Default.InvincibleShip;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.SoundOn = (SoundOn.IsChecked == true);
            Properties.Settings.Default.SaucerAttack = (SaucerAttack.IsChecked == true);
            Properties.Settings.Default.InvincibleShip = (InvincibleShip.IsChecked == true);
            Properties.Settings.Default.Save();
            DialogResult = true;
        }
    }
}
