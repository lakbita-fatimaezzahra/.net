using LocationVoitures.Data.Models;
using LocationVoitures.Data.Services;
using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace LocationVoitures.BackOffice
{
    public partial class TerminerLocationWindow : Window
    {
        private readonly LocationListPage _locationListPage;
        private readonly LocationService _locationService;
        private readonly Location _location;
        private double _kmInitial;
        private double _prixInitial;
        private int _nbJours;
        private bool _isLoaded = false;

        public TerminerLocationWindow(int locationId)
        {
            InitializeComponent();
            _locationService = new LocationService();
            _location = _locationService.GetById(locationId);

            if (_location == null)
            {
                MessageBox.Show("Location introuvable.", "Erreur",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                DialogResult = false; // ✅ AJOUTÉ: Définir DialogResult à false
                Close();
                return;
            }

            // ✅ AJOUTÉ: Vérifier le statut
            if (_location.Statut != "En cours")
            {
                MessageBox.Show($"Cette location ne peut pas être terminée.\nStatut actuel: {_location.Statut}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                DialogResult = false; // ✅ AJOUTÉ: Définir DialogResult à false
                Close();
                return;
            }

            ChargerInformations();
            _isLoaded = true;
        }

        private void ChargerInformations()
        {
            TxtLocationInfo.Text = $"N° Location: {_location.Id} | Statut: {_location.Statut}";
            TxtClient.Text = $"{_location.Client.Nom} {_location.Client.Prenom}";
            TxtVehicule.Text = $"{_location.Voiture.Marque} {_location.Voiture.Modele} ({_location.Voiture.Matricule})";
            TxtPeriode.Text = $"Du {_location.DateDebut:dd/MM/yyyy} au {_location.DateFin:dd/MM/yyyy}";
            TxtPrixInitial.Text = $"{_location.PrixTotal:F2} MAD";

            _kmInitial = _location.Voiture.Kilometrage;
            _prixInitial = _location.PrixTotal;
            _nbJours = (_location.DateFin - _location.DateDebut).Days;
            if (_nbJours == 0) _nbJours = 1;

            TxtKmInitial.Text = $"{_kmInitial:F2} km";
            TxtRecapPrixInitial.Text = $"{_prixInitial:F2} MAD";
            TxtRecapFrais.Text = "0.00 MAD"; // ✅ AJOUTÉ: Initialiser les frais
            TxtPrixFinal.Text = $"{_prixInitial:F2} MAD"; // ✅ AJOUTÉ: Initialiser le prix final
        }

        private void TxtKmFinal_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (_isLoaded)
                CalculerFrais();
        }

        private void TxtFraisDommages_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (_isLoaded)
                CalculerFrais();
        }

        private void TxtAutresFrais_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (_isLoaded)
                CalculerFrais();
        }

        private void CmbEtatVehicule_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (!_isLoaded) return;

            if (PnlDommages != null)
            {
                if (CmbEtatVehicule.SelectedIndex > 0)
                {
                    PnlDommages.Visibility = Visibility.Visible;
                }
                else
                {
                    PnlDommages.Visibility = Visibility.Collapsed;
                    if (TxtFraisDommages != null) // ✅ AJOUTÉ: Vérifier null
                        TxtFraisDommages.Text = "0";
                }
            }
            CalculerFrais();
        }

        private void CalculerFrais()
        {
            if (!_isLoaded) return;

            double fraisSupplementaires = 0;

            // Kilométrage
            if (double.TryParse(TxtKmFinal.Text, out double kmFinal) && kmFinal > _kmInitial)
            {
                double kmParcouru = kmFinal - _kmInitial;
                double kmAutorise = _nbJours * 200;

                TxtKmParcouru.Text = $"📊 Kilométrage parcouru: {kmParcouru:F2} km | Autorisé: {kmAutorise:F2} km";

                if (kmParcouru > kmAutorise)
                {
                    double kmDepassement = kmParcouru - kmAutorise;
                    double fraisKm = kmDepassement * 0.5;
                    fraisSupplementaires += fraisKm;

                    if (BrdDepassementKm != null)
                    {
                        BrdDepassementKm.Visibility = Visibility.Visible;
                        TxtDepassementKm.Text = $"Dépassement: {kmDepassement:F2} km × 0.50 MAD = {fraisKm:F2} MAD";
                    }
                }
                else
                {
                    if (BrdDepassementKm != null) BrdDepassementKm.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                TxtKmParcouru.Text = "";
                if (BrdDepassementKm != null) BrdDepassementKm.Visibility = Visibility.Collapsed;
            }

            // Retard
            if (DateTime.Now > _location.DateFin)
            {
                int joursRetard = (DateTime.Now - _location.DateFin).Days;
                double prixJour = _location.Voiture.PrixJour;
                double fraisRetard = joursRetard * prixJour * 1.5;
                fraisSupplementaires += fraisRetard;

                if (BrdRetard != null)
                {
                    BrdRetard.Visibility = Visibility.Visible;
                    TxtRetard.Text = $"Retard: {joursRetard} jour(s) × {prixJour:F2} MAD × 150% = {fraisRetard:F2} MAD";
                }
            }
            else
            {
                if (BrdRetard != null) BrdRetard.Visibility = Visibility.Collapsed;
            }

            // Dommages
            if (double.TryParse(TxtFraisDommages?.Text ?? "0", out double fraisDommages))
            {
                fraisSupplementaires += fraisDommages;
            }

            // Autres frais
            if (double.TryParse(TxtAutresFrais?.Text ?? "0", out double autresFrais))
            {
                fraisSupplementaires += autresFrais;
            }

            // Mise à jour affichage
            TxtRecapFrais.Text = $"{fraisSupplementaires:F2} MAD";
            double prixFinal = _prixInitial + fraisSupplementaires;
            TxtPrixFinal.Text = $"{prixFinal:F2} MAD";
        }

        private void TxtKmFinal_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex(@"^[0-9.]+$");
            e.Handled = !regex.IsMatch(e.Text);
        }
        private void BtnTerminer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Vérification de base
                if (!double.TryParse(TxtKmFinal.Text, out double kmFinal))
                {
                    MessageBox.Show("Kilométrage final invalide.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (kmFinal < _kmInitial)
                {
                    MessageBox.Show("Le kilométrage final ne peut pas être inférieur au kilométrage initial.",
                        "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Calcul automatique
                CalculerFrais();

                double prixFinal = double.Parse(TxtPrixFinal.Text.Replace(" MAD", ""));
                double fraisSupplementaires = prixFinal - _prixInitial;

                // Confirmation simple
                if (MessageBox.Show("Voulez-vous terminer cette location ?",
                                    "Confirmation",
                                    MessageBoxButton.YesNo,
                                    MessageBoxImage.Question) != MessageBoxResult.Yes)
                    return;

                // ✔ Terminer dans la base
                _locationService.TerminerLocation(_location.Id, kmFinal, fraisSupplementaires);

                // ✔ Mise à jour auto de la page sans duplication popup
                _locationListPage?.LoadLocations();

                // ✔ Message final unique
                MessageBox.Show("Location terminée avec succès.", "Succès",
                                MessageBoxButton.OK, MessageBoxImage.Information);

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur : {ex.Message}", "Erreur",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnAnnuler_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Voulez-vous vraiment annuler ?\nLes modifications ne seront pas enregistrées.",
                "Confirmation",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                DialogResult = false; // ✅ AJOUTÉ: Définir explicitement DialogResult
                Close();
            }
        }
    }
}