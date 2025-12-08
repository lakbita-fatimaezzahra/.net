using LocationVoitures.BackOffice.Pages;
using LocationVoitures.Data.models;
using LocationVoitures.Data.Models;
using LocationVoitures.Data.Services;
using LocationVoitures.Pages;
using Microsoft.Win32;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace LocationVoitures.BackOffice
{
    public partial class VoitureListPage : Page
    {
        private readonly VoitureService _service = new VoitureService();
        private readonly ImportExportService _importExportService;
        private readonly MaintenanceService _maintenanceService;
        private readonly AlerteService _alerteService;
        private int _page = 1;
        private int _pageSize = 10;
        private int _totalPages = 1;
        private bool _isInitialized = false;

        public VoitureListPage()
        {
            InitializeComponent();

            // Initialiser les services
            var context = new LocationContext();
            _importExportService = new ImportExportService(context);
            _maintenanceService = new MaintenanceService();
            _alerteService = new AlerteService();

            Loaded += VoitureListPage_Loaded;
        }

        private void VoitureListPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadVoitures();
            _isInitialized = true;

            // Vérifier et afficher les alertes de maintenance
            VerifierAlertesMaintenanceCritiques();
        }

        private void LoadVoitures()
        {
            if (DataGridVoitures == null) return;

            var result = _service.GetPagedVoitures(_page, _pageSize);

            DataGridVoitures.ItemsSource = result.Items;
            _totalPages = result.TotalPages;
            TxtPageInfo.Text = $"Page {_page} / {_totalPages}";

            // Mettre à jour le compteur d'alertes
            AfficherCompteursAlertes();
        }

        // ✅ CORRIGÉ: Utiliser VoitureAvecUrgence au lieu de tuples
        private void VerifierAlertesMaintenanceCritiques()
        {
            try
            {
                var voituresAvecAlertes = _service.GetVoituresAvecAlertes();

             
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur vérification alertes : {ex.Message}");
            }
        }

        private void AfficherCompteursAlertes()
        {
            try
            {
                var stats = _alerteService.CompterAlertesParUrgence();
                int totalAlertes = stats["Critique"] + stats["Urgent"] + stats["Attention"] + stats["À Prévoir"];

                if (totalAlertes > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"Alertes actives : {totalAlertes}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur compteurs alertes : {ex.Message}");
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_isInitialized ||
                _service == null ||
                SearchBox == null ||
                DataGridVoitures == null ||
                rbMatricule == null ||
                rbMarque == null ||
                rbModele == null ||
                rbType == null)
            {
                return;
            }

            string search = SearchBox.Text?.Trim() ?? string.Empty;

            if (string.IsNullOrEmpty(search) || search == "Rechercher une voiture...")
            {
                LoadVoitures();
                return;
            }

            string criterium = rbMatricule.IsChecked == true ? "Matricule" :
                               rbMarque.IsChecked == true ? "Marque" :
                               rbModele.IsChecked == true ? "Modele" :
                               "Type";

            try
            {
                DataGridVoitures.ItemsSource = _service.Filter(search, criterium);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la recherche : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SearchBox == null) return;

            if (SearchBox.Text == "Rechercher une voiture...")
            {
                SearchBox.Text = "";
            }
        }

        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (SearchBox == null) return;

            if (string.IsNullOrWhiteSpace(SearchBox.Text))
            {
                SearchBox.Text = "Rechercher une voiture...";
            }
        }

        private void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            if (_page < _totalPages)
            {
                _page++;
                LoadVoitures();
            }
        }

        private void BtnPrev_Click(object sender, RoutedEventArgs e)
        {
            if (_page > 1)
            {
                _page--;
                LoadVoitures();
            }
        }

        private void PageSize_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (!_isInitialized || _service == null || CmbPageSize?.SelectedItem == null)
                return;

            if (CmbPageSize.SelectedItem is ComboBoxItem item && item.Content != null)
            {
                _pageSize = int.Parse(item.Content.ToString());
                _page = 1;
                LoadVoitures();
            }
        }

        private void Ajouter_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AddEditVoiturePage());
        }

        private void Modifier_Row_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is Voiture v)
            {
                NavigationService?.Navigate(new AddEditVoiturePage(v));
            }
        }

        private void Supprimer_Row_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is Voiture v)
            {
                if (MessageBox.Show(
                        $"Supprimer la voiture {v.Matricule} ?",
                        "Confirmation",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    _service.SupprimerVoiture(v);
                    LoadVoitures();
                    AfficherCompteursAlertes();
                }
            }
        }

        private void Images_Row_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is Voiture v)
            {
                NavigationService?.Navigate(new VoitureImagesPage(v));
            }
        }

        private void Details_Row_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is Voiture v)
            {
                NavigationService?.Navigate(new VoitureDetailsPage(v));
            }
        }

        private void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            if (_service == null || _importExportService == null) return;

            try
            {
                SaveFileDialog saveDialog = new SaveFileDialog
                {
                    Filter = "Fichier CSV (*.csv)|*.csv",
                    FileName = $"Voitures_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    var voitures = _service.GetAllVoitures();
                    _importExportService.ExportVoitures(saveDialog.FileName, voitures);

                    MessageBox.Show($"Export réussi !\n{voitures.Count} voitures exportées.",
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
            if (_importExportService == null) return;

            try
            {
                OpenFileDialog openDialog = new OpenFileDialog
                {
                    Filter = "Fichier CSV (*.csv)|*.csv",
                    Title = "Sélectionner un fichier de voitures"
                };

                if (openDialog.ShowDialog() == true)
                {
                    var result = _importExportService.ImportVoitures(openDialog.FileName);

                    if (result.HasErrors)
                    {
                        MessageBox.Show(
                            $"{result.SuccessCount} voiture(s) importée(s) avec succès.\n\n" +
                            $"Erreurs ({result.Errors.Count}):\n{result.GetErrorSummary()}",
                            "Import terminé avec erreurs",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                    }
                    else
                    {
                        MessageBox.Show(
                            $"{result.SuccessCount} voiture(s) importée(s) avec succès !",
                            "Import réussi",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                    }

                    LoadVoitures();
                    _maintenanceService.VerifierToutesLesVoitures();
                    VerifierAlertesMaintenanceCritiques();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur d'import : {ex.Message}", "Erreur",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void VoirAlertes_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AlertesPage());
        }

        // ✅ CORRIGÉ: Utiliser VoitureAvecUrgence au lieu de tuples
        private void VoirVoituresEnMaintenance_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var voituresAvecAlertes = _service.GetVoituresAvecAlertes();

                if (voituresAvecAlertes.Count == 0)
                {
                    MessageBox.Show(
                        "✅ Aucune voiture ne nécessite de maintenance pour le moment.",
                        "Maintenance à jour",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    return;
                }

                // ✅ Utiliser la propriété Voiture au lieu de Item1
                var voitures = voituresAvecAlertes.Select(v => v.Voiture).ToList();
                DataGridVoitures.ItemsSource = voitures;

                MessageBox.Show(
                    $"{voituresAvecAlertes.Count} voiture(s) nécessite(nt) une maintenance.\nLa liste a été filtrée.",
                    "Filtrage appliqué",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur : {ex.Message}", "Erreur",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}