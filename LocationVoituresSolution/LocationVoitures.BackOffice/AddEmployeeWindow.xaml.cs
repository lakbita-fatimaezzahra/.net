using LocationVoitures.BackOffice.Utils;
using LocationVoitures.Data.Models;
using LocationVoitures.Data.Services;
using System.Data;
using System.Windows;

namespace LocationVoitures.BackOffice
{
    public partial class AddEmployeeWindow : Window
    {
        private readonly EmployeeService _employeeService;
        private readonly Employe _existingEmployee; // null si ajout

        public AddEmployeeWindow(EmployeeService employeeService, Employe employee = null)
        {
            InitializeComponent();
            _employeeService = employeeService;
            _existingEmployee = employee;

            // Charger les rôles
            cbRoles.ItemsSource = _employeeService.GetRoles();
            if (cbRoles.Items.Count > 0)
                cbRoles.SelectedIndex = 0;

            // Si mode update, remplir les champs
            if (_existingEmployee != null)
            {
                tbNom.Text = _existingEmployee.Nom;
                tbPrenom.Text = _existingEmployee.Prenom;
                tbEmail.Text = _existingEmployee.Email;
                cbRoles.SelectedValue = _existingEmployee.RoleId;
                tbPoste.Text = _existingEmployee.Poste;
                tbSalaire.Text = _existingEmployee.Salaire.ToString();

                BtnAdd.Content = "Mettre à jour";
                this.Title = "Modifier Employé";
                labelText.Text = "Modifier un employé";



            }
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbNom.Text) ||
                string.IsNullOrWhiteSpace(tbPrenom.Text) ||
                string.IsNullOrWhiteSpace(tbEmail.Text))
            {
                MessageBox.Show("Nom, Prénom et Email sont obligatoires !");
                return;
            }

            if (!double.TryParse(tbSalaire.Text, out double salaire))
            {
                MessageBox.Show("Salaire invalide !");
                return;
            }

            if (_existingEmployee == null)
            {
                // 1) Générer mot de passe aléatoire
                string plainPassword = PasswordHelper.GeneratePassword();

                try
                {
                    // 2) Tenter d'envoyer l'email AVANT l'insertion
                    EmailHelper.SendPasswordEmail(
                        tbEmail.Text.Trim(),
                        tbNom.Text.Trim(),
                        tbPrenom.Text.Trim(),
                        plainPassword
                    );
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Impossible d'envoyer l'email. L'ajout a été annulé.\nErreur : {ex.Message}",
                                    "Erreur d'envoi d'email", MessageBoxButton.OK, MessageBoxImage.Error);
                    return; // arrêt de l'ajout
                }

                // 3) Hash du mot de passe
                string hashedPassword = PasswordHelper.HashPassword(plainPassword);

                // 4) Créer utilisateur
                var user = new Utilisateur
                {
                    Nom = tbNom.Text.Trim(),
                    Prenom = tbPrenom.Text.Trim(),
                    Email = tbEmail.Text.Trim(),
                    Password = hashedPassword,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                // 5) Ajouter employé + utilisateur
                _employeeService.AddEmployee(user, tbPoste.Text.Trim(), salaire, (int)cbRoles.SelectedValue);

                MessageBox.Show("Employé ajouté avec succès ! Mot de passe envoyé par email.");
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                // Update existant
                _existingEmployee.Nom = tbNom.Text.Trim();
                _existingEmployee.Prenom = tbPrenom.Text.Trim();
                _existingEmployee.Email = tbEmail.Text.Trim();
                _existingEmployee.RoleId = (int)cbRoles.SelectedValue;
                _existingEmployee.Poste = tbPoste.Text.Trim();
                _existingEmployee.Salaire = salaire;
                _existingEmployee.UpdatedAt = DateTime.Now;

                _employeeService.UpdateEmployee(_existingEmployee);

                MessageBox.Show("Employé mis à jour avec succès !");
                this.DialogResult = true;
                this.Close();
            }
        }

    }
}
