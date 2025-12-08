using LocationVoitures.Data.Models;
using LocationVoitures.Data.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace LocationVoitures.BackOffice.Pages
{
    public partial class PageMaintenances : Page
    {
        private readonly MaintenanceService _maintenanceService;
        private List<Maintenance> _allMaintenances;
        private List<Maintenance> _pagedMaintenances;

        private int _currentPage = 1;
        private int _pageSize = 10;
        private int _totalPages = 1;

        public PageMaintenances()
        {
            InitializeComponent();
            _maintenanceService = new MaintenanceService();
            LoadMaintenances();
        }

        private void LoadMaintenances()
        {
            // Charger toutes les maintenances avec Voiture incluse
            _allMaintenances = _maintenanceService.GetAllMaintenancesWithVoiture();
            _currentPage = 1;
            ApplyPagination();
        }

        private void ApplyPagination()
        {
            _totalPages = (int)Math.Ceiling((double)_allMaintenances.Count / _pageSize);
            _currentPage = Math.Max(1, Math.Min(_currentPage, _totalPages));

            _pagedMaintenances = _allMaintenances
                .Skip((_currentPage - 1) * _pageSize)
                .Take(_pageSize)
                .ToList();

            dgMaintenances.ItemsSource = _pagedMaintenances;
            TxtPageInfo.Text = $"Page {_currentPage} / {_totalPages}";
        }

        private void BtnPrev_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                ApplyPagination();
            }
        }

        private void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage < _totalPages)
            {
                _currentPage++;
                ApplyPagination();
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string keyword = SearchBox.Text.Trim().ToLower();

            _allMaintenances = _maintenanceService.GetAllMaintenancesWithVoiture()
                .Where(m =>
                {
                    switch (GetSelectedFilter())
                    {
                        case "Type": return m.TypeMaintenance?.ToLower().Contains(keyword) ?? false;
                        case "Garage": return m.Garage?.ToLower().Contains(keyword) ?? false;
                        case "Statut": return m.Statut?.ToLower().Contains(keyword) ?? false;
                        default:
                            return (m.TypeMaintenance?.ToLower().Contains(keyword) ?? false)
                                     || (m.Garage?.ToLower().Contains(keyword) ?? false)
                                     || (m.Statut?.ToLower().Contains(keyword) ?? false);
                    }
                })
                .ToList();

            _currentPage = 1;
            ApplyPagination();
        }

        private string GetSelectedFilter()
        {
            if (rbType.IsChecked == true) return "Type";
            if (rbGarage.IsChecked == true) return "Garage";
            if (rbStatut.IsChecked == true) return "Statut";
            return "";
        }

        // Ajouter nouvelle maintenance
        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new PlanifierMaintenanceWindow(null); // 0 = pas de voiture spécifique
            addWindow.ShowDialog();
            if (addWindow.MaintenanceCreee)
                LoadMaintenances();
        }

        // Modifier maintenance
        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Maintenance maintenance)
            {
                var editWindow = new PlanifierMaintenanceWindow(maintenance.Voiture);

                // Pré-remplir les champs existants
                editWindow.cmbType.SelectedItem = editWindow.cmbType.Items
                    .Cast<ComboBoxItem>()
                    .FirstOrDefault(i => i.Content.ToString() == maintenance.TypeMaintenance);

                editWindow.dpDateMaintenance.SelectedDate = maintenance.DateMaintenance;
                editWindow.txtKmMaintenance.Text = maintenance.KilometrageMaintenance.ToString();
                editWindow.txtGarage.Text = maintenance.Garage;
                editWindow.txtDescription.Text = maintenance.Description;

                editWindow.ShowDialog();

                if (editWindow.MaintenanceCreee)
                    LoadMaintenances();
            }
        }

        // Supprimer maintenance
        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Maintenance maintenance)
            {
                var result = MessageBox.Show(
                    $"Voulez-vous vraiment supprimer la maintenance ID {maintenance.Id} ?",
                    "Confirmation",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    // Ajoute cette méthode dans MaintenanceService si elle n'existe pas
                    _maintenanceService.DeleteMaintenance(maintenance.Id);
                    LoadMaintenances();
                }
            }
        }
    }
}
