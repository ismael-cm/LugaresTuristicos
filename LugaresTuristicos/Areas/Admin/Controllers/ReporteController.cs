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

            return View();
        }

        [HttpGet]
        public IActionResult LugaresMejorPuntuado(string idDepartamento)
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
            return View();
        }

        [HttpGet]
        public IActionResult LugaresPorCategori(string idCategoria)
        {
            var reportGenerator = new Reports();
            var filePath = "archivo.pdf";


            reportGenerator.LugaresPorCategoria(filePath, _dbContext, int.Parse(idCategoria));

            byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, "application/pdf", "Departamentos_categoria.pdf");
        }

        public IActionResult LugaresPorMunicipio()
        {

            return View();
        }

        [HttpGet]
        public IActionResult LugaresPorMunicipi(string idMunicipio)
        {
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
            return View();
        }

        [HttpGet]
        public IActionResult LugaresGratuito(string idDepartamento)
        {
            var reportGenerator = new Reports();
            var filePath = "archivo.pdf";


            reportGenerator.LugaresGratuitos(filePath, _dbContext, int.Parse(idDepartamento));

            byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, "application/pdf", "Lugares_gratuitos.pdf");
        }

        [HttpPost]
        public JsonResult getDepto()
        {
            try
            {
                List<Departamento> lstdepto = _dbContext.Departamentos.ToList();
                return Json(lstdepto);
            }
            catch (Exception ex)
            {
                return Json(new { respuesta = "Error" });
            }

        }

        [HttpPost]
        public JsonResult getCategoria()
        {
            try
            {
                List<Categoria> lstCat = _dbContext.Categorias.ToList();
                return Json(lstCat);
            }
            catch (Exception ex)
            {
                return Json(new { respuesta = "Error" });
            }

        }

        [HttpPost]
        public IActionResult getTablaMunicipioByIdDepto(int id)
        {
            var muni = (from municipi in _dbContext.Municipios
                        where municipi.IdDepto == id && municipi.Estado == true
                        select new
                        {
                            id_muni = municipi.IdMunicipio,
                            municipio = municipi.Municipio1
                        }).ToList();

            return Json(muni);
        }

    }
}
