using mitoSoft.PayloadCalculation.Models;

namespace mitoSoft.PayloadCalculation;

/// <summary>
/// Repräsentiert das Ergebnis einer Befüllung
/// </summary>
public class FillingResult
{
    private readonly Dictionary<string, double> _chamberCapacities;
    private readonly Transporter _transporter;
    private readonly FillingValidator _validator;

    public FillingResult(Transporter transporter, FillingRules rules)
    {
        _transporter = transporter;
        _validator = new FillingValidator(rules);
        _chamberCapacities = new Dictionary<string, double>();
        foreach (var cell in transporter.Cells)
        {
            _chamberCapacities[cell.Name] = 0;
        }
    }

    /// <summary>
    /// Setzt die Kapazität für eine bestimmte Kammer
    /// </summary>
    public void SetCapacity(string chamberName, double capacity)
    {
        _chamberCapacities[chamberName] = capacity;
    }

    /// <summary>
    /// Setzt die Kapazität für eine Kammer anhand des Index
    /// </summary>
    public void SetCapacity(int index, double capacity)
    {
        var cell = _transporter.Cells[index];
        _chamberCapacities[cell.Name] = capacity;
    }

    /// <summary>
    /// Gibt die Kapazität für eine bestimmte Kammer zurück
    /// </summary>
    public double GetCapacity(string chamberName)
    {
        return _chamberCapacities.TryGetValue(chamberName, out var capacity) ? capacity : 0;
    }

    /// <summary>
    /// Gibt die Kapazität für eine Kammer anhand des Index zurück
    /// </summary>
    public double GetCapacity(int index)
    {
        var cell = _transporter.Cells[index];
        return GetCapacity(cell.Name);
    }

    /// <summary>
    /// Gibt alle Kammer-Kapazitäten zurück
    /// </summary>
    public IReadOnlyDictionary<string, double> GetAllCapacities()
    {
        return _chamberCapacities;
    }

    /// <summary>
    /// Verifikation des Kammerinhalts bezgl. der zulässigen Grenzen für eine bestimmte Kammer
    /// </summary>
    public bool VerifyCell(string chamberName)
    {
        var cell = _transporter
            .Cells
            .Single(c => c.Name == chamberName);

        if (cell == null) return false;

        var capacity = GetCapacity(chamberName);
        return _validator.VerifyLoad(cell, capacity);
    }

    /// <summary>
    /// Verifikation aller Kammerinhalte
    /// </summary>
    public bool VerifyAll()
    {
        foreach (var cell in _transporter.Cells)
        {
            var capacity = GetCapacity(cell.Name);
            if (!_validator.VerifyLoad(cell, capacity))
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Gibt die Gesamtmenge zurück
    /// </summary>
    public long GetTotalAmount()
    {
        return (long)_chamberCapacities.Values.Sum();
    }
}
