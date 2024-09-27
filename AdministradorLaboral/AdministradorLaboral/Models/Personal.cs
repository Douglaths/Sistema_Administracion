using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdministradorLaboral.Models
{
    public class Personal
    {
        public int Id { get; set; }
        public string DNI { get; set; }
        public string Nombre { get; set; }
        public string Direccion { get; set; }
        public string Telefono { get; set; }
        public string Email { get; set; }
        public string Horarios { get; set; }
        public string Genero { get; set; }
        public string Especializacion { get; set; }
        public string Experiencia { get; set; }
        public string Certificaciones { get; set; }
        public string[] CertificacionesArray { get; set; }
        public DateTime FechaNacimiento { get; set; }
        public string Foto { get; set; }
        public string Centros { get; set; }
        public string Rol { get; set; }
    }
}