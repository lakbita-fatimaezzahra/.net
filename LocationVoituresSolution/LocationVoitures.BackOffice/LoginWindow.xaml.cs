using LocationVoitures.Data.Models;
using LocationVoitures.Data.Services;
using LocationVoitures.BackOffice.Utils;
using System.Windows;

namespace LocationVoitures.BackOffice
{
    public partial class LoginWindow : Window
    {
        private UserService _userService;

        public LoginWindow()
        {
            InitializeComponent();
            _userService = new UserService();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string email = txtEmail.Text;
            string password = txtPassword.Password;

            var user = _userService.Login(email, password);

            if (user != null)
            {
                // ✅ Stocker l'utilisateur connecté
                CurrentUser.SetUser(user);

                MessageBox.Show("Connexion réussie !", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);

                if (user is Employe employeUser && employeUser.Role != null)
                {
                    switch (employeUser.Role.Nom.ToLower())
                    {
                        case "admin":
                            new AdminWindow().Show();
                            break;
                        case "employe":
                        case "employé":
                            new EmployeWindow().Show();
                            break;
                        default:
                            MessageBox.Show("Rôle non reconnu.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                            break;
                    }
                }

                this.Close();
            }
            else
            {
                MessageBox.Show("Email ou mot de passe incorrect.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


    }
}
