using LugaresTuristicos.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;


namespace LugaresTuristicos.Areas.Admin.Controllers {
    [Area("admin")]
    [Route("admin/[controller]/[action]")]
    public class MunicipioController : Controller
    {
        private readonly ILogger<MunicipioController> _logger;
        private SitesContext _dbContext = new SitesContext();

        public MunicipioController(ILogger<MunicipioController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            var municipios = _dbContext.Municipios.Include(l => l.IdDeptoNavigation).ToList();

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

            return View(municipios);
        }

        public IActionResult Create()
        {
            ViewBag.Departamentos = _dbContext.Departamentos.ToList();
            Municipio municipio = new Municipio();
            return View(municipio);
        }

        [HttpPost]
        public IActionResult Create(Municipio model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Lógica para guardar el usuario en la base de datos o realizar otras acciones necesarias
                    var existing = _dbContext.Municipios.FirstOrDefault(s => s.Municipio1.Equals(model));

                    if (existing != null)
                    {
                        TempData["ErrorMessage"] = "The Municipio already exists.";
                        return RedirectToAction("Index");
                    }

                    _dbContext.Municipios.Add(model);
                    _dbContext.SaveChanges();

                    // Redirigir al usuario a la página de inicio de sesión después de un registro exitoso
                    TempData["MessageCorrectAdd"] = "Registro almacenado correctamente";
                    return RedirectToAction("Index");
                }
            }
            catch (DbUpdateException ex)
            {
                // Manejar la excepción específica de la base de datos (por ejemplo, violación de clave única)
                TempData["ErrorMessage"] = "Error al crear el Municipio. Por favor, revise los datos ingresados.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // Manejar otras excepciones generales
                TempData["ErrorMessage"] = "Error al crear el rol. Por favor, inténtelo nuevamente más tarde.";
                return RedirectToAction("Index");
            }

            return View(model);
        }

        public IActionResult Edit(int id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "The Municipio do not match.";
                return RedirectToAction("Index");
            }

            var municipio = _dbContext.Municipios.Find(id);
            ViewBag.Departamentos = _dbContext.Departamentos.ToList();

            if (municipio == null)
            {
                TempData["ErrorMessage"] = "The Municipio do not match.";
                return RedirectToAction("Index");
            }

            return View(municipio);
        }

        [HttpPost]
        public IActionResult Edit(int id, Municipio model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var existingModel = _dbContext.Municipios.Find(id);
                    var existingMunicipio = _dbContext.Municipios.FirstOrDefault(s => s.Municipio1 == model.Municipio1);

                    if (existingMunicipio != null)
                    {
                        if (existingModel.IdMunicipio != existingMunicipio.IdMunicipio)
                        {
                            TempData["ErrorMessage"] = "The Municipio already exists.";
                            return RedirectToAction("Index");
                        }
                    }
                    else
                        existingMunicipio = existingModel;

                    _dbContext.Entry(existingModel).CurrentValues.SetValues(model);
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
            return View(model);

        }

        public IActionResult Delete(int id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "The municipio do not match.";
                return RedirectToAction("Index");
            }

            var municipio = _dbContext.Municipios.Include(l => l.IdDeptoNavigation)
                                            .FirstOrDefault(u => u.IdMunicipio == id);

            if (municipio == null)
            {
                TempData["ErrorMessage"] = "The municipio do not match.";
                return RedirectToAction("Index");
            }

            return View(municipio);
        }

        [HttpPost]
        [ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            try
            {
                var model = _dbContext.Municipios.Find(id);
                if (model == null)
                {
                    TempData["ErrorMessage"] = "The Municipio do not match.";
                    return RedirectToAction("Index");
                }

                _dbContext.Municipios.Remove(model);
                _dbContext.SaveChanges();

                TempData["MessageCorrectDelete"] = "Registro eliminado correctamente";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // Manejar cualquier excepción que pueda ocurrir al eliminar el Municipio
                TempData["ErrorMessageDelete"] = "Ocurrio un error al procesar el formulario.";
                return RedirectToAction("Index");
            }
        }
    }
}