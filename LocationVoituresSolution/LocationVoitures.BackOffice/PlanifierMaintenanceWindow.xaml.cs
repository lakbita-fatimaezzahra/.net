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
        private readonly Voiture _voiture;

        public bool MaintenanceCreee { get; private set; } = false;

        public PlanifierMaintenanceWindow(Voiture voiture)
        {
            InitializeComponent();

            if (voiture == null)
            {
                MessageBox.Show("Aucune voiture sélectionnée.", "Erreur",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
                return;
            }

            _voiture = voiture;
            _voitureId = voiture.Id;
            _maintenanceService = new MaintenanceService();

            RemplirChamps();
        }

        private void RemplirChamps()
        {
            try
            {
                // Type par défaut
                if (cmbType.Items.Count > 0)
                {
                    cmbType.SelectedIndex = 0;
                }

                // Date par défaut (aujourd'hui)
                dpDateMaintenance.SelectedDate = DateTime.Now;

                // ✅ Kilométrage - conversion simple
                txtKmMaintenance.Text = _voiture.Kilometrage.ToString("F0");

                // ✅ Garage - construction simple
                string marqueValue = _voiture.Marque;
                if (string.IsNullOrEmpty(marqueValue))
                {
                    marqueValue = "Inconnu";
                }
                txtGarage.Text = "Garage " + marqueValue;

                // ✅ Description - construction simple
                string matriculeValue = _voiture.Matricule;
                string modeleValue = _voiture.Modele;

                if (string.IsNullOrEmpty(matriculeValue))
                {
                    matriculeValue = "";
                }
                if (string.IsNullOrEmpty(modeleValue))
                {
                    modeleValue = "";
                }
                if (string.IsNullOrEmpty(marqueValue))
                {
                    marqueValue = "";
                }

                txtDescription.Text = "Maintenance pour " + matriculeValue + " - " + marqueValue + " " + modeleValue;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors du remplissage des champs : " + ex.Message,
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validation des champs obligatoires
                if (cmbType.SelectedItem == null)
                {
                    MessageBox.Show("Veuillez sélectionner un type de maintenance.", "Erreur",
                                    MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (dpDateMaintenance.SelectedDate == null)
                {
                    MessageBox.Show("Veuillez sélectionner une date.", "Erreur",
                                    MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtKmMaintenance.Text))
                {
                    MessageBox.Show("Veuillez entrer le kilométrage.", "Erreur",
                                    MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Validation du kilométrage
                if (!double.TryParse(txtKmMaintenance.Text, out double km))
                {
                    MessageBox.Show("Veuillez entrer un kilométrage valide.", "Erreur",
                                    MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Limiter le kilométrage pour éviter les erreurs SQL
                if (km < 0 || km > 2000000000)
                {
                    MessageBox.Show("Le kilométrage doit être entre 0 et 2,000,000,000.", "Erreur",
                                    MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Récupérer la date
                DateTime dateMaintenance = dpDateMaintenance.SelectedDate.Value;

                // Vérifier que la date est valide pour SQL Server
                if (dateMaintenance < new DateTime(1900, 1, 1))
                {
                    dateMaintenance = DateTime.Now;
                }

                // ✅ Déterminer le statut selon la date
                string statut = "Planifiée";
                if (dateMaintenance.Date < DateTime.Now.Date)
                {
                    statut = "En retard";
                }
                else if (dateMaintenance.Date == DateTime.Now.Date)
                {
                    statut = "En cours";
                }

                // ✅ Récupérer le type de maintenance
                ComboBoxItem selectedItem = cmbType.SelectedItem as ComboBoxItem;
                string typeMaintenance = "Maintenance";
                if (selectedItem != null && selectedItem.Content != null)
                {
                    typeMaintenance = selectedItem.Content.ToString();
                }

                // ✅ Garage et description
                string garageValue = txtGarage.Text;
                if (string.IsNullOrWhiteSpace(garageValue))
                {
                    garageValue = "Garage non spécifié";
                }
                else
                {
                    garageValue = garageValue.Trim();
                }

                string descriptionValue = txtDescription.Text;
                if (string.IsNullOrWhiteSpace(descriptionValue))
                {
                    descriptionValue = "";
                }
                else
                {
                    descriptionValue = descriptionValue.Trim();
                }

                // ✅ Créer l'objet maintenance SANS calcul automatique
                var maintenance = new Maintenance
                {
                    VoitureId = _voitureId,
                    TypeMaintenance = typeMaintenance,
                    DateMaintenance = dateMaintenance,
                    KilometrageMaintenance = km,
                    Garage = garageValue,
                    Description = descriptionValue,
                    Statut = statut,
                    DateCreation = DateTime.Now,
                    Cout = 0,
                    DateProchaineMaintenance = dateMaintenance, // ✅ Même date
                    KilometrageProchain = km, // ✅ Même kilométrage
                    Priorite = "Moyenne"
                };

               
                _maintenanceService.CreateMaintenance(maintenance);

                

                // Message de succès
                string message = "✅ Maintenance planifiée avec succès!";
                if (statut == "En retard")
                {
                    message = "⚠️ Maintenance planifiée avec statut 'En retard' (date dans le passé)!";
                }
                else if (statut == "En cours")
                {
                    message = "✅ Maintenance planifiée pour aujourd'hui !";
                }
                this.DialogResult = true;

                MessageBox.Show(message, "Succès",
                                MessageBoxButton.OK, MessageBoxImage.Information);

                MaintenanceCreee = true;
                
                this.Close();
            }
            catch (Exception ex)
            {
                string errorMsg = "Erreur lors de l'enregistrement :\n\n" + ex.Message;
                if (ex.InnerException != null)
                {
                    errorMsg = errorMsg + "\n\nDétails : " + ex.InnerException.Message;
                }
                MessageBox.Show(errorMsg, "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnAnnuler_Click(object sender, RoutedEventArgs e)
        {
            // Demander confirmation avant de fermer si des données ont été saisies
            bool hasData = false;

            if (!string.IsNullOrWhiteSpace(txtDescription.Text))
            {
                hasData = true;
            }

            if (!string.IsNullOrWhiteSpace(txtGarage.Text))
            {
                hasData = true;
            }

            if (cmbType.SelectedIndex > 0)
            {
                hasData = true;
            }

            if (hasData)
            {
                var result = MessageBox.Show(
                    "Voulez-vous vraiment annuler ?\nLes données saisies seront perdues.",
                    "Confirmation",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.No)
                {
                    return;
                }
            }

            MaintenanceCreee = false;
            this.DialogResult = false;
            this.Close();
        }
    }
}