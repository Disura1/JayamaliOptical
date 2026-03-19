using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JayamaliOptical.Web.Models
{
    public class Prescription
    {
        public int Id { get; set; }

        // User Information
        public string UserId { get; set; } = string.Empty;
        public IdentityUser? User { get; set; }

        // Prescription Details
        [Required(ErrorMessage = "Prescription name is required")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;  // e.g., "My Regular Glasses"

        [StringLength(500)]
        public string? Description { get; set; }  // Optional notes

        // Prescription Values (for text-based prescriptions)
        [StringLength(20)]
        public string? RightEyeSphere { get; set; }  // OD Sphere
        [StringLength(20)]
        public string? RightEyeCylinder { get; set; }  // OD Cylinder
        [StringLength(20)]
        public string? RightEyeAxis { get; set; }  // OD Axis
        [StringLength(20)]
        public string? RightEyeAdd { get; set; }  // OD Add

        [StringLength(20)]
        public string? LeftEyeSphere { get; set; }  // OS Sphere
        [StringLength(20)]
        public string? LeftEyeCylinder { get; set; }  // OS Cylinder
        [StringLength(20)]
        public string? LeftEyeAxis { get; set; }  // OS Axis
        [StringLength(20)]
        public string? LeftEyeAdd { get; set; }  // OS Add

        [StringLength(20)]
        public string? PupillaryDistance { get; set; }  // PD

        // File Upload (Image/Document)
        [StringLength(255)]
        public string? FileName { get; set; }
        [StringLength(255)]
        public string? FilePath { get; set; }
        [StringLength(100)]
        public string? FileMimeType { get; set; }

        // Metadata
        public bool IsDefault { get; set; } = false;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? LastUsedDate { get; set; }
    }

    public class PrescriptionViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Prescription name is required")]
        [StringLength(100)]
        [Display(Name = "Prescription Name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "Notes (Optional)")]
        public string? Description { get; set; }

        // Right Eye (OD)
        [Display(Name = "Sphere (SPH)")]
        public string? RightEyeSphere { get; set; }
        [Display(Name = "Cylinder (CYL)")]
        public string? RightEyeCylinder { get; set; }
        [Display(Name = "Axis")]
        public string? RightEyeAxis { get; set; }
        [Display(Name = "Add")]
        public string? RightEyeAdd { get; set; }

        // Left Eye (OS)
        [Display(Name = "Sphere (SPH)")]
        public string? LeftEyeSphere { get; set; }
        [Display(Name = "Cylinder (CYL)")]
        public string? LeftEyeCylinder { get; set; }
        [Display(Name = "Axis")]
        public string? LeftEyeAxis { get; set; }
        [Display(Name = "Add")]
        public string? LeftEyeAdd { get; set; }

        [Display(Name = "Pupillary Distance (PD)")]
        public string? PupillaryDistance { get; set; }

        // File Upload
        [Display(Name = "Upload Prescription Image")]
        public IFormFile? PrescriptionFile { get; set; }

        public bool IsDefault { get; set; }
    }
}