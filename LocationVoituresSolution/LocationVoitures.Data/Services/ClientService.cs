using LocationVoitures.Data.models;
using LocationVoitures.Data.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace LocationVoitures.Data.Services
{
    public class ClientService
    {
        private readonly LocationContext _context;

        public ClientService()
        {
            _context = new LocationContext();
        }

        // Constructeur avec injection de dépendance (optionnel)
        public ClientService(LocationContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        // Récupérer tous les clients
        public List<Client> GetAllClients()
        {
            try
            {
                return _context.Clients
                    .OrderBy(c => c.Nom)
                    .ThenBy(c => c.Prenom)
                    .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur GetAllClients : {ex.Message}");
                throw;
            }
        }

        // Pagination pour les clients
        public PagedResult<Client> GetPagedClients(int page, int pageSize)
        {
            try
            {
                var query = _context.Clients
                    .OrderBy(c => c.Nom)
                    .ThenBy(c => c.Prenom);

                int totalItems = query.Count();
                int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

                var items = query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                return new PagedResult<Client>
                {
                    Items = items,
                    Page = page,
                    PageSize = pageSize,
                    TotalItems = totalItems,
                    TotalPages = totalPages
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur GetPagedClients : {ex.Message}");
                throw;
            }
        }

        // Ajouter un client
        public void AddClient(Client client)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));

            try
            {
                // Vérifier si le CIN existe déjà
                if (!string.IsNullOrEmpty(client.CIN))
                {
                    var existingCIN = _context.Clients.Any(c => c.CIN == client.CIN);
                    if (existingCIN)
                        throw new InvalidOperationException("Un client avec ce CIN existe déjà.");
                }

                // Vérifier si l'email existe déjà
                if (!string.IsNullOrEmpty(client.Email))
                {
                    var existingEmail = _context.Clients.Any(c => c.Email == client.Email);
                    if (existingEmail)
                        throw new InvalidOperationException("Un client avec cet email existe déjà.");
                }

                client.CreatedAt = DateTime.Now;
                client.UpdatedAt = DateTime.Now;

                _context.Clients.Add(client);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur AddClient : {ex.Message}");
                throw;
            }
        }

        // Modifier un client
        public void UpdateClient(Client client)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));

            try
            {
                var existing = _context.Clients.Find(client.Id);

                if (existing == null)
                    throw new InvalidOperationException("Client introuvable.");

                // Vérifier si le CIN existe déjà (sauf pour ce client)
                if (!string.IsNullOrEmpty(client.CIN))
                {
                    var duplicateCIN = _context.Clients.Any(c => c.CIN == client.CIN && c.Id != client.Id);
                    if (duplicateCIN)
                        throw new InvalidOperationException("Un autre client avec ce CIN existe déjà.");
                }

                // Vérifier si l'email existe déjà (sauf pour ce client)
                if (!string.IsNullOrEmpty(client.Email))
                {
                    var duplicateEmail = _context.Clients.Any(c => c.Email == client.Email && c.Id != client.Id);
                    if (duplicateEmail)
                        throw new InvalidOperationException("Un autre client avec cet email existe déjà.");
                }

                client.UpdatedAt = DateTime.Now;
                _context.Entry(existing).CurrentValues.SetValues(client);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur UpdateClient : {ex.Message}");
                throw;
            }
        }

        // Supprimer un client
        public bool DeleteClient(Client client)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));

            try
            {
                var existing = _context.Clients.Find(client.Id);

                if (existing == null)
                    return false;

                _context.Clients.Remove(existing);
                _context.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur DeleteClient : {ex.Message}");
                return false;
            }
        }

        // Supprimer un client par ID
        public bool DeleteClient(int clientId)
        {
            try
            {
                var client = _context.Clients.Find(clientId);

                if (client == null)
                    return false;

                _context.Clients.Remove(client);
                _context.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur DeleteClient : {ex.Message}");
                return false;
            }
        }

        // Rechercher par un champ spécifique
        public List<Client> SearchClients(string value, string field)
        {
            if (string.IsNullOrWhiteSpace(value))
                return GetAllClients();

            try
            {
                value = value.ToLower();
                var query = _context.Clients.AsQueryable();

                switch (field.ToLower())
                {
                    case "nom":
                        query = query.Where(c => c.Nom.ToLower().Contains(value));
                        break;
                    case "prenom":
                        query = query.Where(c => c.Prenom.ToLower().Contains(value));
                        break;
                    case "cin":
                        query = query.Where(c => c.CIN.ToLower().Contains(value));
                        break;
                    case "telephone":
                        query = query.Where(c => c.Telephone.ToLower().Contains(value));
                        break;
                    case "email":
                        query = query.Where(c => c.Email.ToLower().Contains(value));
                        break;
                    default:
                        // Recherche globale sur tous les champs
                        query = query.Where(c =>
                            c.Nom.ToLower().Contains(value) ||
                            c.Prenom.ToLower().Contains(value) ||
                            c.CIN.ToLower().Contains(value) ||
                            c.Telephone.ToLower().Contains(value) ||
                            c.Email.ToLower().Contains(value));
                        break;
                }

                return query.OrderBy(c => c.Nom).ThenBy(c => c.Prenom).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur SearchClients : {ex.Message}");
                throw;
            }
        }

        // Obtenir un client par ID
        public Client GetClientById(int id)
        {
            try
            {
                return _context.Clients.Find(id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur GetClientById : {ex.Message}");
                throw;
            }
        }

        // Vérifier si un CIN existe
        public bool CINExists(string cin, int? excludeId = null)
        {
            try
            {
                if (string.IsNullOrEmpty(cin))
                    return false;

                if (excludeId.HasValue)
                    return _context.Clients.Any(c => c.CIN == cin && c.Id != excludeId.Value);

                return _context.Clients.Any(c => c.CIN == cin);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur CINExists : {ex.Message}");
                return false;
            }
        }

        // Vérifier si un email existe
        public bool EmailExists(string email, int? excludeId = null)
        {
            try
            {
                if (string.IsNullOrEmpty(email))
                    return false;

                if (excludeId.HasValue)
                    return _context.Clients.Any(c => c.Email == email && c.Id != excludeId.Value);

                return _context.Clients.Any(c => c.Email == email);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur EmailExists : {ex.Message}");
                return false;
            }
        }

        // Libérer les ressources
        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}