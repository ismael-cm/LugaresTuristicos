using LugaresTuristicos.Commod;
using LugaresTuristicos.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace LugaresTuristicos.Areas.Admin.Controllers.Controllers
{
    [Area("admin")]
    [Route("admin/[controller]/[action]")]
    [Authorize(Roles = "ADMINISTRADOR")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private SitesContext _dbContext = new SitesContext();
        private ValidationClass validationClass = new ValidationClass();
        SitesContext contexto = new SitesContext();

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Dashboard()
        {
            try
            {
                List<Lugare> lista = _dbContext.Lugares.Include(l => l.Comentarios)
                                                       .Include(l => l.IdMunicipioNavigation)
                                                       .Include(l => l.IdCategoriaNavigation)
                                                       .Include(l => l.IdUsuarioNavigation)
                                                       .ToList();
                return View(lista);
            }
            catch (Exception ex)
            {
                Lugare model = new Lugare();
                return View(model);
            }
        }


        [HttpPost]
        public IActionResult getTablaBlackList()
        {
            List<Blacklist> lstBlackList = contexto.Blacklists.Where(x => x.Estado == true).ToList();
            return Json(lstBlackList);
        }
        [HttpPost]
        public IActionResult getTablaMunicipio()
        {
            //List<Municipio> lstMunicipio = contexto.Municipios.Where(x => x.Estado == true).ToList();
            var lstMunicipio = (from municipio in contexto.Municipios
                                join departamento in contexto.Departamentos
                                on municipio.IdDepto equals departamento.IdDepto
                                where municipio.Estado == true
                                select new
                                {
                                    idMunicipio = municipio.IdMunicipio,
                                    municipio1 = municipio.Municipio1,
                                    idDepto = municipio.IdDepto,
                                    depto = departamento.Departamento1
                                }).ToList();
            return Json(lstMunicipio);
        }

        [HttpPost]
        public IActionResult getTablaDepartamento()
        {
            List<Departamento> lstDepartamento = contexto.Departamentos.Where(x => x.Estado == true).ToList();
            return Json(lstDepartamento);
        }

        [HttpPost]
        public ActionResult guardarBlackList(Blacklist black)
        {
            Blacklist obj = new Blacklist();
            obj.Palabra = black.Palabra;
            obj.Estado = true;
            try
            {
                contexto.Blacklists.Add(obj);
                contexto.SaveChanges();
                return Json(true);
            }
            catch (Exception ex)
            {
                return Json(false);
            }
        }

        [HttpPost]
        public ActionResult guardarMunicipio(Municipio municipio)
        {
            Municipio obj = new Municipio();
            obj.IdDepto = municipio.IdDepto;
            obj.Municipio1 = municipio.Municipio1;
            obj.Estado = true;

            try
            {
                contexto.Municipios.Add(obj);
                contexto.SaveChanges();
                return Json(true);
            }
            catch (Exception ex)
            {
                return Json(false);
            }
        }

        [HttpPost]
        public ActionResult guardarDepartamento(Departamento departamento)
        {
            Departamento obj = new Departamento();
            obj.Departamento1 = departamento.Departamento1;
            obj.Estado = true;

            try
            {
                contexto.Departamentos.Add(obj);
                contexto.SaveChanges();
                return Json(true);
            }
            catch (Exception ex)
            {
                return Json(false);
            }
        }

        [HttpPost]
        public IActionResult EliminarBlackList(int id)
        {
            try
            {
                var objDel = contexto.Blacklists.FirstOrDefault(x => x.IdBlacklist == id);
                objDel.Estado = false;
                //contexto.Blacklists.Remove(objDel);
                contexto.Blacklists.Update(objDel);
                contexto.SaveChanges();
                return Json(true);
            }

            catch (Exception ex)
            {
                return Json(false);
            }

        }

        [HttpPost]
        public IActionResult EliminarMunicipio(int id)
        {
            try
            {
                var objDel = contexto.Municipios.FirstOrDefault(x => x.IdMunicipio == id);
                objDel.Estado = false;
                //contexto.Municipios.Remove(objDel);
                contexto.Municipios.Update(objDel);
                contexto.SaveChanges();
                return Json(true);
            }

            catch (Exception ex)
            {
                return Json(false);
            }

        }

        [HttpPost]
        public IActionResult EliminarDepartamento(int id)
        {
            try
            {
                var objDel = contexto.Departamentos.FirstOrDefault(x => x.IdDepto == id);
                objDel.Estado = false;
                //contexto.Departamentos.Remove(objDel);
                contexto.Departamentos.Update(objDel);
                contexto.SaveChanges();
                return Json(true);
            }

            catch (Exception ex)
            {
                return Json(false);
            }

        }

        [HttpPost]
        public IActionResult ActualizarBlackList(int id, string valor)
        {
            try
            {
                var objUpt = contexto.Blacklists.FirstOrDefault(x => x.IdBlacklist == id);
                objUpt.Palabra = valor;
                contexto.Blacklists.Update(objUpt);
                contexto.SaveChanges();
                return Json(true);
            }

            catch (Exception ex)
            {
                return Json(false);
            }

        }
        [HttpPost]
        public IActionResult ActualizarMunicipio(int idMunicipio, int idDepto, string municipio)
        {
            try
            {
                var objUpt = contexto.Municipios.FirstOrDefault(x => x.IdMunicipio == idMunicipio);
                objUpt.IdDepto = idDepto;
                objUpt.IdMunicipio = idMunicipio;
                objUpt.Municipio1 = municipio;
                contexto.Municipios.Update(objUpt);
                contexto.SaveChanges();
                return Json(true);
            }

            catch (Exception ex)
            {
                return Json(false);
            }

        }

        [HttpPost]
        public IActionResult ActualizarDepartamento(int idDepto, string departamento)
        {
            try
            {
                var objUpt = contexto.Departamentos.FirstOrDefault(x => x.IdDepto == idDepto);
                objUpt.Departamento1 = departamento;
                contexto.Departamentos.Update(objUpt);
                contexto.SaveChanges();
                return Json(true);
            }

            catch (Exception ex)
            {
                return Json(false);
            }

        }

        public IActionResult vBlackList()
        {
            return View();
        }

        public IActionResult vDepartamentos()
        {
            return View();
        }

        public IActionResult vMunicipios()
        {
            ViewBag.Departamentos = contexto.Departamentos.ToList();
            Municipio municipio = new Municipio();
            return View(municipio);
        }


    }
}