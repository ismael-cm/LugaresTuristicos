using LugaresTuristicos.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static System.Net.Mime.MediaTypeNames;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;
using System.IO;
using Image = SixLabors.ImageSharp.Image;
using LugaresTuristicos.Commod;
using System.Text;
using System.Security.Cryptography;

namespace LugaresTuristicos.Areas.Admin.Controllers
{
    [Area("admin")]
    [Route("admin/[controller]/[action]")]
    public class UsuarioController : Controller
    {
        private readonly ILogger<UsuarioController> _logger;
        private SitesContext _dbContext = new SitesContext();
        private ValidationClass validationClass = new ValidationClass();

        public UsuarioController(ILogger<UsuarioController> logger)
        {
            _logger = logger;
        }

        // GET: Usuario
        public IActionResult Index()
        {
            var usuarios = _dbContext.Usuarios.ToList();

            if (!string.IsNullOrEmpty(TempData["MessageCorrectAdd"] as string))
                @ViewBag.MessageCorrect = TempData["MessageCorrectAdd"];

            if (!string.IsNullOrEmpty(TempData["ErrorMessagePassword"] as string))
                @ViewBag.ErrorMessagePassword = TempData["ErrorMessagePassword"];

            if (!string.IsNullOrEmpty(TempData["ErrorMessage"] as string))
                @ViewBag.ErrorMessage = TempData["ErrorMessage"];

            if (!string.IsNullOrEmpty(TempData["MessageCorrectEdit"] as string))
                @ViewBag.MessageCorrectEdit = TempData["MessageCorrectEdit"];

            if (!string.IsNullOrEmpty(TempData["MessageCorrectDelete"] as string))
                @ViewBag.MessageCorrectDelete = TempData["MessageCorrectDelete"];

            if (!string.IsNullOrEmpty(TempData["ErrorMessageDelete"] as string))
                @ViewBag.ErrorMessageDelete = TempData["ErrorMessageDelete"];

            return View(usuarios);
        }

        // GET: Usuario/Create
        public IActionResult Create()
        {
            ViewBag.Roles = _dbContext.Rols.ToList();
            Usuario usuario = new Usuario();
            return View(usuario);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Usuario model, IFormFile imagenArchivo, string ConfirmPassword)
        {
            try
            {
                if (model != null)
                    if (model.Correo != "" && model.Password != "" && ConfirmPassword != "" && model.Nombre != "" && model.Apellido != "")
                    {
                        if (model.Password != ConfirmPassword)
                        {
                            TempData["ErrorMessagePassword"] = "The password and confirmation password do not match.";
                            return RedirectToAction("Index");
                        }

                        // Lógica para guardar el usuario en la base de datos o realizar otras acciones necesarias
                        var existing = _dbContext.Usuarios.FirstOrDefault(s => s.Correo.Equals(model.Correo));

                        if (existing != null)
                        {
                            TempData["ErrorMessage"] = "The email already exists.";
                            return RedirectToAction("Index");
                        }

                        // Metodo para convertir la imagen a typo Byte[]
                        if (imagenArchivo != null && imagenArchivo.Length > 0)
                        {
                            // Leer los datos del archivo de imagen en un arreglo de bytes
                            using (var stream = new MemoryStream())
                            {
                                imagenArchivo.CopyTo(stream);
                                model.Imagen = stream.ToArray();
                            }
                        }
                        else
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

                            model.Imagen = imageBytes;
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

                        if (string.IsNullOrEmpty(model.IdRol.ToString()))
                            model.IdRol = 3; //Colocaremos el Rol 3 que sera por defecto USUARIO

                        // Ejemplo de lógica para registrar al usuario
                        var user = new Usuario
                        {
                            Nombre = model.Nombre,
                            Apellido = model.Apellido,
                            Edad = model.Edad,
                            Correo = model.Correo,
                            Password = encryptedResult,
                            Estado = model.Estado,
                            UsuarioEmprendedor = model.UsuarioEmprendedor,
                            CuentaCompletada = model.CuentaCompletada,
                            IdRol = model.IdRol,
                            Imagen = model.Imagen,
                            FechaCreacion = DateTime.Now,
                        };

                        _dbContext.Usuarios.Add(user);
                        _dbContext.SaveChanges();

                        // Redirigir al usuario a la página de inicio de sesión después de un registro exitoso
                        TempData["MessageCorrectAdd"] = "Registro almacenado correctamente";
                        return RedirectToAction("Index");
                    }
            }
            catch (Exception)
            {
                @ViewBag.ErrorMessage = "Error. Por favor, intenta de nuevo.";
                return View(model);
            }

            @ViewBag.ErrorMessage = "Error. Por favor, intenta de nuevo.";
            return View(model);
        }


        // GET: Usuario/Edit/5
        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "The user do not match.";
                return RedirectToAction("Index");
            }

            var usuario = _dbContext.Usuarios.Find(id);
            ViewBag.Roles = _dbContext.Rols.ToList();

