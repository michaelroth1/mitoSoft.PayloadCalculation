using mitoSoft.PayloadCalculation.Models;

namespace mitoSoft.PayloadCalculation;

public class Transporter : List<Cell>
{
    private const long MaxCells = 10;

    /// <summary>
    /// Neue Kammer anlegen
    /// </summary>
    /// <param name="priority">Je höher die Priorität, desto wichtiger ist es, dass diese Kammer abgefüllt wird!</param>
    public void NewCell(string name, long volume, long baffles = 0, short priority = 0)
    {
        if (Count <= MaxCells)
        {
            Add(new Cell(name, volume, baffles, priority));
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
