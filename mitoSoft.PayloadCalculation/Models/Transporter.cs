namespace mitoSoft.PayloadCalculation.Models;

public class Transporter
{
    public int MaxCells { get; set; } = 10;

    /// <summary>
    /// Collection of cells in the transporter
    /// </summary>
    public List<Cell> Cells { get; } = [];
}