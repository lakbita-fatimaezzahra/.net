using iTextSharp.text;
using iTextSharp.text.pdf;
using LocationVoitures.Data.Models;
using QRCoder;

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

// Aliases pour éviter les conflits
using PdfFont = iTextSharp.text.Font;
using PdfImage = iTextSharp.text.Image;
using PdfRectangle = iTextSharp.text.Rectangle;

namespace LocationVoitures.Data.Services
{
    public class QRCodePdfService
    {
        // 🔹 Génération du QR Code
        public static byte[] GenerateQRCode(Location location)
        {
            try
            {
                string qrContent = $"LOCATION#{location.Id}\n" +
                                   $"Client: {location.Client.Nom} {location.Client.Prenom}\n" +
                                   $"Véhicule: {location.Voiture.Marque} {location.Voiture.Modele}\n" +
                                   $"Matricule: {location.Voiture.Matricule}\n" +
                                   $"Début: {location.DateDebut:dd/MM/yyyy}\n" +
                                   $"Fin: {location.DateFin:dd/MM/yyyy}\n" +
                                   $"Prix: {location.PrixTotal:F2} MAD";

                using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
                using (QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrContent, QRCodeGenerator.ECCLevel.Q))
                using (QRCode qrCode = new QRCode(qrCodeData))
                using (Bitmap qrCodeImage = qrCode.GetGraphic(20))
                using (MemoryStream ms = new MemoryStream())
                {
                    qrCodeImage.Save(ms, ImageFormat.Png);
                    return ms.ToArray();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur génération QR Code : {ex.Message}");
            }
        }

