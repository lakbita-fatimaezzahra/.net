using System;
using System.Net;
using System.Net.Mail;

namespace LocationVoitures.BackOffice.Utils
{
    public class EmailHelper
    {
        private static readonly string SmtpEmail = "lakbitafatimaezzahra@gmail.com";
        private static readonly string SmtpPassword = "nxcr xyko bcmc uagk";

        // Email de création de compte (existant)
        public static void SendPasswordEmail(string email, string nom, string prenom, string plainPassword)
        {
            var smtp = CreateSmtpClient();
            string subject = "Votre compte - Location Voitures";
            string body =
                $"Bonjour {prenom} {nom},\n\n" +
                "Votre compte a été créé avec succès.\n\n" +
                $"🔑 Identifiant : {email}\n" +
                $"🔐 Mot de passe : {plainPassword}\n\n" +
                "⚠️ Pensez à changer votre mot de passe après la première connexion.\n\n" +
                "Cordialement,\n" +
                "L'équipe Location Voitures";

            SendEmail(smtp, email, subject, body);
        }

        // 1️⃣ Confirmation de réservation
        public static void SendReservationConfirmation(string email, string nomClient, string marqueVoiture,
            string modeleVoiture, DateTime dateDebut, DateTime dateFin, double prixTotal, string numeroReservation)
        {
            var smtp = CreateSmtpClient();
            string subject = $"Confirmation de réservation #{numeroReservation}";
            string body =
                $"Bonjour {nomClient},\n\n" +
                "✅ Votre réservation a été confirmée avec succès !\n\n" +
                "📋 Détails de la réservation :\n" +
                $"   • N° réservation : {numeroReservation}\n" +
                $"   • Véhicule : {marqueVoiture} {modeleVoiture}\n" +
                $"   • Date de début : {dateDebut:dd/MM/yyyy}\n" +
                $"   • Date de fin : {dateFin:dd/MM/yyyy}\n" +
                $"   • Durée : {(dateFin - dateDebut).Days} jour(s)\n" +
                $"   • Prix total : {prixTotal:F2} MAD\n\n" +
                "📄 Un PDF avec QR Code vous sera envoyé séparément.\n\n" +
                "Merci de votre confiance !\n\n" +
                "Cordialement,\n" +
                "L'équipe Location Voitures";

            SendEmail(smtp, email, subject, body);
        }

        // 2️⃣ Rappel fin de location
        public static void SendLocationEndReminder(string email, string nomClient, string marqueVoiture,
            DateTime dateFin, string numeroLocation)
        {
            var smtp = CreateSmtpClient();
            string subject = $"Rappel : Fin de location #{numeroLocation}";
            string body =
                $"Bonjour {nomClient},\n\n" +
                "⏰ Votre location arrive bientôt à échéance.\n\n" +
                "📋 Détails :\n" +
                $"   • N° location : {numeroLocation}\n" +
                $"   • Véhicule : {marqueVoiture}\n" +
                $"   • Date de fin : {dateFin:dd/MM/yyyy à HH:mm}\n\n" +
                "⚠️ Merci de restituer le véhicule avant cette date pour éviter des frais supplémentaires.\n\n" +
                "Cordialement,\n" +
                "L'équipe Location Voitures";

            SendEmail(smtp, email, subject, body);
        }

        // 3️⃣ Rappel entretien véhicule (pour admin)
        public static void SendMaintenanceReminder(string emailAdmin, string matricule, string marque,
            string modele, string typeEntretien, DateTime prochainEntretien)
        {
            var smtp = CreateSmtpClient();
            string subject = $"⚠️ Entretien requis - {matricule}";
            string body =
                $"Bonjour,\n\n" +
                "🔧 Un véhicule nécessite un entretien :\n\n" +
                "📋 Détails :\n" +
                $"   • Véhicule : {marque} {modele}\n" +
                $"   • Matricule : {matricule}\n" +
                $"   • Type d'entretien : {typeEntretien}\n" +
                $"   • Date prévue : {prochainEntretien:dd/MM/yyyy}\n\n" +
                "⚠️ Planifiez cet entretien rapidement pour maintenir la flotte en bon état.\n\n" +
                "Cordialement,\n" +
                "Système de gestion - Location Voitures";

            SendEmail(smtp, emailAdmin, subject, body);
        }

