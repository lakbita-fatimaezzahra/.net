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
                ImagePrincipale.Source = new BitmapImage(new System.Uri(images[0].Chemin));
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
                    Stretch = System.Windows.Media.Stretch.Uniform,
                    Source = new BitmapImage(new System.Uri(img.Chemin))
                };

                // Permet cliquer pour changer l'image principale
                miniature.MouseLeftButtonUp += (s, e) =>
                {
                    ImagePrincipale.Source = miniature.Source;
                };

                CarouselImages.Children.Add(miniature);
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

        /*private void PlanifierMaintenance_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AddMaintenancePage(_voiture));
        }*/
        private void PlanifierMaintenance_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Naviguer vers la page pour planifier la maintenance
            // Exemple : NavigationService.Navigate(new AddMaintenancePage(_voiture));
            MessageBox.Show($"Planification maintenance pour {_voiture.Matricule}", "Maintenance");
        }

        private void Retour_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}
