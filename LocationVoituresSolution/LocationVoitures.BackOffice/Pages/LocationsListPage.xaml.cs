using LocationVoitures.BackOffice.Utils;
using LocationVoitures.Data.Models;
using LocationVoitures.Data.Services;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace LocationVoitures.BackOffice
{
    public partial class LocationListPage : Page
    {
        private readonly LocationService _locationService;
        private readonly ImportExportService _importExportService;
        private int _currentPage = 1;
        private int _pageSize = 25;
        private string _searchTerm = "";
        private string _statutFilter = "Tous";
        private bool _isInitialized = false;

        public LocationListPage()
        {
            InitializeComponent();

            _locationService = new LocationService();
            var context = new LocationVoitures.Data.models.LocationContext();
            _importExportService = new ImportExportService(context);

            LoadLocations();
            _isInitialized = true;
        }

        // ✅✅✅ MÉTHODE CORRIGÉE - FORCE LE RECHARGEMENT COMPLET
        public void LoadLocations()
        {
            if (_locationService == null || DgLocations == null) return;

            try
            {
                Debug.WriteLine("═══════════════════════════════════════════");
                Debug.WriteLine($"[LoadLocations] DÉBUT - Page {_currentPage}");

                // ✅ 1. VIDER COMPLÈTEMENT la grille
                DgLocations.ItemsSource = null;
                DgLocations.Items.Refresh();

                // ✅ 2. FORCER la mise à jour visuelle
                DgLocations.UpdateLayout();

                // ✅ 3. CRÉER UN NOUVEAU SERVICE (nouveau contexte)
                var freshService = new LocationService();

                // ✅ 4. CHARGER les données avec le nouveau service
                var result = freshService.GetPagedLocations(_currentPage, _pageSize, _searchTerm, _statutFilter);

                Debug.WriteLine($"[LoadLocations] {result.Items.Count} items chargés");
                Debug.WriteLine($"[LoadLocations] Total: {result.TotalItems}, Page: {result.Page}/{result.TotalPages}");

                // ✅ 5. AFFECTER les nouvelles données
                DgLocations.ItemsSource = result.Items;

                // ✅ 6. Mettre à jour les infos de pagination
                if (TxtTotalItems != null)
                    TxtTotalItems.Text = result.TotalItems.ToString();

                if (TxtPageInfo != null)
                    TxtPageInfo.Text = $"Page {result.Page} / {result.TotalPages}";

                if (BtnPrevious != null)
                    BtnPrevious.IsEnabled = result.Page > 1;

                if (BtnNext != null)
                    BtnNext.IsEnabled = result.Page < result.TotalPages;

                // ✅ 7. FORCER le rafraîchissement de la vue
                DgLocations.Items.Refresh();
                DgLocations.UpdateLayout();

                var view = CollectionViewSource.GetDefaultView(DgLocations.ItemsSource);
                if (view != null)
                {
                    view.Refresh();
                }

                Debug.WriteLine($"[LoadLocations] ✅ TERMINÉ AVEC SUCCÈS");
                Debug.WriteLine("═══════════════════════════════════════════");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[LoadLocations] ❌ ERREUR : {ex.Message}");
                Debug.WriteLine($"[LoadLocations] Stack: {ex.StackTrace}");
                MessageBox.Show($"Erreur de chargement : {ex.Message}", "Erreur",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ✅✅✅ MÉTHODE BtnTerminer_Click CORRIGÉE
        private void BtnTerminer_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Location location)
            {
                Debug.WriteLine($"[BtnTerminer] Début pour location {location.Id}");

                // ✅ Vérification du statut
                if (location.Statut != "En cours")
                {
                    MessageBox.Show(
                        $"Cette location ne peut pas être terminée.\nStatut actuel: {location.Statut}",
                        "Information",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    return;
                }

                try
                {
                    // ✅ Ouvrir la fenêtre
                    var window = new TerminerLocationWindow(location.Id);
                   // window.Owner = Window.GetWindow(this);
                    if (window.ShowDialog() == true)
                    {
                        MessageBox.Show(
                            "✅ La location a été terminée avec succès !",
                            "Succès",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                        LoadLocations();
                    }
                    else
                    {
                        MessageBox.Show($"[BtnTerminer] ❌ Annulé ou erreur");
                    }
                }
                catch (Exception ex)
                {
                   
                    MessageBox.Show(
                        $"❌ Erreur : {ex.Message}",
                        "Erreur",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }

        // Les autres méthodes restent identiques...
        private void BtnNouvelleLocation_Click(object sender, RoutedEventArgs e)
        {
            var window = new AddLocationWindow();
            if (window.ShowDialog() == true)
            {
                LoadLocations();
            }
        }

        private void BtnDetails_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Location location)
            {
                var window = new LocationDetailsWindow(location);
                window.ShowDialog();
            }
        }

        private void BtnPaiements_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Location location)
            {
                var window = new PaiementsWindow(location.Id);
                window.ShowDialog();
                LoadLocations();
            }
        }

        private void BtnAnnuler_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Location location)
            {
                var result = MessageBox.Show(
                    $"Voulez-vous vraiment annuler la location n°{location.Id} ?\n\n" +
                    $"Client: {location.Client.Nom} {location.Client.Prenom}\n" +
                    $"Véhicule: {location.Voiture.Marque} {location.Voiture.Modele}\n\n" +
                    $"⚠️ Cette action est irréversible !",
                    "Confirmation d'annulation",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        _locationService.AnnulerLocation(location.Id);

                        // 🔹 Envoi automatique de l'e-mail de notification
                        if (!string.IsNullOrEmpty(location.Client.Email))
                        {
                            try
                            {
                                EmailHelper.SendCancellationNotification(
                                    email: location.Client.Email,
                                    nomClient: location.Client.Nom + " " + location.Client.Prenom,
                                    numeroLocation: location.Id.ToString()
                                );
                            }
                            catch (Exception mailEx)
                            {
                                MessageBox.Show($"Erreur lors de l'envoi du mail : {mailEx.Message}", "Erreur",
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }

                        MessageBox.Show("Location annulée avec succès !", "Succès",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadLocations();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Erreur : {ex.Message}", "Erreur",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_isInitialized || _locationService == null || TxtSearch == null) return;
            _searchTerm = TxtSearch.Text ?? "";
            _currentPage = 1;
            LoadLocations();
        }

        private void CmbStatut_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_isInitialized || _locationService == null || CmbStatut?.SelectedItem == null) return;
            if (CmbStatut.SelectedItem is ComboBoxItem item && item.Content != null)
            {
                _statutFilter = item.Content.ToString() == "Tous les statuts" ? "Tous" : item.Content.ToString();
                _currentPage = 1;
                LoadLocations();
            }
        }

        private void BtnPrevious_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                LoadLocations();
            }
        }

        private void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            _currentPage++;
            LoadLocations();
        }

        private void CmbPageSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_isInitialized || _locationService == null || CmbPageSize?.SelectedItem == null) return;
            if (CmbPageSize.SelectedItem is ComboBoxItem item && item.Content != null)
            {
                _pageSize = int.Parse(item.Content.ToString());
                _currentPage = 1;
                LoadLocations();
            }
        }

        private void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            if (_locationService == null || _importExportService == null) return;
            try
            {
                SaveFileDialog saveDialog = new SaveFileDialog
                {
                    Filter = "Fichier CSV (*.csv)|*.csv",
                    FileName = $"Locations_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    var locations = _locationService.GetAll();
                    _importExportService.ExportLocations(saveDialog.FileName, locations);
                    MessageBox.Show($"Export réussi !\n{locations.Count} locations exportées.",
                        "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur d'export : {ex.Message}", "Erreur",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnGeneratePdf_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Location location)
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
                        var fullLocation = _locationService.GetById(location.Id);
                        Directory.CreateDirectory(Path.GetDirectoryName(saveDialog.FileName));
                        QRCodePdfService.GenerateLocationPdf(fullLocation, saveDialog.FileName);

                        MessageBox.Show("PDF généré avec succès !", "Succès",
                            MessageBoxButton.OK, MessageBoxImage.Information);

                        ProcessStartInfo psi = new ProcessStartInfo
                        {
                            FileName = saveDialog.FileName,
                            UseShellExecute = true
                        };
                        Process.Start(psi);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur génération PDF : {ex.Message}", "Erreur",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }

    // ✅✅✅ CONVERTISSEUR AJOUTÉ ICI
    /// <summary>
    /// Convertisseur pour afficher/masquer les boutons "Terminer" et "Annuler" selon le statut
    /// </summary>
    public class StatutToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return Visibility.Collapsed;

            string statut = value.ToString();

            // Afficher les boutons uniquement si le statut est "En cours"
            if (statut == "En cours")
                return Visibility.Visible;

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}