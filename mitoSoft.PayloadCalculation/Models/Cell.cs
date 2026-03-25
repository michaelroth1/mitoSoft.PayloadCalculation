namespace mitoSoft.PayloadCalculation.Models;

/// <summary>
/// Repräsentiert eine Kammer der Transportkomponente
/// </summary>
public class Cell
{
    /// <summary>
    /// Constructor
    /// </summary>
    public Cell(string name, long capacity, long baffles, int priority)
    {
        Name = name;
        Capacity = capacity;
        Baffles = baffles;
        Priority = priority;
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
    public int Priority { get; set; }
}
