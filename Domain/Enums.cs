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
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum WorkOrderStatus
    {
        Pending,        // Recién creada
        Allocated,      // Inventario reservado (Nadie más lo puede tocar)
        InProgress,     // Producción en curso
        QualityControl, // Esperando revisión
        Completed,      // Finalizada, stock descontado
        Canceled
    }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum PickTaskStatus
    {
        Pending,    // Pendiente de recolección
        InProgress, // Operador en camino
        Completed,  // Material depositado en zona de producción/despacho
        Cancelled   // Tarea cancelada (pedido/orden cancelada)
    }

    // ─── Outbound / Órdenes de Venta ──────────────────────────────────────────

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum SalesOrderStatus
    {
        Draft,          // Borrador — creada sin confirmar
        Confirmed,      // Confirmada — stock reservado (AllocatedQuantity)
        Picking,        // Operador recogiendo materiales
        ReadyToShip,    // Todo pickeado — pendiente de despacho físico
        Shipped,        // Despachado — StockMovement(Sale) registrado
        Delivered,      // Confirmación de entrega recibida
        Cancelled       // Cancelada — AllocatedQuantity liberado
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum SalesOrderLineStatus
    {
        Pending,            // Sin picks confirmados
        PartiallyPicked,    // Algunos picks confirmados
        Fulfilled           // Cantidad completa recogida
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum SalesChannel
    {
        Manual,         // Creada manualmente en el WMS
        Internal,       // Transferencia interna entre almacenes
        WooCommerce,    // Tienda WooCommerce/WordPress
        Shopify,        // Tienda Shopify
        MercadoLibre,   // Marketplace MercadoLibre
        Amazon,         // Marketplace Amazon
        Other           // Otro canal externo
    }
}
