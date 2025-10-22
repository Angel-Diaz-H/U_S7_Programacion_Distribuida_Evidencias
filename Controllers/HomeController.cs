using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Pollo_Loco_clone.Models;
using System.Text.Json;

namespace Pollo_Loco_clone.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly List<User> _users;
        private const string SessionKey = "UserSession";

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
            // Definir los 3 usuarios
            _users = new List<User>
            {
                new User { Id = 1, Username = "admin", Password = "admin123", Name = "Administrador", Email = "admin@polloloco.com", Role = "Admin" },
                new User { Id = 2, Username = "usuario1", Password = "user123", Name = "Usuario Uno", Email = "usuario1@polloloco.com", Role = "User" },
                new User { Id = 3, Username = "usuario2", Password = "user456", Name = "Usuario Dos", Email = "usuario2@polloloco.com", Role = "User" }
            };
        }

        public IActionResult Index()
        {
            var session = GetUserSession();
            ViewBag.UserSession = session;
            return View();
        }

        public IActionResult Ayuda()
        {
            var session = GetUserSession();
            ViewBag.UserSession = session;
            return View();
        }

        public IActionResult Historia()
        {
            var session = GetUserSession();
            ViewBag.UserSession = session;
            return View();
        }

        public IActionResult Login()
        {
            var session = GetUserSession();
            if (session.IsLoggedIn)
            {
                return RedirectToAction("Index");
            }
            return View();
        }

        [HttpPost]
        public IActionResult Login([FromBody] LoginRequest loginRequest)
        {
            try
            {
                object response;

                var user = _users.FirstOrDefault(u =>
                    u.Username == loginRequest.Username && u.Password == loginRequest.Password);

                if (user != null)
                {
                    // Crear sesión
                    var session = new LoginSession
                    {
                        IsLoggedIn = true,
                        CurrentUser = user,
                        LoginTime = DateTime.Now
                    };

                    HttpContext.Session.SetString(SessionKey, JsonSerializer.Serialize(session));

                    response = new
                    {
                        success = true,
                        message = "Login exitoso",
                        user = new
                        {
                            id = user.Id,
                            username = user.Username,
                            name = user.Name,
                            email = user.Email,
                            role = user.Role
                        },
                        token = Guid.NewGuid().ToString(),
                        timestamp = DateTime.Now
                    };
                }
                else
                {
                    response = new
                    {
                        success = false,
                        message = "Credenciales incorrectas"
                    };
                }

                Console.WriteLine(" JSON GENERADO ");
                string jsonString = System.Text.Json.JsonSerializer.Serialize(response, new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = true
                });
                Console.WriteLine(jsonString);
                Console.WriteLine(" FIN DEL JSON ");

                // 3️⃣ GUARDAR EN UN ARCHIVO TEMPORAL (OPCIONAL)
                string tempPath = Path.Combine(Path.GetTempPath(), "pollo_loco_json.txt");
                System.IO.File.WriteAllText(tempPath, jsonString);
                Console.WriteLine($" JSON guardado en: {tempPath}");

                return Json(response);
            }
            catch (Exception ex)
            {
                var errorResponse = new
                {
                    success = false,
                    message = "Error interno del servidor",
                    error = ex.Message
                };

                Console.WriteLine("💥 ERROR:");
                Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(errorResponse, new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = true
                }));

                return Json(errorResponse);
            }
        }

        [HttpGet]
        public IActionResult DebugJson()
        {
            // Ver el JSON
            var testData = new
            {
                success = true,
                message = "ESTE ES UN JSON DE PRUEBA DIRECTO",
                user = new
                {
                    id = 99,
                    username = "usuario_prueba",
                    name = "Usuario de Prueba",
                    email = "prueba@polloloco.com",
                    role = "Tester"
                },
                token = "token-prueba-12345",
                timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };

            // Guardar en archivo
            string jsonString = System.Text.Json.JsonSerializer.Serialize(testData, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true
            });

            string tempPath = Path.Combine(Path.GetTempPath(), "pollo_loco_debug.json");
            System.IO.File.WriteAllText(tempPath, jsonString);

            Console.WriteLine(" JSON DE DEBUG CREADO:");
            Console.WriteLine(jsonString);
            Console.WriteLine($"📁 Guardado en: {tempPath}");

            return Json(testData);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Remove(SessionKey);
            return RedirectToAction("Index", new { logout = "success" });
        }

        private LoginSession GetUserSession()
        {
            var sessionJson = HttpContext.Session.GetString(SessionKey);
            if (!string.IsNullOrEmpty(sessionJson))
            {
                return JsonSerializer.Deserialize<LoginSession>(sessionJson) ?? new LoginSession();
            }
            return new LoginSession();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}