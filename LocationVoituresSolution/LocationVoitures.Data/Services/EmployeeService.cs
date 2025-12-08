using LocationVoitures.Data.models;
using LocationVoitures.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;

using System.Data.Entity;  // ← OBLIGATOIRE POUR Include()


namespace LocationVoitures.Data.Services
{
    public class EmployeeService
    {
        private readonly LocationContext _context;

        public EmployeeService(LocationContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        // Pagination
        public PagedResult<Employe> GetPagedEmployees(int page, int pageSize)
        {
            var query = _context.Employes
                .Include(e => e.Role)
                .OrderBy(e => e.Nom);

            int totalItems = query.Count();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            var items = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PagedResult<Employe>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = totalPages
            };
        }

        // Tous les employés
        public List<Employe> GetEmployeesUsers()
        {
            return _context.Employes
                           .Include(e => e.Role)
                           .ToList();
        }

        // Filtre par recherche
        public List<Employe> Filter(string search, string criterium)
        {
            if (string.IsNullOrWhiteSpace(search))
                return GetEmployeesUsers(); // retourne tous les employés si recherche vide

            search = search.ToLower();
            var query = _context.Employes.AsQueryable();

            switch (criterium)
            {
                case "Nom":
                    query = query.Where(emp => emp.Nom.ToLower().Contains(search));
                    break;
                case "Prenom":
                    query = query.Where(emp => emp.Prenom.ToLower().Contains(search));
                    break;
                case "Poste":
                    query = query.Where(emp => emp.Poste.ToLower().Contains(search));
                    break;
                default:
                    query = query.Where(emp =>
                        emp.Nom.ToLower().Contains(search) ||
                        emp.Prenom.ToLower().Contains(search) ||
                        emp.Poste.ToLower().Contains(search));
                    break;
            }

            return query.OrderBy(e => e.Nom).ToList();
        }

        public List<Role> GetRoles()
        {
            return _context.Roles.ToList();
        }

        public void AddUser(Utilisateur user)
        {
            _context.Utilisateurs.Add(user);
            _context.SaveChanges();
        }

        public void AddEmployee(Utilisateur user, string poste, double salaire, int role)
        {
            var employe = new Employe
            {
                Id = user.Id,
                Nom = user.Nom,
                Prenom = user.Prenom,
                Email = user.Email,
                Password = user.Password,
                RoleId = role,
                Poste = poste,
                Salaire = salaire,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            _context.Employes.Add(employe);
            _context.SaveChanges();
        }
        public void UpdateEmployee(Employe employee)
        {
            if (employee == null)
                throw new ArgumentNullException(nameof(employee));

            var emp = _context.Employes.Find(employee.Id);
            if (emp != null)
            {
                emp.Nom = employee.Nom;
                emp.Prenom = employee.Prenom;
                emp.Email = employee.Email;

                // Mettre à jour le mot de passe uniquement si fourni
                if (!string.IsNullOrEmpty(employee.Password))
                    emp.Password = employee.Password;

                emp.RoleId = employee.RoleId;
                emp.Poste = employee.Poste;
                emp.Salaire = employee.Salaire;
                emp.UpdatedAt = DateTime.Now;

                _context.SaveChanges();
            }
            else
            {
                throw new InvalidOperationException("Employé introuvable pour la mise à jour.");
            }
        }
        // Supprimer un employé par ID
        public bool DeleteEmployee(int employeeId)
        {
            try
            {
                var employee = _context.Employes.Find(employeeId);

                if (employee == null)
                    return false;

                _context.Employes.Remove(employee);  // ← supprime aussi l'utilisateur
                _context.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la suppression : {ex.Message}");
                return false;
            }
        }

    }
}
