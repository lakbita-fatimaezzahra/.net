using LocationVoitures.Data.Models;
using LocationVoitures.Data.Services;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace LocationVoitures.BackOffice.Pages
{
    public partial class TypeVoiturePage : Page
    {
        private readonly TypeVoitureService _service;

        public TypeVoiturePage()
        {
            InitializeComponent();
            _service = new TypeVoitureService();
            LoadData();
        }

        private void LoadData()
        {
            List<TypeVoiture> types = _service.GetAllTypes(); 
            TypeVoitureDataGrid.ItemsSource = types;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new TypeVoitureFormWindow();
            if (window.ShowDialog() == true)
            {
                _service.AddType(window.TypeVoiture); 
                LoadData();
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (TypeVoitureDataGrid.SelectedItem is TypeVoiture selected)
            {
                var window = new TypeVoitureFormWindow(selected);
                if (window.ShowDialog() == true)
                {
                    _service.UpdateType(window.TypeVoiture); // Changé de Update() à UpdateType()
                    LoadData();
                }
            }
            else
            {
                MessageBox.Show("Veuillez sélectionner un type de voiture à modifier.",
                    "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (TypeVoitureDataGrid.SelectedItem is TypeVoiture selected)
            {
                if (MessageBox.Show($"Voulez-vous vraiment supprimer '{selected.Nom}' ?",
                    "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    _service.DeleteType(selected); // Changé de Delete(selected.Id) à DeleteType(selected)
                    LoadData();
                }
            }
            else
            {
                MessageBox.Show("Veuillez sélectionner un type de voiture à supprimer.",
                    "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

    }
}