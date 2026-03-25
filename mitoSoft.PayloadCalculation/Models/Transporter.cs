namespace mitoSoft.PayloadCalculation.Models;

public class Transporter
{
    private const long MaxCells = 10;

    /// <summary>
    /// Collection of cells in the transporter
    /// </summary>
    public List<Cell> Cells { get; } = [];
}