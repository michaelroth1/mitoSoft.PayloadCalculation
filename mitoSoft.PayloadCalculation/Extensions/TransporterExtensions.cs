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
    /// <exception cref="Exceptions.MaxCellException">Thrown when the number of cells exceeds the maximum</exception>
    public static void VerifyMaxCells(this Models.Transporter transporter)
    {
        if (transporter.Cells.Count >= transporter.MaxCells)
        {
            throw new MaxCellException($"Maximum number of cells ({transporter.MaxCells}) would be exceeded. Current count: {transporter.Cells.Count}");
        }
    }
}