using LocationVoitures.Data.Models;
using LocationVoitures.Data.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        // ✅ MÉTHODE CORRIGÉE - FORCE LE RECHARGEMENT COMPLET
        private void LoadMaintenances()
        {
            try
            {
                Debug.WriteLine("═══════════════════════════════════════════");
                Debug.WriteLine($"[LoadMaintenances] DÉBUT - Page {_currentPage}");

                // ✅ 1. VIDER COMPLÈTEMENT la grille
                if (dgMaintenances != null)
                {
                    dgMaintenances.ItemsSource = null;
                    dgMaintenances.Items.Refresh();
                    dgMaintenances.UpdateLayout();
                }

                // ✅ 2. CRÉER UN NOUVEAU SERVICE (sans using)
                var freshService = new MaintenanceService();

                // ✅ 3. CHARGER les données fraîches depuis la base
                _allMaintenances = freshService.GetAllMaintenancesWithVoiture();

                Debug.WriteLine($"[LoadMaintenances] {_allMaintenances.Count} maintenances chargées");

                // ✅ 4. Réinitialiser la page
                _currentPage = 1;

                // ✅ 5. Appliquer la pagination
                ApplyPagination();

                Debug.WriteLine($"[LoadMaintenances] ✅ TERMINÉ AVEC SUCCÈS");
                Debug.WriteLine("═══════════════════════════════════════════");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[LoadMaintenances] ❌ ERREUR : {ex.Message}");
                MessageBox.Show($"Erreur de chargement : {ex.Message}", "Erreur",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApplyPagination()
        {
            try
            {
                if (_allMaintenances == null || _allMaintenances.Count == 0)
                {
                    _totalPages = 1;
                    _currentPage = 1;
                    _pagedMaintenances = new List<Maintenance>();
                }
                else
                {
                    _totalPages = (int)Math.Ceiling((double)_allMaintenances.Count / _pageSize);
                    _currentPage = Math.Max(1, Math.Min(_currentPage, _totalPages));

                    _pagedMaintenances = _allMaintenances
                        .Skip((_currentPage - 1) * _pageSize)
                        .Take(_pageSize)
                        .ToList();
                }

                // ✅ Affecter les données à la grille
                if (dgMaintenances != null)
                {
                    dgMaintenances.ItemsSource = _pagedMaintenances;
                    dgMaintenances.Items.Refresh();
                }

                // ✅ Mettre à jour l'affichage de la pagination
                if (TxtPageInfo != null)
                {
                    TxtPageInfo.Text = _totalPages > 0
                        ? $"Page {_currentPage} / {_totalPages}"
                        : "Aucune maintenance";
                }

                Debug.WriteLine($"[ApplyPagination] Page {_currentPage}/{_totalPages}, {_pagedMaintenances.Count} items affichés");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ApplyPagination] Erreur : {ex.Message}");
            }
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
            try
            {
                string keyword = SearchBox?.Text?.Trim().ToLower() ?? "";

                // ✅ Recharger depuis la base avec un nouveau service
                var freshService = new MaintenanceService();
                var allData = freshService.GetAllMaintenancesWithVoiture();

                if (string.IsNullOrWhiteSpace(keyword))
                {
                    _allMaintenances = allData;
                }
                else
                {
                    _allMaintenances = allData
                        .Where(m =>
                        {
                            switch (GetSelectedFilter())
                            {
                                case "Type":
                                    return m.TypeMaintenance?.ToLower().Contains(keyword) ?? false;
                                case "Garage":
                                    return m.Garage?.ToLower().Contains(keyword) ?? false;
                                case "Statut":
                                    return m.Statut?.ToLower().Contains(keyword) ?? false;
                                default:
                                    return (m.TypeMaintenance?.ToLower().Contains(keyword) ?? false)
                                         || (m.Garage?.ToLower().Contains(keyword) ?? false)
                                         || (m.Statut?.ToLower().Contains(keyword) ?? false)
                                         || (m.Voiture?.Matricule?.ToLower().Contains(keyword) ?? false);
                            }
                        })
                        .ToList();
                }

                _currentPage = 1;
                ApplyPagination();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SearchBox_TextChanged] Erreur : {ex.Message}");
            }
        }

        private string GetSelectedFilter()
        {
            if (rbType?.IsChecked == true) return "Type";
            if (rbGarage?.IsChecked == true) return "Garage";
            if (rbStatut?.IsChecked == true) return "Statut";
            return "";
        }

        // Modifier maintenance
        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Maintenance maintenance)
            {
                try
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
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur : {ex.Message}", "Erreur",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // ✅ MÉTHODE CORRIGÉE - Marquer comme effectuée
        private void BtnMarquerEffectuee_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Maintenance maintenance)
            {
                Debug.WriteLine($"[BtnMarquerEffectuee] Début pour maintenance {maintenance.Id}");

                // Vérifier que la maintenance est bien planifiée ou en cours
                if (maintenance.Statut != "Planifiée" &&
                    maintenance.Statut != "En cours" &&
                    maintenance.Statut != "En retard")
                {
                    MessageBox.Show(
                        $"Cette maintenance ne peut pas être marquée comme effectuée.\nStatut actuel : {maintenance.Statut}",
                        "Attention",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                try
                {
                    var window = new MarquerMaintenanceEffectueeWindow(maintenance);
                    window.Owner = Window.GetWindow(this);

                    Debug.WriteLine($"[BtnMarquerEffectuee] Fenêtre ouverte, attente de DialogResult...");

                    bool? result = window.ShowDialog();

                    Debug.WriteLine($"[BtnMarquerEffectuee] DialogResult = {result}");

                    if (result == true)
                    {
                        Debug.WriteLine($"[BtnMarquerEffectuee] ✅ Maintenance effectuée, rechargement...");

                        // ✅ FORCER LE RECHARGEMENT COMPLET
                        LoadMaintenances();

                        Debug.WriteLine($"[BtnMarquerEffectuee] ✅ Rechargement terminé");

                        MessageBox.Show(
                            "✅ Maintenance marquée comme effectuée avec succès !\n\n" ,
                            
                            "Succès",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                    }
                    else
                    {
                        Debug.WriteLine($"[BtnMarquerEffectuee] ❌ Annulé ou erreur");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[BtnMarquerEffectuee] ❌ EXCEPTION : {ex.Message}");
                    MessageBox.Show(
                        $"Erreur : {ex.Message}",
                        "Erreur",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
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
                    try
                    {
                        _maintenanceService.DeleteMaintenance(maintenance.Id);
                        LoadMaintenances();

                        MessageBox.Show(
                            "Maintenance supprimée avec succès !",
                            "Succès",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(
                            $"Erreur lors de la suppression : {ex.Message}",
                            "Erreur",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }
                }
            }
        }
    }
}