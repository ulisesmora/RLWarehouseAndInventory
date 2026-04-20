using System;
using System.Collections.Generic;

namespace Inventory.Application.Dashboard.Queries
{
    public class DashboardSummaryDto
    {
        // ── Inventario ────────────────────────────────────────────────────────
        public int     TotalMaterials    { get; set; }
        public int     LowStockCount     { get; set; }
        public decimal TotalStockValue   { get; set; }
        public decimal TotalStockUnits   { get; set; }

        // ── Lotes ─────────────────────────────────────────────────────────────
        public int ExpiringIn30Days { get; set; }
        public int ExpiringIn7Days  { get; set; }

        // ── Ventas (Outbound) ────────────────────────────────────────────────
        public int OrdersDraft       { get; set; }
        public int OrdersPicking     { get; set; }
        public int OrdersReadyToShip { get; set; }
        public int OrdersShippedToday { get; set; }
        public int OrdersTotalActive  { get; set; }

        // ── Producción ───────────────────────────────────────────────────────
        public int WorkOrdersPending        { get; set; }
        public int WorkOrdersInProgress     { get; set; }
        public int WorkOrdersQualityControl { get; set; }

        // ── Integraciones ────────────────────────────────────────────────────
        public int UnmappedProducts      { get; set; }
        public int ConnectedChannels     { get; set; }

        // ── Listas de alertas ────────────────────────────────────────────────
        public List<LowStockAlertDto>   LowStockAlerts  { get; set; } = new();
        public List<ExpiringLotAlertDto> ExpiringLots   { get; set; } = new();
        public List<RecentOrderDto>      RecentOrders   { get; set; } = new();

        // ── Gráficas ─────────────────────────────────────────────────────────
        public List<DailyOrderCountDto> OrderTrend   { get; set; } = new();  // últimos 7 días
        public List<StockByZoneDto>     StockByZone  { get; set; } = new();
    }

    public class LowStockAlertDto
    {
        public string  MaterialName  { get; set; } = string.Empty;
        public string  Sku           { get; set; } = string.Empty;
        public decimal OnHand        { get; set; }
        public decimal ReorderPoint  { get; set; }
    }

    public class ExpiringLotAlertDto
    {
        public string    LotNumber      { get; set; } = string.Empty;
        public string    MaterialName   { get; set; } = string.Empty;
        public DateTime  ExpirationDate { get; set; }
        public int       DaysRemaining  { get; set; }
    }

    public class RecentOrderDto
    {
        public string   OrderNumber   { get; set; } = string.Empty;
        public string   CustomerName  { get; set; } = string.Empty;
        public string   Status        { get; set; } = string.Empty;
        public string   Channel       { get; set; } = string.Empty;
        public DateTime CreatedAt     { get; set; }
    }

    public class DailyOrderCountDto
    {
        public string Label { get; set; } = string.Empty;  // "Lun", "Mar", etc.
        public int    Count { get; set; }
    }

    public class StockByZoneDto
    {
        public string  ZoneName { get; set; } = string.Empty;
        public decimal Units    { get; set; }
    }
}
