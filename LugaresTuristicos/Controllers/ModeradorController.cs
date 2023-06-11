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
    public class ModeradorController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private SitesContext contexto = new SitesContext();
        private ValidationClass validationClass = new ValidationClass();
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Profile()
        {
            ClaimsPrincipal claimUser = HttpContext.User;
            // Verificar si el usuario ha iniciado sesión
            if (!claimUser.Identity.IsAuthenticated)
            {
                // Si no ha iniciado sesión, redirigir al inicio de sesión
                return RedirectToAction("Login", "Home");
            }

            var user_id = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var usuario = contexto.Usuarios.Include(l => l.Rol).FirstOrDefault(s => s.IdUsuario.Equals(int.Parse(user_id)));

            CommonProfile allData = new CommonProfile();
            allData.Usuario = usuario;

            return View(allData);
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

                List<Comentario> lista = contexto.Comentarios.Where(x => x.Estado == true && x.Revision == "En Revisión")
                                                       .Include(l => l.IdUsuarioNavigation)
                                                       .Include(l => l.IdLugarNavigation)
                                                       .ToList();

                //Para obtener el user id del usuario autenticado
                var user_id = User.FindFirst(ClaimTypes.NameIdentifier).Value;

                CommonProfile allData = new CommonProfile();
                var currentUser = contexto.Usuarios.FirstOrDefault(s => s.IdUsuario.Equals(int.Parse(user_id)));
                List<string> bl = contexto.Blacklists.Select(x => x.Palabra).ToList();

                allData.Usuario = currentUser;
                allData.Comentario = lista;
                allData.blacklist = bl;
                return View(allData);
            }
            catch (Exception ex)
            {
                Lugare model = new Lugare();
                return View(model);
            }
        }

        [HttpPost]
        public IActionResult cargarComentariosAjax()
        {
            var listaComentario = (from com in contexto.Comentarios
                                   join usu in contexto.Usuarios on com.IdUsuario equals usu.IdUsuario
                                   where com.Estado == true && com.Revision == "En Revisión"
                                   select new
                                   {
                                       idComentario = com.IdComentario,
                                       idLugar = com.IdLugar,
                                       idUsuario = com.IdUsuario,
                                       NombreCompleto = usu.Nombre + " " + usu.Apellido,
                                       comentario = com.Comentario1,
                                       fecha = com.Fecha
                                   }).ToList();
            return Json(listaComentario);
        }

        public IActionResult obtenerImagenInicio()
        {
            try
            {
                ClaimsPrincipal claimUser = HttpContext.User;
                var user_id = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var imagen = (from user in contexto.Usuarios
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

                    var objUpdate = contexto.Usuarios.FirstOrDefault(x => x.IdUsuario == idUser);
                    objUpdate.Imagen = memoryStream.ToArray();

                    contexto.Usuarios.Update(objUpdate);
                    await contexto.SaveChangesAsync();
                }

                return Json(true);
            }
            catch (Exception ex)
            {
                return Json(false);
            }
        }

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

        [HttpPost]
        public IActionResult ActualizarPerfilSinImagen(int id_usuario, string nombre, string apellido, int edad)
        {
            try
            {
                var objUpt = contexto.Usuarios.FirstOrDefault(x => x.IdUsuario == id_usuario);
                objUpt.Nombre = nombre;
                objUpt.Apellido = apellido;
                objUpt.Edad = edad;
                contexto.Usuarios.Update(objUpt);
                contexto.SaveChanges();
                return Json(true);
            }

            catch (Exception ex)
            {
                return Json(false);
            }

        }

        [HttpPost]
        public IActionResult getTablaBlackList()
        {
            List<Blacklist> lstBlackList = contexto.Blacklists.Where(x => x.Estado == true).ToList();
            return Json(lstBlackList);
        }

        [HttpGet]
        public IActionResult GetModelBlacklistEstado(string palabra)
        {
            //var blacklist = (from x in contexto.Blacklists
            //                where x.Palabra == palabra && x.Estado == true
            //                select new
            //                {
            //                    estado = x.Estado
            //                }
            //                ).ToList();

            var blacklist = contexto.Blacklists.Where(x => x.Estado == true).Select(item => item.Palabra).ToList();

            var estado = blacklist.Contains(palabra);
            return Json(estado);
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
        public IActionResult RechazarComentario(int id)
        {
            try
            {
                var objDel = contexto.Comentarios.FirstOrDefault(x => x.IdComentario == id);
                objDel.Revision = "Rechazado";
                objDel.Estado = false;
                //contexto.Blacklists.Remove(objDel);
                contexto.Comentarios.Update(objDel);
                contexto.SaveChanges();
                return Json(true);
            }

            catch (Exception ex)
            {
                return Json(false);
            }

        }

        [HttpPost]
        public IActionResult AprobarComentario(int id)
        {
            try
            {
                var objDel = contexto.Comentarios.FirstOrDefault(x => x.IdComentario == id);
                objDel.Revision = "Válido";
                //contexto.Blacklists.Remove(objDel);
                contexto.Comentarios.Update(objDel);
                contexto.SaveChanges();
                return Json(true);
            }

            catch (Exception ex)
            {
                return Json(false);
            }

        }


        [HttpPost]
        public ActionResult guardarBlackList(string black)
        {
            Blacklist obj = new Blacklist();
            obj.Palabra = black;
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

        public IActionResult vBlackList()
        {
            ClaimsPrincipal claimUser = HttpContext.User;
            // Verificar si el usuario ha iniciado sesión
            if (!claimUser.Identity.IsAuthenticated)
            {
                // Si no ha iniciado sesión, redirigir al inicio de sesión
                return RedirectToAction("Login", "Home");
            }

            var user_id = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var usuario = contexto.Usuarios.Include(l => l.Rol).FirstOrDefault(s => s.IdUsuario.Equals(int.Parse(user_id)));

            CommonProfile allData = new CommonProfile();
            allData.Usuario = usuario;

            return View(allData);
        }

    }
}
