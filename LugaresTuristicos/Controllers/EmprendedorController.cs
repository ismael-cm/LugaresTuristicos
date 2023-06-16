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
    public class EmprendedorController : Controller
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
            ClaimsPrincipal claimUser = HttpContext.User;
            // Verificar si el usuario ha iniciado sesión
            if (!claimUser.Identity.IsAuthenticated)
            {
                // Si no ha iniciado sesión, redirigir al inicio de sesión
                return RedirectToAction("Login", "Home");
            }

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

        [HttpPost]
        public async Task<IActionResult> UploadImageUpdatePost(IFormFile imageFile, [FromForm] string lugaresObj)
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
                    var lugar = Newtonsoft.Json.JsonConvert.DeserializeObject<Lugare>(lugaresObj);

                    var objUpdate = _dbContext.Lugares.FirstOrDefault(x => x.IdLugar == lugar.IdLugar);
                    objUpdate.IdCategoria = lugar.IdCategoria;
                    objUpdate.NombreLugar = lugar.NombreLugar;
                    objUpdate.Descripcion = lugar.Descripcion;
                    objUpdate.IdMunicipio = lugar.IdMunicipio;
                    objUpdate.Precio = lugar.Precio;
                    objUpdate.Imagen = memoryStream.ToArray();
                    objUpdate.FechaPublicacion = DateTime.Now;

                    _dbContext.Lugares.Update(objUpdate);
                    await  _dbContext.SaveChangesAsync();
                }

                return Json(true);
            }
            catch (Exception ex)
            {
                return Json(false);
            }
        }

        [HttpPost]
        public IActionResult getTablaLugares()
        {
            ClaimsPrincipal claimUser = HttpContext.User;
            var user_id = User.FindFirst(ClaimTypes.NameIdentifier).Value;

            var lstLugares = (from lugares in _dbContext.Lugares
                              join usuarios in _dbContext.Usuarios on lugares.IdUsuario equals usuarios.IdUsuario
                              join categorias in _dbContext.Categorias on lugares.IdCategoria equals categorias.IdCategoria
                              join municipio in _dbContext.Municipios on lugares.IdMunicipio equals municipio.IdMunicipio
                              join departamento in _dbContext.Departamentos on municipio.IdDepto equals departamento.IdDepto
                              where lugares.IdUsuario == int.Parse(user_id) && usuarios.Estado == true && categorias.Estado == true && municipio.Estado == true && lugares.Estado == true
                              orderby lugares.FechaPublicacion descending
                              select new
                              {
                                  idLugar = lugares.IdLugar,
                                  idUser = usuarios.IdUsuario,
                                  idMunicipio = lugares.IdMunicipio,
                                  idDepto = municipio.IdDepto,
                                  idCategoria = lugares.IdCategoria,
                                  user = usuarios.Nombre + " " + usuarios.Apellido,
                                  categoria = categorias.NombreCategoria,
                                  nombre = lugares.NombreLugar,
                                  descripcion = lugares.Descripcion,
                                  municipio = municipio.Municipio1,
                                  departamento = departamento.Departamento1,
                                  precio = "$" + Math.Round((Decimal)lugares.Precio, 2, MidpointRounding.AwayFromZero),
                                  precio2 = Math.Round((Decimal)lugares.Precio, 2, MidpointRounding.AwayFromZero),
                                  imagen = lugares.Imagen,
                                  imagenUser = usuarios.Imagen,
                                  fecha = lugares.FechaPublicacion
                              }).ToList();

            return Json(lstLugares);
        }

        [HttpPost]
        public IActionResult EliminarLugar(int id)
        {
            try
            {
                var objDel = _dbContext.Lugares.FirstOrDefault(x => x.IdLugar == id);
                objDel.Estado = false;
                //contexto.Lugares.Remove(objDel);
                _dbContext.Lugares.Update(objDel);
                _dbContext.SaveChanges();
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
                var user_id = User.FindFirst(ClaimTypes.NameIdentifier).Value;

                List<Lugare> lista = _dbContext.Lugares.Where(x => x.Estado == true).Where(x=>x.IdUsuario==int.Parse(user_id)).Include(l => l.Comentarios)
                                                       .Include(l => l.IdMunicipioNavigation)
                                                       .Include(l => l.IdMunicipioNavigation.IdDeptoNavigation)
                                                       .Include(l => l.IdCategoriaNavigation)
                                                       .Include(l => l.IdUsuarioNavigation)
                                                       .ToList();

                //Para obtener el user id del usuario autenticado
                
                ViewBag.Categoria = _dbContext.Categorias.Where(x => x.Estado == true).ToList();
                ViewBag.Departamento = _dbContext.Departamentos.Where(x => x.Estado == true).ToList();
                ViewBag.Municipio = _dbContext.Municipios.Where(x => x.Estado == true).ToList();
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

        public IActionResult AllPost()
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

                List<Lugare> lista = _dbContext.Lugares.Where(x => x.Estado == true).OrderByDescending(x=>x.FechaPublicacion).Include(l => l.Comentarios)
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

                var idU= Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier).Value);
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

        [HttpPost]
        public async Task<IActionResult> UploadImage(IFormFile imageFile, [FromForm] string lugaresObj)
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
                ClaimsPrincipal claimUser = HttpContext.User;
                using (var memoryStream = new MemoryStream())
                {
                    await imageFile.CopyToAsync(memoryStream);
                    var lugar = Newtonsoft.Json.JsonConvert.DeserializeObject<Lugare>(lugaresObj);


                    var image = new Lugare
                    {
                        IdUsuario = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier).Value),
                        IdCategoria = lugar.IdCategoria,
                        NombreLugar = lugar.NombreLugar,
                        Descripcion = lugar.Descripcion,
                        IdMunicipio = lugar.IdMunicipio,
                        Precio = lugar.Precio,
                        FechaPublicacion = DateTime.Now,
                        Estado = true,
                        Imagen = memoryStream.ToArray()
                    };

                    _dbContext.Lugares.Add(image);
                    await _dbContext.SaveChangesAsync();
                }

                return Json(true);
            }
            catch (Exception ex)
            {
                return Json(false);
            }
        }

        [HttpPost]
        public IActionResult ActualizarLugarSinImagen(int id_lugar, int id_categoria, string nombre, string descripcion, int id_municipio, decimal precio)
        {
            try
            {
                var objUpt = _dbContext.Lugares.FirstOrDefault(x => x.IdLugar == id_lugar);
                objUpt.IdCategoria = id_categoria;
                objUpt.NombreLugar = nombre;
                objUpt.Descripcion = descripcion;
                objUpt.IdMunicipio = id_municipio;
                objUpt.Precio = precio;
                objUpt.FechaPublicacion = DateTime.Now;
                _dbContext.Lugares.Update(objUpt);
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
