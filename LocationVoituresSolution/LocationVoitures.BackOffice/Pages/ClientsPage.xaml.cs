using LocationVoitures.Data.models;
using LocationVoitures.Data.Models;
using LocationVoitures.Data.Services;
using Microsoft.Win32;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LocationVoitures.BackOffice.Pages
{
    public partial class ClientsPage : Page
    {
        private readonly ClientService _clientService;
        private readonly ImportExportService _importExportService;
        private bool _isInitialized;

        private int _page = 1;
        private int _pageSize = 10;
        private int _totalPages = 1;

        public ClientsPage()
        {
            InitializeComponent();

            // Initialiser les services
            var context = new LocationContext();
            _clientService = new ClientService(context);
            _importExportService = new ImportExportService(context);

            // Charger les données
            LoadClients();

            // Marquer comme initialisé
            _isInitialized = true;
        }

        private void LoadClients()
        {
            if (_clientService == null || dgClients == null) return;

            try
            {
                var result = _clientService.GetPagedClients(_page, _pageSize);

                dgClients.ItemsSource = result.Items;
                _totalPages = result.TotalPages;

                if (TxtPageInfo != null)
                    TxtPageInfo.Text = $"Page {_page} / {_totalPages}";

                if (BtnPrev != null) BtnPrev.IsEnabled = _page > 1;
                if (BtnNext != null) BtnNext.IsEnabled = _page < _totalPages;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement des clients : {ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            if (_page < _totalPages)
            {
                _page++;
                LoadClients();
            }
        }

        private void BtnPrev_Click(object sender, RoutedEventArgs e)
        {
            if (_page > 1)
            {
                _page--;
                LoadClients();
            }
        }

        private void PageSize_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (!_isInitialized || CmbPageSize?.SelectedItem == null)
                return;

            if (CmbPageSize.SelectedItem is ComboBoxItem { Content: not null } item)
            {
                _pageSize = int.Parse(item.Content.ToString());
                _page = 1;
                LoadClients();
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_isInitialized ||
                _clientService == null ||
                SearchBox == null ||
                dgClients == null ||
                rbNom == null ||
                rbPrenom == null ||
                rbCIN == null ||
                rbTelephone == null)
            {
                return;
            }

            try
            {
                string filter = SearchBox.Text?.Trim()?.ToLower() ?? string.Empty;

                if (string.IsNullOrEmpty(filter) || filter == "rechercher un client...")
                {
                    var allClients = _clientService.GetAllClients();
                    dgClients.ItemsSource = allClients;

                    if (TxtPageInfo != null) TxtPageInfo.Text = "";
                    return;
                }

                string field = rbNom.IsChecked == true ? "nom" :
                               rbPrenom.IsChecked == true ? "prenom" :
                               rbCIN.IsChecked == true ? "cin" : "telephone";

                var results = _clientService.SearchClients(filter, field);
                dgClients.ItemsSource = results;

                if (TxtPageInfo != null) TxtPageInfo.Text = "";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la recherche : {ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SearchBox?.Text == "Rechercher un client...")
            {
                SearchBox.Text = "";
                SearchBox.Foreground = Brushes.Black;
            }
        }

        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (SearchBox != null && string.IsNullOrWhiteSpace(SearchBox.Text))
            {
                SearchBox.Text = "Rechercher un client...";
                SearchBox.Foreground = Brushes.Gray;
            }
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService == null) return;

            try
            {
                NavigationService.Navigate(new ClientFormPage());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur : {ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService == null) return;

            if (sender is Button { DataContext: Client selectedClient })
            {
                try
                {
                    NavigationService.Navigate(new ClientFormPage(selectedClient));
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur : {ex.Message}",
                        "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Impossible de récupérer les données du client.",
                    "Attention", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_clientService == null) return;

            if (sender is Button { DataContext: Client selectedClient })
            {
                var result = MessageBox.Show(
                    $"Voulez-vous vraiment supprimer {selectedClient.Prenom} {selectedClient.Nom} ?",
                    "Confirmation de suppression",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        if (_clientService.DeleteClient(selectedClient.Id))
                        {
                            MessageBox.Show("Client supprimé avec succès !",
                                "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
                            LoadClients();
                        }
                        else
                        {
                            MessageBox.Show("Erreur : Client introuvable.",
                                "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Erreur lors de la suppression : {ex.Message}",
                            "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Impossible de récupérer les données du client.",
                    "Attention", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // 📊 Exporter
        private void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            if (_clientService == null || _importExportService == null) return;

            try
            {
                SaveFileDialog saveDialog = new SaveFileDialog
                {
                    Filter = "Fichier CSV (*.csv)|*.csv",
                    FileName = $"Clients_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    var clients = _clientService.GetAllClients();
                    _importExportService.ExportClients(saveDialog.FileName, clients);

                    MessageBox.Show($"Export réussi !\n{clients.Count} clients exportés.",
                        "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur d'export : {ex.Message}", "Erreur",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // 📥 Importer
        private void BtnImport_Click(object sender, RoutedEventArgs e)
        {
            if (_importExportService == null) return;

            try
            {
                OpenFileDialog openDialog = new OpenFileDialog
                {
                    Filter = "Fichier CSV (*.csv)|*.csv",
                    Title = "Sélectionner un fichier de clients"
                };

                if (openDialog.ShowDialog() == true)
                {
                    var result = _importExportService.ImportClients(openDialog.FileName);

                    if (result.HasErrors)
                    {
                        MessageBox.Show(
                            $"{result.SuccessCount} client(s) importé(s) avec succès.\n\n" +
                            $"Erreurs ({result.Errors.Count}):\n{result.GetErrorSummary()}",
                            "Import terminé avec erreurs",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                    }
                    else
                    {
                        MessageBox.Show(
                            $"{result.SuccessCount} client(s) importé(s) avec succès !",
                            "Import réussi",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                    }

                    LoadClients();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur d'import : {ex.Message}", "Erreur",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}