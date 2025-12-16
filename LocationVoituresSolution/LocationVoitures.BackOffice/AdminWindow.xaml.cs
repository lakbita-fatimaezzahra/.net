using LocationVoitures.BackOffice.Pages;
using LocationVoitures.BackOffice.Utils;
using LocationVoitures.Data.Services;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace LocationVoitures.BackOffice
{
    public partial class AdminWindow : Window
    {
        private readonly MaintenanceService _maintenanceService;
        private readonly DispatcherTimer _verificationTimer;

        public AdminWindow()
        {
            InitializeComponent();

            try
            {
                _maintenanceService = new MaintenanceService();

                // ✅ Charger les informations de l'utilisateur connecté
                LoadUserInfo();

                // Vérification au démarrage
                VerifierMaintenancesAutomatiquement();

                // Configuration du timer pour vérification périodique (toutes les 30 minutes)
                _verificationTimer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromMinutes(30) // Changer selon vos besoins
                };
                _verificationTimer.Tick += VerificationTimer_Tick;
                _verificationTimer.Start();

                MainFrame.Navigate(new DashboardPage());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur d'initialisation : {ex.Message}\n\n{ex.InnerException?.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ✅ NOUVEAU: Charger les informations de l'utilisateur
        private void LoadUserInfo()
        {
            try
            {
                var user = CurrentUser.GetUser();
                if (user != null)
                {
                    TxtUserName.Text = CurrentUser.GetFullName();
                    TxtUserEmail.Text = CurrentUser.GetEmail();
                }
                else
                {
                    TxtUserName.Text = "Non connecté";
                    TxtUserEmail.Text = "";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur chargement info utilisateur : {ex.Message}");
            }
        }

        private void VerificationTimer_Tick(object sender, EventArgs e)
        {
            // Vérification silencieuse en arrière-plan
            try
            {
                _maintenanceService?.VerifierToutesLesVoitures();
                System.Diagnostics.Debug.WriteLine($"[{DateTime.Now:HH:mm:ss}] Vérification automatique des maintenances effectuée");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur vérification automatique : {ex.Message}");
            }
        }

        private void VerifierMaintenancesAutomatiquement()
        {
            try
            {
                if (_maintenanceService == null) return;

                _maintenanceService.VerifierToutesLesVoitures();

                var alerteService = new AlerteService();
                int alertesCritiques = alerteService.CompterAlertesPrioritaires();

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur vérification maintenance : {ex.Message}");
                // Ne pas afficher de MessageBox pour ne pas bloquer le démarrage
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            // Arrêter le timer lors de la fermeture
            _verificationTimer?.Stop();
            base.OnClosed(e);
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

        private void EmployeesButton_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new EmployeesPage());
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