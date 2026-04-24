using ExamenII.Data;
using ExamenII.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ExamenII.Controllers
{
    [Authorize]
    public class EvidenciasController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public EvidenciasController(
            AppDbContext context,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment env)
        {
            _context = context;
            _userManager = userManager;
            _env = env;
        }

        // =========================
        // LISTADO
        // =========================
        public IActionResult Index()
        {
            var userId = _userManager.GetUserId(User);

            var data = User.IsInRole("Administrador")
                ? _context.Evidencias.ToList()
                : _context.Evidencias.Where(x => x.UserId == userId).ToList();

            return View(data);
        }

        // =========================
        // CREATE GET
        // =========================
        public IActionResult Create()
        {
            return View();
        }

        // =========================
        // CREATE POST 
        // =========================
        [HttpPost]
        public async Task<IActionResult> Create(string titulo, string descripcion, IFormFile imagen)
        {
            try
            {
                var userId = _userManager.GetUserId(User);

                if (string.IsNullOrWhiteSpace(titulo) ||
                    string.IsNullOrWhiteSpace(descripcion) ||
                    imagen == null || imagen.Length == 0)
                {
                    ModelState.AddModelError("", "Todos los campos son obligatorios");
                    return View();
                }

                var count = _context.Evidencias.Count(x => x.UserId == userId);
                if (count >= 5)
                {
                    ModelState.AddModelError("", "Máximo 5 evidencias permitidas");
                    return View();
                }

                var allowed = new[] { ".jpg", ".jpeg", ".png" };
                var ext = Path.GetExtension(imagen.FileName).ToLower();

                if (!allowed.Contains(ext))
                {
                    ModelState.AddModelError("", "Solo JPG, JPEG o PNG");
                    return View();
                }

                if (imagen.Length > 10 * 1024 * 1024)
                {
                    ModelState.AddModelError("", "Máximo 10MB");
                    return View();
                }

                var webRoot = _env.WebRootPath;

                if (string.IsNullOrWhiteSpace(webRoot))
                    return StatusCode(500, "WWWROOT NOT FOUND");

                var folder = Path.Combine(webRoot, "uploads");

                Directory.CreateDirectory(folder);

                var fileName = Guid.NewGuid() + ext;
                var fullPath = Path.Combine(folder, fileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await imagen.CopyToAsync(stream);
                }

                var evidencia = new Evidencia
                {
                    Titulo = titulo.Trim(),
                    Descripcion = descripcion.Trim(),
                    ImagenPath = "/uploads/" + fileName,
                    FechaRegistro = DateTime.Now,
                    UserId = userId
                };

                _context.Evidencias.Add(evidencia);
                _context.SaveChanges();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error interno: " + ex.Message);
                return View();
            }
        }

        // =========================
        // DELETE 
        // =========================
        public IActionResult Delete(int id)
        {
            var evidencia = _context.Evidencias.FirstOrDefault(x => x.Id == id);

            if (evidencia == null)
                return RedirectToAction(nameof(Index));

            if (!User.IsInRole("Administrador"))
                return Forbid();

            if (DateTime.Now > evidencia.FechaRegistro.AddMinutes(10))
            {
                TempData["Error"] = "No se puede eliminar después de 10 minutos";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var webRoot = _env.WebRootPath;

                if (!string.IsNullOrWhiteSpace(webRoot) &&
                    !string.IsNullOrWhiteSpace(evidencia.ImagenPath))
                {
                    var relative = evidencia.ImagenPath.TrimStart('/');
                    var fullPath = Path.Combine(webRoot, relative);

                    if (System.IO.File.Exists(fullPath))
                        System.IO.File.Delete(fullPath);
                }
            }
            catch
            {
            }

            _context.Evidencias.Remove(evidencia);
            _context.SaveChanges();

            TempData["Success"] = "Evidencia eliminada correctamente";

            return RedirectToAction(nameof(Index));
        }
    }
}