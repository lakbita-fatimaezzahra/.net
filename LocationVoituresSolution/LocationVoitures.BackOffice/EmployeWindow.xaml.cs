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
            // Évite la propagation vers les enfants
            e.Handled = true;

            Expander exp = sender as Expander;

            // Fermer les autres expanders
            if (exp != ClientsExpander) ClientsExpander.IsExpanded = false;
            if (exp != VehiclesExpander) VehiclesExpander.IsExpanded = false;
            if (exp != RentalsExpander) RentalsExpander.IsExpanded = false;
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
                
                this.Close();
                LoginWindow login = new LoginWindow();
                login.Show();
            }
        }

        // ✅ NOUVEAU: Bouton pour accéder au profil (si ajouté dans XAML)
        private void ProfilButton_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new ProfilPage());
        }
        // Boutons navigation
        private void DashboardButton_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new DashboardPage());

        }

        private void ClientsButton_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new ClientsPage());
        }
        /* private void QRCodeButton_Click(object sender, RoutedEventArgs e)
         {
             MainFrame.Navigate(new QRCodePage());
         }
        */
        /* private void EmailButton_Click(object sender, RoutedEventArgs e)
         {
             MainFrame.Navigate(new EmailNotificationPage());
         }*/
        private void VehiclesButton_Click(object sender, RoutedEventArgs e)

        {
            try
            {
                MainFrame.Navigate(new VoitureListPage());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur : " + ex.Message);
            }
        }
    }

    /* private void TarifsButton_Click(object sender, RoutedEventArgs e)
     {
         MainFrame.Navigate(new TarifsPage());
     }

     private void AvailabilityButton_Click(object sender, RoutedEventArgs e)
     {
         MainFrame.Navigate(new AvailabilityPage());
     }

     private void RentalsButton_Click(object sender, RoutedEventArgs e)
     {
         MainFrame.Navigate(new RentalsPage());
     }

     private void PaymentsButton_Click(object sender, RoutedEventArgs e)
     {
         MainFrame.Navigate(new PaymentsPage());
     }

     private void PDFButton_Click(object sender, RoutedEventArgs e)
     {
         MainFrame.Navigate(new PDFPage());
     }    */


}