        // 🔹 Génération du PDF complet avec QR Code
        public static void GenerateLocationPdf(Location location, string outputPath)
        {
            try
            {
                Document document = new Document(PageSize.A4, 50, 50, 50, 50);
                PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(outputPath, FileMode.Create));
                document.Open();

                // Polices
                PdfFont titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 20, BaseColor.DARK_GRAY);
                PdfFont headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14, BaseColor.BLACK);
                PdfFont normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 11, BaseColor.BLACK);
                PdfFont boldFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11, BaseColor.BLACK);

                // 📌 En-tête
                Paragraph title = new Paragraph("CONTRAT DE LOCATION", titleFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 20
                };
                document.Add(title);

                Paragraph locationNumber = new Paragraph($"N° Location : {location.Id}", boldFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 30
                };
                document.Add(locationNumber);

                // 📌 Informations Client
                document.Add(new Paragraph("INFORMATIONS CLIENT", headerFont) { SpacingAfter = 10 });
                PdfPTable clientTable = new PdfPTable(2) { WidthPercentage = 100, SpacingAfter = 20 };
                clientTable.SetWidths(new float[] { 1, 2 });

                AddTableRow(clientTable, "Nom complet :", $"{location.Client.Nom} {location.Client.Prenom}", boldFont, normalFont);
                AddTableRow(clientTable, "CIN :", location.Client.CIN, boldFont, normalFont);
                AddTableRow(clientTable, "Téléphone :", location.Client.Telephone, boldFont, normalFont);
                AddTableRow(clientTable, "Email :", location.Client.Email, boldFont, normalFont);
                AddTableRow(clientTable, "Adresse :", location.Client.Adresse, boldFont, normalFont);
                AddTableRow(clientTable, "Permis :", location.Client.PermisConduire, boldFont, normalFont);

                document.Add(clientTable);

                // 📌 Informations Véhicule
                document.Add(new Paragraph("INFORMATIONS VÉHICULE", headerFont) { SpacingAfter = 10 });
                PdfPTable vehicleTable = new PdfPTable(2) { WidthPercentage = 100, SpacingAfter = 20 };
                vehicleTable.SetWidths(new float[] { 1, 2 });

                AddTableRow(vehicleTable, "Véhicule :", $"{location.Voiture.Marque} {location.Voiture.Modele}", boldFont, normalFont);
                AddTableRow(vehicleTable, "Matricule :", location.Voiture.Matricule, boldFont, normalFont);
                AddTableRow(vehicleTable, "Type :", location.Voiture.TypeVoiture.Nom, boldFont, normalFont);
                AddTableRow(vehicleTable, "Kilométrage :", $"{location.Voiture.Kilometrage:F2} km", boldFont, normalFont);

                document.Add(vehicleTable);

                // 📌 Détails de la Location
                document.Add(new Paragraph("DÉTAILS DE LA LOCATION", headerFont) { SpacingAfter = 10 });
                PdfPTable locationTable = new PdfPTable(2) { WidthPercentage = 100, SpacingAfter = 20 };
                locationTable.SetWidths(new float[] { 1, 2 });

                int nbJours = (location.DateFin - location.DateDebut).Days;
                AddTableRow(locationTable, "Date de début :", location.DateDebut.ToString("dd/MM/yyyy à HH:mm"), boldFont, normalFont);
                AddTableRow(locationTable, "Date de fin :", location.DateFin.ToString("dd/MM/yyyy à HH:mm"), boldFont, normalFont);
                AddTableRow(locationTable, "Durée :", $"{nbJours} jour(s)", boldFont, normalFont);
                AddTableRow(locationTable, "Prix par jour :", $"{location.Voiture.PrixJour:F2} MAD", boldFont, normalFont);
                AddTableRow(locationTable, "Prix total :", $"{location.PrixTotal:F2} MAD", boldFont, normalFont);
                AddTableRow(locationTable, "Statut :", location.Statut, boldFont, normalFont);

                document.Add(locationTable);

                // 📌 QR Code
                byte[] qrCodeBytes = GenerateQRCode(location);
                PdfImage qrImage = PdfImage.GetInstance(qrCodeBytes);
                qrImage.ScaleAbsolute(120f, 120f);
                qrImage.Alignment = Element.ALIGN_CENTER;
                qrImage.SpacingBefore = 20;
                qrImage.SpacingAfter = 20;
                document.Add(qrImage);

                // 📌 Conditions
                document.Add(new Paragraph("CONDITIONS GÉNÉRALES", headerFont) { SpacingBefore = 20, SpacingAfter = 10 });
                Paragraph conditions = new Paragraph(
                    "1. Le locataire s'engage à restituer le véhicule dans l'état initial.\n" +
                    "2. Toute restitution tardive entraînera des frais supplémentaires.\n" +
                    "3. Le véhicule est assuré, mais le locataire reste responsable en cas de dommages.\n" +
                    "4. Le carburant doit être au même niveau qu'au départ.\n" +
                    "5. Interdiction de fumer dans le véhicule.\n" +
                    "6. Le kilométrage ne doit pas dépasser 200 km/jour (frais en cas de dépassement).",
                    normalFont
                )
                {
                    SpacingAfter = 30
                };
                document.Add(conditions);

                // 📌 Signatures
                PdfPTable signatureTable = new PdfPTable(2) { WidthPercentage = 100 };
                signatureTable.AddCell(new PdfPCell(new Phrase("Signature du Client\n\n\n\n", boldFont))
                {
                    Border = PdfRectangle.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_CENTER
                });
                signatureTable.AddCell(new PdfPCell(new Phrase("Signature de l'Agence\n\n\n\n", boldFont))
                {
                    Border = PdfRectangle.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_CENTER
                });
                document.Add(signatureTable);

                // 📌 Pied de page
                Paragraph footer = new Paragraph(
                    $"Document généré le {DateTime.Now:dd/MM/yyyy à HH:mm}\nLocation Voitures - Tous droits réservés",
                    FontFactory.GetFont(FontFactory.HELVETICA, 9, BaseColor.GRAY)
                )
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingBefore = 30
                };
                document.Add(footer);

                document.Close();
                writer.Close();
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur génération PDF : {ex.Message}");
            }
        }

        // 🔹 Génération de reçu de paiement
        public static void GeneratePaymentReceipt(Paiement paiement, string outputPath)
        {
            try
            {
                Document document = new Document(PageSize.A4, 50, 50, 50, 50);
                PdfWriter.GetInstance(document, new FileStream(outputPath, FileMode.Create));
                document.Open();

                PdfFont titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18, BaseColor.BLACK);
                PdfFont normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 11, BaseColor.BLACK);
                PdfFont boldFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11, BaseColor.BLACK);

                document.Add(new Paragraph("REÇU DE PAIEMENT", titleFont) { Alignment = Element.ALIGN_CENTER, SpacingAfter = 30 });

                PdfPTable table = new PdfPTable(2) { WidthPercentage = 100 };
                table.SetWidths(new float[] { 1, 2 });

                AddTableRow(table, "N° Reçu :", paiement.Id.ToString(), boldFont, normalFont);
                AddTableRow(table, "Date :", paiement.DatePaiement.ToString("dd/MM/yyyy à HH:mm"), boldFont, normalFont);
                AddTableRow(table, "Client :", $"{paiement.Location.Client.Nom} {paiement.Location.Client.Prenom}", boldFont, normalFont);
                AddTableRow(table, "N° Location :", paiement.LocationId.ToString(), boldFont, normalFont);
                AddTableRow(table, "Montant payé :", $"{paiement.Montant:F2} MAD", boldFont, normalFont);
                AddTableRow(table, "Mode de paiement :", paiement.ModePaiement, boldFont, normalFont);
                AddTableRow(table, "Statut :", paiement.Statut, boldFont, normalFont);

                document.Add(table);

                document.Add(new Paragraph($"\n\nGénéré le {DateTime.Now:dd/MM/yyyy à HH:mm}",
                    FontFactory.GetFont(FontFactory.HELVETICA, 9, BaseColor.GRAY))
                { Alignment = Element.ALIGN_CENTER });

                document.Close();
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur génération reçu : {ex.Message}");
            }
        }

        // Méthode utilitaire pour ajouter une ligne au tableau
        private static void AddTableRow(PdfPTable table, string label, string value, PdfFont labelFont, PdfFont valueFont)
        {
            table.AddCell(new PdfPCell(new Phrase(label, labelFont))
            {
                Border = PdfRectangle.NO_BORDER,
                PaddingBottom = 5
            });
            table.AddCell(new PdfPCell(new Phrase(value, valueFont))
            {
                Border = PdfRectangle.NO_BORDER,
                PaddingBottom = 5
            });
        }
    }
}
