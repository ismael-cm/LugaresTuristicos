using LugaresTuristicos.Commod;
using LugaresTuristicos.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using System.Xml.Linq;
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Authorization;

namespace LugaresTuristicos.Controllers
{
    [Authorize]
    public class TuristaController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private SitesContext _dbContext = new SitesContext();
        private ValidationClass validationClass = new ValidationClass();

        public IActionResult Index()
        {
            return View();
        }


        public IActionResult Profile()
        {
            var user_id = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var usuario = _dbContext.Usuarios.Include(l => l.Rol).FirstOrDefault(s => s.IdUsuario.Equals(int.Parse(user_id)));

            CommonProfile allData = new CommonProfile();
            allData.Usuario = usuario;

            return View(allData);
        }


        [HttpPost]
        public IActionResult ActualizarPerfilSinImagen(int id_usuario, string nombre, string apellido, int edad)
        {
            try
            {
                var objUpt = _dbContext.Usuarios.FirstOrDefault(x => x.IdUsuario == id_usuario);
                objUpt.Nombre = nombre;
                objUpt.Apellido = apellido;
                objUpt.Edad = edad;
                _dbContext.Usuarios.Update(objUpt);
                _dbContext.SaveChanges();
                return Json(true);
            }

            catch (Exception ex)
            {
                return Json(false);
            }

        }


        [HttpPost]
        public async Task<IActionResult> UploadImageUpdate(IFormFile imageFile, int idUser)
        {
            if (imageFile == null || imageFile.Length == 0)
            {
                return Json(false);
            }

            if (!IsValidImage(imageFile))
            {
                return Json(false);
            }

            try
            {
                using (var memoryStream = new MemoryStream())
                {
                    await imageFile.CopyToAsync(memoryStream);

                    var objUpdate = _dbContext.Usuarios.FirstOrDefault(x => x.IdUsuario == idUser);
                    objUpdate.Imagen = memoryStream.ToArray();

                    _dbContext.Usuarios.Update(objUpdate);
                    await _dbContext.SaveChangesAsync();
                }

                return Json(true);
            }
            catch (Exception ex)
            {
                return Json(false);
            }
        }

        [AllowAnonymous]
        private bool IsValidImage(IFormFile file)
        {
            if (file.ContentType.ToLower() != "image/jpeg" &&
                file.ContentType.ToLower() != "image/jpg" &&
                file.ContentType.ToLower() != "image/png")
            {
                return false;
            }

            return true;
        }

        [AllowAnonymous]
        public IActionResult Dashboard()
        {
            ClaimsPrincipal claimUser = HttpContext.User;




            try
            {
                List<Lugare> lista = _dbContext.Lugares.Where(x => x.Estado == true).OrderByDescending(x => x.FechaPublicacion).Include(l => l.Comentarios)
                                                       .Include(l => l.IdMunicipioNavigation)
                                                       .Include(l => l.IdMunicipioNavigation.IdDeptoNavigation)
                                                       .Include(l => l.IdCategoriaNavigation)
                                                       .Include(l => l.IdUsuarioNavigation)
                                                       .ToList();

                int user_id = 0;
                Usuario currentUser = new Usuario();
                CommonProfile allData = new CommonProfile();
                if (claimUser.Identity.IsAuthenticated)
                {
                    user_id = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                    currentUser = _dbContext.Usuarios.FirstOrDefault(s => s.IdUsuario.Equals(user_id));

                    allData.Usuario = currentUser;
                }
                allData.Lugares = lista;

                return View(allData);
            }
            catch (Exception ex)
            {
                Lugare model = new Lugare();
                return View(model);
            }
        }

        [Authorize]
        public IActionResult obtenerImagenInicio()
        {
            try
            {
                ClaimsPrincipal claimUser = HttpContext.User;
                var user_id = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var imagen = (from user in _dbContext.Usuarios
                              where user.IdUsuario == int.Parse(user_id)
                              select new
                              {
                                  Imagen = user.Imagen
                              }).ToList();

                return Json(imagen);
            }
            catch (Exception error)
            {
                return Json(new { respuesta = "Error al consultar la imagen" });
            }
        }
        [AllowAnonymous]
        public IActionResult PostDetails(int? id)
        {
            try
            {
                TempData["d"] = id;

                if (TempData["IsLoggedIn"] != null && (bool)TempData["IsLoggedIn"])
                    TempData["NameUser"] = TempData["IsLoggedInNameUser"];

                List<Lugare> currentPlace = _dbContext.Lugares.Include(l => l.Comentarios).ThenInclude(c => c.IdUsuarioNavigation)
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

        [Authorize]
        [HttpPost]
        public JsonResult CreateComment(int IdLugar, string Comentario)
        {
            ClaimsPrincipal claimUser = HttpContext.User;
            try
            {
                List<string> bl = _dbContext.Blacklists.Select(x => x.Palabra).ToList();
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

        [AllowAnonymous]
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

        [AllowAnonymous]
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

        [AllowAnonymous]
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

        [AllowAnonymous]
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

        [AllowAnonymous]
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

        [AllowAnonymous]
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
