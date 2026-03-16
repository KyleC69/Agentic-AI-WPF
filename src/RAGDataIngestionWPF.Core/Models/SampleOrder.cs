// Build Date: 2026/03/16
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF.Core
// File:         SampleOrder.cs
// Author: Kyle L. Crowder
// Build Num: 051917



namespace RAGDataIngestionWPF.Core.Models;





// Remove this class once your pages/features are using your data.
// This is used by the SampleDataService.
// It is the model class we use to display data on pages like Grid, Chart, and List Details.
public sealed class SampleOrder
{

    public override string ToString()
    {
        return $"{Company} {Status}";
    }








    public string Company { get; set; }

    public ICollection<SampleOrderDetail> Details { get; set; }

    public double Freight { get; set; }

    public DateTime OrderDate { get; set; }
    public long OrderId { get; set; }

    public double OrderTotal { get; set; }

    public DateTime RequiredDate { get; set; }

    public DateTime ShippedDate { get; set; }

    public string ShipperName { get; set; }

    public string ShipperPhone { get; set; }

    public string ShipTo { get; set; }

    public string ShortDescription
    {
        get { return $"Order ID: {OrderId}"; }
    }

    public string Status { get; set; }

    public char Symbol
    {
        get { return (char)SymbolCode; }
    }

    public int SymbolCode { get; set; }
}