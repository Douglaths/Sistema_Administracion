using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdministradorLaboral.Models
{
    public class Centro
    {
        public int Id { get; set; }
        public string IdCentro { get; set; }
        public string Nombre { get; set; }
        public string Direccion { get; set; }
        public string Telefono { get; set; }
        public string Email { get; set; }
        public string Horarios { get; set; }
        public int CapacidadMaxima { get; set; }
        public string Centros { get; set; }
        public string Rol { get; set; }
    }
}