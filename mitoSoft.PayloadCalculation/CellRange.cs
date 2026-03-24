namespace mitoSoft.PayloadCalculation;

public class CellRange
{
    /// <summary>
    /// Constructor
    /// </summary>
    public CellRange()
    {
        LowerLimit = 0;
        UpperLimit = 0;
        Volume = 0;
    }

    /// <summary>
    /// Untere Grenze des Bereichs
    /// </summary>
    public long LowerLimit { get; set; }

    /// <summary>
    /// Obere Grenze des Bereichs
    /// </summary>
    public long UpperLimit { get; set; }

    /// <summary>
    /// Volumen/Gesamtvolumen des Bereichs
    /// </summary>
    public long Volume { get; set; }

    /// <summary>
    /// Priorität des Bereichs
    /// </summary>
    public short Priority { get; set; }

    /// <summary>
    /// Gibt die Mitte des Bereichs zurück
    /// </summary>
    public double HalfLimit()
    {
        return ((UpperLimit - LowerLimit) / 2.0) + LowerLimit;
    }

    /// <summary>
    /// Gibt die Spanne des Bereichs zurück
    /// </summary>
    public long Range()
    {
        return UpperLimit - LowerLimit;
    }
}
