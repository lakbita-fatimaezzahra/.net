using LocationVoitures.BackOffice.Utils;
using LocationVoitures.Data.Models;
using LocationVoitures.Data.Services;
using Microsoft.Win32;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using static QRCoder.PayloadGenerator;

namespace LocationVoitures.BackOffice
{
    // Classes pour ComboBox
    public class ClientComboBoxItem
    {
        public int Id { get; set; }
        public Client? Client { get; set; }
        public string DisplayText { get; set; } = string.Empty;
    }

    public class VoitureComboBoxItem
    {
        public int Id { get; set; }
        public Voiture? Voiture { get; set; }
        public string DisplayText { get; set; } = string.Empty;
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

            // Initialiser les placeholders
            InitializePlaceholders();

            LoadClients();
            LoadVoitures();

            // Dates par défaut
            DpDateDebut.SelectedDate = DateTime.Now;
            DpDateFin.SelectedDate = DateTime.Now.AddDays(1);
        }

        // ===== INITIALISATION =====
        private void InitializePlaceholders()
        {
            TxtSearchClient.Text = "🔍 Rechercher par nom, prénom, CIN ou téléphone...";
            TxtSearchClient.Foreground = Brushes.Gray;

            TxtSearchVoiture.Text = "🔍 Rechercher par marque, modèle ou matricule...";
            TxtSearchVoiture.Foreground = Brushes.Gray;
        }

        // ===== CHARGEMENT DES DONNÉES =====
        private void LoadClients()
        {
            try
            {
                var clients = _clientService.GetAllClients()
                    .Select(c => new ClientComboBoxItem
                    {
                        Id = c.Id,
                        Client = c,
                        DisplayText = $"{c.Nom} {c.Prenom} - {c.CIN ?? "Sans CIN"}"
                    })
                    .ToList();

                CmbClient.ItemsSource = clients;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur chargement clients : {ex.Message}", "Erreur",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

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

                        // Sinon, afficher toutes les voitures disponibles
                        return v.Statut == "Disponible";
                    })
                    .Select(v => new VoitureComboBoxItem
                    {
                        Id = v.Id,
                        Voiture = v,
                        DisplayText = $"{v.Marque} {v.Modele} - {v.Matricule} ({v.PrixJour:F2} MAD/j)"
                    })
                    .ToList();

                CmbVoiture.ItemsSource = voitures;

