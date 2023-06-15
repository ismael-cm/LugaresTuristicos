using LugaresTuristicos.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.Diagnostics.Metrics;

namespace LugaresTuristicos.Areas.Admin.Controllers
{

    [Area("admin")]
    [Route("admin/[controller]/[action]")]
    [Authorize(Roles = "ADMINISTRADOR")]
    public class ReporteController : Controller
    {
        SitesContext _dbContext = new SitesContext();
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ReporteCategorias(string categoria)
        {
            var reportGenerator = new Reports();
            var filePath = "archivo.pdf";


            reportGenerator.GenerarPdfReporteClientes(filePath, _dbContext);

            byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, "application/pdf", "ReporteClientes.pdf");
        }

        public IActionResult LugaresMejorPuntuados()
        {
            List<Departamento> deptos = new List<Departamento>();
            deptos = _dbContext.Departamentos.ToList();

            return View(deptos);
        }

        [HttpPost]
        public IActionResult LugaresMejorPuntuados(string idDepartamento)
        {
            if (idDepartamento.IsNullOrEmpty() && !(idDepartamento is int))
            {
                return RedirectToAction("LugaresMejorPuntuados");
            }

            var reportGenerator = new Reports();
            var filePath = "archivo.pdf";


            reportGenerator.LugaresMejorPuntuados(filePath, _dbContext, int.Parse(idDepartamento));

            byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, "application/pdf", "Departamentos_Puntuados.pdf");
        }

        public IActionResult LugaresPorCategoria()
        {
            List<Categoria> categorias = new List<Categoria>();
            categorias = _dbContext.Categorias.ToList();

            return View(categorias);
        }

        [HttpPost]
        public IActionResult LugaresPorCategoria(string idCategoria)
        {
            if (idCategoria.IsNullOrEmpty() && !(idCategoria is int))
            {
                return RedirectToAction("LugaresPorCategoria");
            }

            var reportGenerator = new Reports();
            var filePath = "archivo.pdf";


            reportGenerator.LugaresPorCategoria(filePath, _dbContext, int.Parse(idCategoria));

            byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, "application/pdf", "Departamentos_categoria.pdf");
        }

        public IActionResult LugaresPorMunicipio()
        {
            List<Municipio> municipios = new List<Municipio>();
            municipios = _dbContext.Municipios.ToList();

            return View(municipios);
        }

        [HttpPost]
        public IActionResult LugaresPorMunicipio(string idMunicipio)
        {
            if (idMunicipio.IsNullOrEmpty() && !(idMunicipio is int))
            {
                return RedirectToAction("LugaresPorMunicipio");
            }

            var reportGenerator = new Reports();
            var filePath = "archivo.pdf";


            reportGenerator.LugaresPorMunicipio(filePath, _dbContext, int.Parse(idMunicipio));

            byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, "application/pdf", "Departamentos_municipio.pdf");
        }

        public IActionResult Emprendedores()
        {
            var reportGenerator = new Reports();
            var filePath = "archivo.pdf";


            reportGenerator.Emprendedores(filePath, _dbContext);

            byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, "application/pdf", "emprendedores.pdf");
        }


        public IActionResult LugaresGratuitos()
        {
            List<Departamento> deptos = new List<Departamento>();
            deptos = _dbContext.Departamentos.ToList();

            return View(deptos);
        }

        [HttpPost]
        public IActionResult LugaresGratuitos(string idDepartamento)
        {
            if (idDepartamento.IsNullOrEmpty() && !(idDepartamento is int))
            {
                return RedirectToAction("LugaresGratuitos");
            }

            var reportGenerator = new Reports();
            var filePath = "archivo.pdf";


            reportGenerator.LugaresGratuitos(filePath, _dbContext, int.Parse(idDepartamento));

            byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, "application/pdf", "Lugares_gratuitos.pdf");
        }
    }
}
