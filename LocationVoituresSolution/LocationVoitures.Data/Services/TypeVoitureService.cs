using LocationVoitures.Data.models;
using LocationVoitures.Data.Models;
using System.Collections.Generic;
using System.Linq;

namespace LocationVoitures.Data.Services
{
    public class TypeVoitureService
    {
        private readonly LocationContext _context;

        public TypeVoitureService()
        {
            _context = new LocationContext();
        }

        public List<TypeVoiture> GetAllTypes()
        {
            return _context.TypesVoitures
                           .OrderBy(t => t.Nom)
                           .ToList();
        }

        public TypeVoiture GetById(int id)
        {
            return _context.TypesVoitures.FirstOrDefault(t => t.Id == id);
        }

        public void AddType(TypeVoiture type)
        {
            _context.TypesVoitures.Add(type);
            _context.SaveChanges();
        }

        public void UpdateType(TypeVoiture type)
        {
            var existing = _context.TypesVoitures.Find(type.Id);
            if (existing == null) return;

            existing.Nom = type.Nom;
            existing.Description = type.Description;

            _context.SaveChanges();
        }

        public void DeleteType(TypeVoiture type)
        {
            var existing = _context.TypesVoitures.Find(type.Id);
            if (existing == null) return;

            _context.TypesVoitures.Remove(existing);
            _context.SaveChanges();
        }
    }
}
