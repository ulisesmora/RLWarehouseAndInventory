using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Inventory.Domain
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum PurchaseOrderStatus
    {
        Draft,      // Borrador
        Confirmed,  // Enviada al proveedor
        Partial,    // Recibido parcialmente
        Completed,  // Recibido total
        Cancelled
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum QualityStatus
    {
        Pending,    // Esperando revisión
        Approved,   // Aprobado (Pasa a Stock Disponible)
        Rejected,   // Rechazado (Devolución o Scrap)
        Conditional // Aprobado con condiciones
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum MaterialType
    {
        RawMaterial = 1,    // Materia Prima (Tela, Hilo) - Tiene Stock
        FinishedGood = 2,   // Producto Terminado (Camisa) - Tiene Stock
        Consumable = 3,     // Consumible (Agujas, Aceite máquina) - Tiene Stock
        Service = 4         // Servicio (Luz, Mano de obra externa) - NO tiene Stock
    }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum CostType
    {
        Labor = 1,          // Esfuerzo Humano (Mano de obra)
        Machine = 2,        // Uso de Máquina (Desgaste/Amortización)
        Energy = 3,         // Gasto Energético (Luz, Gas)
        Overhead = 4        // Costos Indirectos (Alquiler del taller, etc.)
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum StockMovementType
    {
        // Entradas (+)
        InitialBalance = 1,     // Carga inicial de inventario
        Purchase = 2,           // Compra a proveedor
        ProductionOutput = 3,   // Producto terminado que sale de manufactura
        ReturnFromCustomer = 4, // Devolución de cliente
        TransferIn = 5,         // Entrada por traslado de otra bodega
        AdjustmentIn = 6,       // Ajuste positivo (ej: conteo físico sobrante)

        // Salidas (-)
        Sale = 10,              // Venta a cliente
        ProductionInput = 11,   // Materia prima consumida en manufactura
        ReturnToSupplier = 12,  // Devolución a proveedor
        TransferOut = 13,       // Salida por traslado a otra bodega
        AdjustmentOut = 14,     // Ajuste negativo (ej: conteo físico faltante, robo)
        Scrap = 15              // Merma / Desperdicio (dañado)
    }
}
