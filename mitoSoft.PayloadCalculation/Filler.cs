using mitoSoft.PayloadCalculation.Extensions;
using mitoSoft.PayloadCalculation.Models;

namespace mitoSoft.PayloadCalculation;

/// <summary>
/// Repräsentiert den 'Befüller' der Transportkomponente
/// </summary>
public class Filler
{
    //private long _iterations;
    //private const long MaxValue = 100000000;

    /// <summary>
    /// Konstruktor -> Werte mit Defaultwerten vorbesetzen
    /// </summary>
    public Filler() : this(new FillingRules())
    {
    }

    /// <summary>
    /// Konstruktor mit benutzerdefinierten Regeln
    /// </summary>
    public Filler(FillingRules rules)
    {
        MaxTolerance = 0.1;
        MinTolerance = 0.1;
        RoundDigits = 0;
        Rules = rules;
    }

    /// <summary>
    /// Gesetzliche Regeln für die Kammerfüllung
    /// </summary>
    public FillingRules Rules { get; }

    /// <summary>
    /// Maximale Toleranzgrenze ('Defaultwert' = 10%)
    /// </summary>
    public double MaxTolerance { get; set; }

    /// <summary>
    /// Minimale Toleranzgrenze ('Defaultwert' = 10%)
    /// </summary>
    public double MinTolerance { get; set; }

    /// <summary>
    /// Stellen auf die gerundet werden soll ('Defaultwert' = 0)
    /// </summary>
    public long RoundDigits { get; set; }

    public long MaxIterations { get; set; }

    /// <summary>
    /// Befüllung des Tankzugs
    /// </summary>
    public string Fill(long amount, Transporter transporter)
    {
        transporter.VerifyMaxCells();

        var solver = new Solver(this);
        var solution = new FillingResult(transporter, Rules);
        string status;

        var doc = new Xml.XmlDocument();
        status = $"<Filling Amount=\"{amount}\">";

        // Startet den Optimierungsalgoritmus
        status += solver.Solve(1, amount, transporter, solution);

        doc.LoadXml(status + "</Filling>");
        if (doc.SelectSingleNode("Filling/Check", "Status").ToUpper() != "BAD")
        {
            // Abschliessende Prüfung
            if (VerifyLoad(amount, transporter, result))
            {
                status += "<Verify Status=\"Good\"/>";
            }
            else
            {
                status += "<Verify Status=\"Bad\"/>";
            }
        }
        else
        {
            status += "<Verify Status=\"NoVerify\"/>";
        }

        // Zeilenumbrüche einfügen!
        status = status.Replace(">", ">" + Environment.NewLine);
        return status + "</Filling>";
    }

    /// <summary>
    /// Prüft nochmal gegen die Regeln
    /// </summary>
    /// <returns>TRUE -> wenn alle Regeln erfüllt;FALSE -> Wenn Regeln verletzt</returns>
    private bool VerifyLoad(long amount, Transporter transporter, FillingResult result)
    {
        var validator = new FillingValidator(Rules);
        long calculated = 0;

        // Jede Kammer nochmal (doppelt) gegen die Regeln prüfen
        foreach (var cell in transporter.Cells)
        {
            var capacity = result.GetCapacity(cell.Name);
            if (!validator.VerifyLoad(cell, capacity) || capacity < 0 || capacity > cell.Capacity)
            {
                return false;
            }
            //Gesamtmasse
            calculated += (long)capacity;
        }

        // Gesamtmasse nochmal prüfen
        if (calculated != amount)
        {
            return false;
        }

        return true;
    }
}