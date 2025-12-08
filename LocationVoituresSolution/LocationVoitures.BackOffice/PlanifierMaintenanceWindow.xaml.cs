using LocationVoitures.Data.Models;
using LocationVoitures.Data.Services;
using System;
using System.Windows;
using System.Windows.Controls;

namespace LocationVoitures.BackOffice
{
    public partial class PlanifierMaintenanceWindow : Window
    {
        private readonly int _voitureId;
        private readonly MaintenanceService _maintenanceService;

        public bool MaintenanceCreee { get; private set; } = false;

        private readonly Voiture _voiture;

        public PlanifierMaintenanceWindow(Voiture voiture)
        {
            InitializeComponent();
            _voiture = voiture;
            _voitureId = voiture.Id;
            _maintenanceService = new MaintenanceService();

            RemplirChamps();   // 👈 nouvelle méthode
        }
        private void RemplirChamps()
        {
            // Type par défaut
            cmbType.SelectedIndex = 0;

            // Date par défaut
            dpDateMaintenance.SelectedDate = DateTime.Now;

            // Kilométrage actuel
            txtKmMaintenance.Text = _voiture.Kilometrage.ToString();

            // Garage par défaut (tu peux changer)
            txtGarage.Text = "Garage " + _voiture.Marque;

            // Description automatique
            txtDescription.Text =
                $"Maintenance pour {_voiture.Matricule} - {_voiture.Marque} {_voiture.Modele}";
        }


        private void Enregistrer_Click(object sender, RoutedEventArgs e)
        {
            var maintenance = new Maintenance
            {
                VoitureId = _voitureId,
                TypeMaintenance = ((ComboBoxItem)cmbType.SelectedItem).Content.ToString(),
                DateMaintenance = dpDateMaintenance.SelectedDate.Value,
                DateCreation = DateTime.Now,
                Description = txtDescription.Text,
                Statut = "Planifiée"
            };

            _maintenanceService.CreateMaintenance(maintenance);
            MaintenanceCreee = true;

            MessageBox.Show("Maintenance planifiée avec succès", "Succès",
                MessageBoxButton.OK, MessageBoxImage.Information);

            Close();
        }
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (cmbType.SelectedItem == null || dpDateMaintenance.SelectedDate == null)
            {
                MessageBox.Show("Veuillez remplir tous les champs obligatoires.", "Erreur",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!double.TryParse(txtKmMaintenance.Text, out double km))
            {
                MessageBox.Show("Veuillez entrer un kilométrage valide.", "Erreur",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Limiter le kilométrage pour éviter les erreurs SQL
            if (km > 2000000000)
            {
                MessageBox.Show("Kilométrage trop élevé pour la base de données.", "Erreur",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DateTime dateMaintenance = dpDateMaintenance.SelectedDate.Value < new DateTime(1900, 1, 1)
                ? DateTime.Now
                : dpDateMaintenance.SelectedDate.Value;

            // ✅ Déterminer le statut selon la date
            string statut = "Planifiée";
            if (dateMaintenance < DateTime.Now)
            {
                statut = "En retard"; // Si la date est dans le passé, c'est en retard
            }

            var maintenance = new Maintenance
            {
                VoitureId = _voitureId,
                TypeMaintenance = (cmbType.SelectedItem as ComboBoxItem).Content.ToString(),
                DateMaintenance = dateMaintenance,
                KilometrageMaintenance = km,
                Garage = txtGarage.Text,
                Description = txtDescription.Text,
                Statut = statut, // ✅ Utiliser le statut déterminé
                DateCreation = DateTime.Now
            };

            maintenance.DateProchaineMaintenance = maintenance.DateMaintenance.AddMonths(6);
            maintenance.KilometrageProchain = maintenance.KilometrageMaintenance + 10000;

            try
            {
                _maintenanceService.Add(maintenance);
                
                // ✅ Vérifier et créer des alertes pour les maintenances planifiées
                _maintenanceService.VerifierMaintenancesPlanifiees();
                
                string message = statut == "En retard" 
                    ? "⚠️ Maintenance planifiée avec statut 'En retard' (date dans le passé)!" 
                    : "Maintenance planifiée avec succès!";
                    
                MessageBox.Show(message, "Succès",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                MaintenanceCreee = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors de l'enregistrement : " + ex.Message,
                                "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}
