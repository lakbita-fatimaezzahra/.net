using LocationVoitures.BackOffice.Utils;
using LocationVoitures.Data.Models;
using LocationVoitures.Data.Services;
using System;
using System.Windows;
using System.Windows.Controls;

namespace LocationVoitures.BackOffice.Pages
{
    public partial class ProfilPage : Page
    {
        private readonly UserService _userService;
        private Utilisateur _currentUser;

        public ProfilPage()
        {
            InitializeComponent();
            _userService = new UserService();
            LoadUserProfile();
        }

        private void LoadUserProfile()
        {
            try
            {
                _currentUser = CurrentUser.GetUser();

                if (_currentUser == null)
                {
                    MessageBox.Show("Aucun utilisateur connecté.", "Erreur", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Recharger l'utilisateur depuis la base de données pour avoir les dernières données
                _currentUser = _userService.GetUserById(_currentUser.Id);

                // Afficher les informations
                TxtWelcome.Text = $"Bienvenue, {CurrentUser.GetFullName()}";
                TxtNom.Text = _currentUser.Nom;
                TxtPrenom.Text = _currentUser.Prenom;
                TxtEmail.Text = _currentUser.Email;
                TxtRole.Text = CurrentUser.GetRole();
                TxtCreatedAt.Text = _currentUser.CreatedAt.ToString("dd/MM/yyyy HH:mm");
                TxtUpdatedAt.Text = _currentUser.UpdatedAt.ToString("dd/MM/yyyy HH:mm");

                // Afficher les informations employé si applicable
                if (_currentUser is Employe employe)
                {
                    GbxEmployeInfo.Visibility = Visibility.Visible;
                    TxtPoste.Text = employe.Poste ?? "N/A";
                    TxtSalaire.Text = $"{employe.Salaire:F2} MAD";
                }
                else
                {
                    GbxEmployeInfo.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement du profil : {ex.Message}", "Erreur",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnSaveProfile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validation
                if (string.IsNullOrWhiteSpace(TxtNom.Text))
                {
                    MessageBox.Show("Le nom est obligatoire.", "Validation",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    TxtNom.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(TxtPrenom.Text))
                {
                    MessageBox.Show("Le prénom est obligatoire.", "Validation",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    TxtPrenom.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(TxtEmail.Text))
                {
                    MessageBox.Show("L'email est obligatoire.", "Validation",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    TxtEmail.Focus();
                    return;
                }

                // Vérifier le format de l'email
                if (!TxtEmail.Text.Contains("@") || !TxtEmail.Text.Contains("."))
                {
                    MessageBox.Show("Veuillez entrer un email valide.", "Validation",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    TxtEmail.Focus();
                    return;
                }

                // Confirmation
                var result = MessageBox.Show(
                    "Voulez-vous enregistrer les modifications de votre profil ?",
                    "Confirmation",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    // Mettre à jour le profil
                    _userService.UpdateProfile(
                        _currentUser.Id,
                        TxtNom.Text.Trim(),
                        TxtPrenom.Text.Trim(),
                        TxtEmail.Text.Trim()
                    );

                    // Mettre à jour l'utilisateur dans CurrentUser
                    _currentUser = _userService.GetUserById(_currentUser.Id);
                    CurrentUser.SetUser(_currentUser);

                    MessageBox.Show("Profil mis à jour avec succès !", "Succès",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    // Recharger les informations
                    LoadUserProfile();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la mise à jour du profil : {ex.Message}", "Erreur",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnChangePassword_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validation
                if (string.IsNullOrWhiteSpace(PwdCurrentPassword.Password))
                {
                    MessageBox.Show("Veuillez entrer votre mot de passe actuel.", "Validation",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    PwdCurrentPassword.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(PwdNewPassword.Password))
                {
                    MessageBox.Show("Veuillez entrer un nouveau mot de passe.", "Validation",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    PwdNewPassword.Focus();
                    return;
                }

                if (PwdNewPassword.Password.Length < 6)
                {
                    MessageBox.Show("Le nouveau mot de passe doit contenir au moins 6 caractères.", "Validation",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    PwdNewPassword.Focus();
                    return;
                }

                if (PwdNewPassword.Password != PwdConfirmPassword.Password)
                {
                    MessageBox.Show("Les mots de passe ne correspondent pas.", "Validation",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    PwdConfirmPassword.Focus();
                    return;
                }

                // Confirmation
                var result = MessageBox.Show(
                    "Voulez-vous vraiment changer votre mot de passe ?",
                    "Confirmation",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    // Changer le mot de passe
                    _userService.ChangePassword(
                        _currentUser.Id,
                        PwdCurrentPassword.Password,
                        PwdNewPassword.Password
                    );

                    MessageBox.Show("Mot de passe changé avec succès !", "Succès",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    // Vider les champs
                    PwdCurrentPassword.Password = "";
                    PwdNewPassword.Password = "";
                    PwdConfirmPassword.Password = "";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du changement de mot de passe : {ex.Message}", "Erreur",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}


