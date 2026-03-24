namespace mitoSoft.PayloadCalculation;

/// <summary>
/// Repräsentiert den 'Befüller' der Transportkomponente
/// </summary>
public class Filler
{
    private long _iterations;
    private const long MaxValue = 100000000;

    /// <summary>
    /// Konstruktor -> Werte mit Defaultwerten vorbesetzen
    /// </summary>
    public Filler() : this(new LoadingRules())
    {
    }

    /// <summary>
    /// Konstruktor mit benutzerdefinierten Regeln
    /// </summary>
    public Filler(LoadingRules rules)
    {
        MaxTolerance = 0.1;
        MinTolerance = 0.1;
        RoundDigits = 0;
        Rules = rules;
    }

    /// <summary>
    /// Gesetzliche Regeln für die Kammerfüllung
    /// </summary>
    public LoadingRules Rules { get; }

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
    public string Fill(long amount, ref Transporter transporter)
    {
        var solver = new Solver(this);
        var solution = new FillingSolution(transporter, Rules);
        string status;
        status = $"<Filling Status=\"Good\" Amount=\"{amount}\">";
        // Startet den Optimierungsalgoritmus
        var doc = new Xml.XmlDocument();
        status += solver.Solve(1, ref amount, ref transporter, solution);
        doc.LoadXml(status + "</Filling>");
        if (doc.SelectSingleNode("Filling/Check", "Status").ToUpper() != "BAD")
        {
            // Abschliessende Prüfung
            if (Verify(amount, transporter, solution))
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
    /// Hilfsfunction, um "none" als sehr große Zahl zu behandeln -> Minimiernungskriterium
    /// </summary>
    private long GetValue(string stringToConvert)
    {
        if (long.TryParse(stringToConvert, out long result))
        {
            return result;
        }
        else
        {
            return MaxValue;
        }
    }

    /// <summary>
    /// Prüft nochmal gegen die Regeln
    /// </summary>
    /// <returns>TRUE -> wenn alle Regeln erfüllt;FALSE -> Wenn Regeln verletzt</returns>
    private bool Verify(long amount, Transporter transporter, FillingSolution solution)
    {
        var validator = new LoadValidator(Rules);
        long temp = 0;
        // Jede Kammer gegen die Regeln prüfen
        foreach (var cell in transporter)
        {
            var capacity = solution.GetCapacity(cell.Name);
            if (!validator.VerifyLoad(cell, capacity) || capacity < 0 || capacity > cell.Capacity)
            {
                return false;
            }
            //Gesamtmasse
            temp += (long)capacity;
        }
        //Gesamtmasse prüfen
        if (temp != amount)
        {
            return false;
        }
        return true;
    }
}
