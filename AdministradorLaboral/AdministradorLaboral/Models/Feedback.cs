using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdministradorLaboral.Models
{
    public class Feedback
    {
        public int Id { get; set; }                    // Identificador único del feedback
        public int ClienteId { get; set; }             // Identificador del cliente que da el feedback
        public int TrabajadorId { get; set; }          // Identificador del trabajador sobre el que se da el feedback
        public int CitaId { get; set; }                // Identificador de la cita
        public int Calificacion { get; set; }          // Calificación dada por el cliente (1-5)
        public string Comentario { get; set; }         // Comentario del cliente sobre el servicio
        public DateTime Fecha { get; set; }            // Fecha en la que se dejó el feedback

        // Propiedades opcionales para mostrar información en vistas
        public string NombreCliente { get; set; }      // Nombre del cliente (para mostrar en vistas)
        public string NombreTrabajador { get; set; }   // Nombre del trabajador (para mostrar en vistas)
        public string Servicio { get; set; }           // Servicio recibido en la cita (para mostrar en vistas)
    }
}