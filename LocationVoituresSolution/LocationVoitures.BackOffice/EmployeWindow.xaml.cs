using LocationVoitures.BackOffice.Pages;
using LocationVoitures.BackOffice.Utils;
using System;
using System.Windows;
using System.Windows.Controls;


namespace LocationVoitures.BackOffice
{
    /// <summary>
    /// Logique d'interaction pour EmployeWindow.xaml
    /// </summary>
    public partial class EmployeWindow : Window



    {
        public EmployeWindow()
        {
            InitializeComponent();
            // Affiche le dashboard par défaut
            MainFrame.Navigate(new DashboardPage());
        }
        private void Expander_Expanded(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            Expander exp = sender as Expander;

            if (exp != VehiclesExpander) VehiclesExpander.IsExpanded = false;

        }
        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Voulez-vous vraiment vous déconnecter ?",
                "Déconnexion",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // ✅ Déconnecter l'utilisateur
                CurrentUser.Logout();
                LoginWindow login = new LoginWindow();
                login.Show();
                this.Close();
               
            }
        }

        // ✅ NOUVEAU: Clic sur l'Ellipse (avatar) pour afficher le profil
        private void EllipseProfil_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            MainFrame.Navigate(new ProfilPage());
        }

        // ✅ NOUVEAU: Clic sur le nom pour afficher le profil
        private void TxtUserName_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            MainFrame.Navigate(new ProfilPage());
        }

        // ✅ NOUVEAU: Clic sur l'email pour afficher le profil
        private void TxtUserEmail_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            MainFrame.Navigate(new ProfilPage());
        }

        private void DashboardButton_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new DashboardPage());
        }

      
        private void ClientsButton_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new ClientsPage());
        }

        private void VehiclesButton_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new VoitureListPage());
        }

        private void Type_voitureButton_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new TypeVoiturePage());
        }

        private void LocationButton_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new LocationListPage());
        }

        private void AlertesButton_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new AlertesPage());
        }

        private void MaintenancesButton_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new PageMaintenances());
        }

    }

}