using LocationVoitures.BackOffice.Utils;
using LocationVoitures.Data.Models;
using LocationVoitures.Data.Services;
using System;
using System.Windows;
using System.Windows.Controls;

namespace LocationVoitures.BackOffice.Pages
{
    public partial class ClientFormPage : Page
    {
        public Client Client { get; private set; }
        private bool isEditMode;

        public ClientFormPage(Client client = null)
        {
            InitializeComponent();

            if (client != null)
            {
                isEditMode = true;
                Client = client;

                this.Title = "Modifier Client";

                tbNom.Text = client.Nom;
                tbPrenom.Text = client.Prenom;
                tbEmail.Text = client.Email;
                tbCIN.Text = client.CIN;
                tbTelephone.Text = client.Telephone;
                tbAdresse.Text = client.Adresse;
                tbPermis.Text = client.PermisConduire;
                dpDateNaissance.SelectedDate = client.DateNaissance;
            }
            else
            {
                isEditMode = false;
                Client = new Client();
                this.Title = "Ajouter Client";
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbNom.Text) ||
                string.IsNullOrWhiteSpace(tbPrenom.Text) ||
                string.IsNullOrWhiteSpace(tbEmail.Text))
            {
                MessageBox.Show("Nom, Prénom et Email sont obligatoires.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DateTime? selectedDate = dpDateNaissance.SelectedDate;
            if (!selectedDate.HasValue)
            {
                MessageBox.Show("Sélectionnez une date de naissance valide.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Client.Nom = tbNom.Text.Trim();
            Client.Prenom = tbPrenom.Text.Trim();
            Client.Email = tbEmail.Text.Trim();
            Client.CIN = tbCIN.Text.Trim();
            Client.Telephone = tbTelephone.Text.Trim();
            Client.Adresse = tbAdresse.Text.Trim();
            Client.PermisConduire = tbPermis.Text.Trim();
            Client.DateNaissance = selectedDate.Value;

            try
            {
                var service = new ClientService();

                if (!isEditMode)
                {
                    // 1) Générer mot de passe aléatoire
                    string plainPassword = PasswordHelper.GeneratePassword();
                    string hashedPassword = PasswordHelper.HashPassword(plainPassword);

                    // 2) Créer le client avec mot de passe hashé
                    Client.Password = hashedPassword;

                    // 3) Envoyer l’email AVANT d’ajouter le client
                    try
                    {
                        EmailHelper.SendPasswordEmail(
                            Client.Email,
                            Client.Nom,
                            Client.Prenom,
                            plainPassword
                        );
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Impossible d'envoyer l'email. L'ajout a été annulé.\nErreur : {ex.Message}",
                                        "Erreur d'envoi d'email", MessageBoxButton.OK, MessageBoxImage.Error);
                        return; // arrêt de l'ajout
                    }

                    // 4) Ajouter le client dans la base
                    service.AddClient(Client);
                    MessageBox.Show("Client ajouté avec succès ! Mot de passe envoyé par email.", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    service.UpdateClient(Client);
                    MessageBox.Show("Client modifié avec succès !", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                NavigationService?.GoBack();
              
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }
    }
}
