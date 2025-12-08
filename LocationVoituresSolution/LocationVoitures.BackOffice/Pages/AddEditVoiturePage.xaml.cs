using LocationVoitures.Data.Models;
using LocationVoitures.Data.Services;
using System;
using System.Data.Entity;
using System.Windows;
using System.Windows.Controls;

namespace LocationVoitures.BackOffice.Pages
{
    public partial class AddEditVoiturePage : Page
    {
        private readonly Voiture _voiture;
        private readonly VoitureService _service = new VoitureService();
        private readonly TypeVoitureService _typeService = new TypeVoitureService();
        private readonly bool _isEditMode = false;

        public AddEditVoiturePage()
        {
            InitializeComponent(); // DOIT être le premier !

            // Maintenant les contrôles existent
            CbTypeVoiture.DisplayMemberPath = "Nom";
            CbTypeVoiture.SelectedValuePath = "Id";

            TitrePage.Text = "Ajouter une voiture";

            LoadTypes();
        }

        private void LoadTypes()
        {
            var types = _typeService.GetAllTypes();
            CbTypeVoiture.ItemsSource = types;
            CbTypeVoiture.DisplayMemberPath = "Nom";
            CbTypeVoiture.SelectedValuePath = "Id";

            if (types.Count > 0)
                CbTypeVoiture.SelectedIndex = 0;
        }

        public AddEditVoiturePage(Voiture voiture) : this()
        {
            _voiture = voiture;

            if (_voiture != null)
            {
                _isEditMode = true;
                TitrePage.Text = "Modifier une voiture";

                TxtMatricule.Text = _voiture.Matricule;
                TxtMarque.Text = _voiture.Marque;
                TxtModele.Text = _voiture.Modele;
                TxtKilometrage.Text = _voiture.Kilometrage.ToString();
                TxtPrixJour.Text = _voiture.PrixJour.ToString();

                DpAnnee.SelectedDate = _voiture.Annee;
                DpMaintenance.SelectedDate = _voiture.DerniereMaintenance;

                CbTypeVoiture.SelectedValue = _voiture.TypeId;

                CbStatut.SelectedIndex =
                    _voiture.Statut == "Disponible" ? 0 :
                    _voiture.Statut == "Louée" ? 1 : 2;
            }
        }

        private void Enregistrer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validations
                if (string.IsNullOrWhiteSpace(TxtMatricule.Text) ||
                    string.IsNullOrWhiteSpace(TxtMarque.Text) ||
                    string.IsNullOrWhiteSpace(TxtModele.Text) ||
                    string.IsNullOrWhiteSpace(TxtKilometrage.Text) ||
                    string.IsNullOrWhiteSpace(TxtPrixJour.Text))
                {
                    MessageBox.Show("Veuillez remplir tous les champs obligatoires.", "Erreur");
                    return;
                }

                if (!double.TryParse(TxtKilometrage.Text, out double kilometrage))
                {
                    MessageBox.Show("Kilométrage invalide.", "Erreur");
                    return;
                }

                if (!double.TryParse(TxtPrixJour.Text, out double prixJour))
                {
                    MessageBox.Show("Prix par jour invalide.", "Erreur");
                    return;
                }

                DateTime annee = DpAnnee.SelectedDate ?? DateTime.Today;
                DateTime derniereMaintenance = DpMaintenance.SelectedDate ?? DateTime.Today;

                if (_isEditMode)
                {
                    // Modification
                    _voiture.Matricule = TxtMatricule.Text.Trim();
                    _voiture.Marque = TxtMarque.Text.Trim();
                    _voiture.Modele = TxtModele.Text.Trim();
                    _voiture.Kilometrage = kilometrage;
                    _voiture.PrixJour = prixJour;
                    _voiture.Annee = annee;
                    _voiture.DerniereMaintenance = derniereMaintenance;
                    _voiture.TypeId = (int)CbTypeVoiture.SelectedValue;
                    _voiture.Statut = ((ComboBoxItem)CbStatut.SelectedItem).Content.ToString();

                    _service.Update(_voiture);
                    MessageBox.Show("Voiture modifiée avec succès !");
                }
                else
                {
                    // Nouvelle voiture
                    var voiture = new Voiture
                    {
                        Matricule = TxtMatricule.Text.Trim(),
                        Marque = TxtMarque.Text.Trim(),
                        Modele = TxtModele.Text.Trim(),
                        Kilometrage = kilometrage,
                        PrixJour = prixJour,
                        Annee = annee,
                        DerniereMaintenance = DpMaintenance.SelectedDate.Value,
                        TypeId = (int)CbTypeVoiture.SelectedValue,
                        Statut = ((ComboBoxItem)CbStatut.SelectedItem).Content.ToString()
                    };

                    _service.Add(voiture);
                    MessageBox.Show("Voiture ajoutée avec succès !");
                }

                NavigationService?.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'enregistrement : {ex.Message}", "Erreur");
            }
        }

        private void Annuler_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }
    }
}
