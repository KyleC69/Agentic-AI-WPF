// Build Date: 2026/03/15
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF.Core
// File:         SampleOrderDetail.cs
// Author: Kyle L. Crowder
// Build Num: 091029



namespace RAGDataIngestionWPF.Core.Models;





// Remove this class once your pages/features are using your data.
// This is used by the SampleDataService.
// It is the model class we use to display data on pages like Grid, Chart, and List Details.
public sealed class SampleOrderDetail
{

    public string CategoryDescription { get; set; }

    public string CategoryName { get; set; }

    public double Discount { get; set; }
    public long ProductId { get; set; }

    public string ProductName { get; set; }

    public int Quantity { get; set; }

    public string QuantityPerUnit { get; set; }





    public string ShortDescription
    {
        get { return $"Product ID: {ProductId} - {ProductName}"; }
    }





    public double Total { get; set; }

    public double UnitPrice { get; set; }
}