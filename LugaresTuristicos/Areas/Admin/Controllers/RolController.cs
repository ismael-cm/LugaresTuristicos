using LugaresTuristicos.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LugaresTuristicos.Areas.Admin.Controllers
{
    [Area("admin")]
    [Route("admin/[controller]/[action]")]
    public class RolController : Controller
    {
        private readonly ILogger<RolController> _logger;
        private SitesContext _dbContext = new SitesContext();

        public RolController(ILogger<RolController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            var roles = _dbContext.Rols.ToList();

            if (!string.IsNullOrEmpty(TempData["MessageCorrectAdd"] as string))
                @ViewBag.MessageCorrect = TempData["MessageCorrectAdd"];

            if (!string.IsNullOrEmpty(TempData["ErrorMessage"] as string))
                @ViewBag.ErrorMessage = TempData["ErrorMessage"];

            if (!string.IsNullOrEmpty(TempData["MessageCorrectEdit"] as string))
                @ViewBag.MessageCorrectEdit = TempData["MessageCorrectEdit"];

            if (!string.IsNullOrEmpty(TempData["MessageCorrectDelete"] as string))
                @ViewBag.MessageCorrectDelete = TempData["MessageCorrectDelete"];

            if (!string.IsNullOrEmpty(TempData["ErrorMessageDelete"] as string))
                @ViewBag.ErrorMessageDelete = TempData["ErrorMessageDelete"];

            return View(roles);
        }

        public IActionResult Create()
        {
            Rol rol = new Rol();
            return View(rol);
        }

        [HttpPost]
        public IActionResult Create(Rol rol)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (rol.Estado == null)
                        rol.Estado = false;

                    // Lógica para guardar el usuario en la base de datos o realizar otras acciones necesarias
                    var existing = _dbContext.Rols.FirstOrDefault(s => s.NombreRol.Equals(rol.NombreRol));

                    if (existing != null)
                    {
                        TempData["ErrorMessage"] = "The rol already exists.";
                        return RedirectToAction("Index");
                    }

                    _dbContext.Rols.Add(rol);
                    _dbContext.SaveChanges();

                    // Redirigir al usuario a la página de inicio de sesión después de un registro exitoso
                    TempData["MessageCorrectAdd"] = "Registro almacenado correctamente";
                    return RedirectToAction("Index");
                }
            }
            catch (DbUpdateException ex)
            {
                // Manejar la excepción específica de la base de datos (por ejemplo, violación de clave única)
                TempData["ErrorMessage"] = "Error al crear el rol. Por favor, revise los datos ingresados.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // Manejar otras excepciones generales
                TempData["ErrorMessage"] = "Error al crear el rol. Por favor, inténtelo nuevamente más tarde.";
                return RedirectToAction("Index");
            }

            return View(rol);
        }

        public IActionResult Edit(int id)
        {
            var rol = _dbContext.Rols.Find(id);
            if (rol == null)
            {
                TempData["ErrorMessage"] = "The rol do not match.";
                return RedirectToAction("Index");
            }

            @ViewBag.ErrorMessage = "Error. Por favor, intenta de nuevo.";
            return View(rol);
        }

        [HttpPost]
        public IActionResult Edit(int id, Rol rol)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var existingRol = _dbContext.Rols.Find(id);
                    var existingNombreRol = _dbContext.Rols.FirstOrDefault(s => s.NombreRol == rol.NombreRol);

                    if (existingNombreRol != null)
                    {
                        if (existingRol.IdRol != existingNombreRol.IdRol)
                        {
                            TempData["ErrorMessage"] = "The rol already exists.";
                            return RedirectToAction("Index");
                        }
                    }
                    else
                        existingNombreRol = existingRol;

                    _dbContext.Entry(existingRol).CurrentValues.SetValues(rol);
                    _dbContext.SaveChanges();

                    // Redirigir al usuario a la página de inicio de sesión después de un registro exitoso
                    TempData["MessageCorrectEdit"] = "Registro modificado correctamente";
                    return RedirectToAction("Index");
                }
            }
            catch (DbUpdateConcurrencyException ex)
            {
                // Manejar la excepción de concurrencia optimista
                TempData["ErrorMessage"] = "Los datos han sido modificados o eliminados por otro usuario. Por favor, actualice la página y vuelva a intentarlo.";
                return RedirectToAction("Index");
            }
            catch (DbUpdateException ex)
            {
                // Manejar la excepción específica de la base de datos
                TempData["ErrorMessage"] = "Error al actualizar el rol. Por favor, revise los datos ingresados.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // Manejar otras excepciones generales
                TempData["ErrorMessage"] = "Error al actualizar el rol. Por favor, inténtelo nuevamente más tarde.";
                return RedirectToAction("Index");
            }

            @ViewBag.ErrorMessage = "Error. Por favor, intenta de nuevo.";
            return View(rol);

        }

        public IActionResult Delete(int id)
        {
            var rol = _dbContext.Rols.Find(id);
            if (rol == null)
            {
                TempData["ErrorMessage"] = "The rol do not match.";
                return RedirectToAction("Index");
            }

            return View(rol);
        }

        [HttpPost]
        [ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            try
            {
                var rol = _dbContext.Rols.Find(id);
                if (rol == null)
                {
                    TempData["ErrorMessage"] = "The rol do not match.";
                    return RedirectToAction("Index");
                }

                _dbContext.Rols.Remove(rol);
                _dbContext.SaveChanges();

                TempData["MessageCorrectDelete"] = "Registro eliminado correctamente";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // Manejar cualquier excepción que pueda ocurrir al eliminar el rol
                TempData["ErrorMessageDelete"] = "Ocurrio un error al procesar el formulario.";
                return RedirectToAction("Index");
            }
        }
    }
}