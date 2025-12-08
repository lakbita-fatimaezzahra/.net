using LocationVoitures.Data.Models;
using LocationVoitures.Data.Services;
using Microsoft.Win32;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace LocationVoitures.BackOffice
{
    public partial class LocationDetailsWindow : Window
    {
        private readonly LocationService _locationService;
        private readonly PaiementService _paiementService;
        private readonly Location _location;

        public LocationDetailsWindow(Location location)
        {
            InitializeComponent();
            _locationService = new LocationService();
            _paiementService = new PaiementService();
            _location = location;

            LoadDetails();
        }

        private void LoadDetails()
        {
            try
            {
                // Titre et statut
                TxtTitle.Text = $"👁️ Détails de la Location n°{_location.Id}";
                TxtStatut.Text = _location.Statut;

                // Style du statut
                switch (_location.Statut)
                {
                    case "En cours":
                        BrdStatut.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D5F4E6"));
                        TxtStatut.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#27AE60"));
                        break;
                    case "Terminée":
                        BrdStatut.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D6EAF8"));
                        TxtStatut.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3498DB"));
                        break;
                    case "Annulée":
                        BrdStatut.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FADBD8"));
                        TxtStatut.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E74C3C"));
                        break;
                }

                // Informations générales
                TxtLocationId.Text = _location.Id.ToString();
                TxtDateDebut.Text = _location.DateDebut.ToString("dd/MM/yyyy à HH:mm");
                TxtDateFin.Text = _location.DateFin.ToString("dd/MM/yyyy à HH:mm");

                int nbJours = (_location.DateFin - _location.DateDebut).Days;
                if (nbJours == 0) nbJours = 1;
                TxtDuree.Text = $"{nbJours} jour{(nbJours > 1 ? "s" : "")}";

                // Informations financières
                double montantPaye = _location.Paiements?.Sum(p => p.Montant) ?? 0;
                double resteAPayer = Math.Max(0, _location.PrixTotal - montantPaye);

                TxtPrixTotal.Text = $"{_location.PrixTotal:F2} MAD";
                TxtMontantPaye.Text = $"{montantPaye:F2} MAD";
                TxtResteAPayer.Text = $"{resteAPayer:F2} MAD";

                // Client
                TxtClientNom.Text = $"{_location.Client.Nom} {_location.Client.Prenom}";
                TxtClientCIN.Text = _location.Client.CIN;
                TxtClientTel.Text = _location.Client.Telephone;
                TxtClientEmail.Text = _location.Client.Email;
                TxtClientAdresse.Text = _location.Client.Adresse;

                // Véhicule
                TxtVehicule.Text = $"{_location.Voiture.Marque} {_location.Voiture.Modele}";
                TxtMatricule.Text = _location.Voiture.Matricule;
                TxtType.Text = _location.Voiture.TypeVoiture?.Nom ?? "N/A";
                TxtKilometrage.Text = $"{_location.Voiture.Kilometrage:F2} km";
                TxtPrixJour.Text = $"{_location.Voiture.PrixJour:F2} MAD";

                // Paiements
                if (_location.Paiements != null && _location.Paiements.Any())
                {
                    BrdPaiements.Visibility = Visibility.Visible;
                    DgPaiements.ItemsSource = _location.Paiements.OrderBy(p => p.DatePaiement);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur de chargement : {ex.Message}", "Erreur",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnPaiements_Click(object sender, RoutedEventArgs e)
        {
            var window = new PaiementsWindow(_location.Id);
            window.ShowDialog();

            // Recharger les données
            var updatedLocation = _locationService.GetById(_location.Id);
            if (updatedLocation != null)
            {
                _location.Paiements = updatedLocation.Paiements;
                LoadDetails();
            }
        }

        private void BtnGeneratePdf_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFileDialog saveDialog = new SaveFileDialog
                {
                    Filter = "Fichier PDF (*.pdf)|*.pdf",
                    FileName = $"Contrat_Location_{_location.Id}_{DateTime.Now:yyyyMMdd}.pdf"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    var fullLocation = _locationService.GetById(_location.Id);
                    QRCodePdfService.GenerateLocationPdf(fullLocation, saveDialog.FileName);

                    MessageBox.Show("PDF généré avec succès !", "Succès",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    System.Diagnostics.Process.Start(saveDialog.FileName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur génération PDF : {ex.Message}", "Erreur",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnFermer_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}