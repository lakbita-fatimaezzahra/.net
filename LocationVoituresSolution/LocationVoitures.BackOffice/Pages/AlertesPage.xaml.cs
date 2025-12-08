using LocationVoitures.Data.Services;
using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using LocationVoitures.BackOffice;
namespace LocationVoitures.BackOffice.Pages
{
    public partial class AlertesPage : Page
    {
        private readonly AlerteService _alerteService;
        private readonly MaintenanceService _maintenanceService;

        public AlertesPage()
        {
            InitializeComponent();
            _alerteService = new AlerteService();
            _maintenanceService = new MaintenanceService();
            ChargerAlertes();
        }

        private void ChargerAlertes()
        {
            // ✅ Vider la grille avant de recharger
            dgAlertes.ItemsSource = null;
            
            var alertes = _alerteService.GetAllAlertes();
            dgAlertes.ItemsSource = alertes;

            // Mettre à jour les statistiques par urgence
            var stats = _alerteService.CompterAlertesParUrgence();

            txtCritique.Text = stats["Critique"].ToString();
            txtUrgent.Text = stats["Urgent"].ToString();
            txtAttention.Text = stats["Attention"].ToString();
            txtAPrevoir.Text = stats["À Prévoir"].ToString();

            var alertesResolues = alertes.Count(a => a.Statut == "Résolue");
            txtResolues.Text = alertesResolues.ToString();
        }

        private void Filtre_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbFiltre.SelectedIndex == -1) return;

            var filtre = (cmbFiltre.SelectedItem as ComboBoxItem)?.Content.ToString();
            if (_alerteService == null)
            {
                return;
            }
            
            // ✅ Vider la grille avant d'appliquer le filtre
            dgAlertes.ItemsSource = null;
            
            var alertes = _alerteService.GetAllAlertes();

            switch (filtre)
            {
                case "🔴 Critiques":
                    dgAlertes.ItemsSource = alertes.Where(a => a.Message.Contains("🔴 CRITIQUE")).ToList();
                    break;
                case "🔴 Urgentes":
                    dgAlertes.ItemsSource = alertes.Where(a => a.Message.Contains("🔴 URGENT")).ToList();
                    break;
                case "🟠 Attention":
                    dgAlertes.ItemsSource = alertes.Where(a => a.Message.Contains("🟠 ATTENTION")).ToList();
                    break;
                case "🟡 À prévoir":
                    dgAlertes.ItemsSource = alertes.Where(a => a.Message.Contains("🟡 À PRÉVOIR")).ToList();
                    break;
                case "✓ Résolues":
                    dgAlertes.ItemsSource = alertes.Where(a => a.Statut == "Résolue").ToList();
                    break;
                default:
                    dgAlertes.ItemsSource = alertes;
                    break;
            }
        }

        private void Resoudre_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag != null && int.TryParse(button.Tag.ToString(), out int alerteId))
            {
                var alerte = _alerteService.GetAllAlertes().FirstOrDefault(a => a.Id == alerteId);
                if (alerte == null) return;

                // 🔥 Ouvrir popup planification
                var popup = new PlanifierMaintenanceWindow(alerte.Voiture);

                var dialogResult = popup.ShowDialog();

                // Résoudre seulement après planification si la fenêtre a été fermée avec succès
                if (dialogResult == true || popup.MaintenanceCreee)
                {
                    // Résoudre l'alerte
                    _alerteService.MarquerCommeResolue(alerteId);

                    MessageBox.Show("Alerte résolue et maintenance planifiée !",
                                    "Succès",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);

                    // ✅ Vider la grille puis recharger
                    dgAlertes.ItemsSource = null;
                    ChargerAlertes();
                }
            }
        }


        // ✅ CORRIGÉ: Nom de la méthode (enlevé l'espace)
        private void Details_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag != null && int.TryParse(button.Tag.ToString(), out int voitureId))
            {
                // Afficher les détails de la voiture et ses maintenances
                var maintenances = _maintenanceService.GetMaintenancesByVoiture(voitureId);
                var (_, urgence, message) = _maintenanceService.VerifierMaintenanceAvecUrgence(voitureId);

                string details = $"Niveau d'urgence: {urgence}\n\n{message}\n\n";
                details += $"Historique des maintenances ({maintenances.Count}):\n";

                foreach (var m in maintenances.Take(5))
                {
                    details += $"- {m.TypeMaintenance}: {m.DateMaintenance:dd/MM/yyyy} ({m.KilometrageMaintenance:N0} km)\n";
                }

                MessageBox.Show(details, "Détails de la maintenance", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void Supprimer_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag != null && int.TryParse(button.Tag.ToString(), out int id))
            {
                var result = MessageBox.Show(
                    "Voulez-vous vraiment supprimer cette alerte ?",
                    "Confirmation",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    _alerteService.Delete(id);
                    MessageBox.Show("Alerte supprimée !", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    // ✅ Vider la grille puis recharger
                    dgAlertes.ItemsSource = null;
                    ChargerAlertes();
                }
            }
        }
       

        private void SupprimerResolues_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Voulez-vous supprimer toutes les alertes résolues ?",
                "Confirmation",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                var alertesResolues = _alerteService.GetAllAlertes().Where(a => a.Statut == "Résolue").ToList();

                foreach (var alerte in alertesResolues)
                {
                    _alerteService.Delete(alerte.Id);
                }

                MessageBox.Show($"{alertesResolues.Count} alerte(s) supprimée(s) !", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
                
                // ✅ Vider la grille puis recharger
                dgAlertes.ItemsSource = null;
                ChargerAlertes();
            }
        }
    }

    /// <summary>
    /// Convertisseur pour masquer le bouton "Résoudre" si l'alerte est déjà résolue
    /// </summary>
    public class StatutAlerteToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string statut)
            {
                // Masquer le bouton si l'alerte est résolue
                return statut == "Résolue" ? Visibility.Collapsed : Visibility.Visible;
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}