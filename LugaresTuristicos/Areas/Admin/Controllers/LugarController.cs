using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LugaresTuristicos.Models;
using LugaresTuristicos.Commod;
using System.Text;
using Microsoft.AspNetCore.Authorization;

namespace LugaresTuristicos.Areas.Admin.Controllers
{
    [Area("admin")]
    [Route("admin/[controller]/[action]")]
    [Authorize(Roles = "ADMINISTRADOR")]
    public class LugarController : Controller
    {
        private readonly ILogger<LugarController> _logger;
        private SitesContext _dbContext = new SitesContext();

        public LugarController(ILogger<LugarController> logger)
        {
            _logger = logger;
        }

        // GET: Lugar
        public IActionResult Index()
        {

            var lugares = _dbContext.Lugares.Include(l => l.IdCategoriaNavigation)
                                           .Include(l => l.IdMunicipioNavigation)
                                           .Include(l => l.IdUsuarioNavigation)
                                           .ToList();

            if (!string.IsNullOrEmpty(TempData["MessageCorrectAdd"] as string))
                @ViewBag.MessageCorrect = TempData["MessageCorrectAdd"];

            if (!string.IsNullOrEmpty(TempData["ErrorMessagePassword"] as string))
                @ViewBag.ErrorMessagePassword = TempData["ErrorMessagePassword"];

            if (!string.IsNullOrEmpty(TempData["ErrorMessage"] as string))
                @ViewBag.ErrorMessage = TempData["ErrorMessage"];

            if (!string.IsNullOrEmpty(TempData["MessageCorrectEdit"] as string))
                @ViewBag.MessageCorrectEdit = TempData["MessageCorrectEdit"];

            if (!string.IsNullOrEmpty(TempData["MessageCorrectDelete"] as string))
                @ViewBag.MessageCorrectDelete = TempData["MessageCorrectDelete"];

            if (!string.IsNullOrEmpty(TempData["ErrorMessageDelete"] as string))
                @ViewBag.ErrorMessageDelete = TempData["ErrorMessageDelete"];

            return View(lugares);

        }

        // GET: Lugar/Create
        public IActionResult Create()
        {
            // Aquí puedes configurar cualquier dato adicional necesario antes de crear un nuevo lugar
            ViewBag.Categorias = _dbContext.Categorias.ToList();
            ViewBag.Municipios = _dbContext.Municipios.Include(l => l.IdDeptoNavigation).ToList();

            Lugare lugar = new Lugare();
            return View(lugar);
        }

        // POST: Lugar/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Lugare model, IFormFile imagenArchivo)
        {
            try
            {
                if (model != null)
                {
                    // Lógica para guardar el usuario en la base de datos o realizar otras acciones necesarias
                    var existing = _dbContext.Lugares.Find(model.IdLugar);

                    if (existing != null)
                    {
                        TempData["ErrorMessage"] = "The post already exists.";
                        return RedirectToAction("Index");
                    }

                    // Metodo para convertir la imagen a typo Byte[]
                    if (imagenArchivo != null && imagenArchivo.Length > 0)
                    {
                        // Leer los datos del archivo de imagen en un arreglo de bytes
                        using (var stream = new MemoryStream())
                        {
                            imagenArchivo.CopyTo(stream);
                            model.Imagen = stream.ToArray();
                        }
                    }
                    else
                    {
                        @ViewBag.ErrorMessage = "Error. Por favor, selecciona una imagen.";
                        return RedirectToAction("Index");
                    }

                    model.IdUsuario = Convert.ToInt32(HttpContext.Session.GetString("IdUser"));
                    model.FechaPublicacion = DateTime.Now;

                    _dbContext.Lugares.Add(model);
                    _dbContext.SaveChanges();

                    // Redirigir al usuario a la página de inicio de sesión después de un registro exitoso
                    TempData["MessageCorrectAdd"] = "Registro almacenado correctamente";
                    return RedirectToAction("Index");
                }
                else
                {
                    @ViewBag.ErrorMessage = "Error. Por favor, intenta de nuevo.";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                @ViewBag.ErrorMessage = "Error. Por favor, intenta de nuevo.";
                return RedirectToAction("Index");
            }
        }

        // GET: Lugar/Edit/5
        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "The place do not match.";
                return RedirectToAction("Index");
            }

            var lugares = _dbContext.Lugares.Find(id);
            if (lugares == null)
            {
                TempData["ErrorMessage"] = "The place do not match.";
                return RedirectToAction("Index");
            }

            ViewBag.Categorias = _dbContext.Categorias.ToList();
            ViewBag.Municipios = _dbContext.Municipios.ToList();

            return View(lugares);
        }

        // POST: Lugar/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Lugare model, IFormFile imagenArchivo)
        {
            try
            {
                if (model != null)
                {
                    if (id != model.IdLugar)
                    {
                        TempData["ErrorMessage"] = "The place do not match.";
                        return RedirectToAction("Index");
                    }

                    // Lógica para guardar el usuario en la base de datos o realizar otras acciones necesarias
                    var existing = _dbContext.Lugares.Find(model.IdLugar);

                    if (existing == null)
                    {
                        TempData["ErrorMessage"] = "The post not exists.";
                        return RedirectToAction("Index");
                    }

                    // Metodo para convertir la imagen a typo Byte[]
                    if (imagenArchivo != null && imagenArchivo.Length > 0)
                    {
                        // Leer los datos del archivo de imagen en un arreglo de bytes
                        using (var stream = new MemoryStream())
                        {
                            imagenArchivo.CopyTo(stream);
                            model.Imagen = stream.ToArray();
                        }
                    }
                    else
                        model.Imagen = existing.Imagen;

                    model.IdUsuario = existing.IdUsuario;
                    model.FechaPublicacion = existing.FechaPublicacion;

                    _dbContext.Entry(existing).CurrentValues.SetValues(model);
                    _dbContext.SaveChanges();

                    // Redirigir al usuario a la página de inicio de sesión después de un registro exitoso
                    TempData["MessageCorrectAdd"] = "Registro modificado correctamente";
                    return RedirectToAction("Index");
                }
                else
                {
                    @ViewBag.ErrorMessage = "Error. Por favor, intenta de nuevo.";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception)
            {
                @ViewBag.ErrorMessage = "Error. Por favor, intenta de nuevo.";
                return RedirectToAction("Index");
            }
        }

        // GET: Lugar/Delete/5
        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "The place do not match.";
                return RedirectToAction("Index");
            }

            var lugares = _dbContext.Lugares.Include(l => l.IdCategoriaNavigation)
                                           .Include(l => l.IdMunicipioNavigation)
                                           .Include(l => l.IdUsuarioNavigation)
                                           .FirstOrDefault(m => m.IdLugar == id);
            if (lugares == null)
            {
                TempData["ErrorMessage"] = "The place do not match.";
                return RedirectToAction("Index");
            }

            return View(lugares);
        }

        // POST: Lugare/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            try
            {
                var lugares = _dbContext.Lugares.Find(id);
                _dbContext.Lugares.Remove(lugares);
                _dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessageDelete"] = "Ocurrio un error al procesar el formulario.";
                return RedirectToAction("Index");
            }

            TempData["MessageCorrectDelete"] = "Registro eliminado correctamente";
            return RedirectToAction("Index");
        }
    }
}
