using LocationVoitures.Data.Services;
using System.Windows;

namespace LocationVoitures.BackOffice
{
    public partial class MainWindow : Window
    {
        private UserService _userService;

        public MainWindow()
        {
            InitializeComponent();
            _userService = new UserService();
        }

        private void BtnLoadUsers_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var utilisateurs = _userService.GetAllUsers();
                dgUsers.ItemsSource = utilisateurs;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Erreur lors du chargement des utilisateurs");
            }
        }

    }
}
