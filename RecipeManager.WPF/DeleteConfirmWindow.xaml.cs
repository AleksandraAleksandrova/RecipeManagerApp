using System.Windows;

namespace RecipeManager.WPF
{
    public partial class DeleteConfirmWindow : Window
    {
        public bool IsConfirmed { get; private set; }

        public DeleteConfirmWindow(string message)
        {
            InitializeComponent();
            MessageText.Text = message;
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            IsConfirmed = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            IsConfirmed = false;
            Close();
        }
    }
}
