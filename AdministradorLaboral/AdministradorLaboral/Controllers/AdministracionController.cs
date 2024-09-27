using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.Mvc;

namespace AdministradorLaboral.Controllers
{
    public class AdministracionController : Controller
    {
        private readonly string conexionDB = ConfigurationManager.ConnectionStrings["Cadena"].ConnectionString;


        // GET: Administracion
        public ActionResult Login()
        {
            return View();
        }

        
        [HttpPost]
        public JsonResult Login(string username, string password)
        {
            // Encriptar o hashear la contraseña antes de compararla
            

            using (SqlConnection con = new SqlConnection(conexionDB))
            {
                string query = "SELECT Id, Usuario, Contraseña, Rol, Nombre, IdCentro FROM Usuarios WHERE Usuario = @Usuario AND Contraseña = @Contraseña";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@Usuario", username);
                cmd.Parameters.AddWithValue("@Contraseña", password);

                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    // Obtenemos los datos del usuario
                    int userId = reader.GetInt32(0);
                    string userRole = reader.GetString(3);
                    string userName = reader.GetString(4);
                    int userCenter = reader.GetInt32(5);

                    // Guardar información del usuario en la sesión o similar
                    Session["UserId"] = userId;
                    Session["UserName"] = userName;
                    Session["UserRole"] = userRole;
                    Session["UserCenter"] = userCenter;

                    // Determinar la vista a la que se redirige según el rol
                    string redirectUrl;
                    switch (userRole)
                    {
                        case "Administrador":
                            redirectUrl = Url.Action("AdminDashboard", "Administrador", new { userId, userName, userRole, userCenter });
                            break;
                        case "Supervisor":
                            redirectUrl = Url.Action("SupervisorDashboard", "Supervisor", new { userId, userName, userRole, userCenter });
                            break;
                        case "Trabajador":
                            redirectUrl = Url.Action("WorkerDashboard", "Trabajador", new { userId, userName, userRole, userCenter });
                            break;
                        case "Recepcionista":
                            redirectUrl = Url.Action("ReceptionistDashboard", "Recepcion", new { userId, userName, userRole, userCenter });
                            break;
                        default:
                            redirectUrl = Url.Action("Index", "Home");
                            break;
                    }


                    return Json(new { success = true, redirectUrl });
                }
                else
                {
                    return Json(new { success = false, message = "Usuario o contraseña incorrectos." });
                }
            }
        }
    }
}