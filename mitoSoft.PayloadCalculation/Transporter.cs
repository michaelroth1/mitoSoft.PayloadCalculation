namespace mitoSoft.PayloadCalculation;

public class Transporter : List<Cell>
{
    private const long MaxCells = 10;

    /// <summary>
    /// Konstruktor -> besetzt die Eigenschaften mit Defaultwerten vor
    /// </summary>
    public Transporter()
    {
        LowerMin = 0.0;
        LowerMax = 0.25;
        UpperMin = 0.8;
        UpperMax = 0.9;
        SplitLimit = 7500;
    }

    public double LowerMin { get; set; }
    public double LowerMax { get; set; }
    public double UpperMin { get; set; }
    public double UpperMax { get; set; }

    /// <summary>
    /// Kammergröße, ab der die Berechnung keine 2 Bereiche mehr kennt ('Defaultwert' = 7500)
    /// </summary>
    public long SplitLimit { get; set; }

    /// <summary>
    /// Neue Kammer anlegen
    /// </summary>
    /// <param name="priority">Je höher die Priorität, desto wichtiger ist es, dass diese Kammer abgefüllt wird!</param>
    public void NewCell(string name, long volume, long baffles = 0, short priority = 0)
    {
        if (Count <= MaxCells)
        {
            var temp = this;
            Add(new Cell(name, volume, baffles, priority, ref temp));
        }
        else
        {
            throw new MaxCellException(); // Zu viele Kammern angelegt
        }
    }

    /// <summary>
    /// Alle Kammern entfernen
    /// </summary>
    public void DeleteCells()
    {
        RemoveRange(0, Count);
    }

    /// <summary>
    /// Stellt eine äquivalente Function zu 'Item' dar
    /// Soll die Schnittstelle verständlicher halten
    /// </summary>
    public Cell Cell(short index)
    {
        return this[index];
    }

    /// <summary>
    /// Kammer der Transportkomponente wird anhand des Namens 
    /// zurück gegeben
    /// </summary>
    public Cell? CellByName(string name)
    {
        foreach (var cell in this)
        {
            if (cell.Name == name)
                return cell;
        }
        return null;
    }
}
