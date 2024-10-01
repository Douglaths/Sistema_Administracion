using AdministradorLaboral.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Hosting;
using System.Web;
using System.Web.Mvc;
using System.Net;
using System.Web.Http.Cors;


namespace AdministradorLaboral.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class AdministradorController : Controller
    {
        private readonly string conexionDB = ConfigurationManager.ConnectionStrings["Cadena"].ConnectionString;
        private readonly string baseFotoPath = "~/Fotos/"; // Ruta base para las fotos en el proyecto
        private readonly string defaultFotoPath = "~/Images/default.jpg";

        // GET: Administrador
        //-------------//-----------------//---------------//---------------//------------------//-----------------//-----------//

        //DashBoard
        public ActionResult AdminDashboard(int userId, string userName, string userRole, int userCenter)
        {            

            ViewBag.UserId = userId;
            ViewBag.UserName = userName;
            ViewBag.UserRole = userRole;
            ViewBag.UserCenter = userCenter;

            return View();
        }

        //-------------//-----------------//---------------//---------------//------------------//-----------------//-----------//

        //Centros

        public ActionResult Centros(int userId, string userName, string userRole, int userCenter)
        {
            List<Centro> centros = new List<Centro>();

            using (SqlConnection connection = new SqlConnection(conexionDB))
            {
                string query = "SELECT Id, Nombre, Direccion, Telefono, Email, HorariosOperacion, CapacidadMaxima, IdCentro FROM Centros";

                SqlCommand command = new SqlCommand(query, connection);

                try
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        Centro centro = new Centro
                        {
                            Id = reader.GetInt32(0),
                            Nombre = reader.GetString(1),
                            Direccion = reader.GetString(2),
                            Telefono = reader.GetString(3),
                            Email = reader.GetString(4),
                            Horarios = reader.GetString(5),
                            CapacidadMaxima = reader.GetInt32(6),
                            IdCentro = reader.GetString(7)
                        };
                        centros.Add(centro);
                    }
                }
                catch (SqlException ex)
                {
                    // Manejo de errores
                    Console.WriteLine(ex.Message);
                }
            }
            
            ViewBag.UserId = userId;
            ViewBag.UserName = userName;
            ViewBag.UserRole = userRole;
            ViewBag.UserCenter = userCenter;

            return View(centros);
        }

        [HttpPost]
        public ActionResult AgregarCentros(Centro centro, string usuario, string id, string centros, string role)
        {
            if (!ModelState.IsValid)
            {
                // Si el modelo no es válido, vuelve a mostrar el formulario
                return View(centro);
            }
            
            // Insertar datos en la base de datos
            using (SqlConnection connection = new SqlConnection(conexionDB))
            {
                string query = @"
                INSERT INTO Centros (Nombre, Direccion, Telefono, Email, HorariosOperacion, CapacidadMaxima, FechaRegistro, IdCentro)
                VALUES (@Nombre, @Direccion, @Telefono, @Email, @HorariosOperacion, @CapacidadMaxima, GETDATE(), @IdCentro)";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Nombre", centro.Nombre);
                    command.Parameters.AddWithValue("@Direccion", centro.Direccion);
                    command.Parameters.AddWithValue("@Telefono", centro.Telefono);
                    command.Parameters.AddWithValue("@Email", centro.Email);
                    command.Parameters.AddWithValue("@HorariosOperacion", centro.Horarios);
                    command.Parameters.AddWithValue("@CapacidadMaxima", centro.CapacidadMaxima);
                    command.Parameters.AddWithValue("@IdCentro", centro.IdCentro);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }

            // Redirigir a una vista de éxito o de listado
            return RedirectToAction("Centros", new { userName = usuario, userId = id, userCenter = centros, userRole = role }); // O la acción a la que quieras redirigir después de guardar
        }

        [HttpPost]
        public async Task<ActionResult> EliminarCentro(int id)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(conexionDB))
                {
                    await conn.OpenAsync();

                    string query = "DELETE FROM Centros WHERE Id = @Id";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", id);

                        int rowsAffected = await cmd.ExecuteNonQueryAsync();

                        if (rowsAffected > 0)
                        {
                            // Registro eliminado correctamente
                            return Json(new { success = true });
                        }
                        else
                        {
                            // No se encontró el registro
                            return Json(new { success = false, message = "Centro no encontrado." });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Manejo de errores
                return Json(new { success = false, message = "Ocurrió un error al eliminar el centro: " + ex.Message });
            }
        }

        [HttpGet]
        public JsonResult ObtenerCentroId(int id)
        {
            Centro centro = null;

            using (SqlConnection connection = new SqlConnection(conexionDB))
            {
                connection.Open();

                string query = "SELECT Id, Nombre, Direccion, Telefono, Email, HorariosOperacion, CapacidadMaxima, IdCentro FROM Centros WHERE Id = @Id";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            centro = new Centro
                            {
                                Id = reader.GetInt32(0),
                                Nombre = reader.GetString(1),
                                Direccion = reader.GetString(2),
                                Telefono = reader.GetString(3),
                                Email = reader.GetString(4),
                                Horarios = reader.GetString(5),
                                CapacidadMaxima = reader.GetInt32(6),
                                IdCentro = reader.GetString(7)
                            };
                        }
                    }
                }
            }

            if (centro != null)
            {
                return Json(centro, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { success = false, message = "Centro no encontrado." }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult ActualizarCentro(Centro model, string usuario, string idUser, string centros, string role)
        {
            if (ModelState.IsValid)
            {
                

                using (SqlConnection connection = new SqlConnection(conexionDB))
                {
                    connection.Open();

                    string query = "UPDATE Centros SET Nombre = @Nombre, Direccion = @Direccion, Telefono = @Telefono, Email = @Email, HorariosOperacion = @HorariosOperacion, CapacidadMaxima = @CapacidadMaxima, IdCentro = @IdCentro WHERE Id = @Id";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id", model.Id);
                        command.Parameters.AddWithValue("@Nombre", model.Nombre);
                        command.Parameters.AddWithValue("@Direccion", model.Direccion);
                        command.Parameters.AddWithValue("@Telefono", model.Telefono);
                        command.Parameters.AddWithValue("@Email", model.Email);
                        command.Parameters.AddWithValue("@HorariosOperacion", model.Horarios);
                        command.Parameters.AddWithValue("@CapacidadMaxima", model.CapacidadMaxima);
                        command.Parameters.AddWithValue("@IdCentro", model.IdCentro);

                        command.ExecuteNonQuery();
                    }
                }

                return RedirectToAction("Centros", new { userName = usuario, userId = idUser, userCenter =  centros, userRole = role }); // Redirigir a la vista de lista de centros
            }

            return View(model); // Mostrar el formulario con errores
        }

        [HttpGet]
        public JsonResult PersonalDisponible()
        {
            var availablePersonalList = new List<Personal>();

            // Consulta SQL para obtener el personal que no está asignado en CentroTrabajadores
            string query = @"SELECT P.*
                     FROM Personal P
                     LEFT JOIN CentroTrabajadores CT ON P.Id = CT.IdPersonal
                     WHERE CT.IdPersonal IS NULL";

            using (var connection = new SqlConnection(conexionDB))
            {
                using (var command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            availablePersonalList.Add(new Personal
                            {
                                Id = reader.GetInt32(0),
                                Nombre = reader.GetString(1),
                                DNI = reader.GetString(2),
                                Direccion = reader.GetString(3),
                                Telefono = reader.GetString(4),
                                Email = reader.GetString(5),
                                Especializacion = reader.GetString(8),
                                // Agrega otros campos necesarios
                            });
                        }
                    }
                }
            }

            // Devuelve la lista de personal disponible como JSON
            return Json(availablePersonalList, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public JsonResult ObtenerPersonalCentro(string idCentro)
        {
            var personalList = new List<Personal>();

            string query = @"SELECT P.*
                     FROM Personal P
                     INNER JOIN CentroTrabajadores CT ON P.Id = CT.IdPersonal
                     WHERE CT.IdCentro = @IdCentro";

            using (var connection = new SqlConnection(conexionDB))
            {
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@IdCentro", idCentro);
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            personalList.Add(new Personal
                            {
                                Id = reader.GetInt32(0),
                                Nombre = reader.GetString(1),
                                DNI = reader.GetString(2),
                                Direccion = reader.GetString(3),
                                Telefono = reader.GetString(4),
                                Email = reader.GetString(5),
                                Especializacion = reader.GetString(8),
                                // Agrega otros campos necesarios
                            });
                        }
                    }
                }
            }

            // Si no hay personal asignado
            if (personalList.Count == 0)
            {
                return Json(new { message = "No hay personal en este centro." }, JsonRequestBehavior.AllowGet);
            }

            return Json(personalList, JsonRequestBehavior.AllowGet);
        }

        public ActionResult EliminarPersonalCentro(int id)
        {
            using (SqlConnection conn = new SqlConnection(conexionDB))
            {
                conn.Open();

                string query = "DELETE FROM CentroTrabajadores WHERE IdPersonal = @IdPersonal";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IdPersonal", id);
                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        return Json(new { success = true, message = "Registro eliminado correctamente." });
                    }
                    else
                    {
                        return Json(new { success = false, message = "No se encontró ningún registro con el IdPersonal proporcionado." });
                    }
                }
            }
        }


        [HttpPost]
        public JsonResult AñadirPersonalCentro(string idCentro, List<int> personalIds)
        {
            string query = "INSERT INTO CentroTrabajadores (IdCentro, IdPersonal) VALUES (@IdCentro, @IdPersonal)";

            using (var connection = new SqlConnection(conexionDB))
            {
                connection.Open();
                foreach (var personalId in personalIds)
                {
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@IdCentro", idCentro);
                        command.Parameters.AddWithValue("@IdPersonal", personalId);
                        command.ExecuteNonQuery();
                    }
                }
            }

            return Json(new { success = true, message = "Personal agregado correctamente al centro." }, JsonRequestBehavior.AllowGet);
        }

        //-------------//-----------------//---------------//---------------//------------------//-----------------//-----------//

        //Personal

        public ActionResult Personal(int userId, string userName, string userRole, int userCenter)
        {
            List<Personal> personales = new List<Personal>();

            using (SqlConnection connection = new SqlConnection(conexionDB))
            {
                string query = "SELECT Id, Nombre, DNI, Direccion, Telefono, Email, FechaNacimiento, Genero, Especializacion, Experiencia, Certificaciones, HorariosOperacion, Foto FROM Personal;";

                SqlCommand command = new SqlCommand(query, connection);

                try
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        Personal personal = new Personal
                        {
                            Id = reader.GetInt32(0),
                            Nombre = reader.GetString(1),
                            DNI = reader.GetString(2),
                            Direccion = reader.GetString(3),
                            Telefono = reader.GetString(4),
                            Email = reader.GetString(5),
                            FechaNacimiento = reader.GetDateTime(6),
                            Genero = reader.GetString(7),
                            Especializacion = reader.GetString(8),
                            Experiencia = reader.GetString(9),
                            Certificaciones = reader.GetString(10),
                            Horarios = reader.GetString(11)
                            
                        };
                        personales.Add(personal);
                    }
                }
                catch (SqlException ex)
                {
                    // Manejo de errores
                    Console.WriteLine(ex.Message);
                }
            }

            ViewBag.UserId = userId;
            ViewBag.UserName = userName;
            ViewBag.UserRole = userRole;
            ViewBag.UserCenter = userCenter;

            return View(personales);
        }

        [HttpGet]
        public async Task<ActionResult> ObtenerPerfil(int id)
        {
            if (id <= 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "ID inválido.");
            }

            try
            {
                using (var connection = new SqlConnection(conexionDB))
                {
                    await connection.OpenAsync();
                    var query = "SELECT * FROM Personal WHERE Id = @Id";
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {

                                byte[] imagenData = (byte[])reader["Foto"];
                                string base64cadena = Convert.ToBase64String(imagenData, 0, imagenData.Length);

                                // Obtener la ruta de la carpeta de certificados desde la base de datos
                                var rutaCarpetaCertificaciones = Url.Content(reader["Certificaciones"].ToString());
                                // Ruta física en el servidor (cambia Server.MapPath si es ASP.NET Core)
                                var rutaFisica = Server.MapPath(rutaCarpetaCertificaciones);

                                // Listar todos los archivos dentro de esa carpeta
                                var archivosCertificaciones = Directory.GetFiles(rutaFisica)
                                    .Select(cert => Url.Content(Path.Combine(rutaCarpetaCertificaciones, Path.GetFileName(cert))))
                                    .ToArray();

                                var personal = new
                                {
                                    Nombre = reader["Nombre"].ToString(),
                                    DNI = reader["DNI"].ToString(),
                                    Direccion = reader["Direccion"].ToString(),
                                    Telefono = reader["Telefono"].ToString(),
                                    Email = reader["Email"].ToString(),
                                    FechaNacimiento = reader["FechaNacimiento"],
                                    Genero = reader["Genero"].ToString(),
                                    Especializacion = reader["Especializacion"].ToString(),
                                    Experiencia = reader["Experiencia"].ToString(),
                                    Certificaciones = archivosCertificaciones, // Lista de archivos en la carpeta
                                    HorariosOperacion = reader["HorariosOperacion"].ToString(),
                                    Foto = base64cadena // Ruta accesible de la foto
                                };

                                return Json(personal, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                return HttpNotFound("Personal no encontrado.");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Registra el error o maneja la excepción como corresponda
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, $"Error interno del servidor: {ex.Message}");
            }
        }

        [HttpPost]
        public ActionResult AgregarPersonal(Personal model, HttpPostedFileBase foto, HttpPostedFileBase[] certificaciones, string usuario, string idUser, string centros, string role)
        {
            // Procesar las certificaciones
            string folderName = model.DNI + "_" + model.Nombre;
            string certPath = HostingEnvironment.MapPath("~/Certificaciones/" + folderName);
            string fotoPath = HostingEnvironment.MapPath("~/Fotos/" + folderName);

            string nombre = Path.GetFileName(foto.FileName);
            byte[] imagenData;

            using (var binaryReader = new BinaryReader(foto.InputStream))
            {
                imagenData = binaryReader.ReadBytes(foto.ContentLength);
            }

            // Verificar si el directorio de certificaciones existe, si no, crearlo
            if (!Directory.Exists(certPath))
            {
                Directory.CreateDirectory(certPath);
            }

            // Guardar los archivos de certificación si existen
            if (certificaciones != null && certificaciones.Any())
            {
                foreach (var archivo in certificaciones)
                {
                    if (archivo != null && archivo.ContentLength > 0)
                    {
                        string certFilePath = Path.Combine(certPath, Path.GetFileName(archivo.FileName));
                        archivo.SaveAs(certFilePath);
                    }
                }

                // Guardar la ruta de certificaciones en el modelo
                model.Certificaciones = "~/Certificaciones/" + folderName;
            }
            else
            {
                // Si no se adjuntaron archivos, almacenar solo la ruta de la carpeta
                model.Certificaciones = "~/Certificaciones/" + folderName;
            }

            // Guardar los datos en la base de datos
            using (SqlConnection conn = new SqlConnection(conexionDB))
            {
                conn.Open();
                string query = @"
                INSERT INTO Personal (DNI, Nombre, Direccion, Telefono, Email, FechaNacimiento, Especializacion, Certificaciones, Foto, HorariosOperacion, Genero, Experiencia)
                VALUES (@DNI, @Nombre, @Direccion, @Telefono, @Email, @FechaNacimiento, @Especializacion, @Certificaciones, @Foto, @HorariosOperacion, @Genero, @Experiencia)";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@DNI", model.DNI);
                    cmd.Parameters.AddWithValue("@Nombre", model.Nombre);
                    cmd.Parameters.AddWithValue("@Direccion", model.Direccion);
                    cmd.Parameters.AddWithValue("@Telefono", model.Telefono);
                    cmd.Parameters.AddWithValue("@Email", model.Email);
                    cmd.Parameters.AddWithValue("@FechaNacimiento", model.FechaNacimiento);
                    cmd.Parameters.AddWithValue("@Especializacion", model.Especializacion);
                    cmd.Parameters.AddWithValue("@Certificaciones", model.Certificaciones); 
                    cmd.Parameters.AddWithValue("@Foto", imagenData); 
                    cmd.Parameters.AddWithValue("@HorariosOperacion", model.Horarios);
                    cmd.Parameters.AddWithValue("@Genero", model.Genero);
                    cmd.Parameters.AddWithValue("@Experiencia", model.Experiencia);

                    cmd.ExecuteNonQuery();
                }
            }

            // Redirigir a la vista de éxito
            return RedirectToAction("Personal", new { userName = usuario, userId = idUser, userCenter = centros, userRole = role });
        }

        [HttpPost]
        public async Task<ActionResult> EliminarPersonal(int id)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(conexionDB))
                {
                    await conn.OpenAsync();

                    string query = "DELETE FROM Personal WHERE Id = @Id";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", id);

                        int rowsAffected = await cmd.ExecuteNonQueryAsync();

                        if (rowsAffected > 0)
                        {
                            // Registro eliminado correctamente
                            return Json(new { success = true });
                        }
                        else
                        {
                            // No se encontró el registro
                            return Json(new { success = false, message = "Personal no encontrado." });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Manejo de errores
                return Json(new { success = false, message = "Ocurrió un error al eliminar el personal: " + ex.Message });
            }
        }

        [HttpPost]
        public ActionResult EliminarCertificacion(string path)
        {
            // Imprimir la ruta recibida
            Console.WriteLine($"Ruta recibida: {path}");

            // Reemplazar las barras invertidas por barras normales
            path = path.Replace("\\", "/");

            // Convertir la ruta relativa a una ruta absoluta
            string absolutePath = Server.MapPath(path);

            // Imprimir la ruta transformada
            Console.WriteLine($"Ruta transformada: {absolutePath}");

            // Verificar si el archivo existe
            if (System.IO.File.Exists(absolutePath))
            {
                System.IO.File.Delete(absolutePath);
                return Json(new { success = true, message = "Archivo eliminado exitosamente" });
            }
            else
            {
                return Json(new { success = false, message = "Archivo no encontrado" });
            }
        }



        [HttpGet]
        public JsonResult ObtenerPersonalId(int id)
        {
            Personal personal = null;

            using (SqlConnection connection = new SqlConnection(conexionDB))
            {
                connection.Open();

                string query = "SELECT Id, Nombre, DNI, Direccion, Telefono, Email, FechaNacimiento, Genero, Especializacion, Experiencia, Certificaciones, HorariosOperacion, Foto FROM Personal WHERE Id = @Id";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            byte[] imagenData = (byte[])reader["Foto"];
                            string base64cadena = Convert.ToBase64String(imagenData, 0, imagenData.Length);

                            // Obtener la ruta de la carpeta de certificados desde la base de datos
                            var rutaCarpetaCertificaciones = Url.Content(reader["Certificaciones"].ToString());
                            // Ruta física en el servidor (cambia Server.MapPath si es ASP.NET Core)
                            var rutaFisica = Server.MapPath(rutaCarpetaCertificaciones);

                            // Listar todos los archivos dentro de esa carpeta
                            var archivosCertificaciones = Directory.GetFiles(rutaFisica)
                                .Select(cert => Url.Content(Path.Combine(rutaCarpetaCertificaciones, Path.GetFileName(cert))))
                                .ToArray();
                            personal = new Personal
                            {
                                Id = reader.GetInt32(0),
                                Nombre = reader.GetString(1),
                                DNI = reader.GetString(2),
                                Direccion = reader.GetString(3),
                                Telefono = reader.GetString(4),
                                Email = reader.GetString(5),
                                FechaNacimiento = reader.GetDateTime(6),
                                Genero = reader.GetString(7),
                                Especializacion = reader.GetString(8),
                                Experiencia = reader.GetString(9),
                                CertificacionesArray = archivosCertificaciones,
                                Horarios = reader.GetString(11),
                                Foto = base64cadena
                            };
                        }
                    }
                }
            }

            if (personal != null)
            {
                return Json(personal, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { success = false, message = "Personal no encontrado." }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult ActualizarPersonal(Personal model, HttpPostedFileBase foto, HttpPostedFileBase[] certificaciones, string usuario, string idUser, string centros, string role)
        {
            if (ModelState.IsValid)
            {


                using (SqlConnection connection = new SqlConnection(conexionDB))
                {
                    connection.Open();
                    string folderName = model.DNI + "_" + model.Nombre;
                    string certPath = HostingEnvironment.MapPath("~/Certificaciones/" + folderName);

                    string nombre = Path.GetFileName(foto.FileName);
                    byte[] imagenData;

                    using (var binaryReader = new BinaryReader(foto.InputStream))
                    {
                        imagenData = binaryReader.ReadBytes(foto.ContentLength);
                    }

                    // Verificar si el directorio de certificaciones existe, si no, crearlo
                    if (!Directory.Exists(certPath))
                    {
                        Directory.CreateDirectory(certPath);
                    }

                    // Guardar los archivos de certificación si existen
                    if (certificaciones != null && certificaciones.Any())
                    {
                        foreach (var archivo in certificaciones)
                        {
                            if (archivo != null && archivo.ContentLength > 0)
                            {
                                string certFilePath = Path.Combine(certPath, Path.GetFileName(archivo.FileName));
                                archivo.SaveAs(certFilePath);
                            }
                        }

                        // Guardar la ruta de certificaciones en el modelo
                        model.Certificaciones = "~/Certificaciones/" + folderName;
                    }
                    else
                    {
                        // Si no se adjuntaron archivos, almacenar solo la ruta de la carpeta
                        model.Certificaciones = "~/Certificaciones/" + folderName;
                    }

                    string query = @"
                        UPDATE Personal
                        SET 
                            Nombre = @Nombre,
                            DNI = @DNI,
                            Direccion = @Direccion,
                            Telefono = @Telefono,
                            Email = @Email,
                            FechaNacimiento = @FechaNacimiento,
                            Genero = @Genero,
                            Especializacion = @Especializacion,
                            Experiencia = @Experiencia,
                            Certificaciones = @Certificaciones,
                            HorariosOperacion = @HorariosOperacion,
                            Foto = @Foto
                        WHERE Id = @Id";


                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Nombre", model.Nombre);
                        command.Parameters.AddWithValue("@DNI", model.DNI);
                        command.Parameters.AddWithValue("@Direccion", model.Direccion);
                        command.Parameters.AddWithValue("@Telefono", model.Telefono);
                        command.Parameters.AddWithValue("@Email", model.Email);
                        command.Parameters.AddWithValue("@FechaNacimiento", model.FechaNacimiento);
                        command.Parameters.AddWithValue("@Genero", model.Genero);
                        command.Parameters.AddWithValue("@Especializacion", model.Especializacion);
                        command.Parameters.AddWithValue("@Experiencia", model.Experiencia);
                        command.Parameters.AddWithValue("@Certificaciones", model.Certificaciones);
                        command.Parameters.AddWithValue("@HorariosOperacion", model.Horarios);
                        command.Parameters.AddWithValue("@Foto", imagenData); // Reemplaza con el array de bytes de la nueva foto
                        command.Parameters.AddWithValue("@Id", model.Id);

                        command.ExecuteNonQuery();
                    }
                }

                return RedirectToAction("Personal", new { userName = usuario, userId = idUser, userCenter = centros, userRole = role }); // Redirigir a la vista de lista de centros
            }

            return View(model); // Mostrar el formulario con errores
        }

        //-------------//-----------------//---------------//---------------//------------------//-----------------//-----------//

        //Clientes

        public ActionResult Clientes(int userId, string userName, string userRole, int userCenter)
        {
            List<Cliente> clientes = new List<Cliente>();

            using (SqlConnection connection = new SqlConnection(conexionDB))
            {
                string query = "SELECT Id, Nombre, Apellido, Email, Telefono, Direccion, FechaNacimiento, Preferencias, Notas FROM Clientes;";

                SqlCommand command = new SqlCommand(query, connection);

                try
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        Cliente cliente = new Cliente
                        {
                            Id = reader.GetInt32(0),
                            Nombre = reader.GetString(1),
                            Apellido = reader.GetString(2),
                            Email = reader.GetString(3),
                            Telefono = reader.GetString(4),
                            Direccion = reader.GetString(5),
                            FechaNacimiento = reader.GetDateTime(6),
                            Preferencias = reader.GetString(7),
                            Notas = reader.GetString(8)
                        };
                        clientes.Add(cliente);
                    }
                }
                catch (SqlException ex)
                {
                    // Manejo de errores
                    Console.WriteLine(ex.Message);
                }
            }

            ViewBag.UserId = userId;
            ViewBag.UserName = userName;
            ViewBag.UserRole = userRole;
            ViewBag.UserCenter = userCenter;

            return View(clientes);
        }

        [HttpPost]
        public ActionResult AgregarClientes(Cliente cliente, string usuario, string id, string centros, string role)
        {
            if (!ModelState.IsValid)
            {
                // Si el modelo no es válido, vuelve a mostrar el formulario
                return View(cliente);
            }

            // Insertar datos en la base de datos
            using (SqlConnection connection = new SqlConnection(conexionDB))
            {
                string query = @"
                INSERT INTO Clientes (Nombre, Apellido, Direccion, Telefono, Email, FechaNacimiento, Preferencias, Notas)
                VALUES (@Nombre, @Apellido, @Direccion, @Telefono, @Email, @FechaNacimiento, @Preferencias, @Notas)";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Nombre", cliente.Nombre);
                    command.Parameters.AddWithValue("@Direccion", cliente.Direccion);
                    command.Parameters.AddWithValue("@Telefono", cliente.Telefono);
                    command.Parameters.AddWithValue("@Email", cliente.Email);
                    command.Parameters.AddWithValue("@Apellido", cliente.Apellido);
                    command.Parameters.AddWithValue("@FechaNacimiento", cliente.FechaNacimiento);
                    command.Parameters.AddWithValue("@Preferencias", cliente.Preferencias);
                    command.Parameters.AddWithValue("@Notas", cliente.Notas);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }

            // Redirigir a una vista de éxito o de listado
            return RedirectToAction("Clientes", new { userName = usuario, userId = id, userCenter = centros, userRole = role }); // O la acción a la que quieras redirigir después de guardar
        }

        [HttpGet]
        public JsonResult ObtenerClienteId(int id)
        {
            Cliente cliente = null;

            using (SqlConnection connection = new SqlConnection(conexionDB))
            {
                connection.Open();

                string query = "SELECT Id, Nombre, Apellido, Email, Telefono, Direccion, FechaNacimiento, Preferencias, Notas FROM Clientes WHERE Id = @Id";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            cliente = new Cliente
                            {
                                Id = reader.GetInt32(0),
                                Nombre = reader.GetString(1),
                                Apellido = reader.GetString(2),
                                Email = reader.GetString(3),
                                Telefono = reader.GetString(4),
                                Direccion = reader.GetString(5),
                                FechaNacimiento = reader.GetDateTime(6),
                                Preferencias = reader.GetString(7),
                                Notas = reader.GetString(8)
                            };
                        }
                    }
                }
            }

            if (cliente != null)
            {
                return Json(cliente, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { success = false, message = "Centro no encontrado." }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult ActualizarClientes(Cliente model, string usuario, string idUser, string centros, string role)
        {
            if (ModelState.IsValid)
            {


                using (SqlConnection connection = new SqlConnection(conexionDB))
                {
                    connection.Open();

                    string query = "UPDATE Clientes SET Nombre = @Nombre, Apellido = @Apellido, Email = @Email, Telefono = @Telefono, Direccion = @Direccion, FechaNacimiento = @FechaNacimiento, Preferencias = @Preferencias, Notas = @Notas WHERE Id = @Id";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id", model.Id);
                        command.Parameters.AddWithValue("@Nombre", model.Nombre ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Apellido", model.Apellido ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Email", model.Email ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Telefono", model.Telefono ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Direccion", model.Direccion ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@FechaNacimiento", model.FechaNacimiento == default ? (object)DBNull.Value : model.FechaNacimiento);
                        command.Parameters.AddWithValue("@Preferencias", model.Preferencias ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Notas", model.Notas ?? (object)DBNull.Value);

                        command.ExecuteNonQuery();
                    }
                }

                return RedirectToAction("Clientes", new { userName = usuario, userId = idUser, userCenter = centros, userRole = role }); // Redirigir a la vista de lista de centros
            }

            return View(model); // Mostrar el formulario con errores
        }

        [HttpPost]
        public async Task<ActionResult> EliminarCliente(int id)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(conexionDB))
                {
                    await conn.OpenAsync();

                    string query = "DELETE FROM Clientes WHERE Id = @Id";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", id);

                        int rowsAffected = await cmd.ExecuteNonQueryAsync();

                        if (rowsAffected > 0)
                        {
                            // Registro eliminado correctamente
                            return Json(new { success = true });
                        }
                        else
                        {
                            // No se encontró el registro
                            return Json(new { success = false, message = "Cliente no encontrado." });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Manejo de errores
                return Json(new { success = false, message = "Ocurrió un error al eliminar el cliente: " + ex.Message });
            }
        }

        //-------------//-----------------//---------------//---------------//------------------//-----------------//-----------//

        //Citas

        public ActionResult Citas(int userId, string userName, string userRole, int userCenter)
        {
            ViewBag.UserId = userId;
            ViewBag.UserName = userName;
            ViewBag.UserRole = userRole;
            ViewBag.UserCenter = userCenter;
            return View();
        }

        [HttpGet]
        public JsonResult ObtenerCitas()
        {
            List<Citas> citas = new List<Citas>();

            using (SqlConnection con = new SqlConnection(conexionDB))
            {
                con.Open();
                string query = "SELECT Id, Cliente, Trabajador, Servicio, FechaHoraInicio, FechaHoraFin, Duracion, Notas FROM Citas";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Citas cita = new Citas
                            {
                                Id = reader.GetInt32(0),
                                Cliente = reader.GetInt32(1),
                                Trabajador = reader.GetInt32(2),
                                Servicio = reader.GetString(3),
                                FechaHoraInicio = reader.GetDateTime(4),
                                FechaHoraFin = reader.GetDateTime(5),
                                Duracion = reader.GetTimeSpan(6),
                                Notas = reader.GetString(7)
                            };
                            citas.Add(cita);
                        }
                    }
                }
            }

            return Json(citas, JsonRequestBehavior.AllowGet);
        }

        // Método para obtener los detalles de una cita por ID
        [HttpGet]
        public JsonResult ObtenerCitaPorId(int id)
        {
            Citas cita = null;
            string nombreTrabajador = null;
            string nombreCliente = null;
            string apellidoCliente = null;

            using (SqlConnection con = new SqlConnection(conexionDB))
            {
                con.Open();

                // Consulta para obtener los detalles de la cita
                string queryCita = @"
            SELECT Id, Cliente, Trabajador, Servicio, FechaHoraInicio, FechaHoraFin, Duracion, Notas, Categoria
            FROM Citas
            WHERE Id = @Id";

                using (SqlCommand cmd = new SqlCommand(queryCita, con))
                {
                    cmd.Parameters.AddWithValue("@Id", id);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            cita = new Citas
                            {
                                Id = reader.GetInt32(0),
                                Cliente = reader.GetInt32(1),
                                Trabajador = reader.GetInt32(2),
                                Servicio = reader.GetString(3),
                                FechaHoraInicio = reader.GetDateTime(4),
                                FechaHoraFin = reader.GetDateTime(5),
                                Duracion = reader.GetTimeSpan(6),
                                Notas = reader.GetString(7),
                                Categoria = reader.GetString(8)
                            };
        
                    reader.Close(); // Cierra el DataReader antes de ejecutar otra consulta

                            // Obtener el nombre del trabajador
                            int trabajadorId = cita.Trabajador;
                            string queryTrabajador = "SELECT Nombre FROM Personal WHERE Id = @TrabajadorId";

                            using (SqlCommand cmdTrabajador = new SqlCommand(queryTrabajador, con))
                            {
                                cmdTrabajador.Parameters.AddWithValue("@TrabajadorId", trabajadorId);

                                using (SqlDataReader readerTrabajador = cmdTrabajador.ExecuteReader())
                                {
                                    if (readerTrabajador.Read())
                                    {
                                        nombreTrabajador = readerTrabajador.GetString(0);
                                    }

                                    readerTrabajador.Close();

                                    int clienteId = cita.Cliente;
                                    string queryCliente = "SELECT Nombre, Apellido FROM Clientes WHERE Id = @ClienteId";

                                    using (SqlCommand cmdCliente = new SqlCommand(queryCliente, con))
                                    {
                                        cmdCliente.Parameters.AddWithValue("@ClienteId", clienteId);

                                        using (SqlDataReader readerCliente = cmdCliente.ExecuteReader())
                                        {
                                            if (readerCliente.Read())
                                            {
                                                nombreCliente = readerCliente.GetString(0);
                                                apellidoCliente = readerCliente.GetString(1);
                                            }
                                        }
                                    }

                                }
                            }
                        }
                    }
                }
            }

            // Agregar el nombre del trabajador a la cita
            if (cita != null)
            {
                cita.NombreTrabajador = nombreTrabajador;
                cita.NombreCliente = nombreCliente + " " + apellidoCliente;
            }

            return Json(cita, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult ActualizarCita(Citas model)
        {
            double duracionMinutos = model.Duracion.TotalMinutes;

            // Calcular la FechaHoraFin a partir de FechaHoraInicio y Duracion en minutos
            DateTime fechaHoraFin = model.FechaHoraInicio.AddMinutes(duracionMinutos);
            using (SqlConnection conn = new SqlConnection(conexionDB))
            {
                string query = "UPDATE Citas SET Cliente = @ClienteId, Trabajador = @TrabajadorId, Servicio = @Servicio, FechaHoraInicio = @FechaHoraInicio, FechaHoraFin = @FechaHoraFin, Duracion = @Duracion, Notas = @Notas, Categoria = @Categoria WHERE Id = @Id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ClienteId", model.Cliente);
                cmd.Parameters.AddWithValue("@TrabajadorId", model.Trabajador);
                cmd.Parameters.AddWithValue("@Servicio", model.Servicio);
                cmd.Parameters.AddWithValue("@FechaHoraInicio", model.FechaHoraInicio);
                cmd.Parameters.AddWithValue("@FechaHoraFin", fechaHoraFin);
                cmd.Parameters.AddWithValue("@Duracion", model.Duracion);
                cmd.Parameters.AddWithValue("@Notas", model.Notas);
                cmd.Parameters.AddWithValue("@Categoria", model.Categoria);
                cmd.Parameters.AddWithValue("@Id", model.Id);

                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
            }

            return Json(new { success = true });
        }

        public JsonResult ObtenerCategorias()
        {
            List<string> categorias = new List<string>();

            
            string query = "SELECT DISTINCT Categoria FROM Servicios";

            using (SqlConnection connection = new SqlConnection(conexionDB))
            {
                SqlCommand command = new SqlCommand(query, connection);
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    categorias.Add(reader["Categoria"].ToString());
                }
            }

            return Json(categorias, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetServicios(string categoria)
        {
            List<Servicio> servicios = new List<Servicio>();

            using (SqlConnection con = new SqlConnection(conexionDB))
            {
                con.Open();
                string query = "SELECT Nombre FROM Servicios WHERE Categoria = @Categoria";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Categoria", categoria);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            servicios.Add(new Servicio
                            {
                                Nombre = reader.GetString(0)
                            });
                        }
                    }
                }
            }

            return Json(servicios, JsonRequestBehavior.AllowGet);
        }


        public JsonResult GetClientes()
        {
            List<Cliente> clientes = new List<Cliente>();

            using (SqlConnection con = new SqlConnection(conexionDB))
            {
                con.Open();
                string query = "SELECT Id, Nombre, Apellido FROM Clientes";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            clientes.Add(new Cliente
                            {
                                Id = reader.GetInt32(0),
                                Nombre = reader.GetString(1) + " " + reader.GetString(2)
                            });
                        }
                    }
                }
            }

            return Json(clientes, JsonRequestBehavior.AllowGet);
        }

        // Método para cargar la lista de personal
        public JsonResult GetPersonal()
        {
            List<Personal> personal = new List<Personal>();

            using (SqlConnection con = new SqlConnection(conexionDB))
            {
                con.Open();
                string query = "SELECT Id, Nombre, HorariosOperacion FROM Personal";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            personal.Add(new Personal
                            {
                                Id = reader.GetInt32(0),
                                Nombre = reader.GetString(1),
                                Horarios = reader.GetString(2)
                            });
                        }
                    }
                }
            }

            return Json(personal, JsonRequestBehavior.AllowGet);
        }

        public JsonResult VerificarDisponibilidad(int trabajadorId, DateTime fechaInicio, TimeSpan duracion)
        {
            bool estaDisponible = true;

            using (SqlConnection con = new SqlConnection(conexionDB))
            {
                con.Open();

                // Verificar si el trabajador tiene citas en el rango de tiempo
                string query = @"
                SELECT COUNT(*)
                FROM Citas
                WHERE Trabajador = @Trabajador
                AND ((@FechaInicio BETWEEN FechaHoraInicio AND FechaHoraFin)
                OR (DATEADD(minute, DATEDIFF(minute, 0, Duracion), @FechaInicio) BETWEEN FechaHoraInicio AND FechaHoraFin))";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Trabajador", trabajadorId);
                    cmd.Parameters.AddWithValue("@FechaInicio", fechaInicio);

                    int count = (int)cmd.ExecuteScalar();
                    if (count > 0)
                    {
                        estaDisponible = false;
                    }
                }

                // Verificar horarios de operación del trabajador
                if (estaDisponible)
                {
                    string horariosQuery = "SELECT HorariosOperacion FROM Personal WHERE Id = @Id";
                    using (SqlCommand horariosCmd = new SqlCommand(horariosQuery, con))
                    {
                        horariosCmd.Parameters.AddWithValue("@Id", trabajadorId);

                        string horarios = (string)horariosCmd.ExecuteScalar();
                        estaDisponible = VerificarHorario(horarios, fechaInicio, duracion);
                    }
                }
            }

            return Json(estaDisponible, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult EliminarCita(int id)
        {
            bool success = false;

            using (SqlConnection connection = new SqlConnection(conexionDB))
            {
                string query = "DELETE FROM Citas WHERE Id = @Id";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Id", id);

                try
                {
                    connection.Open();
                    int result = command.ExecuteNonQuery();
                    success = result > 0;
                }
                catch (Exception ex)
                {
                    // Manejar la excepción
                    Console.WriteLine(ex.Message);
                }
            }

            return Json(new { success = success });
        }

        [HttpPost]
        public JsonResult AgendarCita(Citas cita)
        {
            using (SqlConnection con = new SqlConnection(conexionDB))
            {
                con.Open();
                double duracionMinutos = cita.Duracion.TotalMinutes;

                // Calcular la FechaHoraFin a partir de FechaHoraInicio y Duracion en minutos
                DateTime fechaHoraFin = cita.FechaHoraInicio.AddMinutes(duracionMinutos);
                string query = @"
                INSERT INTO Citas (Cliente, Trabajador, Servicio, FechaHoraInicio, FechaHoraFin, Duracion, Notas, Categoria)
                VALUES (@Cliente, @Trabajador, @Servicio, @FechaHoraInicio, @FechaHoraFin, @Duracion, @Notas, @Categoria)";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Cliente", cita.Cliente);
                    cmd.Parameters.AddWithValue("@Trabajador", cita.Trabajador);
                    cmd.Parameters.AddWithValue("@Servicio", cita.Servicio);
                    cmd.Parameters.AddWithValue("@FechaHoraInicio", cita.FechaHoraInicio);
                    cmd.Parameters.AddWithValue("@FechaHoraFin", fechaHoraFin);
                    cmd.Parameters.AddWithValue("@Duracion", cita.Duracion);
                    cmd.Parameters.AddWithValue("@Notas", cita.Notas);
                    cmd.Parameters.AddWithValue("@Categoria", cita.Categoria);

                    cmd.ExecuteNonQuery();
                }
            }

            return Json(new { success = true });
        }

        private bool VerificarHorario(string horariosOperacion, DateTime fechaInicio, TimeSpan duracion)
        {
            // Separar los días y las horas de operación
            var partes = horariosOperacion.Split(new[] { " - Hora: " }, StringSplitOptions.None);
            var diasDisponibles = partes[0].Split(','); // Días separados por comas (por ejemplo: "Viernes", "Sábado", "Domingo")
            var horasOperacion = partes[1].Split(new[] { " a " }, StringSplitOptions.None); // Rango de horas (por ejemplo: "23:48 a 01:48")

            // Parsear las horas de inicio y fin del rango
            TimeSpan horaInicioTrabajo = TimeSpan.Parse(horasOperacion[0]);
            TimeSpan horaFinTrabajo = TimeSpan.Parse(horasOperacion[1]);

            // Obtener el día de la semana de la cita
            string diaSemana = fechaInicio.ToString("dddd", new System.Globalization.CultureInfo("es-ES"));

            // Verificar si el día de la cita está dentro de los días disponibles
            bool diaDisponible = diasDisponibles.Any(d => d.Trim().Equals(diaSemana, StringComparison.OrdinalIgnoreCase));
            if (!diaDisponible)
            {
                return false; // El trabajador no está disponible en ese día
            }

            // Verificar la hora de la cita
            TimeSpan horaInicioCita = fechaInicio.TimeOfDay;
            TimeSpan horaFinCita = horaInicioCita.Add(duracion);

            // Caso donde la hora de fin de trabajo es menor a la de inicio (cruza la medianoche)
            if (horaFinTrabajo < horaInicioTrabajo)
            {
                // Caso 1: La cita empieza antes de medianoche y termina después (cruza la medianoche)
                if ((horaInicioCita >= horaInicioTrabajo && horaFinCita <= TimeSpan.FromHours(24)) ||
                    (horaInicioCita >= TimeSpan.Zero && horaFinCita <= horaFinTrabajo))
                {
                    return true;
                }
            }
            else
            {
                // Verificar si la cita está dentro del rango del mismo día
                if (horaInicioCita >= horaInicioTrabajo && horaFinCita <= horaFinTrabajo)
                {
                    return true; // El trabajador está disponible
                }
            }

            return false; // La cita no está dentro del rango de horas
        }

        //-------------//-----------------//---------------//---------------//------------------//-----------------//-----------//

        //Servicios

        public ActionResult Servicios(int userId, string userName, string userRole, int userCenter)
        {
            List<Servicio> servicios = new List<Servicio>();

            using (SqlConnection connection = new SqlConnection(conexionDB))
            {
                string query = "SELECT Id, Nombre, Descripcion, Duracion, Precio, Categoria, TrabajadorEspecialidad FROM Servicios";

                SqlCommand command = new SqlCommand(query, connection);

                try
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        double duracionMinutos = reader.GetTimeSpan(3).TotalMinutes;
                        Servicio servicio = new Servicio
                        {
                            Id = reader.GetInt32(0),
                            Nombre = reader.GetString(1),
                            Descripcion = reader.GetString(2),
                            DuracionNumero = duracionMinutos,
                            Precio = reader.GetInt32(4),
                            Categoria = reader.GetString(5),
                            TrabajadorEspecialidad = reader.GetString(6),                           
                        };
                        servicios.Add(servicio);
                    }
                }
                catch (SqlException ex)
                {
                    // Manejo de errores
                    Console.WriteLine(ex.Message);
                }
            }

            ViewBag.UserId = userId;
            ViewBag.UserName = userName;
            ViewBag.UserRole = userRole;
            ViewBag.UserCenter = userCenter;
            return View(servicios);
        }

        [HttpPost]
        public ActionResult AgregarServicios(Servicio servicio, string usuario, string id, string centros, string role)
        {
            if (!ModelState.IsValid)
            {
                // Si el modelo no es válido, vuelve a mostrar el formulario
                return View(servicio);
            }

           

            // Insertar datos en la base de datos
            using (SqlConnection connection = new SqlConnection(conexionDB))
            {
                string query = @"INSERT INTO Servicios (Nombre, Descripcion, Duracion, Precio, Categoria, TrabajadorEspecialidad)
                                VALUES (@Nombre, @Descripcion, @Duracion, @Precio, @Categoria, @TrabajadorEspecialidad)"
                ;

                

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Nombre", servicio.Nombre);
                    command.Parameters.AddWithValue("@Descripcion", servicio.Descripcion);
                    command.Parameters.AddWithValue("@Duracion", servicio.Duracion);
                    command.Parameters.AddWithValue("@Precio", servicio.Precio);
                    command.Parameters.AddWithValue("@Categoria", servicio.Categoria);
                    command.Parameters.AddWithValue("@TrabajadorEspecialidad", servicio.TrabajadorEspecialidad);
                   

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }

            // Redirigir a una vista de éxito o de listado
            return RedirectToAction("Servicios", new { userName = usuario, userId = id, userCenter = centros, userRole = role }); // O la acción a la que quieras redirigir después de guardar
        }

        [HttpGet]
        public JsonResult ObtenerServicioId(int id)
        {
            Servicio servicio = null;

            using (SqlConnection connection = new SqlConnection(conexionDB))
            {
                connection.Open();

                string query = "SELECT Id, Nombre, Descripcion, Duracion, Precio, Categoria, TrabajadorEspecialidad FROM Servicios WHERE Id = @Id";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            double duracionMinutos = reader.GetTimeSpan(3).TotalMinutes;
                            servicio = new Servicio
                            {
                                Id = reader.GetInt32(0),
                                Nombre = reader.GetString(1),
                                Descripcion = reader.GetString(2),
                                DuracionNumero = duracionMinutos,
                                Precio = reader.GetInt32(4),
                                Categoria = reader.GetString(5),
                                TrabajadorEspecialidad = reader.GetString(6),                               
                            };
                        }
                    }
                }
            }

            if (servicio != null)
            {
                return Json(servicio, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { success = false, message = "Servicio no encontrado." }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult ActualizarServicios(Servicio model, string usuario, string idUser, string centros, string role)
        {
            if (ModelState.IsValid)
            {
                using (SqlConnection connection = new SqlConnection(conexionDB))
                {
                    connection.Open();

                    string query = "UPDATE Servicios SET Nombre = @Nombre, Descripcion = @Descripcion, Duracion = @Duracion, Precio = @Precio, Categoria = @Categoria, TrabajadorEspecialidad = @TrabajadorEspecialidad WHERE Id = @Id";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id", model.Id);
                        command.Parameters.AddWithValue("@Nombre", model.Nombre);
                        command.Parameters.AddWithValue("@Descripcion", model.Descripcion);
                        command.Parameters.AddWithValue("@Duracion", model.Duracion);
                        command.Parameters.AddWithValue("@Precio", model.Precio);
                        command.Parameters.AddWithValue("@Categoria", model.Categoria);
                        command.Parameters.AddWithValue("@TrabajadorEspecialidad", model.TrabajadorEspecialidad);

                        command.ExecuteNonQuery();
                    }
                }

                return RedirectToAction("Servicios", new { userName = usuario, userId = idUser, userCenter = centros, userRole = role }); // Redirigir a la vista de lista de centros
            }

            return View(model); // Mostrar el formulario con errores
        }

        [HttpPost]
        public async Task<ActionResult> EliminarServicio(int id)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(conexionDB))
                {
                    await conn.OpenAsync();

                    string query = "DELETE FROM Servicios WHERE Id = @Id";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", id);

                        int rowsAffected = await cmd.ExecuteNonQueryAsync();

                        if (rowsAffected > 0)
                        {
                            // Registro eliminado correctamente
                            return Json(new { success = true });
                        }
                        else
                        {
                            // No se encontró el registro
                            return Json(new { success = false, message = "Servicio no encontrado." });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Manejo de errores
                return Json(new { success = false, message = "Ocurrió un error al eliminar el Servicio: " + ex.Message });
            }
        }

        public ActionResult CitasHistorial(int userId, string userName, string userRole, int userCenter)
        {
            
            ViewBag.UserId = userId;
            ViewBag.UserName = userName;
            ViewBag.UserRole = userRole;
            ViewBag.UserCenter = userCenter;

            List<Citas> citas = new List<Citas>();

            using (SqlConnection con = new SqlConnection(conexionDB))
            {
                con.Open();
                string query = "SELECT C.Id, C.Cliente, C.Trabajador, C.Categoria, C.Servicio, C.FechaHoraInicio, C.FechaHoraFin, C.Duracion, C.Notas, CL.Nombre AS NombreCliente, CL.Apellido AS ApellidoCliente, P.Nombre AS NombreTrabajador " +
                               "FROM Citas C " +
                               "INNER JOIN Clientes CL ON C.Cliente = CL.Id " +
                               "INNER JOIN Personal P ON C.Trabajador = P.Id";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Citas cita = new Citas
                            {
                                Id = reader.GetInt32(0),
                                Cliente = reader.GetInt32(1),
                                Trabajador = reader.GetInt32(2),
                                Categoria = reader.GetString(3),
                                Servicio = reader.GetString(4),
                                FechaHoraInicio = reader.GetDateTime(5),
                                FechaHoraFin = reader.GetDateTime(6),
                                Duracion = reader.GetTimeSpan(7),
                                Notas = reader.GetString(8),
                                NombreCliente = reader.GetString(9) + " " + reader.GetString(10), // Nombre y Apellido del Cliente
                                NombreTrabajador = reader.GetString(11) // Nombre del Trabajador
                            };
                            citas.Add(cita);
                        }
                    }
                }
            }

            return View(citas);
        }


        //AQUI ESTA FACUTACION ---------------- //

        // Vista principal de facturación, muestra el listado de facturas
            public ActionResult FacturacionHistorial(int userId, string userName, string userRole, int userCenter)
            {
            ViewBag.UserId = userId;
            ViewBag.UserName = userName;
            ViewBag.UserRole = userRole;
            ViewBag.UserCenter = userCenter;

            List<Facturacion> facturas = new List<Facturacion>();

            using (SqlConnection con = new SqlConnection(conexionDB))
            {
                con.Open();

                string query = @"
                SELECT F.Id, F.CitaId, F.FechaEmision, F.Total, F.Pagado, F.MetodoPago, F.FechaPago, F.NumeroRecibo, F.Detalles, 
                       CL.Nombre AS NombreCliente, CL.Apellido AS ApellidoCliente, P.Nombre AS NombreTrabajador
                FROM Facturacion F
                INNER JOIN Citas C ON F.CitaId = C.Id
                INNER JOIN Clientes CL ON C.Cliente = CL.Id
                INNER JOIN Personal P ON C.Trabajador = P.Id";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Facturacion factura = new Facturacion
                            {
                                Id = reader.GetInt32(0),
                                CitaId = reader.GetInt32(1),
                                FechaEmision = reader.GetDateTime(2),
                                Total = reader.GetDecimal(3),
                                Pagado = reader.GetBoolean(4),
                                MetodoPago = reader.GetString(5),
                                FechaPago = reader.IsDBNull(6) ? (DateTime?)null : reader.GetDateTime(6),
                                NumeroRecibo = reader.GetString(7),
                                Detalles = reader.GetString(8),
                                Cita = new Citas
                                {
                                    NombreCliente = reader.GetString(9),
                                    ApellidoCliente = reader.GetString(10),
                                    NombreTrabajador = reader.GetString(11)
                                }
                            };
                            facturas.Add(factura);
                        }
                    }
                }
            }
            

            return View("FacturacionHistorial", facturas);
            }


            public ActionResult Facturacion(int userId, string userName, string userRole, int userCenter)
            {
                ViewBag.UserId = userId;
                ViewBag.UserName = userName;
                ViewBag.UserRole = userRole;
                ViewBag.UserCenter = userCenter;

                // Lista para almacenar las citas
                List<Citas> citas = new List<Citas>();

                // Establecer conexión con la base de datos
                using (SqlConnection con = new SqlConnection(conexionDB))
                {
                    con.Open();

                    // Consulta SQL con INNER JOIN para obtener las citas y el nombre del cliente
                    string query = @"
                SELECT Citas.Id, Clientes.Nombre, Citas.Servicio 
                FROM Citas 
                INNER JOIN Clientes ON Citas.Cliente = Clientes.Id";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            // Leer cada cita y agregarla a la lista
                            while (reader.Read())
                            {
                                Citas cita = new Citas
                                {
                                    Id = reader.GetInt32(0),
                                    NombreCliente = reader.GetString(1),  // Aquí estamos accediendo al nombre del cliente desde la tabla Clientes
                                    Servicio = reader.GetString(2)
                                };
                                citas.Add(cita);
                            }
                        }
                    }
                }

                // Pasar la lista de citas a la vista mediante ViewBag
                ViewBag.Citas = citas;

                // Retornar la vista Facturacion
                return View("Facturacion");
            }


        [HttpPost]
            public ActionResult CrearFactura(int citaId, decimal total, string metodoPago, string detalles, int userId, string userName, string userRole, int userCenter)
            {
                using (SqlConnection con = new SqlConnection(conexionDB))
                {
                    con.Open();

                    // Consulta SQL para insertar una nueva factura
                    string query = @"
            INSERT INTO Facturacion (CitaId, FechaEmision, Total, Pagado, MetodoPago, NumeroRecibo, Detalles) 
            VALUES (@CitaId, GETDATE(), @Total, 0, @MetodoPago, @NumeroRecibo, @Detalles)";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        // Asignar parámetros a la consulta
                        cmd.Parameters.AddWithValue("@CitaId", citaId);
                        cmd.Parameters.AddWithValue("@Total", total);
                        cmd.Parameters.AddWithValue("@MetodoPago", metodoPago);
                        cmd.Parameters.AddWithValue("@NumeroRecibo", GenerarNumeroRecibo()); // Puedes crear una función que genere un número de recibo
                        cmd.Parameters.AddWithValue("@Detalles", detalles);

                        // Ejecutar la consulta
                        cmd.ExecuteNonQuery();
                    }
                }

                // Redirigir al historial de facturación después de crear la factura
                return RedirectToAction("FacturacionHistorial", new { userId, userName, userRole, userCenter });
            }


        // Marcar una factura como pagada
        [HttpPost]
            public ActionResult MarcarComoPagado(int facturaId)
            {
                using (SqlConnection con = new SqlConnection(conexionDB))
                {
                    con.Open();
                    string query = "UPDATE Facturacion SET Pagado = 1, FechaPago = GETDATE() WHERE Id = @FacturaId";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@FacturaId", facturaId);
                        cmd.ExecuteNonQuery();
                    }
                }

                return RedirectToAction("FacturacionHistorial");
            }

            // Generar número de recibo aleatorio
            private string GenerarNumeroRecibo()
            {
                return "REC-" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
            }

            // Descargar recibo en PDF o cualquier formato
            public ActionResult DescargarRecibo(int facturaId)
            {
                // Aquí puedes implementar la lógica para generar y descargar el recibo en formato PDF.
                // Esto incluirá detalles de la factura, fecha de emisión, total y número de recibo.

                // Ejemplo simplificado:
                Facturacion factura = ObtenerFacturaPorId(facturaId);

                if (factura == null)
                {
                    return HttpNotFound("Factura no encontrada.");
                }

                // Lógica para generar un PDF basado en los detalles de la factura
                // (Puedes usar bibliotecas como iTextSharp para generar PDFs)

                return View(factura); // Debe ser reemplazado por la descarga del archivo generado
            }

            // Obtener detalles de una factura por su Id
            private Facturacion ObtenerFacturaPorId(int id)
            {
                Facturacion factura = null;

                using (SqlConnection con = new SqlConnection(conexionDB))
                {
                    con.Open();
                    string query = @"
                SELECT F.Id, F.CitaId, F.FechaEmision, F.Total, F.Pagado, F.MetodoPago, F.FechaPago, F.NumeroRecibo, F.Detalles
                FROM Facturacion F
                WHERE F.Id = @Id";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@Id", id);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                factura = new Facturacion
                                {
                                    Id = reader.GetInt32(0),
                                    CitaId = reader.GetInt32(1),
                                    FechaEmision = reader.GetDateTime(2),
                                    Total = reader.GetDecimal(3),
                                    Pagado = reader.GetBoolean(4),
                                    MetodoPago = reader.GetString(5),
                                    FechaPago = reader.IsDBNull(6) ? (DateTime?)null : reader.GetDateTime(6),
                                    NumeroRecibo = reader.GetString(7),
                                    Detalles = reader.GetString(8)
                                };
                            }
                        }
                    }
                }

                return factura;
            }

        //-----------------------------//_----------------------//--------------------------------------
        //FEEDBACK MODULO

            public ActionResult Feedback(int userId, string userName, string userRole, int userCenter)
            {
                ViewBag.UserId = userId;
                ViewBag.UserName = userName;
                ViewBag.UserRole = userRole;
                ViewBag.UserCenter = userCenter;

                // Lista para almacenar las citas y trabajadores
                List<Citas> citas = new List<Citas>();
                List<Personal> trabajadores = new List<Personal>();

                using (SqlConnection con = new SqlConnection(conexionDB))
                {
                    con.Open();

                    // Obtener citas del cliente
                    string queryCitas = @"
                SELECT Citas.Id, Clientes.Nombre AS NombreCliente, Citas.Servicio 
                FROM Citas 
                INNER JOIN Clientes ON Citas.Cliente = Clientes.Id";

                    using (SqlCommand cmd = new SqlCommand(queryCitas, con))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                citas.Add(new Citas
                                {
                                    Id = reader.GetInt32(0),
                                    NombreCliente = reader.GetString(1),
                                    Servicio = reader.GetString(2)
                                });
                            }
                        }
                    }

                    // Obtener trabajadores
                    string queryTrabajadores = "SELECT Id, Nombre FROM Personal";
                    using (SqlCommand cmdTrabajadores = new SqlCommand(queryTrabajadores, con))
                    {
                        using (SqlDataReader readerTrabajadores = cmdTrabajadores.ExecuteReader())
                        {
                            while (readerTrabajadores.Read())
                            {
                                trabajadores.Add(new Personal
                                {
                                    Id = readerTrabajadores.GetInt32(0),
                                    Nombre = readerTrabajadores.GetString(1)
                                });
                            }
                        }
                    }
                }

                // Pasar las citas y trabajadores a la vista
                ViewBag.Citas = citas;
                ViewBag.Trabajadores = trabajadores;

                return View();
            }

            [HttpPost]
            public ActionResult CrearFeedback(Feedback feedback)
            {
                if (ModelState.IsValid)
                {
                    using (SqlConnection con = new SqlConnection(conexionDB))
                    {
                        con.Open();

                        string query = @"
                    INSERT INTO Feedback (ClienteId, TrabajadorId, CitaId, Calificacion, Comentario, Fecha)
                    VALUES (@ClienteId, @TrabajadorId, @CitaId, @Calificacion, @Comentario, GETDATE())";

                        using (SqlCommand cmd = new SqlCommand(query, con))
                        {
                            cmd.Parameters.AddWithValue("@ClienteId", feedback.ClienteId);
                            cmd.Parameters.AddWithValue("@TrabajadorId", feedback.TrabajadorId);
                            cmd.Parameters.AddWithValue("@CitaId", feedback.CitaId);
                            cmd.Parameters.AddWithValue("@Calificacion", feedback.Calificacion);
                            cmd.Parameters.AddWithValue("@Comentario", feedback.Comentario);

                            cmd.ExecuteNonQuery();
                        }
                    }

                    return RedirectToAction("Feedback");
                }

                return View(feedback);
            }

        // ---------- ahora para el feedback resumen
            public ActionResult FeedbackResumen(int userId, string userName, string userRole, int userCenter, int? trabajadorId, string servicio)
            {
                ViewBag.UserId = userId;
                ViewBag.UserName = userName;
                ViewBag.UserRole = userRole;
                ViewBag.UserCenter = userCenter;

                // Lista para almacenar los feedbacks
                List<Feedback> feedbacks = new List<Feedback>();

                using (SqlConnection con = new SqlConnection(conexionDB))
                {
                    con.Open();

                    // Consulta SQL para obtener el feedback con joins para obtener datos de clientes y trabajadores
                    string query = @"
                SELECT F.Id, C.Nombre AS NombreCliente, P.Nombre AS NombreTrabajador, F.Calificacion, F.Comentario, F.Fecha, Citas.Servicio
                FROM Feedback F
                INNER JOIN Clientes C ON F.ClienteId = C.Id
                INNER JOIN Personal P ON F.TrabajadorId = P.Id
                INNER JOIN Citas ON F.CitaId = Citas.Id
                WHERE (@TrabajadorId IS NULL OR P.Id = @TrabajadorId)
                AND (@Servicio IS NULL OR Citas.Servicio = @Servicio)";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@TrabajadorId", trabajadorId.HasValue ? (object)trabajadorId.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@Servicio", string.IsNullOrEmpty(servicio) ? (object)DBNull.Value : servicio);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                feedbacks.Add(new Feedback
                                {
                                    Id = reader.GetInt32(0),
                                    NombreCliente = reader.GetString(1),
                                    NombreTrabajador = reader.GetString(2),
                                    Calificacion = reader.GetInt32(3),
                                    Comentario = reader.GetString(4),
                                    Fecha = reader.GetDateTime(5),
                                    Servicio = reader.GetString(6)
                                });
                            }
                        }
                    }
                }

                // Cargar trabajadores y servicios para los filtros
                List<Personal> trabajadores = new List<Personal>();
                List<string> servicios = new List<string>();

                using (SqlConnection con = new SqlConnection(conexionDB))
                {
                    con.Open();

                    // Obtener lista de trabajadores
                    string queryTrabajadores = "SELECT Id, Nombre FROM Personal";
                    using (SqlCommand cmd = new SqlCommand(queryTrabajadores, con))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                trabajadores.Add(new Personal
                                {
                                    Id = reader.GetInt32(0),
                                    Nombre = reader.GetString(1)
                                });
                            }
                        }
                    }

                    // Obtener lista de servicios
                    string queryServicios = "SELECT DISTINCT Servicio FROM Citas";
                    using (SqlCommand cmd = new SqlCommand(queryServicios, con))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                servicios.Add(reader.GetString(0));
                            }
                        }
                    }
                }

                ViewBag.Trabajadores = trabajadores;
                ViewBag.Servicios = servicios;

                return View(feedbacks);
            }

        //-----------------------------//_----------------------//--------------------------------------
        //PROMOCIONES MODULO

            public ActionResult Promociones(int userId, string userName, string userRole, int userCenter)
            {
                ViewBag.UserId = userId;
                ViewBag.UserName = userName;
                ViewBag.UserRole = userRole;
                ViewBag.UserCenter = userCenter;

                List<Promocion> promociones = new List<Promocion>();

                using (SqlConnection con = new SqlConnection(conexionDB))
                {
                    con.Open();
                    string query = "SELECT Id, Nombre, Descripcion, PorcentajeDescuento, FechaInicio, FechaFin, Servicio, Activo FROM Promociones";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                promociones.Add(new Promocion
                                {
                                    Id = reader.GetInt32(0),
                                    Nombre = reader.GetString(1),
                                    Descripcion = reader.GetString(2),
                                    PorcentajeDescuento = reader.GetDecimal(3),
                                    FechaInicio = reader.GetDateTime(4),
                                    FechaFin = reader.GetDateTime(5),
                                    Servicio = reader.IsDBNull(6) ? null : reader.GetString(6),
                                    Activo = reader.GetBoolean(7)
                                });
                            }
                        }
                    }
                }

                return View(promociones);
            }

            [HttpPost]
            public ActionResult CrearPromocion(Promocion promocion)
            {
                using (SqlConnection con = new SqlConnection(conexionDB))
                {
                    con.Open();
                    string query = @"
                    INSERT INTO Promociones (Nombre, Descripcion, PorcentajeDescuento, FechaInicio, FechaFin, Servicio, Activo) 
                    VALUES (@Nombre, @Descripcion, @PorcentajeDescuento, @FechaInicio, @FechaFin, @Servicio, 1)";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@Nombre", promocion.Nombre);
                        cmd.Parameters.AddWithValue("@Descripcion", promocion.Descripcion ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@PorcentajeDescuento", promocion.PorcentajeDescuento);
                        cmd.Parameters.AddWithValue("@FechaInicio", promocion.FechaInicio);
                        cmd.Parameters.AddWithValue("@FechaFin", promocion.FechaFin);
                        cmd.Parameters.AddWithValue("@Servicio", string.IsNullOrEmpty(promocion.Servicio) ? (object)DBNull.Value : promocion.Servicio);

                        cmd.ExecuteNonQuery();
                    }
                }

                // Redirigir nuevamente a la vista de Promociones para ver la lista actualizada
                return RedirectToAction("Promociones", new { userId = ViewBag.UserId, userName = ViewBag.UserName, userRole = ViewBag.UserRole, userCenter = ViewBag.UserCenter });
            }



    }


}