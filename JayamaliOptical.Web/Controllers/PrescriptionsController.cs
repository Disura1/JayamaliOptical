using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using JayamaliOptical.Web.Models;
using JayamaliOptical.Web.Services;
using System.Security.Claims;

namespace JayamaliOptical.Web.Controllers
{
    [Authorize]  // Require login
    public class PrescriptionsController : Controller
    {
        private readonly IPrescriptionService _prescriptionService;

        public PrescriptionsController(IPrescriptionService prescriptionService)
        {
            _prescriptionService = prescriptionService;
        }

        // GET: Prescriptions (List all user prescriptions)
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var prescriptions = await _prescriptionService.GetUserPrescriptionsAsync(userId);
            return View(prescriptions);
        }

        // GET: Prescriptions/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Prescriptions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PrescriptionViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var prescription = new Prescription
                {
                    UserId = userId,
                    Name = model.Name,
                    Description = model.Description,
                    RightEyeSphere = model.RightEyeSphere,
                    RightEyeCylinder = model.RightEyeCylinder,
                    RightEyeAxis = model.RightEyeAxis,
                    RightEyeAdd = model.RightEyeAdd,
                    LeftEyeSphere = model.LeftEyeSphere,
                    LeftEyeCylinder = model.LeftEyeCylinder,
                    LeftEyeAxis = model.LeftEyeAxis,
                    LeftEyeAdd = model.LeftEyeAdd,
                    PupillaryDistance = model.PupillaryDistance,
                    IsDefault = model.IsDefault,
                    CreatedDate = DateTime.Now
                };

                var result = await _prescriptionService.CreatePrescriptionAsync(prescription, model.PrescriptionFile);

                if (result)
                {
                    TempData["Success"] = "Prescription saved successfully!";
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError("", "Failed to save prescription. Please try again.");
            }
            return View(model);
        }

        // GET: Prescriptions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var prescription = await _prescriptionService.GetPrescriptionAsync(id.Value, userId);

            if (prescription == null) return NotFound();

            var model = new PrescriptionViewModel
            {
                Id = prescription.Id,
                Name = prescription.Name,
                Description = prescription.Description,
                RightEyeSphere = prescription.RightEyeSphere,
                RightEyeCylinder = prescription.RightEyeCylinder,
                RightEyeAxis = prescription.RightEyeAxis,
                RightEyeAdd = prescription.RightEyeAdd,
                LeftEyeSphere = prescription.LeftEyeSphere,
                LeftEyeCylinder = prescription.LeftEyeCylinder,
                LeftEyeAxis = prescription.LeftEyeAxis,
                LeftEyeAdd = prescription.LeftEyeAdd,
                PupillaryDistance = prescription.PupillaryDistance,
                IsDefault = prescription.IsDefault
            };

            return View(model);
        }

        // POST: Prescriptions/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PrescriptionViewModel model)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var prescription = await _prescriptionService.GetPrescriptionAsync(id, userId);
                if (prescription == null) return NotFound();

                prescription.Name = model.Name;
                prescription.Description = model.Description;
                prescription.RightEyeSphere = model.RightEyeSphere;
                prescription.RightEyeCylinder = model.RightEyeCylinder;
                prescription.RightEyeAxis = model.RightEyeAxis;
                prescription.RightEyeAdd = model.RightEyeAdd;
                prescription.LeftEyeSphere = model.LeftEyeSphere;
                prescription.LeftEyeCylinder = model.LeftEyeCylinder;
                prescription.LeftEyeAxis = model.LeftEyeAxis;
                prescription.LeftEyeAdd = model.LeftEyeAdd;
                prescription.PupillaryDistance = model.PupillaryDistance;
                prescription.IsDefault = model.IsDefault;

                var result = await _prescriptionService.UpdatePrescriptionAsync(prescription, model.PrescriptionFile);

                if (result)
                {
                    TempData["Success"] = "Prescription updated successfully!";
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError("", "Failed to update prescription. Please try again.");
            }
            return View(model);
        }

        // GET: Prescriptions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var prescription = await _prescriptionService.GetPrescriptionAsync(id.Value, userId);

            if (prescription == null) return NotFound();

            return View(prescription);
        }

        // POST: Prescriptions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _prescriptionService.DeletePrescriptionAsync(id, userId);

            if (result)
            {
                TempData["Success"] = "Prescription deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Prescriptions/SetDefault/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetDefault(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _prescriptionService.SetDefaultPrescriptionAsync(id, userId);

            if (result)
            {
                TempData["Success"] = "Default prescription updated!";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Prescriptions/Details/5 (View prescription)
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var prescription = await _prescriptionService.GetPrescriptionAsync(id.Value, userId);

            if (prescription == null) return NotFound();

            return View(prescription);
        }
    }
}