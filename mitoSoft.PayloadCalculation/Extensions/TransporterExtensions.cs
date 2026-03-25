using mitoSoft.PayloadCalculation.Exceptions;

namespace mitoSoft.PayloadCalculation.Extensions;

public static class TransporterExtensions
{
    public static string Fill(this Models.Transporter transporter, long amount)
    {
        return (new Filler()).Fill(amount, transporter);
    }

    public static long TotalVolume(this Models.Transporter transporter)
    {
        long totalVolume = 0;
        foreach (var cell in transporter.Cells)
        {
            totalVolume += cell.Capacity;
        }
        return totalVolume;
    }

    /// <summary>
    /// Verifies that the number of cells does not exceed the maximum allowed count
    /// </summary>
    /// <param name="transporter">The transporter to verify</param>
    /// <param name="maxCells">Maximum allowed number of cells (default: 10)</param>
    /// <exception cref="Exceptions.MaxCellException">Thrown when the number of cells exceeds the maximum</exception>
    public static void VerifyMaxCells(this Models.Transporter transporter, int maxCells = 10)
    {
        if (transporter.Cells.Count >= maxCells)
        {
            throw new MaxCellException($"Maximum number of cells ({maxCells}) would be exceeded. Current count: {transporter.Cells.Count}");
        }
    }
}