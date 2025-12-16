using LocationVoitures.Data.Models;
using LocationVoitures.Data.Services;
using QRCoder;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace LocationVoitures.BackOffice.Pages
{
    public partial class VoitureDetailsPage : Page
    {
        private readonly Voiture _voiture;
        private readonly ImageVoitureService _imageService = new ImageVoitureService();

        public VoitureDetailsPage(Voiture voiture)
        {
            InitializeComponent();
            _voiture = voiture;
            DataContext = voiture;

            ChargerImages();
            GenererQRCode();
        }

        // Charger les images + mettre la première comme image principale
        private void ChargerImages()
        {
            var images = _imageService.GetImagesByVoiture(_voiture.Id);

            if (images.Count > 0)
            {
                SetImageSource(images[0].Chemin, ImagePrincipale);
            }

            // Miniatures du carousel
            CarouselImages.Children.Clear();

            foreach (var img in images)
            {
                Image miniature = new Image
                {
                    Width = 80,
                    Height = 80,
                    Margin = new Thickness(5),
                    Stretch = System.Windows.Media.Stretch.Uniform
                };

                SetImageSource(img.Chemin, miniature);

                // Stocker le chemin pour le clic
                string imgPath = img.Chemin;
                miniature.MouseLeftButtonUp += (s, e) =>
                {
                    SetImageSource(imgPath, ImagePrincipale);
                };

                CarouselImages.Children.Add(miniature);
            }
        }

        private void SetImageSource(string relativePath, Image imageControl)
        {
            if (imageControl == null || string.IsNullOrEmpty(relativePath))
            {
                imageControl.Source = null;
                return;
            }

            try
            {
                // Racine du projet FrontOffice/wwwroot
                string exeDir = AppDomain.CurrentDomain.BaseDirectory;
                string frontOfficeRoot = Path.GetFullPath(Path.Combine(exeDir, @"..\..\..\..\lovationVoiture.FrontOffice\wwwroot"));

                // Chemin complet du fichier sur disque
                string fullPath = Path.Combine(frontOfficeRoot, relativePath.TrimStart('/').Replace('/', '\\'));

                // Vérifier si le fichier existe avant de créer l'URI
                if (File.Exists(fullPath))
                {
                    imageControl.Source = new BitmapImage(new Uri(fullPath, UriKind.Absolute));
                }
                else
                {
                    // Log ou debug
                    Console.WriteLine($"Fichier non trouvé: {fullPath}");
                    imageControl.Source = null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur chargement image: {ex.Message}");
                imageControl.Source = null;
            }
        }

        // Génération QR Code
        private void GenererQRCode()
        {
            var generator = new QRCodeGenerator();
            var data = generator.CreateQrCode($"VOITURE-{_voiture.Id}-{_voiture.Matricule}",
                                              QRCodeGenerator.ECCLevel.Q);

            var code = new QRCode(data);
            var bitmap = code.GetGraphic(20);

            using (MemoryStream ms = new MemoryStream())
            {
                bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                ms.Position = 0;

                BitmapImage qrImage = new BitmapImage();
                qrImage.BeginInit();
                qrImage.StreamSource = ms;
                qrImage.CacheOption = BitmapCacheOption.OnLoad;
                qrImage.EndInit();

                QRCodeImage.Source = qrImage;
            }
        }

       
      

        private void Retour_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}
