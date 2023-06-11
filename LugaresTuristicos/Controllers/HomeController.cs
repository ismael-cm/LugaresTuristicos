using LugaresTuristicos.Commod;
using LugaresTuristicos.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;

namespace LugaresTuristicos.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private SitesContext _dbContext = new SitesContext();
        private ValidationClass validationClass = new ValidationClass();

        public IActionResult Login()
        {
            if (!string.IsNullOrEmpty(TempData["MessageCorrectAdd"] as string))
                @ViewBag.MessageCorrect = "Inicia sesion para continuar.";

            ClaimsPrincipal claimUser = HttpContext.User;
            if (claimUser.Identity.IsAuthenticated)
            {
                if(claimUser.IsInRole("ADMINISTRADOR"))
                {
                    return RedirectToAction("Dashboard", "Home", new { area = "Admin" });
                }

                if (claimUser.IsInRole("USUARIO"))
                {
                    return RedirectToAction("Dashboard", "Turista");
                }

                if (claimUser.IsInRole("MODERADOR"))
                {
                    return RedirectToAction("Dashboard", "Moderador");
                }

                if (claimUser.IsInRole("EMPRENDEDOR"))
                {
                    return RedirectToAction("Dashboard", "Emprendedor");
                }

                //return RedirectToAction("Dashboard", "Turista");
            }

            return View();
        }

        public IActionResult Register()
        {
            ClaimsPrincipal claimUser = HttpContext.User;
            bool isdws = claimUser.Identity.IsAuthenticated;
            string name = claimUser.Identity.Name;
           
            return View();
        }

        [Authorize]
        public IActionResult Index()
        {
            ClaimsPrincipal claimUser = HttpContext.User;
            // Verificar si el usuario ha iniciado sesión
            if (claimUser.Identity.IsAuthenticated)
            {
                // Si no ha iniciado sesión, redirigir al inicio de sesión
                return RedirectToAction("Login"); // Reemplaza "Account" con el controlador y acción de inicio de sesión en tu aplicación
            }

            return View();
        }


        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            TempData["error"] = "";
            HttpContext.Session.Clear();
            
            return RedirectToAction("Login", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> Login(Usuario usuario)
        {
            try
            {
                if (usuario.Correo != "" && usuario.Password != "")
                {
                    // Lógica de autenticación y validación del usuario
                    if (IsValidCredentials(usuario.Correo, usuario.Password))
                    {
                        // Iniciar sesión exitosamente
                        // Guardar información del usuario en la sesión, establecer cookies, etc.
                        var user = _dbContext.Usuarios
                            .Include(u => u.Rol)
                            .FirstOrDefault(s => s.Correo.Equals(usuario.Correo));

                        List<Claim> claims = new List<Claim>()
                        {
                            new Claim(ClaimTypes.NameIdentifier, user.IdUsuario.ToString()),
                            new Claim(ClaimTypes.Name, user.Nombre),
                            new Claim("Correo", usuario.Correo),
                            //Agregar Roles.
                            new Claim(ClaimTypes.Role, user.Rol.NombreRol)
                        };

                        


                        ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims,
                            CookieAuthenticationDefaults.AuthenticationScheme);

                        AuthenticationProperties properties = new AuthenticationProperties()
                        {
                            AllowRefresh = true,
                            IsPersistent = false
                        };

                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                            new ClaimsPrincipal(claimsIdentity), properties);
                        return RedirectToAction("Login", "Home");
                    }
                    else
                    {
                        // Credenciales inválidas
                        TempData["ErrorMessage"] = "Credenciales invalidas. Por favor, intenta de nuevo.";
                    }
                }
            }
            catch (Exception exe)
            {
                // Credenciales inválidas
                TempData["ErrorMessage"] = "Credenciales invalidas. Por favor, intenta de nuevo.  ";
            }

            return RedirectToAction("Login");
        }

        [HttpPost]
        public IActionResult Register(Usuario model, string confirmPassword)
        {
            try
            {
                // Verificar si se encontró un usuario y la contraseña coincide
                if (model != null)
                    if (model.Correo != "" && model.Password != "" && confirmPassword != "" && model.Nombre != "" && model.Apellido != "")
                    {
                        if (model.Password != confirmPassword)
                        {
                            ViewBag.ErrorMessagePassword = "The password and confirmation password do not match.";
                            return View(model);
                        }

                        // Valida si existe el correo
                        var existing = _dbContext.Usuarios.FirstOrDefault(s => s.Correo.Equals(model.Correo));

                        if (existing != null)
                        {
                            //ViewBag.ErrorMessage = "Ocurrió un error al procesar el formulario.";
                            ViewBag.ErrorMessage = "El correo ingresado ya existe";
                            return View(model);
                        }

                        // Establece una imagen por defecto para el usuario que se esta registrando
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
                            Edad = model.Edad,
                            Estado = true,
                            Correo = model.Correo,
                            Password = encryptedResult,
                            IdRol = 3, //Todos seran 3 por el rol que se les adicionara siempre como Rol USUARIO
                            Imagen = imageBytes,
                            FechaCreacion = DateTime.Now,
                        };

                        _dbContext.Usuarios.Add(user);
                        _dbContext.SaveChanges();

                        // Redirigir al usuario a la página de inicio de sesión después de un registro exitoso
                        TempData["MessageCorrectAdd"] = "Registro almacenado correctamente";
                        return RedirectToAction("Login", "Home");
                    }
            }
            catch (Exception ex)
            {
                // Si el modelo no es válido, vuelve a mostrar el formulario de registro con los mensajes de error
                return View(model);
            }

            // Si el modelo no es válido, vuelve a mostrar el formulario de registro con los mensajes de error
            return View(model);
        }

        private bool IsValidCredentials(string correo, string password)
        {
            try
            {
                // Buscar un usuario en la base de datos con el correo especificado
                var usuario = _dbContext.Usuarios.FirstOrDefault(u => u.Correo == correo);

                if (usuario == null)
                    return false;

                /***********************Decryption**********************************************/
                // Get the bytes of the string
                byte[] bytesToBeDecrypted = Convert.FromBase64String(usuario.Password);
                byte[] passwordBytesdecrypt = Encoding.UTF8.GetBytes(password);
                passwordBytesdecrypt = SHA256.Create().ComputeHash(passwordBytesdecrypt);

                byte[] bytesDecrypted = validationClass.AES_Decrypt(bytesToBeDecrypted, passwordBytesdecrypt);

                string decryptedResult = Encoding.UTF8.GetString(bytesDecrypted);
                /***********************End*Decryption******************************************/

                if (usuario != null && password.Equals(decryptedResult))
                {
                    // Credenciales válidas
                    return true;
                }

                // Credenciales inválidas
                return false;
            }
            catch (Exception ex)
            {
                // Credenciales inválidas
                return false;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateComentario(string IdLugar, string Comentario)
        {
            try
            {
                Comentario comentario = new Comentario();
                comentario.IdLugar = Convert.ToInt32(IdLugar);
                comentario.Comentario1 = Comentario;
                comentario.Fecha = DateTime.Now;
                comentario.IdUsuario = Convert.ToInt32(HttpContext.Session.GetString("IdUser"));

                _dbContext.Comentarios.Add(comentario);
                _dbContext.SaveChanges();
            }
            catch (Exception ex)
            {

            }
            return RedirectToAction("Dashboard");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        

        public IActionResult Privacity()
        {
            return View();
        }
    }
}
