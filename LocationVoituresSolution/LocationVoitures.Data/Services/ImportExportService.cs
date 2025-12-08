using LocationVoitures.Data.models;
using LocationVoitures.Data.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LocationVoitures.Data.Services
{
    public class ImportExportService
    {
        private readonly LocationContext _context;

        public ImportExportService(LocationContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        #region Export

        // Export Locations
        public void ExportLocations(string filePath, List<Location> locations)
        {
            try
            {
                using (var writer = new StreamWriter(filePath, false, Encoding.UTF8))
                {
                    // En-tête
                    writer.WriteLine("N° Location,Client Nom,Client Prénom,Client CIN,Véhicule Marque,Véhicule Modèle,Matricule,Date Début,Date Fin,Prix Total,Statut");

                    // Données
                    foreach (var loc in locations)
                    {
                        writer.WriteLine($"{loc.Id}," +
                            $"\"{loc.Client?.Nom ?? ""}\"," +
                            $"\"{loc.Client?.Prenom ?? ""}\"," +
                            $"\"{loc.Client?.CIN ?? ""}\"," +
                            $"\"{loc.Voiture?.Marque ?? ""}\"," +
                            $"\"{loc.Voiture?.Modele ?? ""}\"," +
                            $"\"{loc.Voiture?.Matricule ?? ""}\"," +
                            $"{loc.DateDebut:dd/MM/yyyy}," +
                            $"{loc.DateFin:dd/MM/yyyy}," +
                            $"{loc.PrixTotal:F2}," +
                            $"{loc.Statut}");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur lors de l'export des locations : {ex.Message}");
            }
        }

        // Export Employés
        public void ExportEmployees(string filePath, List<Employe> employees)
        {
            try
            {
                using (var writer = new StreamWriter(filePath, false, Encoding.UTF8))
                {
                    // En-tête
                    writer.WriteLine("ID,Nom,Prénom,Email,Poste,Salaire,Rôle,Date Création");

                    // Données
                    foreach (var emp in employees)
                    {
                        writer.WriteLine($"{emp.Id}," +
                            $"\"{emp.Nom}\"," +
                            $"\"{emp.Prenom}\"," +
                            $"\"{emp.Email}\"," +
                            $"\"{emp.Poste}\"," +
                            $"{emp.Salaire:F2}," +
                            $"\"{emp.Role?.Nom ?? ""}\"," +
                            $"{emp.CreatedAt:dd/MM/yyyy}");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur lors de l'export des employés : {ex.Message}");
            }
        }

        // Export Clients
        public void ExportClients(string filePath, List<Client> clients)
        {
            try
            {
                using (var writer = new StreamWriter(filePath, false, Encoding.UTF8))
                {
                    // En-tête
                    writer.WriteLine("ID,Nom,Prénom,Email,CIN,Téléphone,Adresse,Date Naissance,Permis Conduire,Date Création");

                    // Données
                    foreach (var client in clients)
                    {
                        writer.WriteLine($"{client.Id}," +
                            $"\"{client.Nom}\"," +
                            $"\"{client.Prenom}\"," +
                            $"\"{client.Email}\"," +
                            $"\"{client.CIN}\"," +
                            $"\"{client.Telephone}\"," +
                            $"\"{client.Adresse}\"," +
                            $"{client.DateNaissance:dd/MM/yyyy}," +
                            $"\"{client.PermisConduire}\"," +
                            $"{client.CreatedAt:dd/MM/yyyy}");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur lors de l'export des clients : {ex.Message}");
            }
        }

        // Export Voitures
        public void ExportVoitures(string filePath, List<Voiture> voitures)
        {
            try
            {
                using (var writer = new StreamWriter(filePath, false, Encoding.UTF8))
                {
                    // En-tête
                    writer.WriteLine("ID,Matricule,Marque,Modèle,Année,Prix/Jour,Statut,Kilométrage,Dernière Maintenance,Type");

                    // Données
                    foreach (var voiture in voitures)
                    {
                        writer.WriteLine($"{voiture.Id}," +
                            $"\"{voiture.Matricule}\"," +
                            $"\"{voiture.Marque}\"," +
                            $"\"{voiture.Modele}\"," +
                            $"{voiture.Annee:yyyy}," +
                            $"{voiture.PrixJour:F2}," +
                            $"\"{voiture.Statut}\"," +
                            $"{voiture.Kilometrage:F0}," +
                            $"{voiture.DerniereMaintenance:dd/MM/yyyy}," +
                            $"\"{voiture.TypeVoiture?.Nom ?? ""}\"");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur lors de l'export des voitures : {ex.Message}");
            }
        }

        #endregion

        #region Import

        // Import Clients
        public ImportResult ImportClients(string filePath)
        {
            var result = new ImportResult();

            try
            {
                var lines = File.ReadAllLines(filePath, Encoding.UTF8);

                if (lines.Length < 2)
                {
                    result.Errors.Add("Le fichier est vide ou ne contient pas de données.");
                    return result;
                }

                // Ignorer l'en-tête
                for (int i = 1; i < lines.Length; i++)
                {
                    try
                    {
                        var parts = ParseCsvLine(lines[i]);

                        if (parts.Length < 9)
                        {
                            result.Errors.Add($"Ligne {i + 1}: Format invalide (colonnes manquantes)");
                            continue;
                        }

                        var client = new Client
                        {
                            Nom = parts[1],
                            Prenom = parts[2],
                            Email = parts[3],
                            CIN = parts[4],
                            Telephone = parts[5],
                            Adresse = parts[6],
                            DateNaissance = DateTime.ParseExact(parts[7], "dd/MM/yyyy", null),
                            PermisConduire = parts[8],
                            Password = "123456", // Mot de passe par défaut
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        };

                        // Vérifier si le CIN existe déjà
                        if (_context.Clients.Any(c => c.CIN == client.CIN))
                        {
                            result.Errors.Add($"Ligne {i + 1}: Client avec CIN {client.CIN} existe déjà");
                            continue;
                        }

                        _context.Clients.Add(client);
                        result.SuccessCount++;
                    }
                    catch (Exception ex)
                    {
                        result.Errors.Add($"Ligne {i + 1}: {ex.Message}");
                    }
                }

                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Erreur générale : {ex.Message}");
            }

            return result;
        }

        // Import Employés
        public ImportResult ImportEmployees(string filePath)
        {
            var result = new ImportResult();

            try
            {
                var lines = File.ReadAllLines(filePath, Encoding.UTF8);

                if (lines.Length < 2)
                {
                    result.Errors.Add("Le fichier est vide ou ne contient pas de données.");
                    return result;
                }

                for (int i = 1; i < lines.Length; i++)
                {
                    try
                    {
                        var parts = ParseCsvLine(lines[i]);

                        if (parts.Length < 6)
                        {
                            result.Errors.Add($"Ligne {i + 1}: Format invalide");
                            continue;
                        }

                        // Trouver le rôle
                        var roleName = parts.Length > 6 ? parts[6] : "Employé";
                        var role = _context.Roles.FirstOrDefault(r => r.Nom == roleName);

                        if (role == null)
                        {
                            result.Errors.Add($"Ligne {i + 1}: Rôle '{roleName}' introuvable");
                            continue;
                        }

                        var employee = new Employe
                        {
                            Nom = parts[1],
                            Prenom = parts[2],
                            Email = parts[3],
                            Poste = parts[4],
                            Salaire = double.Parse(parts[5]),
                            RoleId = role.Id,
                            Password = "123456", // Mot de passe par défaut
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        };

                        // Vérifier si l'email existe déjà
                        if (_context.Employes.Any(e => e.Email == employee.Email))
                        {
                            result.Errors.Add($"Ligne {i + 1}: Employé avec email {employee.Email} existe déjà");
                            continue;
                        }

                        _context.Employes.Add(employee);
                        result.SuccessCount++;
                    }
                    catch (Exception ex)
                    {
                        result.Errors.Add($"Ligne {i + 1}: {ex.Message}");
                    }
                }

                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Erreur générale : {ex.Message}");
            }

            return result;
        }

        // Import Voitures
        public ImportResult ImportVoitures(string filePath)
        {
            var result = new ImportResult();

            try
            {
                var lines = File.ReadAllLines(filePath, Encoding.UTF8);

                if (lines.Length < 2)
                {
                    result.Errors.Add("Le fichier est vide ou ne contient pas de données.");
                    return result;
                }

                for (int i = 1; i < lines.Length; i++)
                {
                    try
                    {
                        var parts = ParseCsvLine(lines[i]);

                        if (parts.Length < 10)
                        {
                            result.Errors.Add($"Ligne {i + 1}: Format invalide");
                            continue;
                        }

                        // Trouver le type de voiture
                        var typeName = parts[9];
                        var type = _context.TypesVoitures.FirstOrDefault(t => t.Nom == typeName);

                        if (type == null)
                        {
                            result.Errors.Add($"Ligne {i + 1}: Type '{typeName}' introuvable");
                            continue;
                        }

                        var voiture = new Voiture
                        {
                            Matricule = parts[1],
                            Marque = parts[2],
                            Modele = parts[3],
                            Annee = DateTime.ParseExact(parts[4], "yyyy", null),
                            PrixJour = double.Parse(parts[5]),
                            Statut = parts[6],
                            Kilometrage = double.Parse(parts[7]),
                            DerniereMaintenance = DateTime.ParseExact(parts[8], "dd/MM/yyyy", null),
                            TypeId = type.Id
                        };

                        // Vérifier si le matricule existe déjà
                        if (_context.Voitures.Any(v => v.Matricule == voiture.Matricule))
                        {
                            result.Errors.Add($"Ligne {i + 1}: Voiture avec matricule {voiture.Matricule} existe déjà");
                            continue;
                        }

                        _context.Voitures.Add(voiture);
                        result.SuccessCount++;
                    }
                    catch (Exception ex)
                    {
                        result.Errors.Add($"Ligne {i + 1}: {ex.Message}");
                    }
                }

                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Erreur générale : {ex.Message}");
            }

            return result;
        }

        #endregion

        #region Helpers

        // Parser une ligne CSV en tenant compte des guillemets
        private string[] ParseCsvLine(string line)
        {
            var result = new List<string>();
            var current = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(current.ToString().Trim());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }

            result.Add(current.ToString().Trim());
            return result.ToArray();
        }

        #endregion
    }

    // Classe pour stocker les résultats d'import
    public class ImportResult
    {
        public int SuccessCount { get; set; }
        public List<string> Errors { get; set; }

        public ImportResult()
        {
            Errors = new List<string>();
        }

        public bool HasErrors => Errors.Count > 0;
        public string GetErrorSummary() => string.Join("\n", Errors);
    }
}