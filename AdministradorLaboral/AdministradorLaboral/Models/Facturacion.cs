using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdministradorLaboral.Models
{
    public class Facturacion
    {
            public int Id { get; set; } // ID único de la facturación
            public int CitaId { get; set; } // Relación con la cita
            public DateTime FechaEmision { get; set; } // Fecha de emisión de la factura
            public decimal Total { get; set; } // Total a pagar por el servicio
            public bool Pagado { get; set; } // Estado de pago
            public string MetodoPago { get; set; } // Método de pago (Efectivo, Tarjeta, etc.)
            public DateTime? FechaPago { get; set; } // Fecha en la que se realizó el pago (si ya ha sido pagado)
            public string NumeroRecibo { get; set; } // Número de recibo generado tras el pago
            public string Detalles { get; set; } // Detalles del servicio o pago

            // Relación con el modelo de Citas
            public virtual Citas Cita { get; set; }
        
    }
}