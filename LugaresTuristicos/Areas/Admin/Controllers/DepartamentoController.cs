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
    public class DepartamentoController : Controller
    {
        private readonly ILogger<DepartamentoController> _logger;
        private SitesContext _dbContext = new SitesContext();

        public DepartamentoController(ILogger<DepartamentoController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            var departamentos = _dbContext.Departamentos.ToList();

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

            return View(departamentos);
        }

        public IActionResult Create()
        {
            Departamento departamento = new Departamento();
            return View(departamento);
        }

        [HttpPost]
        public IActionResult Create(Departamento model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Lógica para guardar el usuario en la base de datos o realizar otras acciones necesarias
                    var existing = _dbContext.Departamentos.FirstOrDefault(s => s.Departamento1.Equals(model.Departamento1));

                    if (existing != null)
                    {
                        TempData["ErrorMessage"] = "The departament already exists.";
                        return RedirectToAction("Index");
                    }

                    _dbContext.Departamentos.Add(model);
                    _dbContext.SaveChanges();

                    // Redirigir al usuario a la página de inicio de sesión después de un registro exitoso
                    TempData["MessageCorrectAdd"] = "Registro almacenado correctamente";
                    return RedirectToAction("Index");
                }
            }
            catch (DbUpdateException ex)
            {
                // Manejar la excepción específica de la base de datos (por ejemplo, violación de clave única)
                TempData["ErrorMessage"] = "Error al crear el departamento. Por favor, revise los datos ingresados.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // Manejar otras excepciones generales
                TempData["ErrorMessage"] = "Error al crear el departamento. Por favor, inténtelo nuevamente más tarde.";
                return RedirectToAction("Index");
            }

            return View(model);
        }

        public IActionResult Edit(int id)
        {
            var model = _dbContext.Departamentos.Find(id);
            if (model == null)
            {
                TempData["ErrorMessage"] = "The departament do not match.";
                return RedirectToAction("Index");
            }

            @ViewBag.ErrorMessage = "Error. Por favor, intenta de nuevo.";
            return View(model);
        }

        [HttpPost]
        public IActionResult Edit(int id, Departamento model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var existingDepartamento = _dbContext.Departamentos.Find(id);
                    var existingNombreDepartamento = _dbContext.Departamentos.FirstOrDefault(s => s.Departamento1 == model.Departamento1);

                    if (existingDepartamento == null)
                    {
                        TempData["ErrorMessage"] = "The departament do not match.";
                        return RedirectToAction("Index");
                    }

                    if (existingNombreDepartamento != null)
                    {
                        TempData["ErrorMessage"] = "The departament already exists.";
                        return RedirectToAction("Index");
                    }

                    _dbContext.Entry(existingDepartamento).CurrentValues.SetValues(model);
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
                TempData["ErrorMessage"] = "Error al actualizar el departamento. Por favor, revise los datos ingresados.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // Manejar otras excepciones generales
                TempData["ErrorMessage"] = "Error al actualizar el departamento. Por favor, inténtelo nuevamente más tarde.";
                return RedirectToAction("Index");
            }

            @ViewBag.ErrorMessage = "Error. Por favor, intenta de nuevo.";
            return View(model);

        }

        public IActionResult Delete(int id)
        {
            var model = _dbContext.Departamentos.Find(id);
            if (model == null)
            {
                TempData["ErrorMessage"] = "The departament do not match.";
                return RedirectToAction("Index");
            }

            return View(model);
        }

        [HttpPost]
        [ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            try
            {
                var model = _dbContext.Departamentos.Find(id);
                if (model == null)
                {
                    TempData["ErrorMessage"] = "The departament do not match.";
                    return RedirectToAction("Index");
                }

                _dbContext.Departamentos.Remove(model);
                _dbContext.SaveChanges();

                TempData["MessageCorrectDelete"] = "Registro eliminado correctamente";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // Manejar cualquier excepción que pueda ocurrir al eliminar el departamento
                TempData["ErrorMessageDelete"] = "Ocurrio un error al procesar el formulario.";
                return RedirectToAction("Index");
            }
        }
    }
}