namespace mitoSoft.PayloadCalculation;

/// <summary>
/// Repräsentiert eine Kammer der Transportkomponente
/// </summary>
public class Cell
{
    private readonly Transporter _myBulk;

    /// <summary>
    /// Constructor
    /// </summary>
    public Cell(string name, long capacity, long baffles, short priority, ref Transporter bulk)
    {
        Name = name;
        Capacity = capacity;
        Baffles = baffles;
        Priority = priority;

        _myBulk = bulk;
    }

    public string Name { get; }

    /// <summary>
    /// Kammervolumen/Kammergröße
    /// </summary>
    public long Capacity { get; set; }

    /// <summary>
    /// Bearbeitet die Schwallwände
    /// </summary>
    public long Baffles { get; set; }

    /// <summary>
    /// Priorität der Kammerfüllung
    /// </summary>
    public short Priority { get; set; }

    /// <summary>
    /// Verifikation einer gegebenen Kapazität bezgl. der zulässigen Grenzen
    /// </summary>
    /// <returns>TRUE -> Wenn Kapazität innerhalb der zulässigen Grenzen liegt</returns>
    public bool VerifyCapacity(double capacity)
    {
        if ((UpperMinLimit() <= capacity && capacity <= UpperMaxLimit()) ||
            (LowerMinLimit() <= capacity && capacity <= LowerMaxLimit()))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Berechnet das untere Limit des unteren Bereiches (meist 0) und gibt diese zurück
    /// </summary>
    public long LowerMinLimit()
    {
        return (long)(_myBulk.LowerMin * Capacity);
    }

    /// <summary>
    /// Berechnet das untere Limit des oberen Bereiches und gibt dieses zurück
    /// </summary>
    public long LowerMaxLimit()
    {
        if (Capacity > _myBulk.SplitLimit * (Baffles + 1))
        {
            return (long)(_myBulk.LowerMax * Capacity);
        }
        else
        {
            return (long)(_myBulk.UpperMax * Capacity);
        }
    }

    /// <summary>
    /// Berechnet das obere Limit des unteren Bereiches und gibt dieses zurück
    /// </summary>
    public long UpperMinLimit()
    {
        if (Capacity > _myBulk.SplitLimit * (Baffles + 1))
        {
            return (long)(_myBulk.UpperMin * Capacity);
        }
        else
        {
            return (long)(_myBulk.LowerMin * Capacity);
        }
    }

    /// <summary>
    /// Berechnet das obere Limit des oberen Bereiches und gibt dieses zurück
    /// </summary>
    public long UpperMaxLimit()
    {
        return (long)(_myBulk.UpperMax * Capacity);
    }
}