                // Message si aucune voiture disponible
                if (!voitures.Any() && dateDebut.HasValue && dateFin.HasValue)
                {
                    TxtVoitureInfo.Text = "⚠️ Aucune voiture disponible pour ces dates";
                    PnlVoitureInfo.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur chargement véhicules : {ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ===== RECHERCHE CLIENT =====
        private void TxtSearchClient_GotFocus(object sender, RoutedEventArgs e)
        {
            if (TxtSearchClient.Text == "🔍 Rechercher par nom, prénom, CIN ou téléphone...")
            {
                TxtSearchClient.Text = "";
                TxtSearchClient.Foreground = Brushes.Black;
            }
        }

        private void TxtSearchClient_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtSearchClient.Text))
            {
                TxtSearchClient.Text = "🔍 Rechercher par nom, prénom, CIN ou téléphone...";
                TxtSearchClient.Foreground = Brushes.Gray;
            }
        }

        private void TxtSearchClient_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Ignorer si c'est le placeholder
            if (TxtSearchClient.Text == "🔍 Rechercher par nom, prénom, CIN ou téléphone...")
                return;

            try
            {
                string search = TxtSearchClient.Text.Trim().ToLower();

                var clients = _clientService.GetAllClients()
                    .Where(c => string.IsNullOrWhiteSpace(search) ||
                               c.Nom.ToLower().Contains(search) ||
                               c.Prenom.ToLower().Contains(search) ||
                               (c.CIN != null && c.CIN.ToLower().Contains(search)) ||
                               (c.Telephone != null && c.Telephone.Contains(search)))
                    .Select(c => new ClientComboBoxItem
                    {
                        Id = c.Id,
                        Client = c,
                        DisplayText = $"{c.Nom} {c.Prenom} - {c.CIN ?? "Sans CIN"}"
                    })
                    .ToList();

                CmbClient.ItemsSource = clients;

                if (clients.Count > 0)
                    CmbClient.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur recherche : {ex.Message}", "Erreur",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CmbClient_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CmbClient.SelectedItem is ClientComboBoxItem item && item.Client != null)
            {
                var client = item.Client;

                TxtClientInfo.Text = $"📧 {client.Email}\n" +
                                    $"📞 {client.Telephone ?? "Non renseigné"}\n" +
                                    $"🏠 {client.Adresse ?? "Non renseignée"}";

                PnlClientInfo.Visibility = Visibility.Visible;
            }
            else
            {
                PnlClientInfo.Visibility = Visibility.Collapsed;
            }
        }

        // ===== RECHERCHE VOITURE =====
        private void TxtSearchVoiture_GotFocus(object sender, RoutedEventArgs e)
        {
            if (TxtSearchVoiture.Text == "🔍 Rechercher par marque, modèle ou matricule...")
            {
                TxtSearchVoiture.Text = "";
                TxtSearchVoiture.Foreground = Brushes.Black;
            }
        }

        private void TxtSearchVoiture_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtSearchVoiture.Text))
            {
                TxtSearchVoiture.Text = "🔍 Rechercher par marque, modèle ou matricule...";
                TxtSearchVoiture.Foreground = Brushes.Gray;
            }
        }

        private void TxtSearchVoiture_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Ignorer si c'est le placeholder
            if (TxtSearchVoiture.Text == "🔍 Rechercher par marque, modèle ou matricule...")
                return;

            try
            {
                string search = TxtSearchVoiture.Text.Trim().ToLower();
                DateTime? dateDebut = DpDateDebut.SelectedDate;
                DateTime? dateFin = DpDateFin.SelectedDate;

                var voitures = _voitureService.GetAllVoitures()
                    .Where(v =>
                    {
                        // Filtrer les voitures disponibles
                        if (v.Statut != "Disponible") return false;

                        // Recherche textuelle
                        bool matchSearch = string.IsNullOrWhiteSpace(search) ||
                                         v.Marque.ToLower().Contains(search) ||
                                         v.Modele.ToLower().Contains(search) ||
                                         v.Matricule.ToLower().Contains(search);

                        if (!matchSearch) return false;

                        // Vérifier la disponibilité selon les dates
                        if (dateDebut.HasValue && dateFin.HasValue)
                        {
                            return _locationService.IsVehiculeDisponible(v.Id, dateDebut.Value, dateFin.Value);
                        }

                        return true;
                    })
                    .Select(v => new VoitureComboBoxItem
                    {
                        Id = v.Id,
                        Voiture = v,
                        DisplayText = $"{v.Marque} {v.Modele} - {v.Matricule} ({v.PrixJour:F2} MAD/j)"
                    })
                    .ToList();

                CmbVoiture.ItemsSource = voitures;

                if (voitures.Count > 0)
                    CmbVoiture.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur recherche : {ex.Message}", "Erreur",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CmbVoiture_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CmbVoiture.SelectedItem is VoitureComboBoxItem item && item.Voiture != null)
            {
                var voiture = item.Voiture;

                TxtVoitureInfo.Text = $"🚗 Type: {voiture.TypeVoiture?.Nom ?? "N/A"}\n" +
                                     $"📊 Kilométrage: {voiture.Kilometrage:N0} km\n" +
                                     $"💰 Prix: {voiture.PrixJour:F2} MAD/jour";

                PnlVoitureInfo.Visibility = Visibility.Visible;
                CalculerPrix();
            }
            else
            {
                PnlVoitureInfo.Visibility = Visibility.Collapsed;
            }
        }

        // ===== DATES ET PRIX =====
        private void Dates_Changed(object sender, SelectionChangedEventArgs e)
        {
            CalculerPrix();
            LoadVoitures(); // Recharger les voitures disponibles
        }

        private void BtnCalculerPrix_Click(object sender, RoutedEventArgs e)
        {
            CalculerPrix();
        }

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
                    TxtPrixTotal.Text = "0.00 MAD";
                    TxtPrixDetails.Text = "⚠️ Date de fin invalide";
                    PnlDuree.Visibility = Visibility.Collapsed;
                    return;
                }

                int nbJours = (dateFin - dateDebut).Days;
                if (nbJours == 0) nbJours = 1;

                double prixJour = item.Voiture.PrixJour;
                double sousTotal = nbJours * prixJour;

                // Réductions
                double reduction = 0;
                string detailReduction = "";

                if (nbJours >= 30)
                {
                    reduction = sousTotal * 0.15;
                    detailReduction = "\n💰 Réduction longue durée (15%)";
                }
                else if (nbJours >= 7)
                {
                    reduction = sousTotal * 0.10;
                    detailReduction = "\n💰 Réduction semaine (10%)";
                }

                double total = sousTotal - reduction;

                // Affichage
                TxtDuree.Text = $"📅 Durée: {nbJours} jour{(nbJours > 1 ? "s" : "")}";
                PnlDuree.Visibility = Visibility.Visible;

                TxtPrixDetails.Text = $"💵 Prix journalier: {prixJour:F2} MAD\n" +
                                     $"📊 Sous-total: {sousTotal:F2} MAD" +
                                     detailReduction +
                                     (reduction > 0 ? $"\n✅ Économie: -{reduction:F2} MAD" : "");

                TxtPrixTotal.Text = $"{total:F2} MAD";
            }
            else
            {
                TxtPrixTotal.Text = "-";
                TxtPrixDetails.Text = "";
                PnlDuree.Visibility = Visibility.Collapsed;
            }
        }

        // ===== ACTIONS =====
        private void BtnNouveauClient_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Utilisez la section 'Gestion des Clients' pour ajouter un nouveau client.",
                "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            LoadClients();
        }

        private void BtnAjouterLocation_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validations
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

                // Créer la location
                var location = _locationService.CreerLocation(clientId, voitureId, dateDebut, dateFin);

                    // Options
                    if (ChkEnvoiEmail.IsChecked == true)
                    {
                        try
                        {
                            // Récupérer le client et la voiture
                            var client = ((ClientComboBoxItem)CmbClient.SelectedItem).Client;
                            var voiture = ((VoitureComboBoxItem)CmbVoiture.SelectedItem).Voiture;

                            if (client != null && voiture != null && !string.IsNullOrWhiteSpace(client.Email))
                            {
                                EmailHelper.SendReservationConfirmation(
                                    client.Email,                                 // Email client
                                    $"{client.Prenom} {client.Nom}",             // Nom client
                                    voiture.Marque,                               // Marque
                                    voiture.Modele,                               // Modèle
                                    location.DateDebut,                           // Date début
                                    location.DateFin,                             // Date fin
                                    location.PrixTotal,                           // Prix total
                                    location.Id.ToString()                        // Numéro réservation
                                );
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(
                                $"⚠️ La location a été créée, mais l'email n'a pas pu être envoyé.\n\nDétails : {ex.Message}",
                                "Avertissement",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning
                            );
                        }
                    }

                

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
                                MessageBox.Show($"PDF généré : {saveDialog.FileName}",
                                    "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Erreur PDF : {ex.Message}",
                            "Avertissement", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }

                MessageBox.Show($"✅ Location n°{location.Id} créée avec succès !", "Succès",
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

        private void BtnAnnuler_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}