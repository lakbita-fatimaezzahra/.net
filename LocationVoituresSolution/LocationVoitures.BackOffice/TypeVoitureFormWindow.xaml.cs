using LocationVoitures.Data.Models;
using System.Windows;

namespace LocationVoitures.BackOffice
{
    public partial class TypeVoitureFormWindow : Window
    {
        public TypeVoiture TypeVoiture { get; private set; }

        public TypeVoitureFormWindow(TypeVoiture type = null)
        {
            InitializeComponent();

            if (type != null)
            {
                TypeVoiture = type;
                NomTextBox.Text = type.Nom;
                DescriptionTextBox.Text = type.Description;
                Title = "Modifier un Type de Voiture";
            }
            else
            {
                TypeVoiture = new TypeVoiture();
                Title = "Ajouter un Type de Voiture";
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NomTextBox.Text))
            {
                MessageBox.Show("Le nom est obligatoire.", "Validation",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(DescriptionTextBox.Text))
            {
                MessageBox.Show("La description est obligatoire.", "Validation",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            TypeVoiture.Nom = NomTextBox.Text.Trim();
            TypeVoiture.Description = DescriptionTextBox.Text.Trim();

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}