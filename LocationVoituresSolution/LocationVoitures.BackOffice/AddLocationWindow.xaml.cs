using LocationVoitures.Data.Models;
using LocationVoitures.Data.Services;
using Microsoft.Win32;
using System;
using System.Linq;
using System.Windows;

namespace LocationVoitures.BackOffice
{
    // Classe pour ComboBox - Client
    public class ClientComboBoxItem
    {
        public int Id { get; set; }
        public string DisplayText { get; set; }
    }

    // Classe pour ComboBox - Voiture
    public class VoitureComboBoxItem
    {
        public int Id { get; set; }
        public Voiture Voiture { get; set; }
        public string DisplayText { get; set; }
    }

    public partial class AddLocationWindow : Window
    {
        private readonly ClientService _clientService;
        private readonly VoitureService _voitureService;
        private readonly LocationService _locationService;

        public AddLocationWindow()
        {
            InitializeComponent();
            _clientService = new ClientService();
            _voitureService = new VoitureService();
            _locationService = new LocationService();

            LoadClients();
            LoadVoitures();

            DpDateDebut.SelectedDate = DateTime.Now;
            DpDateFin.SelectedDate = DateTime.Now.AddDays(1);
        }

        // 📥 Charger les clients
        private void LoadClients()
        {
            try
            {
                var clients = _clientService.GetAllClients()
                    .Select(c => new ClientComboBoxItem
                    {
                        Id = c.Id,
                        DisplayText = $"{c.Nom} {c.Prenom} - {c.CIN}"
                    })
                    .ToList();

                CmbClient.ItemsSource = clients;
                CmbClient.DisplayMemberPath = "DisplayText";
                CmbClient.SelectedValuePath = "Id";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur chargement clients : {ex.Message}", "Erreur",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // 📥 Charger les véhicules
        private void LoadVoitures()
        {
            try
            {
                var toutesVoitures = _voitureService.GetAllVoitures();
                DateTime? dateDebut = DpDateDebut.SelectedDate;
                DateTime? dateFin = DpDateFin.SelectedDate;

                var voitures = toutesVoitures
                    .Where(v => 
                    {
                        // Filtrer les voitures disponibles
                        if (v.Statut == "Loué" || v.Statut == "Maintenance" || v.Statut == "En Maintenance")
                            return false;

                        // Si les dates sont sélectionnées, vérifier la disponibilité
                        if (dateDebut.HasValue && dateFin.HasValue)
                        {
                            return _locationService.IsVehiculeDisponible(v.Id, dateDebut.Value, dateFin.Value);
                        }

                        // Sinon, afficher toutes les voitures disponibles (pas en location/maintenance)
                        return v.Statut == "Disponible";
                    })
                    .Select(v => new VoitureComboBoxItem
                    {
                        Id = v.Id,
                        Voiture = v,
                        DisplayText = $"{v.Marque} {v.Modele} - {v.Matricule} [{v.Statut}]"
                    })
                    .ToList();

                CmbVoiture.ItemsSource = voitures;
                CmbVoiture.DisplayMemberPath = "DisplayText";
                CmbVoiture.SelectedValuePath = "Id";

                // Afficher un message si aucune voiture disponible
                if (!voitures.Any() && dateDebut.HasValue && dateFin.HasValue)
                {
                    TxtVoitureInfo.Text = "⚠️ Aucune voiture disponible pour les dates sélectionnées";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur chargement véhicules : {ex.Message}",
                              "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // 🚗 Sélection d'un véhicule
        private void CmbVoiture_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (CmbVoiture.SelectedItem is VoitureComboBoxItem item && item.Voiture != null)
            {
                var voiture = item.Voiture;
                TxtVoitureInfo.Text = $"📊 Type: {voiture.TypeVoiture?.Nom ?? "N/A"} | 💰 Prix: {voiture.PrixJour:F2} MAD/jour | 🛣️ Kilométrage: {voiture.Kilometrage:F2} km";
                CalculerPrix();
            }
        }

        // 📅 Changement de dates
        private void Dates_Changed(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            CalculerPrix();
            // ✅ Recharger les voitures disponibles selon les nouvelles dates
            LoadVoitures();
        }

        // 💰 Calculer le prix
        private void CalculerPrix()
        {
            if (DpDateDebut.SelectedDate.HasValue &&
                DpDateFin.SelectedDate.HasValue &&
                CmbVoiture.SelectedItem is VoitureComboBoxItem item && item.Voiture != null)
            {
                DateTime dateDebut = DpDateDebut.SelectedDate.Value;
                DateTime dateFin = DpDateFin.SelectedDate.Value;

                if (dateFin <= dateDebut)
                {
                    TxtPrixTotal.Text = "-";
                    TxtPrixDetails.Text = "";
                    PnlDuree.Visibility = Visibility.Collapsed;
                    return;
                }

                int nbJours = (dateFin - dateDebut).Days;
                if (nbJours == 0) nbJours = 1;

                double prixJour = item.Voiture.PrixJour;
                double prixTotal = nbJours * prixJour;

                TxtDuree.Text = $"📅 Durée: {nbJours} jour{(nbJours > 1 ? "s" : "")}";
                PnlDuree.Visibility = Visibility.Visible;

                TxtPrixDetails.Text = $"{nbJours} jour(s) × {prixJour:F2} MAD";
                TxtPrixTotal.Text = $"{prixTotal:F2} MAD";
            }
            else
            {
                TxtPrixTotal.Text = "-";
                TxtPrixDetails.Text = "";
                PnlDuree.Visibility = Visibility.Collapsed;
            }
        }

        // ➕ Nouveau client
        private void BtnNouveauClient_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Fonctionnalité à implémenter : ouvrir la fenêtre d'ajout de client",
                "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            LoadClients();
        }

        // ✅ Créer la location
        private void BtnCreer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validation
                if (CmbClient.SelectedValue == null)
                {
                    MessageBox.Show("Veuillez sélectionner un client.", "Validation",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (CmbVoiture.SelectedValue == null)
                {
                    MessageBox.Show("Veuillez sélectionner un véhicule.", "Validation",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!DpDateDebut.SelectedDate.HasValue || !DpDateFin.SelectedDate.HasValue)
                {
                    MessageBox.Show("Veuillez sélectionner les dates.", "Validation",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                DateTime dateDebut = DpDateDebut.SelectedDate.Value;
                DateTime dateFin = DpDateFin.SelectedDate.Value;

                if (dateFin <= dateDebut)
                {
                    MessageBox.Show("La date de fin doit être après la date de début.", "Validation",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                int clientId = (int)CmbClient.SelectedValue;
                int voitureId = (int)CmbVoiture.SelectedValue;

                var location = _locationService.CreerLocation(clientId, voitureId, dateDebut, dateFin);

                // Email
                if (ChkEnvoiEmail.IsChecked == true)
                {
                    // À implémenter
                }

                // PDF
                if (ChkGenererPdf.IsChecked == true)
                {
                    try
                    {
                        SaveFileDialog saveDialog = new SaveFileDialog
                        {
                            Filter = "Fichier PDF (*.pdf)|*.pdf",
                            FileName = $"Contrat_Location_{location.Id}.pdf"
                        };

                        if (saveDialog.ShowDialog() == true)
                        {
                            QRCodePdfService.GenerateLocationPdf(location, saveDialog.FileName);

                            // ✅ CORRECTION : Ouvrir le PDF correctement
                            try
                            {
                                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                                {
                                    FileName = saveDialog.FileName,
                                    UseShellExecute = true
                                });
                            }
                            catch
                            {
                                MessageBox.Show($"PDF généré avec succès !\n\nEmplacement : {saveDialog.FileName}",
                                    "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Location créée mais erreur de génération PDF : {ex.Message}",
                            "Avertissement", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }

                MessageBox.Show($"Location n°{location.Id} créée avec succès !", "Succès",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur : {ex.Message}", "Erreur",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ❌ Annuler
        private void BtnAnnuler_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}