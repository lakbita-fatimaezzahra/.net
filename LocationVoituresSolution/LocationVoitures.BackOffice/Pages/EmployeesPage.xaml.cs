using LocationVoitures.Data.models;
using LocationVoitures.Data.Models;
using LocationVoitures.Data.Services;
using Microsoft.Win32;
using System;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LocationVoitures.BackOffice
{
    public partial class EmployeesPage : Page
    {
        private readonly EmployeeService _EmployeeService;
        private readonly ImportExportService _importExportService;
        private int _page = 1;
        private int _pageSize = 10;
        private int _totalPages = 1;
        private bool _isInitialized = false;

        public EmployeesPage()
        {
            InitializeComponent();

            // Initialiser les services
            var context = new LocationContext();
            _EmployeeService = new EmployeeService(context);
            _importExportService = new ImportExportService(context);

            // Charger les données
            LoadEmployees();

            // Marquer comme initialisé
            _isInitialized = true;
        }

        private void LoadEmployees()
        {
            if (_EmployeeService == null) return;

            var result = _EmployeeService.GetPagedEmployees(_page, _pageSize);

            dgEmployees.ItemsSource = result.Items;
            _totalPages = result.TotalPages;
            TxtPageInfo.Text = $"Page {_page} / {_totalPages}";
        }

        private void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            if (_page < _totalPages)
            {
                _page++;
                LoadEmployees();
            }
        }

        private void BtnPrev_Click(object sender, RoutedEventArgs e)
        {
            if (_page > 1)
            {
                _page--;
                LoadEmployees();
            }
        }

        private void PageSize_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (!_isInitialized || _EmployeeService == null || CmbPageSize?.SelectedItem == null)
                return;

            if (CmbPageSize.SelectedItem is ComboBoxItem item && item.Content != null)
            {
                _pageSize = int.Parse(item.Content.ToString());
                _page = 1;
                LoadEmployees();
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_isInitialized ||
                _EmployeeService == null ||
                SearchBox == null ||
                dgEmployees == null ||
                rbNom == null ||
                rbPrenom == null)
            {
                return;
            }

            string search = SearchBox.Text?.Trim() ?? string.Empty;

            if (string.IsNullOrEmpty(search) || search == "Rechercher un employé...")
            {
                LoadEmployees();
                return;
            }

            string criterium = rbNom.IsChecked == true ? "Nom" :
                               rbPrenom.IsChecked == true ? "Prenom" :
                               "Poste";

            try
            {
                dgEmployees.ItemsSource = _EmployeeService.Filter(search, criterium);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la recherche : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SearchBox == null) return;

            if (SearchBox.Text == "Rechercher un employé...")
            {
                SearchBox.Text = "";
                SearchBox.Foreground = Brushes.Black;
            }
        }

        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (SearchBox == null) return;

            if (string.IsNullOrWhiteSpace(SearchBox.Text))
            {
                SearchBox.Text = "Rechercher un employé...";
                SearchBox.Foreground = Brushes.Gray;
            }
        }

        private void AddEmployee_Click(object sender, RoutedEventArgs e)
        {
            if (_EmployeeService == null) return;

            try
            {
                var addWindow = new AddEmployeeWindow(_EmployeeService);
                addWindow.Owner = Window.GetWindow(this);

                if (addWindow.ShowDialog() == true)
                {
                    LoadEmployees();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (_EmployeeService == null) return;

            if (sender is Button btn && btn.DataContext is Employe emp)
            {
                try
                {
                    var editWindow = new AddEmployeeWindow(_EmployeeService, emp);
                    editWindow.Owner = Window.GetWindow(this);

                    if (editWindow.ShowDialog() == true)
                    {
                        LoadEmployees();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_EmployeeService == null) return;

            if (sender is Button btn && btn.DataContext is Employe emp)
            {
                var result = MessageBox.Show(
                    $"Êtes-vous sûr de vouloir supprimer {emp.Prenom} {emp.Nom} ?",
                    "Confirmation de suppression",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        bool deleted = _EmployeeService.DeleteEmployee(emp.Id);

                        if (deleted)
                        {
                            MessageBox.Show("Employé supprimé avec succès !", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
                            LoadEmployees();
                        }
                        else
                        {
                            MessageBox.Show("Erreur : Employé introuvable.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Erreur lors de la suppression : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        // 📊 Exporter
        private void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            if (_EmployeeService == null || _importExportService == null) return;

            try
            {
                SaveFileDialog saveDialog = new SaveFileDialog
                {
                    Filter = "Fichier CSV (*.csv)|*.csv",
                    FileName = $"Employes_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    var employees = _EmployeeService.GetEmployeesUsers();
                    _importExportService.ExportEmployees(saveDialog.FileName, employees);

                    MessageBox.Show($"Export réussi !\n{employees.Count} employés exportés.",
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
                    Title = "Sélectionner un fichier d'employés"
                };

                if (openDialog.ShowDialog() == true)
                {
                    var result = _importExportService.ImportEmployees(openDialog.FileName);

                    if (result.HasErrors)
                    {
                        MessageBox.Show(
                            $"{result.SuccessCount} employé(s) importé(s) avec succès.\n\n" +
                            $"Erreurs ({result.Errors.Count}):\n{result.GetErrorSummary()}",
                            "Import terminé avec erreurs",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                    }
                    else
                    {
                        MessageBox.Show(
                            $"{result.SuccessCount} employé(s) importé(s) avec succès !",
                            "Import réussi",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                    }

                    LoadEmployees();
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