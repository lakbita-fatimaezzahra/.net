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

        public void LoadLocations()
        {
            if (_locationService == null || DgLocations == null) return;

            try
            {
                // ✅ Vider la grille avant de recharger
                DgLocations.ItemsSource = null;
                
                var result = _locationService.GetPagedLocations(_currentPage, _pageSize, _searchTerm, _statutFilter);

                // ✅ Recharger les données
                DgLocations.ItemsSource = result.Items;

                TxtTotalItems.Text = result.TotalItems.ToString();
                TxtPageInfo.Text = $"Page {result.Page} / {result.TotalPages}";

                BtnPrevious.IsEnabled = result.Page > 1;
                BtnNext.IsEnabled = result.Page < result.TotalPages;

                // ✅ Forcer la mise à jour visuelle
                DgLocations.UpdateLayout();
                CollectionViewSource.GetDefaultView(DgLocations.ItemsSource)?.Refresh();


                // ✅ AJOUTÉ: Log pour debug
                System.Diagnostics.Debug.WriteLine($"[LoadLocations] Chargé {result.Items.Count} locations");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur de chargement : {ex.Message}", "Erreur",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnNouvelleLocation_Click(object sender, RoutedEventArgs e)
        {
            var window = new AddLocationWindow();
            if (window.ShowDialog() == true)
            {
                LoadLocations(); // Rafraîchir après ajout
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

        private void BtnImport_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("L'import de locations n'est pas disponible pour des raisons de sécurité.\n" +
                           "Les locations doivent être créées manuellement pour garantir la validité des données.",
                           "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnDetails_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Location location)
            {
                var window = new LocationDetailsWindow(location);
                window.ShowDialog();
                // Pas besoin de rafraîchir car c'est juste une consultation
            }
        }

        private void BtnPaiements_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Location location)
            {
                var window = new PaiementsWindow(location.Id);
                window.ShowDialog();
                LoadLocations(); // ✅ Rafraîchir après gestion des paiements
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

                        // Assure que le dossier existe
                        Directory.CreateDirectory(Path.GetDirectoryName(saveDialog.FileName));

                        QRCodePdfService.GenerateLocationPdf(fullLocation, saveDialog.FileName);

                        MessageBox.Show(
                            "PDF généré avec succès !",
                            "Succès",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);

                        // 👉 OUVERTURE DU PDF (version corrigée)
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
                    MessageBox.Show(
                        $"Erreur génération PDF : {ex.Message}",
                        "Erreur",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }


        private void BtnTerminer_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Location location)
            {
                // ✅ AJOUTÉ: Log pour debug
                System.Diagnostics.Debug.WriteLine($"[BtnTerminer_Click] Location ID: {location.Id}, Statut: {location.Statut}");

                // ✅ Vérification du statut avant d'ouvrir la fenêtre
                if (location.Statut != "En cours")
                {
                    MessageBox.Show($"Cette location ne peut pas être terminée.\nStatut actuel: {location.Statut}",
                        "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                try
                {
                    var window = new TerminerLocationWindow(location.Id);

                    // ✅✅✅ CRITIQUE: Vérifier DialogResult = true
                    var dialogResult = window.ShowDialog();

                    System.Diagnostics.Debug.WriteLine($"[BtnTerminer_Click] DialogResult: {dialogResult}");

                    if (dialogResult == true)
                    {
                        System.Diagnostics.Debug.WriteLine("[BtnTerminer_Click] Rafraîchissement de la liste...");

                        // ✅ Vider complètement la grille
                        DgLocations.ItemsSource = null;
                        
                        // ✅ Rafraîchir immédiatement la liste avec les nouvelles données
                        LoadLocations();
                        
                        // ✅ Forcer la mise à jour de l'interface de manière asynchrone
                        this.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            LoadLocations();
                        }), System.Windows.Threading.DispatcherPriority.Render);

                        MessageBox.Show("✅ La location a été terminée avec succès !\nLa liste a été mise à jour.",
                            "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("[BtnTerminer_Click] Opération annulée par l'utilisateur");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"❌ Erreur : {ex.Message}\n\n{ex.StackTrace}", "Erreur",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
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
                        MessageBox.Show("Location annulée avec succès !", "Succès",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadLocations(); // ✅ Rafraîchir après annulation
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Erreur : {ex.Message}", "Erreur",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
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
    }

    /// <summary>
    /// Convertisseur pour afficher les boutons uniquement pour les locations "En cours"
    /// </summary>
    public class StatutToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string statut)
            {
                return statut == "En cours" ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}