            if (usuario == null)
            {
                TempData["ErrorMessage"] = "The user do not match.";
                return RedirectToAction("Index");
            }

            return View(usuario);
        }

        // POST: Usuario/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Usuario model, IFormFile imagenArchivo, string ConfirmPassword)
        {
            if (id != model.IdUsuario)
            {
                TempData["ErrorMessage"] = "The user do not match.";
                return RedirectToAction("Index");
            }

            //if (ModelState.IsValid)
            //{
            if (model != null)
                if (model.Correo != "" && model.Password != "" && ConfirmPassword != "" && model.Nombre != "" && model.Apellido != "")
                {
                    if (model.Password != ConfirmPassword)
                    {
                        TempData["ErrorMessagePassword"] = "The password and confirmation password do not match.";
                        return RedirectToAction("Index");
                    }

                    var userId = _dbContext.Usuarios.FirstOrDefault(s => s.IdUsuario == model.IdUsuario);
                    var existing = _dbContext.Usuarios.FirstOrDefault(s => s.Correo.Equals(model.Correo));

                    if (existing != null)
                    {
                        if (userId.IdUsuario != existing.IdUsuario)
                        {
                            TempData["ErrorMessage"] = "The email already exists.";
                            return RedirectToAction("Index");
                        }
                    }
                    else
                        existing = userId;

                    if (existing != null)
                    {
                        if (model.Password != existing.Password)
                        {
                            /***********************Encryption**********************************************/
                            // Get the bytes of the string
                            byte[] bytesToBeEncrypted2 = Encoding.UTF8.GetBytes(model.Password);
                            byte[] passwordBytes2 = Encoding.UTF8.GetBytes(model.Password);

                            // Hash the password with SHA256
                            passwordBytes2 = SHA256.Create().ComputeHash(passwordBytes2);

                            byte[] bytesEncrypted2 = validationClass.AES_Encrypt(bytesToBeEncrypted2, passwordBytes2);

                            string encryptedResult2 = Convert.ToBase64String(bytesEncrypted2);
                            /***********************End*Encryption******************************************/
                            model.Password = encryptedResult2;
                        }

                        // Metodo para convertir la imagen a typo Byte[]
                        if (imagenArchivo == null)
                        {
                            model.Imagen = existing.Imagen;
                        }
                        else
                        {
                            // Leer los datos del archivo de imagen en un arreglo de bytes
                            using (var stream = new MemoryStream())
                            {
                                imagenArchivo.CopyTo(stream);
                                model.Imagen = stream.ToArray();
                            }
                        }
                    }

                    existing.Nombre = model.Nombre;
                    existing.Apellido = model.Apellido;
                    existing.Edad = model.Edad;
                    existing.Correo = model.Correo;
                    existing.Password = model.Password;
                    existing.Estado = model.Estado;
                    existing.UsuarioEmprendedor = model.UsuarioEmprendedor;
                    existing.CuentaCompletada = model.CuentaCompletada;
                    existing.IdRol = model.IdRol;
                    existing.Imagen = model.Imagen;
                    existing.FechaCreacion = model.FechaCreacion;

                    _dbContext.SaveChanges();

                    // Redirigir al usuario a la página de inicio de sesión después de un registro exitoso
                    TempData["MessageCorrectEdit"] = "Registro modificado correctamente";
                    return RedirectToAction("Index");
                }

            @ViewBag.ErrorMessage = "Error. Por favor, intenta de nuevo.";
            return View(model);
        }

        // GET: Usuario/Delete/5
        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "The user do not match.";
                return RedirectToAction("Index");
            }

            var usuario = _dbContext.Usuarios.Include(l => l.IdRolNavigation)
                                            .FirstOrDefault(u => u.IdUsuario == id);

            if (usuario == null)
            {
                TempData["ErrorMessage"] = "The user do not match.";
                return RedirectToAction("Index");
            }

            return View(usuario);
        }

        // POST: Usuario/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            try
            {
                var usuario = _dbContext.Usuarios.Find(id);
                _dbContext.Usuarios.Remove(usuario);
                _dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessageDelete"] = "Ocurrio un error al procesar el formulario.";
                return RedirectToAction("Index");
            }

            TempData["MessageCorrectDelete"] = "Registro eliminado correctamente";
            return RedirectToAction("Index");
        }

        private bool IsValidCredentials(string correo, string password)
        {
            // Buscar un usuario en la base de datos con el correo especificado
            var usuario = _dbContext.Usuarios.FirstOrDefault(u => u.Correo == correo);

            // Verificar si se encontró un usuario y la contraseña coincide
            if (usuario != null && password.Equals(usuario.Password))
            {
                // Credenciales válidas
                return true;
            }

            // Credenciales inválidas
            return false;
        }

        private bool UsuarioExists(int id)
        {
            return _dbContext.Usuarios.Any(u => u.IdUsuario == id);
        }
    }
}
