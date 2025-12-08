using LocationVoitures.Data.models;
using LocationVoitures.Data.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LocationVoitures.Data.Services
{
    public class ImageVoitureService
    {
        private readonly LocationContext _context;

        public ImageVoitureService()
        {
            _context = new LocationContext();
        }

        // --------------------------------------------------------------------
        // --- Récupérer toutes les images d'une voiture ---
        // --------------------------------------------------------------------
        public List<ImageVoiture> GetImagesByVoiture(int voitureId)
        {
            return _context.ImagesVoitures
                           .Where(img => img.VoitureId == voitureId)
                           .ToList();
        }

        // --- Récupérer une image par ID ---
        public ImageVoiture GetById(int id)
        {
            return _context.ImagesVoitures.FirstOrDefault(i => i.Id == id);
        }

        // --------------------------------------------------------------------
        // --- Ajouter une image depuis un fichier uploadé (SOLUTION) ---
        // --------------------------------------------------------------------
        public void AddImage(IFormFile file, int voitureId)
        {
            if (file == null || file.Length == 0)
                throw new InvalidOperationException("Fichier image invalide.");

            var voiture = _context.Voitures.Find(voitureId);
            if (voiture == null)
                throw new InvalidOperationException($"Voiture introuvable (Id = {voitureId}).");

            // 1) Dossier destination : wwwroot/uploads/voitures
            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/voitures");

            // 2) Créer si n’existe pas
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            // 3) Nom unique
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);

            // 4) Chemin physique
            string filePath = Path.Combine(uploadsFolder, fileName);

            // 5) Copier le fichier vers wwwroot
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            // 6) Enregistrer dans la base (chemin WEB, pas Windows)
            var image = new ImageVoiture
            {
                VoitureId = voitureId,
                Chemin = "/uploads/voitures/" + fileName
            };

            _context.ImagesVoitures.Add(image);
            _context.SaveChanges();
        }

        // --------------------------------------------------------------------
        // --- Ajouter une image depuis un chemin existant (ancienne méthode) ---
        // --------------------------------------------------------------------
        public void AddImage(ImageVoiture image)
        {
            if (string.IsNullOrWhiteSpace(image.Chemin))
                throw new InvalidOperationException("Chemin d’image invalide.");

            var voiture = _context.Voitures.Find(image.VoitureId);
            if (voiture == null)
                throw new InvalidOperationException($"Voiture introuvable (Id = {image.VoitureId}).");

            _context.ImagesVoitures.Add(image);
            _context.SaveChanges();
        }

        // --------------------------------------------------------------------
        // --- Supprimer une image (DB + fichier physique) ---
        // --------------------------------------------------------------------
        public void DeleteImage(ImageVoiture image)
        {
            // Supprimer fichier physique
            if (!string.IsNullOrWhiteSpace(image.Chemin))
            {
                string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot" + image.Chemin.Replace("/", "\\"));

                if (File.Exists(filePath))
                    File.Delete(filePath);
            }

            // Supprimer en DB
            _context.ImagesVoitures.Remove(image);
            _context.SaveChanges();
        }

        // --- Supprimer toutes les images d’une voiture ---
        public void DeleteImagesByVoiture(int voitureId)
        {
            var images = _context.ImagesVoitures
                                 .Where(i => i.VoitureId == voitureId)
                                 .ToList();

            foreach (var img in images)
                DeleteImage(img);
        }
    }
}
