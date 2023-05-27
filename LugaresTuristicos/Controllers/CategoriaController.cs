using LugaresTuristicos.Commod;
using LugaresTuristicos.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace LugaresTuristicos.Controllers
{
    public class CategoriaController : Controller
    {
        private readonly ILogger<CategoriaController> _logger;
        private SitesContext _dbContext = new SitesContext();

        public CategoriaController(ILogger<CategoriaController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            var categorias = _dbContext.Categorias.ToList();

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

            return View(categorias);
        }

        public IActionResult Create()
        {
            var categoria = new Categoria();
            return View(categoria);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Categoria categoria)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (categoria.Estado == null)
                        categoria.Estado = false;

                    categoria.FechaCreacion = DateTime.Now;

                    // Lógica para guardar el usuario en la base de datos o realizar otras acciones necesarias
                    var existing = _dbContext.Categorias.FirstOrDefault(s => s.NombreCategoria.Equals(categoria.NombreCategoria));

                    if (existing != null)
                    {
                        TempData["ErrorMessage"] = "The category already exists.";
                        return RedirectToAction("Index");
                    }

                    _dbContext.Categorias.Add(categoria);
                    _dbContext.SaveChanges();

                    // Redirigir al usuario a la página de inicio de sesión después de un registro exitoso
                    TempData["MessageCorrectAdd"] = "Registro almacenado correctamente";
                    return RedirectToAction("Index");
                }
            }
            catch (DbUpdateException ex)
            {
                // Manejar la excepción específica de la base de datos (por ejemplo, violación de clave única)
                TempData["ErrorMessage"] = "Error al crear la categoria. Por favor, revise los datos ingresados.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // Manejar otras excepciones generales
                TempData["ErrorMessage"] = "Error al crear la categoria. Por favor, inténtelo nuevamente más tarde.";
                return RedirectToAction("Index");
            }

            return View(categoria);
        }

        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "The category does not exist.";
                return RedirectToAction("Index");
            }

            var categoria = _dbContext.Categorias.Find(id);
            if (categoria == null)
            {
                TempData["ErrorMessage"] = "The category does not exist.";
                return RedirectToAction("Index");
            }

            return View(categoria);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Categoria categoria)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var existingcategoria = _dbContext.Categorias.Find(id);
                    var existingNombrecategoria = _dbContext.Categorias.FirstOrDefault(s => s.NombreCategoria == categoria.NombreCategoria);

                    if (existingNombrecategoria != null)
                    {
                        if (existingcategoria.IdCategoria != existingNombrecategoria.IdCategoria)
                        {
                            TempData["ErrorMessage"] = "The category already exists.";
                            return RedirectToAction("Index");
                        }
                    }
                    else
                        existingNombrecategoria = existingcategoria;

                    _dbContext.Entry(existingcategoria).CurrentValues.SetValues(categoria);
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
                TempData["ErrorMessage"] = "Error al actualizar la categoria. Por favor, revise los datos ingresados.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // Manejar otras excepciones generales
                TempData["ErrorMessage"] = "Error al actualizar la categoria. Por favor, inténtelo nuevamente más tarde.";
                return RedirectToAction("Index");
            }

            @ViewBag.ErrorMessage = "Error. Por favor, intenta de nuevo.";
            return View(categoria);
        }

        public IActionResult Delete(int? id)
        {
            var categoria = _dbContext.Categorias.Find(id);
            if (categoria == null)
            {
                TempData["ErrorMessage"] = "The category do not match.";
                return RedirectToAction("Index");
            }

            return View(categoria);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            try
            {
                var categoria = _dbContext.Categorias.Find(id);
                if (categoria == null)
                {
                    TempData["ErrorMessage"] = "The category do not match.";
                    return RedirectToAction("Index");
                }

                _dbContext.Categorias.Remove(categoria);
                _dbContext.SaveChanges();

                TempData["MessageCorrectDelete"] = "Registro eliminado correctamente";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // Manejar cualquier excepción que pueda ocurrir al eliminar la categoria
                TempData["ErrorMessageDelete"] = "Ocurrio un error al procesar el formulario.";
                return RedirectToAction("Index");
            }
        }

        private bool CategoriaExists(int id)
        {
            return _dbContext.Categorias.Any(e => e.IdCategoria == id);
        }
    }
}
