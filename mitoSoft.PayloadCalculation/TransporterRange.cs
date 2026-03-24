namespace mitoSoft.PayloadCalculation;

public class TransporterRange : List<CellRange>
{
    public void DeleteItem(long index)
    {
        RemoveAt((int)index);
    }

    /// <summary>
    /// Prüft, ob ein Bereich der selben Größe bereits vorhanden ist
    /// </summary>
    public new bool Contains(CellRange cellRange)
    {
        foreach (var range in this)
        {
            if (range.LowerLimit == cellRange.LowerLimit && range.UpperLimit == cellRange.UpperLimit)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Gibt die Summe der Zellen-Volumen mit
    /// </summary>
    public double Volume()
    {
        long v = 0;
        foreach (var range in this)
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
        foreach (var range in this)
        {
            if (range.UpperLimit <= 0)
            {
                c += 1;
            }
        }
        return c;
    }
}
