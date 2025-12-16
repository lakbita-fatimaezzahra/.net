using LocationVoitures.Data.Models;
using LocationVoitures.Data.Services;
using System;
using System.Windows;

namespace LocationVoitures.BackOffice
{
    /// <summary>
    /// Fenêtre pour marquer une maintenance comme effectuée
    /// </summary>
    public partial class MarquerMaintenanceEffectueeWindow : Window
    {
        private readonly int _maintenanceId;
        private readonly MaintenanceService _maintenanceService;
        private readonly Maintenance _maintenance;

        public bool MaintenanceEffectuee { get; private set; } = false;

        // ✅ AJOUT : Événement pour notifier les changements
        public event EventHandler MaintenanceMarquee;

        public MarquerMaintenanceEffectueeWindow(Maintenance maintenance)
        {
            InitializeComponent();

            _maintenance = maintenance;
            _maintenanceId = maintenance.Id;
            _maintenanceService = new MaintenanceService();

            ChargerInformations();
        }

        private void ChargerInformations()
        {
            // Afficher les informations de la maintenance
            txtVoiture.Text = $"{_maintenance.Voiture.Marque} {_maintenance.Voiture.Modele} ({_maintenance.Voiture.Matricule})";
            txtTypeMaintenance.Text = _maintenance.TypeMaintenance;
            txtDatePrevue.Text = _maintenance.DateMaintenance.ToString("dd/MM/yyyy");
            txtGarage.Text = _maintenance.Garage ?? "Non spécifié";
            txtCoutEstime.Text = _maintenance.Cout > 0 ? $"{_maintenance.Cout:N2} MAD" : "Non estimé";

            // Pré-remplir la date d'exécution avec aujourd'hui
            dpDateExecution.SelectedDate = DateTime.Now;

            // Pré-remplir le coût réel avec le coût estimé
            if (_maintenance.Cout > 0)
            {
                txtCoutReel.Text = _maintenance.Cout.ToString("N2");
            }
        }

        private void BtnEnregistrer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validation
                if (dpDateExecution.SelectedDate == null)
                {
                    MessageBox.Show("Veuillez sélectionner la date d'exécution.",
                        "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtCoutReel.Text))
                {
                    MessageBox.Show("Veuillez saisir le coût réel de la maintenance.",
                        "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!double.TryParse(txtCoutReel.Text, out double coutReel) || coutReel < 0)
                {
                    MessageBox.Show("Le coût réel doit être un nombre positif.",
                        "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"[MarquerWindow] Appel de MarquerCommeEffectuee pour ID {_maintenanceId}");

                // Marquer comme effectuée
                _maintenanceService.MarquerCommeEffectuee(
                    _maintenanceId,
                    coutReel,
                    txtNotesGarage.Text);

                System.Diagnostics.Debug.WriteLine($"[MarquerWindow] ✅ Maintenance marquée avec succès");

                // ✅ Définir DialogResult AVANT Close
                this.DialogResult = true;
                MaintenanceEffectuee = true;

                this.Close();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MarquerWindow] ❌ Erreur : {ex.Message}");
                MessageBox.Show($"Erreur : {ex.Message}", "Erreur",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void BtnAnnuler_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void txtCoutReel_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            // Permettre uniquement les chiffres et le point décimal
            if (!char.IsDigit(e.Text, 0) && e.Text != "." && e.Text != ",")
            {
                e.Handled = true;
            }
        }
    }
}