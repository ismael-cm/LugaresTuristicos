using LugaresTuristicos.Commod;
using LugaresTuristicos.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using System.Xml.Linq;
using System.Diagnostics;
using System.Linq;

namespace LugaresTuristicos.Controllers
{
    public class TuristaController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private SitesContext _dbContext = new SitesContext();
        private ValidationClass validationClass = new ValidationClass();

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Dashboard()
        {
            try
            {
                ClaimsPrincipal claimUser = HttpContext.User;
                // Verificar si el usuario ha iniciado sesión
                if (!claimUser.Identity.IsAuthenticated)
                {
                    // Si no ha iniciado sesión, redirigir al inicio de sesión
                    return RedirectToAction("Login", "Home");
                }

                List<Lugare> lista = _dbContext.Lugares.Where(x => x.Estado == true).Include(l => l.Comentarios)
                                                       .Include(l => l.IdMunicipioNavigation)
                                                       .Include(l => l.IdMunicipioNavigation.IdDeptoNavigation)
                                                       .Include(l => l.IdCategoriaNavigation)
                                                       .Include(l => l.IdUsuarioNavigation)
                                                       .ToList();

                //Para obtener el user id del usuario autenticado
                var user_id = User.FindFirst(ClaimTypes.NameIdentifier).Value;

                var currentUser = _dbContext.Usuarios.FirstOrDefault(s => s.IdUsuario.Equals(int.Parse(user_id)));
                CommonProfile allData = new CommonProfile();
                allData.Usuario = currentUser;
                allData.Lugares = lista;
                return View(allData);
            }
            catch (Exception ex)
            {
                Lugare model = new Lugare();
                return View(model);
            }
        }

        public IActionResult PostDetails(int? id)
        {

            ClaimsPrincipal claimUser = HttpContext.User;

            try
            {
                TempData["d"] = id;
                if (!claimUser.Identity.IsAuthenticated)
                {
                    // Si no ha iniciado sesión, redirigir al inicio de sesión
                    return RedirectToAction("Login", "Home"); // Reemplaza "Account" con el controlador y acción de inicio de sesión en tu aplicación
                }

                if (TempData["IsLoggedIn"] != null && (bool)TempData["IsLoggedIn"])
                    TempData["NameUser"] = TempData["IsLoggedInNameUser"];

                List<Lugare> currentPlace = _dbContext.Lugares.Include(l => l.Comentarios)
                                                       .Include(l => l.IdMunicipioNavigation)
                                                       .Include(l => l.IdMunicipioNavigation.IdDeptoNavigation)
                                                       .Include(l => l.IdCategoriaNavigation)
                                                       .Include(l => l.IdUsuarioNavigation)
                                                       .Where(x => x.IdLugar.Equals(id))
                                                       .ToList();


                return View(currentPlace);
            }
            catch (Exception e)
            {
                Lugare model = new Lugare();
                return View(model);
            }
        }

        [HttpPost]
        public JsonResult CreateComment(int IdLugar, string Comentario)
        {
            ClaimsPrincipal claimUser = HttpContext.User;
            try
            {
                List<string> bl = _dbContext.Blacklists.Select(x=>x.Palabra).ToList();
                string[] palabrasComentario = Comentario.Split(' ');
                Comentario addComentario = new Comentario();
                addComentario.IdLugar = Convert.ToInt32(IdLugar);
                addComentario.Comentario1 = Comentario;
                addComentario.Fecha = DateTime.Now;
                addComentario.Estado = true;
                addComentario.IdUsuario = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                _dbContext.Comentarios.Add(addComentario);

                if (palabrasComentario.Intersect(bl).Any())
                {
                    addComentario.Revision = "En Revisión";
                    _dbContext.Comentarios.Add(addComentario);
                    _dbContext.SaveChanges();
                    return Json(new { respuesta = "En Revisión" });
                }
                else
                {
                    addComentario.Revision = "Válido";
                    _dbContext.Comentarios.Add(addComentario);
                    _dbContext.SaveChanges();
                    return Json(new { respuesta = "Válido" });
                }
                
                
            }
            catch (Exception ex)
            {
                return Json(new { respuesta = "Error" });
            }
            
        }

