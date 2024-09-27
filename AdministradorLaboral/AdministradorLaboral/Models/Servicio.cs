using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdministradorLaboral.Models
{
    public class Servicio
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public TimeSpan Duracion { get; set; }
        public double DuracionNumero { get; set; }
        public int Precio { get; set; }
        public string Categoria { get; set; }
        public string TrabajadorEspecialidad { get; set; }
        
    }
}