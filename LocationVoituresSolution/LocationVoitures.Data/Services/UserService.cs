using LocationVoitures.Data.models;
using LocationVoitures.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Data.Entity;

namespace LocationVoitures.Data.Services
{
    public class UserService
    {
        private readonly LocationContext _context;

        public UserService() { try { _context = new LocationContext(); } catch (Exception ex) { MessageBox.Show(ex.Message, "Erreur LocationContext"); throw; } }
        public List<Utilisateur> GetAllUsers()
        {
            var users = _context.Utilisateurs.Include("Role").ToList();
            return users;
        }

       
    

public Utilisateur Login(string email, string password)
    {
        // 1) Chercher l'utilisateur par email
        var user = _context.Utilisateurs.FirstOrDefault(u => u.Email == email);

        if (user == null)
            return null;

        // 2) Vérifier le mot de passe hashé
        bool correctPassword = BCrypt.Net.BCrypt.Verify(password, user.Password);

        if (!correctPassword)
            return null;

        // 3) Si c'est un employé, recharger avec son rôle
        if (user is Employe)
        {
            user = _context.Employes
                           .Include(e => e.Role)
                           .FirstOrDefault(e => e.Id == user.Id);
        }

        return user;
    }

    // ✅ NOUVEAU: Mettre à jour le profil utilisateur
    public void UpdateProfile(int userId, string nom, string prenom, string email)
    {
        var user = _context.Utilisateurs.Find(userId);
        if (user == null)
            throw new Exception("Utilisateur introuvable.");

        user.Nom = nom;
        user.Prenom = prenom;
        user.Email = email;
        user.UpdatedAt = DateTime.Now;

        _context.SaveChanges();
    }

    // ✅ NOUVEAU: Changer le mot de passe
    public void ChangePassword(int userId, string currentPassword, string newPassword)
    {
        var user = _context.Utilisateurs.Find(userId);
        if (user == null)
            throw new Exception("Utilisateur introuvable.");

        // Vérifier le mot de passe actuel
        bool correctPassword = BCrypt.Net.BCrypt.Verify(currentPassword, user.Password);
        if (!correctPassword)
            throw new Exception("Mot de passe actuel incorrect.");

        // Hasher et enregistrer le nouveau mot de passe
        user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.UpdatedAt = DateTime.Now;

        _context.SaveChanges();
    }

    // ✅ NOUVEAU: Obtenir un utilisateur par ID
    public Utilisateur GetUserById(int userId)
    {
        var user = _context.Utilisateurs.Find(userId);
        
        if (user is Employe)
        {
            return _context.Employes
                           .Include(e => e.Role)
                           .FirstOrDefault(e => e.Id == userId);
        }

        return user;
    }

}
}