        [HttpPost]
        public JsonResult AllComentarios(int idLugar)
        {
            var comments = (from comentario in _dbContext.Comentarios
                            join usuario in _dbContext.Usuarios
                            on comentario.IdUsuario equals usuario.IdUsuario
                            where comentario.IdLugar == idLugar && comentario.Estado == true && comentario.Revision.Equals("Válido")
                            orderby comentario.Fecha descending
                            select new
                            {
                                fecha = comentario.Fecha,
                                imagen = usuario.Imagen,
                                nombre = usuario.Nombre,
                                apellido = usuario.Apellido,
                                comentario = comentario.Comentario1,
                                idUsuario=usuario.IdUsuario,
                                idComentario=comentario.IdComentario
                            }).ToList();


            return Json(comments);
        }

        [HttpPost]
        public JsonResult findByMunicipio(string sValor, string sTipo)
        {
            try
            {
                if (sTipo.Equals("Dpto"))
                {
                    int idDpto = _dbContext.Departamentos.Where(x => x.Departamento1.Equals(sValor)).Select(x => x.IdDepto).FirstOrDefault();
                    List<Lugare> lugares = _dbContext.Lugares.Where(x => x.Estado == true && x.IdMunicipioNavigation.IdDepto==idDpto).ToList();
                    return Json(lugares);
                }
                else
                {
                    int idMunicipio = _dbContext.Municipios.Where(x => x.Municipio1.Equals(sValor)).Select(x => x.IdMunicipio).FirstOrDefault();
                    List<Lugare> lugares = _dbContext.Lugares.Where(x => x.Estado == true && x.IdMunicipio == idMunicipio).ToList();
                    return Json(lugares);
                }
            }
            catch (Exception ex)
            {
                return Json(new { respuesta = "Error" });
            }

        }

        [HttpPost]
        public JsonResult findByCategorias(int sValor)
        {
            try
            {
                List<Lugare> lugares = _dbContext.Lugares.Where(x => x.Estado == true && x.IdCategoria == sValor).ToList();
                return Json(lugares);
            }
            catch (Exception ex)
            {
                return Json(new { respuesta = "Error" });
            }

        }

        [HttpPost]
        public JsonResult findUser()
        {
            try
            {
                int user = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                return Json(user);
            }
            catch (Exception ex)
            {
                return Json(new { respuesta = "Error" });
            }

        }

        [HttpPost]
        public JsonResult findByDescripcion(string sValor)
        {
            try
            {
                string[] palabrasDescripcion = sValor.Split(' ');
                List<Lugare> desc = _dbContext.Lugares.Where(x => x.Estado == true).ToList();
                List<Lugare> lugares = new List<Lugare> { };

                foreach(var l in desc)
                {
                    if (palabrasDescripcion.Any(palabra => l.Descripcion.Contains(palabra)))
                                        {
                                            lugares.Add(l);
                                        }
                }
                
                return Json(lugares);
            }
            catch (Exception ex)
            {
                return Json(new { respuesta = "Error" });
            }

        }

        [HttpPost]
        public JsonResult findByNombre(string sValor)
        {
            try
            {
                string[] palabrasNombre = sValor.Split(' ');
                List<Lugare> desc = _dbContext.Lugares.Where(x => x.Estado == true).ToList();
                List<Lugare> lugares = new List<Lugare> { };

                foreach (var l in desc)
                {
                    if (palabrasNombre.Any(palabra => l.NombreLugar.ToLower().Contains(palabra.ToLower())))
                    {
                        lugares.Add(l);
                    }
                }

                return Json(lugares);
            }
            catch (Exception ex)
            {
                return Json(new { respuesta = "Error" });
            }

        }

        [HttpPost]
        public JsonResult getCategorias()
        {
            try
            {
                List<Categoria> lstCategoria = _dbContext.Categorias.Where(x => x.Estado == true).ToList();
                return Json(lstCategoria);
            }
            catch (Exception ex)
            {
                return Json(new { respuesta = "Error" });
            }

        }

        [HttpPost]
        public IActionResult EliminarComentario(int id)
        {
            try
            {
                var objDel = _dbContext.Comentarios.FirstOrDefault(x => x.IdComentario == id);
                objDel.Estado = false;
                //contexto.Categorias.Remove(objDel);
                _dbContext.Comentarios.Update(objDel);
                _dbContext.SaveChanges();
                return Json(true);
            }

            catch (Exception ex)
            {
                return Json(false);
            }

        }

        [HttpPost]
        public IActionResult ActualizarComentaro(int id, string valor)
        {
            try
            {
                var objUpt = _dbContext.Comentarios.FirstOrDefault(x => x.IdComentario == id);
                objUpt.Comentario1 = valor;
                _dbContext.Comentarios.Update(objUpt);
                _dbContext.SaveChanges();
                return Json(true);
            }

            catch (Exception ex)
            {
                return Json(false);
            }

        }

    }


}
