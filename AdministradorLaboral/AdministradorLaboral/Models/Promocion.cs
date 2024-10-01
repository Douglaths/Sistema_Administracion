using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdministradorLaboral.Models
{
    public class Promocion
    {
        public int Id { get; set; }                     // Identificador de la promoción
        public string Nombre { get; set; }              // Nombre de la promoción
        public string Descripcion { get; set; }         // Descripción de la promoción
        public decimal PorcentajeDescuento { get; set; } // Porcentaje de descuento
        public DateTime FechaInicio { get; set; }       // Fecha de inicio de la promoción
        public DateTime FechaFin { get; set; }          // Fecha de fin de la promoción
        public string Servicio { get; set; }            // Servicio al que aplica (NULL si es general)
        public bool Activo { get; set; }                // Estado de la promoción
    }
}