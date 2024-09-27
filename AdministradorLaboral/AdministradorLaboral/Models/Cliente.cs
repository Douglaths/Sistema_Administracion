using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdministradorLaboral.Models
{
    public class Cliente
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Email { get; set; }
        public string Telefono { get; set; }
        public string Direccion { get; set; }
        public DateTime FechaNacimiento { get; set; }
        public string Preferencias { get; set; }
        public string Notas { get; set; }
    }
}