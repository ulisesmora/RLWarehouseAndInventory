using System;
using System.Collections.Generic;

namespace Inventory.Application.Integrations.Queries
{
    public class ChannelStatusDto
    {
        public string   Channel        { get; set; } = string.Empty;
        public bool     IsConnected    { get; set; }
        public string   StoreUrl       { get; set; } = string.Empty;
        public DateTime? LastSyncAt    { get; set; }
        public int      TotalImported  { get; set; }
        public string?  LastError      { get; set; }
    }

    public class UnmappedProductDto
    {
        public Guid    Id                  { get; set; }
        public string  Channel             { get; set; } = string.Empty;
        public string  ExternalSku         { get; set; } = string.Empty;
        public string  ExternalProductName { get; set; } = string.Empty;
        public string  ExternalProductId   { get; set; } = string.Empty;
        public string  MatchMethod         { get; set; } = string.Empty;
    }

    public class ImportResultDto
    {
        public int     Imported   { get; set; }
        public int     Skipped    { get; set; }   // duplicados
        public int     Unmapped   { get; set; }   // sin mapeo de material
        public string  Message    { get; set; } = string.Empty;
        public List<string> Errors { get; set; } = new();
    }
}
