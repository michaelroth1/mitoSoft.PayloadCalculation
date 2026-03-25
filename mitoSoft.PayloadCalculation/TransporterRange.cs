namespace mitoSoft.PayloadCalculation;

internal class TransporterRange
{
    /// <summary>
    /// Collection of cell ranges in the transporter range
    /// </summary>
    public List<CellRange> CellRanges { get; } = [];

    /// <summary>
    /// Indexer for accessing cell ranges by index
    /// </summary>
    public CellRange this[int index]
    {
        get => CellRanges[index];
        set => CellRanges[index] = value;
    }

    public void DeleteItem(long index)
    {
        CellRanges.RemoveAt((int)index);
    }

    /// <summary>
    /// Prüft, ob ein Bereich der selben Größe bereits vorhanden ist
    /// </summary>
    public bool Contains(CellRange cellRange)
    {
        foreach (var range in CellRanges)
        {
            if (range.LowerLimit == cellRange.LowerLimit && range.UpperLimit == cellRange.UpperLimit)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Adds a cell range to the collection
    /// </summary>
    public void Add(CellRange cellRange)
    {
        CellRanges.Add(cellRange);
    }

    /// <summary>
    /// Gibt die Summe der Zellen-Volumen mit
    /// </summary>
    public double Volume()
    {
        long v = 0;
        foreach (var range in CellRanges)
        {
            v += range.Volume;
        }
        return v;
    }

    /// <summary>
    /// Zählt alle leeren Bereiche!
    /// </summary>
    /// <returns>Anzahl der leeren Bereiche</returns>
    public long CountEmptyCells()
    {
        long c = 0;
        foreach (var range in CellRanges)
        {
            if (range.UpperLimit <= 0)
            {
                c += 1;
            }
        }
        return c;
    }
}