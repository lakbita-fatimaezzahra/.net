using LocationVoitures.Data.Models;
using LocationVoitures.Data.Services;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.IO;


namespace LocationVoitures.Pages
{
    public partial class VoitureImagesPage : Page
    {
        private readonly Voiture _voiture;
        private readonly ImageVoitureService _service = new ImageVoitureService();

        private int _currentIndex = 0;
        private List<ImageVoiture> _images;

        public VoitureImagesPage(Voiture voiture)
        {
            InitializeComponent();
            _voiture = voiture;
            LoadImages();
        }

        // Charger les images
        private void LoadImages()
        {
            _images = _service.GetImagesByVoiture(_voiture.Id);
            ListImages.ItemsSource = _images;

            if (_images.Count > 0)
            {
                _currentIndex = 0;
                DisplayImage(_currentIndex);
                ListImages.SelectedIndex = 0;
            }
            else
            {
                PreviewImage.Source = null;
            }
        }

        // Afficher image
        private void DisplayImage(int index)
        {
            if (index < 0 || index >= _images.Count)
                return;

            try
            {
                // Chemin relatif stocké dans la DB, par ex: "/uploads/voitures/xxx.png"
                string relativePath = _images[index].Chemin;

                // Racine du projet FrontOffice/wwwroot
                string exeDir = AppDomain.CurrentDomain.BaseDirectory;
                string frontOfficeRoot = Path.GetFullPath(Path.Combine(exeDir, @"..\..\..\..\lovationVoiture.FrontOffice\wwwroot"));

                // Chemin complet du fichier sur disque
                string fullPath = Path.Combine(frontOfficeRoot, relativePath.TrimStart('/').Replace('/', '\\'));

                // Affichage
                PreviewImage.Source = new BitmapImage(new Uri(fullPath, UriKind.Absolute));
            }
            catch
            {
                PreviewImage.Source = null;
            }
        }


        // Quand on sélectionne une image
        private void ListImages_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListImages.SelectedIndex >= 0)
            {
                _currentIndex = ListImages.SelectedIndex;
                DisplayImage(_currentIndex);
            }
        }

        // Ajouter image
        private void AjouterImage_Click(object sender, RoutedEventArgs e)
        {
            // Ouvrir la boîte de dialogue pour sélectionner une image
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Images (*.jpg;*.png)|*.jpg;*.png";

            if (dialog.ShowDialog() == true)
            {
                string sourcePath = dialog.FileName; // chemin du fichier choisi

                // 1️⃣ Détecter la racine du FrontOffice/wwwroot
                string exeDir = AppDomain.CurrentDomain.BaseDirectory;
                string frontOfficeRoot = Path.GetFullPath(Path.Combine(exeDir, @"..\..\..\..\lovationVoiture.FrontOffice\wwwroot"));

                // 2️⃣ Dossier uploads/voitures
                string frontOfficeUploads = Path.Combine(frontOfficeRoot, "uploads", "voitures");
                if (!System.IO.Directory.Exists(frontOfficeUploads))
                    System.IO.Directory.CreateDirectory(frontOfficeUploads);

                // 3️⃣ Nom unique pour éviter collision
                string fileName = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(sourcePath);
                string destPath = System.IO.Path.Combine(frontOfficeUploads, fileName);

                // 4️⃣ Copier le fichier physiquement
                System.IO.File.Copy(sourcePath, destPath, true);

                // 5️⃣ Créer l’objet ImageVoiture avec le chemin relatif pour le FrontOffice
                var image = new ImageVoiture
                {
                    Chemin = "/uploads/voitures/" + fileName, // chemin relatif
                    VoitureId = _voiture.Id
                };

                // 6️⃣ Ajouter l’image dans la base de données
                _service.AddImage(image);

                // 7️⃣ Recharger les images dans la page
                LoadImages();
            }
        }


        // Supprimer image
        private void SupprimerImage_Click(object sender, RoutedEventArgs e)
        {
            if (ListImages.SelectedItem is ImageVoiture img)
            {
                _service.DeleteImage(img);
                LoadImages();
                PreviewImage.Source = null;
            }
        }

        // Bouton Next
        private void Next_Click(object sender, RoutedEventArgs e)
        {
            if (_images == null || _images.Count == 0) return;

            _currentIndex++;
            if (_currentIndex >= _images.Count)
                _currentIndex = 0;

            DisplayImage(_currentIndex);
            ListImages.SelectedIndex = _currentIndex;
        }

        // Bouton Previous
        private void Previous_Click(object sender, RoutedEventArgs e)
        {
            if (_images == null || _images.Count == 0) return;

            _currentIndex--;
            if (_currentIndex < 0)
                _currentIndex = _images.Count - 1;

            DisplayImage(_currentIndex);
            ListImages.SelectedIndex = _currentIndex;
        }

        private void Retour_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}
