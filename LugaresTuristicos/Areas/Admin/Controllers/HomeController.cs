using LugaresTuristicos.Commod;
using LugaresTuristicos.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;


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
        public IActionResult getTablaMunicipioByIdDepto(int id)
        {
            var muni = (from municipi in contexto.Municipios
                        where municipi.IdDepto == id && municipi.Estado == true
                        select new
                        {
                            id_depto = municipi.IdDepto,
                            municipio = municipi.Municipio1
                        }).ToList();

            return Json(muni);
        }

        [HttpPost]
        public IActionResult getTablaCategoria()
        {
            List<Categoria> lstCategoria = contexto.Categorias.Where(x => x.Estado == true).ToList();
            return Json(lstCategoria);
        }

        [HttpPost]
        public IActionResult getTablaUsuario()
        {
            var lstUsuario = (from usuario in contexto.Usuarios
                             join rol in contexto.Rols on usuario.IdRol equals rol.IdRol
                             where usuario.Estado == true
                             select new
                             {
                                 idUsuario = usuario.IdUsuario,
                                 nombre = usuario.Nombre,
                                 apellido = usuario.Apellido,
                                 edad = usuario.Edad,
                                 correo = usuario.Correo,
                                 fecha = usuario.FechaCreacion,
                                 idRol = rol.IdRol,
                                 rol = rol.NombreRol
                             }).ToList();

            return Json(lstUsuario);
        }

        [HttpPost]
        public IActionResult getTablaLugares()
        {
            var lstLugares = (from lugares in contexto.Lugares
                              join usuarios in contexto.Usuarios on lugares.IdUsuario equals usuarios.IdUsuario
                              join categorias in contexto.Categorias on lugares.IdCategoria equals categorias.IdCategoria
                              join municipio in contexto.Municipios on lugares.IdMunicipio equals municipio.IdMunicipio
                              join departamento in contexto.Departamentos on municipio.IdDepto equals departamento.IdDepto
                              where usuarios.Estado == true && categorias.Estado == true && municipio.Estado == true && lugares.Estado == true 
                              select new
                              {
                                  idLugar = lugares.IdLugar,
                                  user = usuarios.Nombre + " " + usuarios.Apellido,
                                  categoria = categorias.NombreCategoria,
                                  nombre = lugares.NombreLugar,
                                  descripcion = lugares.Descripcion,
                                  municipio = municipio.Municipio1,
                                  departamento = departamento.Departamento1,
                                  precio = "$" + Math.Round((Decimal)lugares.Precio, 2, MidpointRounding.AwayFromZero),
                                  imagen = lugares.Imagen,
                                  fecha = lugares.FechaPublicacion
                              }).ToList();

            return Json(lstLugares);
        }

        [HttpPost]
        public IActionResult getCorreo(string correo)
        {
            List<Usuario> lstUsuario = contexto.Usuarios.Where(x => x.Correo == correo).ToList();
            return Json(lstUsuario);
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
        public ActionResult guardarCategoria(Categoria categoria)
        {
            Categoria obj = new Categoria();
            obj.NombreCategoria = categoria.NombreCategoria;
            obj.Estado = true;
            try
            {
                contexto.Categorias.Add(obj);
                contexto.SaveChanges();
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
                using (var memoryStream = new MemoryStream())
                {
                    await imageFile.CopyToAsync(memoryStream);
                    var lugar = Newtonsoft.Json.JsonConvert.DeserializeObject<Lugare>(lugaresObj);

                   
                    var image = new Lugare
                    {
                        IdUsuario = lugar.IdUsuario,
                        IdCategoria = lugar.IdCategoria,
                        NombreLugar = lugar.NombreLugar,
                        Descripcion = lugar.Descripcion,
                        IdMunicipio = lugar.IdMunicipio,
                        Precio = lugar.Precio,
                        FechaPublicacion = DateTime.Now,
                        Estado = true,
                        Imagen = memoryStream.ToArray()
                    };

                    contexto.Lugares.Add(image);
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
        public ActionResult guardarUsuario(Usuario model)
        {
            try
            {
                string imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", "DefaultUser.png");

                byte[] imageBytes;
                using (FileStream fileStream = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
                {
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        fileStream.CopyTo(memoryStream);
                        imageBytes = memoryStream.ToArray();
                    }
                }

                /***********************Encryption**********************************************/
                // Get the bytes of the string
                byte[] bytesToBeEncrypted = Encoding.UTF8.GetBytes(model.Password);
                byte[] passwordBytes = Encoding.UTF8.GetBytes(model.Password);

                // Hash the password with SHA256
                passwordBytes = SHA256.Create().ComputeHash(passwordBytes);

                byte[] bytesEncrypted = validationClass.AES_Encrypt(bytesToBeEncrypted, passwordBytes);

                string encryptedResult = Convert.ToBase64String(bytesEncrypted);
                /***********************End*Encryption******************************************/

                // Preparacion del usuario que se guardara a la base de datos
                var user = new Usuario
                {
                    Nombre = model.Nombre,
                    Apellido = model.Apellido,
                    Estado = true,
                    Edad = model.Edad,
                    Correo = model.Correo,
                    Password = encryptedResult,
                    IdRol = model.IdRol,
                    Imagen = imageBytes,
                    FechaCreacion = DateTime.Now,
                };

                _dbContext.Usuarios.Add(user);
                _dbContext.SaveChanges();
                return Json(true);

            }
            catch (Exception ex)
            {
                return Json(false);
            }
            return Json(false);
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
        public IActionResult EliminarCategoria(int id)
        {
            try
            {
                var objDel = contexto.Categorias.FirstOrDefault(x => x.IdCategoria == id);
                objDel.Estado = false;
                //contexto.Categorias.Remove(objDel);
                contexto.Categorias.Update(objDel);
                contexto.SaveChanges();
                return Json(true);
            }

            catch (Exception ex)
            {
                return Json(false);
            }

        }

        [HttpPost]
        public IActionResult EliminarUsuario(int id)
        {
            try
            {
                var objDel = contexto.Usuarios.FirstOrDefault(x => x.IdUsuario == id);
                objDel.Estado = false;
                //contexto.Categorias.Remove(objDel);
                contexto.Usuarios.Update(objDel);
                contexto.SaveChanges();
                return Json(true);
            }

            catch (Exception ex)
            {
                return Json(false);
            }

        }

        [HttpPost]
        public IActionResult EliminarLugar(int id)
        {
            try
            {
                var objDel = contexto.Lugares.FirstOrDefault(x => x.IdLugar == id);
                objDel.Estado = false;
                //contexto.Lugares.Remove(objDel);
                contexto.Lugares.Update(objDel);
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

        [HttpPost]
        public IActionResult ActualizarCategoria(int id, string valor)
        {
            try
            {
                var objUpt = contexto.Categorias.FirstOrDefault(x => x.IdCategoria == id);
                objUpt.NombreCategoria = valor;
                contexto.Categorias.Update(objUpt);
                contexto.SaveChanges();
                return Json(true);
            }

            catch (Exception ex)
            {
                return Json(false);
            }

        }

        [HttpPost]
        public IActionResult ActualizarUsuario(int id, string nombre, string apellido, int edad, int idRol)
        {
            try
            {
                var objUpt = contexto.Usuarios.FirstOrDefault(x => x.IdUsuario == id);
                objUpt.Nombre = nombre;
                objUpt.Apellido = apellido;
                objUpt.Edad = edad;
                objUpt.IdRol = idRol;
                contexto.Usuarios.Update(objUpt);
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

        public IActionResult vCategorias()
        {
            return View();
        }

        public IActionResult vUsuarios()
        {
            ViewBag.Usuarios = contexto.Rols.Where(x => x.Estado == true).ToList();
            Usuario usuario = new Usuario();
            return View(usuario);
        }

        public IActionResult vLugares()
        {
            ViewBag.Usuarios = contexto.Usuarios.Where(x => x.Estado == true && x.IdRol == 4).ToList();
            ViewBag.Categoria = contexto.Categorias.Where(x => x.Estado == true).ToList();
            ViewBag.Departamento = contexto.Departamentos.Where(x => x.Estado == true).ToList();
            ViewBag.Municipio = contexto.Municipios.Where(x => x.Estado == true).ToList();
            Lugare lugares = new Lugare();
            return View(lugares);
        }

    }
}