        // 4️⃣ Facture / Reçu de paiement
        public static void SendPaymentReceipt(string email, string nomClient, string numeroLocation,
            double montant, string modePaiement, DateTime datePaiement, double resteAPayer)
        {
            var smtp = CreateSmtpClient();
            string subject = $"Reçu de paiement - Location #{numeroLocation}";
            string body =
                $"Bonjour {nomClient},\n\n" +
                "✅ Nous avons bien reçu votre paiement.\n\n" +
                "📋 Détails du paiement :\n" +
                $"   • N° location : {numeroLocation}\n" +
                $"   • Montant payé : {montant:F2} MAD\n" +
                $"   • Mode de paiement : {modePaiement}\n" +
                $"   • Date : {datePaiement:dd/MM/yyyy à HH:mm}\n" +
                $"   • Reste à payer : {resteAPayer:F2} MAD\n\n" +
                (resteAPayer > 0
                    ? "⚠️ Un solde reste à régler avant la fin de la location.\n\n"
                    : "✅ Votre location est entièrement payée.\n\n") +
                "Merci de votre confiance !\n\n" +
                "Cordialement,\n" +
                "L'équipe Location Voitures";

            SendEmail(smtp, email, subject, body);
        }

        // 5️⃣ Location terminée avec frais supplémentaires
        public static void SendLocationClosed(string email, string nomClient, string numeroLocation,
            double prixBase, double fraisSupplementaires, double montantTotal)
        {
            var smtp = CreateSmtpClient();
            string subject = $"Location clôturée #{numeroLocation}";
            string body =
                $"Bonjour {nomClient},\n\n" +
                "✅ Votre location a été clôturée avec succès.\n\n" +
                "📋 Résumé financier :\n" +
                $"   • Prix de base : {prixBase:F2} MAD\n" +
                $"   • Frais supplémentaires : {fraisSupplementaires:F2} MAD\n" +
                $"   • Montant total : {montantTotal:F2} MAD\n\n" +
                (fraisSupplementaires > 0
                    ? "⚠️ Des frais supplémentaires ont été appliqués (dépassement km, retard, etc.).\n\n"
                    : "") +
                "Merci d'avoir choisi nos services !\n\n" +
                "Cordialement,\n" +
                "L'équipe Location Voitures";

            SendEmail(smtp, email, subject, body);
        }

        // 6️⃣ Annulation de réservation
        public static void SendCancellationNotification(string email, string nomClient, string numeroLocation)
        {
            var smtp = CreateSmtpClient();
            string subject = $"Annulation de réservation #{numeroLocation}";
            string body =
                $"Bonjour {nomClient},\n\n" +
                "❌ Votre réservation a été annulée.\n\n" +
                $"   • N° réservation : {numeroLocation}\n\n" +
                "Si vous n'êtes pas à l'origine de cette annulation, " +
                "veuillez nous contacter immédiatement.\n\n" +
                "Cordialement,\n" +
                "L'équipe Location Voitures";

            SendEmail(smtp, email, subject, body);
        }

        // 🔧 Méthodes privées utilitaires
        private static SmtpClient CreateSmtpClient()
        {
            return new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(SmtpEmail, SmtpPassword),
                EnableSsl = true,
                Timeout = 10000
            };
        }

        private static void SendEmail(SmtpClient smtp, string toEmail, string subject, string body)
        {
            try
            {
                var mailMessage = new MailMessage
                {
                    From = new MailAddress(SmtpEmail, "Location Voitures"),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = false
                };
                mailMessage.To.Add(toEmail);

                smtp.Send(mailMessage);
                Console.WriteLine($"✅ Email envoyé à {toEmail}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erreur envoi email : {ex.Message}");
                throw;
            }
        }
    }
}