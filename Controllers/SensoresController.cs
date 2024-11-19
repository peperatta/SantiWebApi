using System;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using APISensores.Model;

namespace APISensores.Controllers
{
    [Route("[controller]")]
    public class UsuarioController : Controller
    {
        private readonly string _connectionString = "Server=santiweb-pre-alpha01-tec-1b4c.d.aivencloud.com;Port=21781;Database=defaultdb;Uid=avnadmin;Password=AVNS_Sptn7L46s7Biea9v_pm;";

        // Endpoint para registrar un usuario
        [HttpPost("register")]
        public IActionResult Register([FromBody] Usuario newUser)
        {
            if (newUser == null || string.IsNullOrEmpty(newUser.Username) || string.IsNullOrEmpty(newUser.Contraseña))
            {
                return BadRequest(new { message = "Datos incompletos" });
            }

            using (var connection = new MySqlConnection(_connectionString))
            {
                try
                {
                    connection.Open();

                    // Verificar si el usuario ya existe
                    using (var checkUserCmd = new MySqlCommand("SELECT COUNT(*) FROM usuario WHERE username = @USERNAME", connection))
                    {
                        checkUserCmd.Parameters.AddWithValue("@USERNAME", newUser.Username);
                        var userExists = Convert.ToInt32(checkUserCmd.ExecuteScalar()) > 0;

                        if (userExists)
                        {
                            return Conflict(new { message = "El usuario ya existe" });
                        }
                    }

                    // Insertar nuevo usuario
                    using (var command = new MySqlCommand("INSERT INTO usuario (username, contraseña, nombre) VALUES (@USERNAME, @PASSWORD, @NOMBRE)", connection))
                    {
                        command.Parameters.AddWithValue("@USERNAME", newUser.Username);
                        command.Parameters.AddWithValue("@PASSWORD", newUser.Contraseña);
                        command.Parameters.AddWithValue("@NOMBRE", newUser.Nombre ?? "");

                        command.ExecuteNonQuery();
                    }

                    return Ok(new { message = "Usuario registrado con éxito" });
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { message = "Error en el servidor", error = ex.Message });
                }
            }
        }

        // Endpoint para iniciar sesión
        [HttpPost("login")]
        public IActionResult Login([FromBody] Usuario loginUser)
        {
            if (loginUser == null || string.IsNullOrEmpty(loginUser.Username) || string.IsNullOrEmpty(loginUser.Contraseña))
            {
                return BadRequest(new { message = "Datos incompletos" });
            }

            using (var connection = new MySqlConnection(_connectionString))
            {
                try
                {
                    connection.Open();

                    // Verificar usuario y contraseña
                    using (var command = new MySqlCommand("SELECT COUNT(*) FROM usuario WHERE username = @USERNAME AND contraseña = @PASSWORD", connection))
                    {
                        command.Parameters.AddWithValue("@USERNAME", loginUser.Username);
                        command.Parameters.AddWithValue("@PASSWORD", loginUser.Contraseña);

                        var userExists = Convert.ToInt32(command.ExecuteScalar()) > 0;

                        if (userExists)
                        {
                            return Ok(new { message = "Inicio de sesión exitoso" });
                        }
                        else
                        {
                            return Unauthorized(new { message = "Usuario o contraseña incorrectos" });
                        }
                    }
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { message = "Error en el servidor", error = ex.Message });
                }
            }
        }

        // Nuevo endpoint para crear una publicación
        [HttpPost("publicar")]
        public IActionResult Publicar([FromBody] PublicacionDTO publicacion)
        {
            if (publicacion == null || string.IsNullOrEmpty(publicacion.Username) || string.IsNullOrEmpty(publicacion.Contenido))
            {
                return BadRequest(new { message = "Datos incompletos" });
            }

            using (var connection = new MySqlConnection(_connectionString))
            {
                try
                {
                    connection.Open();

                    // Paso 1: Obtener el user_id usando el username
                    int userId;
                    using (var getUserCmd = new MySqlCommand("SELECT user_id FROM usuario WHERE username = @USERNAME", connection))
                    {
                        getUserCmd.Parameters.AddWithValue("@USERNAME", publicacion.Username);
                        var result = getUserCmd.ExecuteScalar();

                        if (result == null)
                        {
                            return NotFound(new { message = "Usuario no encontrado" });
                        }

                        userId = Convert.ToInt32(result);
                    }

                    // Paso 2: Insertar la nueva publicación en la tabla publicacion
                    using (var insertPostCmd = new MySqlCommand(
                        "INSERT INTO publicacion (contenido, fecha, user_id) VALUES (@CONTENIDO, @FECHA, @USER_ID)", connection))
                    {
                        insertPostCmd.Parameters.AddWithValue("@CONTENIDO", publicacion.Contenido);
                        // Usar DateTime.Now para incluir fecha y hora en formato MySQL
                        insertPostCmd.Parameters.AddWithValue("@FECHA", DateTime.Now);
                        insertPostCmd.Parameters.AddWithValue("@USER_ID", userId);

                        insertPostCmd.ExecuteNonQuery();
                    }

                    return Ok(new { message = "Publicación creada con éxito" });
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { message = "Error en el servidor", error = ex.Message });
                }
            }
        }
         [HttpGet("ultimas-publicaciones")]
        public IActionResult ObtenerUltimasPublicaciones()
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                try
                {
                    connection.Open();

                    // Consulta para obtener las últimas cinco publicaciones
                    string query = @"
                        SELECT p.contenido, p.fecha, u.username 
                        FROM publicacion p
                        JOIN usuario u ON p.user_id = u.user_id
                        ORDER BY p.fecha DESC
                        LIMIT 15";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            var publicaciones = new List<object>();

                            while (reader.Read())
                            {
                                publicaciones.Add(new
                                {
                                    Contenido = reader["contenido"].ToString(),
                                    Fecha = Convert.ToDateTime(reader["fecha"]).ToString("dd/MM/yyyy HH:mm:ss"),
                                    Username = reader["username"].ToString()
                                });
                            }

                            return Ok(publicaciones);
                        }
                    }
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { message = "Error en el servidor", error = ex.Message });
                }
            }
        }
    }
}
