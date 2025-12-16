using LocationVoitures.BackOffice.Utils;
using LocationVoitures.Data.Models;
using LocationVoitures.Data.Services;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LocationVoitures.BackOffice
{
    public partial class PaiementsWindow : Window
    {
        private readonly LocationService _locationService;
        private readonly PaiementService _paiementService;
        private readonly int _locationId;
        private Location _location;
        private double _resteAPayer;

        public PaiementsWindow(int locationId)
        {
            InitializeComponent();
            _locationService = new LocationService();
            _paiementService = new PaiementService();
            _locationId = locationId;

            LoadData();
        }

        private void LoadData()
        {
            try
            {
                _location = _locationService.GetById(_locationId);

                if (_location == null)
                {
                    MessageBox.Show("Location introuvable.", "Erreur",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    Close();
                    return;
                }

                // Informations location
                TxtLocationInfo.Text = $"Location n°{_location.Id} | " +
                    $"Client: {_location.Client.Nom} {_location.Client.Prenom} | " +
                    $"Véhicule: {_location.Voiture.Marque} {_location.Voiture.Modele}";

                // Calculs
                double prixTotal = _location.PrixTotal;
                double montantPaye = _location.Paiements?.Sum(p => p.Montant) ?? 0;
                _resteAPayer = Math.Max(0, prixTotal - montantPaye);

                TxtPrixTotal.Text = $"{prixTotal:F2} MAD";
                TxtMontantPaye.Text = $"{montantPaye:F2} MAD";
                TxtResteAPayer.Text = $"{_resteAPayer:F2} MAD";

                // Liste des paiements
                var paiements = _paiementService.GetPaiementsByLocation(_locationId);
                DgPaiements.ItemsSource = paiements;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur de chargement : {ex.Message}", "Erreur",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnMontantRapide_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag != null)
            {
                string tag = btn.Tag.ToString();

                if (tag == "1") // Reste
                {
                    TxtMontant.Text = _resteAPayer.ToString("F2");
                }
                else if (double.TryParse(tag, out double value))
                {
                    if (value < 1) // Pourcentage
                    {
                        TxtMontant.Text = (_resteAPayer * value).ToString("F2");
                    }
                    else // Montant fixe
                    {
                        TxtMontant.Text = Math.Min(value, _resteAPayer).ToString("F2");
                    }
                }
            }
        }

        private void TxtMontant_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex(@"^[0-9.]+$");
            e.Handled = !regex.IsMatch(e.Text);
        }

        private void BtnEnregistrer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validation
                if (!double.TryParse(TxtMontant.Text, out double montant) || montant <= 0)
                {
                    MessageBox.Show("Veuillez saisir un montant valide.", "Validation",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (montant > _resteAPayer)
                {
                    var result = MessageBox.Show(
                        $"Le montant saisi ({montant:F2} MAD) dépasse le reste à payer ({_resteAPayer:F2} MAD).\n\n" +
                        "Voulez-vous continuer ?",
                        "Confirmation",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result != MessageBoxResult.Yes)
                        return;
                }

                if (CmbModePaiement.SelectedItem == null)
                {
                    MessageBox.Show("Veuillez sélectionner un mode de paiement.", "Validation",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string modePaiement = ((ComboBoxItem)CmbModePaiement.SelectedItem).Content.ToString();
                modePaiement = modePaiement.Split(' ')[1]; // Enlever l'emoji

                // Créer le paiement
                var paiement = _paiementService.CreerPaiement(_locationId, montant, modePaiement);

                MessageBox.Show($"Paiement de {montant:F2} MAD enregistré avec succès !", "Succès",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                // Réinitialiser
                TxtMontant.Clear();
                TxtNotes.Clear();
                LoadData();

                // Envoi automatique du reçu par mail
                if (!string.IsNullOrEmpty(_location.Client.Email))
                {
                    try
                    {
                        EmailHelper.SendPaymentReceipt(
                            email: _location.Client.Email,
                            nomClient: _location.Client.Nom + " " + _location.Client.Prenom,
                            numeroLocation: _location.Id.ToString(),
                            montant: montant,
                            modePaiement: modePaiement,
                            datePaiement: DateTime.Now,
                            resteAPayer: Math.Max(0, _resteAPayer - montant)
                        );
                    }
                    catch (Exception mailEx)
                    {
                        MessageBox.Show($"Erreur lors de l'envoi du mail : {mailEx.Message}", "Erreur",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }

                // Générer reçu PDF
                var genererRecu = MessageBox.Show(
                    "Voulez-vous générer un reçu PDF pour ce paiement ?",
                    "Génération de reçu",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (genererRecu == MessageBoxResult.Yes)
                {
                    GenerateReceipt(paiement);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur : {ex.Message}", "Erreur",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnGenerateReceipt_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Paiement paiement)
            {
                GenerateReceipt(paiement);
            }
        }

        private void GenerateReceipt(Paiement paiement)
        {
            try
            {
                SaveFileDialog saveDialog = new SaveFileDialog
                {
                    Filter = "Fichier PDF (*.pdf)|*.pdf",
                    FileName = $"Recu_Paiement_{paiement.Id}_{DateTime.Now:yyyyMMdd}.pdf"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    // Génération du PDF
                    QRCodePdfService.GeneratePaymentReceipt(paiement, saveDialog.FileName);

                    MessageBox.Show("Reçu généré avec succès !", "Succès",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    // 🔥 Nouvelle méthode d’ouverture compatible avec Windows 10/11 + .NET 6+
                    var psi = new ProcessStartInfo
                    {
                        FileName = saveDialog.FileName,
                        UseShellExecute = true   // indispensable
                    };

                    Process.Start(psi);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur génération reçu : {ex.Message}", "Erreur",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnGenerateFacture_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFileDialog saveDialog = new SaveFileDialog
                {
                    Filter = "Fichier PDF (*.pdf)|*.pdf",
                    FileName = $"Facture_Location_{_location.Id}_{DateTime.Now:yyyyMMdd}.pdf"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    var fullLocation = _locationService.GetById(_locationId);

                    // Génération du PDF
                    QRCodePdfService.GenerateLocationPdf(fullLocation, saveDialog.FileName);

                    MessageBox.Show("Facture générée avec succès !", "Succès",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    // 🔥 Ouvrir le PDF de manière compatible .NET moderne
                    var psi = new ProcessStartInfo
                    {
                        FileName = saveDialog.FileName,
                        UseShellExecute = true  // obligatoire pour ouvrir un fichier
                    };

                    Process.Start(psi);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur génération facture : {ex.Message}", "Erreur",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void BtnAnnuler_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Paiement paiement)
            {
                var result = MessageBox.Show(
                    $"Voulez-vous vraiment annuler le paiement n°{paiement.Id} de {paiement.Montant:F2} MAD ?",
                    "Confirmation d'annulation",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        _paiementService.AnnulerPaiement(paiement.Id);
                        MessageBox.Show("Paiement annulé avec succès !", "Succès",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadData();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Erreur : {ex.Message}", "Erreur",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void BtnFermer_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}