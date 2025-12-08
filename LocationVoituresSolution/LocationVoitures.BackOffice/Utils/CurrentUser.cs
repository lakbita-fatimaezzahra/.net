using LocationVoitures.Data.Models;

namespace LocationVoitures.BackOffice.Utils
{
    /// <summary>
    /// Classe statique pour stocker l'utilisateur connecté
    /// </summary>
    public static class CurrentUser
    {
        private static Utilisateur _user;

        /// <summary>
        /// Définit l'utilisateur connecté
        /// </summary>
        public static void SetUser(Utilisateur user)
        {
            _user = user;
        }

        /// <summary>
        /// Obtient l'utilisateur connecté
        /// </summary>
        public static Utilisateur GetUser()
        {
            return _user;
        }

        /// <summary>
        /// Obtient l'utilisateur connecté en tant qu'Employe
        /// </summary>
        public static Employe GetEmploye()
        {
            return _user as Employe;
        }

        /// <summary>
        /// Vérifie si un utilisateur est connecté
        /// </summary>
        public static bool IsLoggedIn()
        {
            return _user != null;
        }

        /// <summary>
        /// Déconnecte l'utilisateur
        /// </summary>
        public static void Logout()
        {
            _user = null;
        }

        /// <summary>
        /// Obtient le nom complet de l'utilisateur
        /// </summary>
        public static string GetFullName()
        {
            if (_user == null)
                return "Non connecté";
            
            return $"{_user.Prenom} {_user.Nom}";
        }

        /// <summary>
        /// Obtient l'email de l'utilisateur
        /// </summary>
        public static string GetEmail()
        {
            return _user?.Email ?? "";
        }

        /// <summary>
        /// Obtient le rôle de l'utilisateur (si c'est un employé)
        /// </summary>
        public static string GetRole()
        {
            if (_user is Employe employe && employe.Role != null)
                return employe.Role.Nom;
            
            return "Utilisateur";
        }
    }
}
