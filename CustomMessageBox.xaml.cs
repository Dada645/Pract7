using System.Windows;

namespace Curs
{
    public partial class CustomMessageBox : Window
    {
        public CustomMessageBox(string message)
        {
            InitializeComponent();
            MessageText.Text = message;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public static void Show(string message)
        {
            CustomMessageBox box = new CustomMessageBox(message);
            box.ShowDialog();
        }
    }
}
