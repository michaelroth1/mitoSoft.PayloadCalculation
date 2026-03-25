using mitoSoft.PayloadCalculation.Models;

namespace mitoSoft.PayloadCalculation;

/// <summary>
/// Verifikationsklasse für die Prüfung der Beladung von Kammern
/// </summary>
internal class FillingValidator(FillingRules rules)
{
    private readonly FillingRules _rules = rules;

    /// <summary>
    /// Verifikation einer gegebenen Beladung bezgl. der zulässigen Grenzen
    /// </summary>
    /// <param name="cell">Die zu prüfende Kammer</param>
    /// <param name="load">Die zu prüfende Beladung</param>
    /// <returns>TRUE -> Wenn Beladung innerhalb der zulässigen Grenzen liegt</returns>
    public bool VerifyLoad(Cell cell, double load)
    {
        if ((UpperMinLimit(cell) <= load && load <= UpperMaxLimit(cell)) ||
            (LowerMinLimit(cell) <= load && load <= LowerMaxLimit(cell)))
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
    public long LowerMinLimit(Cell cell)
    {
        return (long)(_rules.ChamberLowerMin * cell.Capacity);
    }

    /// <summary>
    /// Berechnet das untere Limit des oberen Bereiches und gibt dieses zurück
    /// </summary>
    public long LowerMaxLimit(Cell cell)
    {
        if (cell.Capacity > _rules.ChamberSplitLimit * (cell.Baffles + 1))
        {
            return (long)(_rules.ChamberLowerMax * cell.Capacity);
        }
        else
        {
            return (long)(_rules.ChamberUpperMax * cell.Capacity);
        }
    }

    /// <summary>
    /// Berechnet das obere Limit des unteren Bereiches und gibt dieses zurück
    /// </summary>
    public long UpperMinLimit(Cell cell)
    {
        if (cell.Capacity > _rules.ChamberSplitLimit * (cell.Baffles + 1))
        {
            return (long)(_rules.ChamberUpperMin * cell.Capacity);
        }
        else
        {
            return (long)(_rules.ChamberLowerMin * cell.Capacity);
        }
    }

    /// <summary>
    /// Berechnet das obere Limit des oberen Bereiches und gibt dieses zurück
    /// </summary>
    public long UpperMaxLimit(Cell cell)
    {
        return (long)(_rules.ChamberUpperMax * cell.Capacity);
    }
}