using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdministradorLaboral.Models
{
    public class Citas
    {
        public int Id { get; set; }
        public int Cliente { get; set; }
        public string NombreCliente { get; set; }
        public int Trabajador { get; set; }
        public string NombreTrabajador { get; set; }
        public string Categoria { get; set; }
        public string Servicio { get; set; }
        public DateTime FechaHoraInicio { get; set; }
        public DateTime FechaHoraFin { get; set; }
        public TimeSpan Duracion { get; set; }
        public string Notas { get; set; }


    }
}