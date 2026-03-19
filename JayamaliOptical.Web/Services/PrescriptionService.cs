using JayamaliOptical.Web.Data;
using JayamaliOptical.Web.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace JayamaliOptical.Web.Services
{
    public interface IPrescriptionService
    {
        Task<List<Prescription>> GetUserPrescriptionsAsync(string userId);
        Task<Prescription?> GetPrescriptionAsync(int id, string userId);
        Task<bool> CreatePrescriptionAsync(Prescription prescription, IFormFile? file);
        Task<bool> UpdatePrescriptionAsync(Prescription prescription, IFormFile? file);
        Task<bool> DeletePrescriptionAsync(int id, string userId);
        Task<bool> SetDefaultPrescriptionAsync(int id, string userId);
        string GetPrescriptionFilePath(string fileName);
    }

    public class PrescriptionService : IPrescriptionService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public PrescriptionService(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public async Task<List<Prescription>> GetUserPrescriptionsAsync(string userId)
        {
            return await _context.Prescriptions
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.IsDefault)
                .ThenByDescending(p => p.CreatedDate)
                .ToListAsync();
        }

        public async Task<Prescription?> GetPrescriptionAsync(int id, string userId)
        {
            return await _context.Prescriptions
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);
        }

        public async Task<bool> CreatePrescriptionAsync(Prescription prescription, IFormFile? file)
        {
            try
            {
                // Handle file upload
                if (file != null && file.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "prescriptions");
                    Directory.CreateDirectory(uploadsFolder);

                    var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    prescription.FileName = file.FileName;
                    prescription.FilePath = $"/uploads/prescriptions/{uniqueFileName}";
                    prescription.FileMimeType = file.ContentType;
                }

                // If this is the first prescription, make it default
                if (!await _context.Prescriptions.AnyAsync(p => p.UserId == prescription.UserId))
                {
                    prescription.IsDefault = true;
                }

                _context.Prescriptions.Add(prescription);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdatePrescriptionAsync(Prescription prescription, IFormFile? file)
        {
            try
            {
                var existing = await _context.Prescriptions.FindAsync(prescription.Id);
                if (existing == null) return false;

                // Update text fields
                existing.Name = prescription.Name;
                existing.Description = prescription.Description;
                existing.RightEyeSphere = prescription.RightEyeSphere;
                existing.RightEyeCylinder = prescription.RightEyeCylinder;
                existing.RightEyeAxis = prescription.RightEyeAxis;
                existing.RightEyeAdd = prescription.RightEyeAdd;
                existing.LeftEyeSphere = prescription.LeftEyeSphere;
                existing.LeftEyeCylinder = prescription.LeftEyeCylinder;
                existing.LeftEyeAxis = prescription.LeftEyeAxis;
                existing.LeftEyeAdd = prescription.LeftEyeAdd;
                existing.PupillaryDistance = prescription.PupillaryDistance;
                existing.IsDefault = prescription.IsDefault;
                existing.LastUsedDate = DateTime.Now;

                // Handle file upload (optional - keep existing if no new file)
                if (file != null && file.Length > 0)
                {
                    // Delete old file if exists
                    if (!string.IsNullOrEmpty(existing.FilePath))
                    {
                        var oldFilePath = Path.Combine(_environment.WebRootPath, existing.FilePath.TrimStart('/'));
                        if (File.Exists(oldFilePath))
                        {
                            File.Delete(oldFilePath);
                        }
                    }

                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "prescriptions");
                    Directory.CreateDirectory(uploadsFolder);

                    var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    existing.FileName = file.FileName;
                    existing.FilePath = $"/uploads/prescriptions/{uniqueFileName}";
                    existing.FileMimeType = file.ContentType;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeletePrescriptionAsync(int id, string userId)
        {
            var prescription = await _context.Prescriptions
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

            if (prescription == null) return false;

            // Delete file if exists
            if (!string.IsNullOrEmpty(prescription.FilePath))
            {
                var filePath = Path.Combine(_environment.WebRootPath, prescription.FilePath.TrimStart('/'));
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }

            _context.Prescriptions.Remove(prescription);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SetDefaultPrescriptionAsync(int id, string userId)
        {
            // Remove default from all user prescriptions
            var userPrescriptions = await _context.Prescriptions
                .Where(p => p.UserId == userId)
                .ToListAsync();

            foreach (var p in userPrescriptions)
            {
                p.IsDefault = false;
            }

            // Set new default
            var prescription = await _context.Prescriptions.FindAsync(id);
            if (prescription != null && prescription.UserId == userId)
            {
                prescription.IsDefault = true;
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public string GetPrescriptionFilePath(string fileName)
        {
            return Path.Combine(_environment.WebRootPath, "uploads", "prescriptions", fileName);
        }
    }
}