using mitoSoft.PayloadCalculation.Models;

namespace mitoSoft.PayloadCalculation;

public class Solver
{
    private readonly Filler _myFiller;
    private long _cellCount;
    
    private enum RoundMode
    {
        Up = 1,
        Down = 2
    }

    public Solver(Filler filler)
    {
        _cellCount = 0;
        _myFiller = filler;
    }

    /// <summary>
    /// Ablauf um den Tankzug optimal zu befüllen
    /// </summary>
    public string Solve(long minRange, ref long amount, ref Transporter transporter, FillingSolution solution)
    {
        var validator = new LoadValidator(_myFiller.Rules);
        RangeCollection rangeCollection;
        TransporterRange? transporterRange = null;
        string ret = "";
        long iteration;
        long maxIterations;

        // Defaultwerte -> Wenn keine werte angegeben wurden
        maxIterations = Math.Max(20, _myFiller.MaxIterations);
        minRange = Math.Max(1, minRange);

        // Initialisieren
        _cellCount = transporter.Count;
        rangeCollection = InitRangeCollection(transporter, validator);

        // Prüfen, ob eine Aufteilung überhaupt möglich ist
        var doc = new Xml.XmlDocument();
        ret = CheckSolve(ref amount, rangeCollection);    // Prüft, ob die Menge überhaupt aufteilbar ist
        doc.LoadXml(ret);

        // Iterativer Algorithmus
        if (rangeCollection.Count > 0 && doc.SelectSingleNode("/Check", "Status").ToUpper() != "BAD")
        {
            for (iteration = 1; iteration <= maxIterations; iteration++)
            {
                //Gewünschte Menge passt nicht in die Behälter -> Bereich löschen 
                Constrains(amount, ref rangeCollection);

                //Optimierungskriterien: Min Kammer-Anzahl & Nutze größte Kammer & Nutze Kleinsten Breich
                //PRIORITÄTEN : VON OBEN NACH UNTEN!!!

                OptimizeCellPriority(ref rangeCollection);
                //OptimizeFirstCellFull(ref rangeCollection); //-> hab ich raus, da das jetzt mit den Prioritäten realisierbar ist!!!
                OptimizeMinimumCells(amount, ref rangeCollection);
                OptimizeMinimumVolume(ref rangeCollection);
                OptimizeMaxCellsFull(transporter, ref rangeCollection, validator);

                //TRICK 17: Alle Werte sind gleich gewichtet -> deshalb nur den ersten weiter nuztzen
                transporterRange = rangeCollection[0];

                // Abbruchkriterium
                if (FinishIteration(minRange, rangeCollection))
                {
                    break;
                }

                // Bereiche nach Newton halbieren
                rangeCollection = MakeHalf(transporterRange);
            }
            // Auf Kammern verteilen
            if (transporterRange != null && Partition(amount, ref transporter, transporterRange, solution, validator) < 0)
            {
                ret += "<Distribution Status=\"Bad\"/>";
            }
            else
            {
                //Status speichern
                ret += $"<Distribution Status=\"Good\" Amount=\"{amount}\">";
                foreach (var cell in transporter)
                {
                    ret += $"<Chamber Name=\"{cell.Name}\" Volume=\"{cell.Capacity}\" Baffles=\"{cell.Baffles}\" Content=\"{solution.GetCapacity(cell.Name)}\"/>";
                }
                ret += "</Distribution>";
            }
        }
        // Rückgabe
        return ret;
    }

    /// <summary>
    /// Es kann vorkommen, dass die Gesamtmenge um max 1 Liter abweicht,
    /// dann muss nochmal optimiert werden
    /// </summary>
    /// <returns>0 -> Keine Aufteilung nötig
    ///          1 -> Aufteilung erfolgt
    ///         -1 -> Kann nicht aufgeteilt werden</returns>
    private long Partition(long amount, ref Transporter transporter, TransporterRange transporterRange, FillingSolution solution, LoadValidator validator)
    {
        // Rückgabe und Mengen in der Transportkomponente speichern
        for (short i = 0; i < transporterRange.Count; i++)
        {
            if (transporterRange[i].UpperLimit == validator.LowerMaxLimit(transporter.Cell(i)) ||
                transporterRange[i].UpperLimit == validator.UpperMaxLimit(transporter.Cell(i)))
            {
                solution.SetCapacity(i, transporterRange[i].UpperLimit);
            }
            else if (transporterRange[i].LowerLimit == validator.LowerMinLimit(transporter.Cell(i)) ||
                     transporterRange[i].LowerLimit == validator.UpperMinLimit(transporter.Cell(i)))
            {
                solution.SetCapacity(i, transporterRange[i].LowerLimit);
            }
            else
            {
                solution.SetCapacity(i, Math.Round(transporterRange[i].HalfLimit() + 0.01, 0));       //Rundet bei 0.5 auch noch ab!!!
            }
        }
        // Genaue Menge wird ermittelt
        double amountTemp = 0.0;
        foreach (var cell in transporter)
        {
            amountTemp += solution.GetCapacity(cell.Name);
        }
        // Eine Abweichung ist aufgetreten
        double rest;
        short ret = 0;
        if (amountTemp != amount)
        {
            rest = amount - amountTemp;
            //Prüfen ob eine Kammer in die Mitte befüllt ist
            for (short i = 0; i < transporter.Count; i++)
            {
                var cell = transporter.Cell(i);
                var capacity = solution.GetCapacity(cell.Name);
                if (validator.VerifyLoad(cell, capacity) && capacity > 0)
                {
                    solution.SetCapacity(i, capacity + rest);
                    ret = 1;
                    rest = 0;
                    break;
                }
            }

            //Kein Abbruch -> Erste Kammer korrigieren
            if (ret < 1)
            {
                foreach (var cell in transporter)
                {
                    var capacity = solution.GetCapacity(cell.Name);
                    if (rest > 0)
                    {
                        // Menge muss befüllt werden
                        if (capacity + rest < validator.LowerMaxLimit(cell) ||
                            capacity + rest < validator.UpperMaxLimit(cell))
                        {
                            solution.SetCapacity(cell.Name, capacity + rest);
                            ret = 1;
                        }
                    }
                    else if (rest < 0)
                    {
                        // Menge muss entleert werden
                        if (validator.LowerMinLimit(cell) < capacity + rest ||
                            validator.UpperMinLimit(cell) < capacity + rest)
                        {
                            solution.SetCapacity(cell.Name, capacity + rest);
                            ret = 1;
                        }
                    }
                }
            }
        }

        return ret;
    }

    /// <summary>
    /// Abbruchkriterium: lRange kleiner als MinRange
    /// </summary>
    private bool FinishIteration(long minRange, RangeCollection rangeCollection)
    {
        bool b = true;
        if (rangeCollection != null)
        {
            foreach (var transporterRange in rangeCollection)
            {
                foreach (var cellRange in transporterRange)
                {
                    if (minRange + 0.5 <= cellRange.Range())
                    {
                        b = false;
                    }
                }
            }
        }
        return b;
    }

    /// <summary>
    /// Alle Anfangsbedingungen werden festgelegt
    /// </summary>
    private RangeCollection InitRangeCollection(Transporter transporter, LoadValidator validator)
    {
        int i, j;
        CellRange cellRange;
        TransporterRange transporterRange;
        RangeCollection rangeCollection;
        long[,] arrDreiersystem;

        rangeCollection = new RangeCollection();
        arrDreiersystem = GetNumbersystem(_cellCount, 3);

        for (i = arrDreiersystem.GetLowerBound(0); i <= arrDreiersystem.GetUpperBound(0); i++)
        {
            transporterRange = new TransporterRange();

            for (j = arrDreiersystem.GetLowerBound(1); j <= arrDreiersystem.GetUpperBound(1); j++)
            {
                cellRange = new CellRange();
                cellRange.Priority = transporter.Cell((short)j).Priority;

                if (arrDreiersystem[i, j] == 0)
                {
                    cellRange.LowerLimit = 0;
                    cellRange.UpperLimit = 0;
                    cellRange.Volume = 0;
                }
                else if (arrDreiersystem[i, j] == 1)
                {
                    cellRange.LowerLimit = validator.LowerMinLimit(transporter.Cell((short)j));       //Beachtet die Regeln
                    cellRange.UpperLimit = validator.LowerMaxLimit(transporter.Cell((short)j));
                    cellRange.Volume = transporter.Cell((short)j).Capacity;
                }
                else if (arrDreiersystem[i, j] == 2)
                {
                    cellRange.LowerLimit = validator.UpperMinLimit(transporter.Cell((short)j));       //Beachtet die Regeln
                    cellRange.UpperLimit = validator.UpperMaxLimit(transporter.Cell((short)j));
                    cellRange.Volume = transporter.Cell((short)j).Capacity;
                }

                transporterRange.Add(cellRange);
            }

            if (!rangeCollection.Contains(transporterRange))
            {
                rangeCollection.Add(transporterRange);
            }
        }

        return rangeCollection;
    }

    /// <summary>
    /// Der übergebene Bereiche wird nach dem Newton-Kriterium halbiert
    /// </summary>
    private RangeCollection MakeHalf(TransporterRange transporterRange)
    {
        long[,] arrDualsystem;
        arrDualsystem = GetNumbersystem(_cellCount, 2);
        var newRangeCollection = new RangeCollection();

        if (transporterRange != null)
        {
            //Alle Kombinationen durchsuchen
            for (int i = arrDualsystem.GetLowerBound(0); i <= arrDualsystem.GetUpperBound(0); i++)
            {
                var newTransporterRange = new TransporterRange();

                for (int j = arrDualsystem.GetLowerBound(1); j <= arrDualsystem.GetUpperBound(1); j++)
                {
                    var newCellRange = new CellRange();
                    newCellRange.Priority = transporterRange[j].Priority;

                    if (arrDualsystem[i, j] == 0)
                    {
                        //Untere Teilbereich der Kammer
                        newCellRange.Volume = transporterRange[j].Volume;
                        newCellRange.LowerLimit = Round(transporterRange[j].LowerLimit, _myFiller.RoundDigits, RoundMode.Up);
                        newCellRange.UpperLimit = Round(transporterRange[j].HalfLimit(), _myFiller.RoundDigits, RoundMode.Down);
                    }
                    else if (arrDualsystem[i, j] == 1)
                    {
                        //Oberer Teilbereich der Kammer
                        newCellRange.Volume = transporterRange[j].Volume;
                        newCellRange.LowerLimit = Round(transporterRange[j].HalfLimit(), _myFiller.RoundDigits, RoundMode.Up);
                        newCellRange.UpperLimit = Round(transporterRange[j].UpperLimit, _myFiller.RoundDigits, RoundMode.Down);
                    }

                    newTransporterRange.Add(newCellRange);
                }

                //Bereich speichern -> wenn noch nicht vorhanden
                if (!newRangeCollection.Contains(newTransporterRange))
                {
                    newRangeCollection.Add(newTransporterRange);
                }
            }
        }

        return newRangeCollection;
    }

    /// <summary>
    /// Nebenbedingungen werden abgearbeitet
    /// Jede Kammer muss innerhalb eines bestimmten Bereiches liegen
    /// </summary>
    private long Constrains(long amount, ref RangeCollection rangeCollection)
    {
        // Initialisieren
        RangeCollection? newRangeCollection = null;
        // Der Bereich wird auf mögliche Befüllung geprüft!!!
        foreach (var transporterRange in rangeCollection)
        {
            long max = 0, min = 0;
            foreach (var cellRange in transporterRange)
            {
                // Es ist möglich auf mehrere Werte innerhalb der Grenzen zu runden
                min += Round(cellRange.LowerLimit, _myFiller.RoundDigits, RoundMode.Up);
                max += Round(cellRange.UpperLimit, _myFiller.RoundDigits, RoundMode.Down);
            }
            if (min <= amount && amount <= max)
            {
                if (newRangeCollection == null)
                {
                    newRangeCollection = new RangeCollection();
                }
                newRangeCollection.Add(transporterRange);
            }
        }
        // Rückgabewerte-> immer zurückgeben, da Nothing auch zurück gegeben werden muss!!!
        rangeCollection = newRangeCollection!;
        newRangeCollection = null;
        return 0;
    }

    /// <summary>
    /// Überprüft, ob eine Aufteilung mit den Parametern überhaupt möglich ist
    /// </summary>
    private string CheckSolve(ref long amount, RangeCollection rangeCollection)
    {
        long max, min;
        long addCharge, minAddCharge;
        long discharge, minDischarge;
        long maxAmount, minAmount = 0;
        long temp1 = -1, temp2 = -1;
        string ret;
        bool goodStatus = false;

        maxAmount = int.MaxValue;
        addCharge = int.MaxValue;
        discharge = int.MaxValue;
        minDischarge = int.MaxValue;
        minAddCharge = int.MaxValue;

        // Grenzen
        minAmount = (long)(amount * (1 - _myFiller.MinTolerance));
        maxAmount = (long)(amount * (1 + _myFiller.MaxTolerance));

        // Menge zu klein
        if (amount <= 0)
        {
            ret = "<Check Status=\"Bad\" Text=\"No amount specified\"/>";
        }
        else
        {
            // Runden
            ret = RoundAmount(ref amount, _myFiller.RoundDigits, minAmount, maxAmount);
            var doc = new Xml.XmlDocument();
            doc.LoadXml(ret);
            if (doc.SelectSingleNode("/Round", "Status").ToUpper() == "BAD")
            {
                ret = $"<Check Status=\"Bad\" Text=\"{doc.SelectSingleNode("/Round", "Text")}\"/>";
            }
            else
            {
                // Prüfen
                if (rangeCollection != null)
                {
                    foreach (var transporterRange in rangeCollection)
                    {
                        max = 0;
                        min = 0;
                        foreach (var cellRange in transporterRange)
                        {
                            min += Round(cellRange.LowerLimit, _myFiller.RoundDigits, RoundMode.Up);
                            max += Round(cellRange.UpperLimit, _myFiller.RoundDigits, RoundMode.Down);
                        }

                        if (Round(min, _myFiller.RoundDigits, RoundMode.Up) <= amount && amount <= Round(max, _myFiller.RoundDigits, RoundMode.Down))
                        {
                            ret = "<Check Status=\"Good\" Text=\"Distribution possible\" Add=\"0\" Discharge =\"0\"/>";
                            temp1 = -2;
                            temp2 = -2;
                            goodStatus = true;
                            break;
                        }
                        else
                        {
                            if (amount <= min)
                            {
                                addCharge = Math.Abs(Math.Abs(min) - Math.Abs(amount));
                                minAddCharge = Round(Math.Min(addCharge, minAddCharge), _myFiller.RoundDigits, RoundMode.Up);
                                temp1 = maxAmount - (amount + minAddCharge);
                            }
                            else if (max <= amount)
                            {
                                discharge = Math.Abs(Math.Abs(amount) - Math.Abs(max));
                                minDischarge = Round(Math.Min(discharge, minDischarge), _myFiller.RoundDigits, RoundMode.Down);
                                temp2 = (amount - minDischarge) - minAmount;
                            }
                        }
                    }

                    // XML Text zusammenstellen
                    if (!goodStatus)
                    {
                        if ((temp1 >= 0 && temp2 >= 0 && temp1 < temp2) || (temp1 >= 0 && temp2 < 0))
                        {
                            // Befüllen
                            amount = amount + minAddCharge;
                            ret = $"<Check Status=\"Add\" Amount=\"{minAddCharge}\"/>";
                        }
                        else if (temp1 >= 0 && temp2 >= 0 && temp1 >= temp2 || (temp1 < 0 && temp2 >= 0))
                        {
                            // Entleeren
                            amount = amount - minDischarge;
                            ret = $"<Check Status=\"Discharge\" Amount=\"{minDischarge}\"/>";
                        }
                        else if (temp1 < 0 && temp2 < 0)
                        {
                            // Nichts von beidem
                            ret = "<Check Status=\"Bad\" Text=\"Distribution not possible\"";
                            if (minDischarge < int.MaxValue)
                            {
                                ret += $" Discharge=\"{minDischarge}\"";
                            }
                            if (minAddCharge < int.MaxValue)
                            {
                                ret += $" Add=\"{minAddCharge}\"";
                            }
                            ret += "/>";
                        }
                    }
                }
            }
        }
        // Rückgabewert
        return ret;
    }

    /// <summary>
    /// Zielfunktion wird bezüglich der Kammernanzahl = MIN optimiert werden
    /// </summary>
    private long OptimizeMinimumCells(long amount, ref RangeCollection rangeCollection)
    {
        //Initialisieren
        var newRangeCollection = new RangeCollection();
        long lEmptyCells = 0;
        // Minimum suchen -> minimal benötigte Kammer-Anzahl
        foreach (var transporterRange in rangeCollection)
        {
            lEmptyCells = Math.Max(transporterRange.CountEmptyCells(), lEmptyCells);
        }
        // Jetzt alle Bereiche speichern die minimale Kammernanzahl verwenden
        foreach (var transporterRange in rangeCollection)
        {
            if (transporterRange.CountEmptyCells() == lEmptyCells)
            {
                newRangeCollection.Add(transporterRange);
            }
        }
        rangeCollection = newRangeCollection;
        newRangeCollection = null;
        return 0;
    }

    /// <summary>
    /// Zielfunktion wird bezüglich der Prioritäten optimiert
    /// dabei gilt: Je höher die Summe der ausgewählten Prioritäten, desto wichtiger ist die jeweilige Kombination
    /// </summary>
    private long OptimizeCellPriority(ref RangeCollection rangeCollection)
    {
        //Initialisieren
        var newRangeCollection = new RangeCollection();
        long lPrio = 0;
        // Prioritäten aller Bereiche auslesen
        foreach (var transporterRange in rangeCollection)
        {
            long lPrioTemp = 0;
            foreach (var cellRange in transporterRange)
            {
                if (cellRange.UpperLimit > 0 && cellRange.Priority > 0)
                {
                    lPrioTemp += cellRange.UpperLimit * cellRange.Priority;
                }
            }
            if (lPrioTemp > lPrio)
            {
                lPrio = lPrioTemp;
                newRangeCollection = new RangeCollection();
                newRangeCollection.Add(transporterRange);
            }
            else if (lPrioTemp == lPrio)
            {
                lPrio = lPrioTemp;
                newRangeCollection.Add(transporterRange);
            }
        }
        rangeCollection = newRangeCollection;
        newRangeCollection = null;
        return 0;
    }

    /// <summary>
    /// Zielfunktion wird bezüglich dem Maximum in die erste Kammer optimiert
    /// </summary>
    private long OptimizeFirstCellFull(ref RangeCollection rangeCollection)
    {
        //Initialisieren
        var newRangeCollection = new RangeCollection();
        long lMax = 0;
        foreach (var transporterRange in rangeCollection)
        {
            lMax = Math.Max(lMax, transporterRange[1].UpperLimit);
        }
        // Jetzt alle Bereiche speichern die die erste Kammer maximal voll haben 
        foreach (var transporterRange in rangeCollection)
        {
            if (transporterRange[1].UpperLimit == lMax)
            {
                newRangeCollection.Add(transporterRange);
            }
        }
        rangeCollection = newRangeCollection;
        newRangeCollection = null;
        return 0;
    }

    /// <summary>
    /// Zielfunktion wird bezüglich der größten Kammer optimiert
    /// </summary>
    private long OptimizeMaxCellsFull(Transporter transporter, ref RangeCollection rangeCollection, LoadValidator validator)
    {
        // Initialisieren
        var newRangeCollection = new RangeCollection();
        long maxCellsFull = 0;
        long cellVolume = 0;
        // Maximum suchen -> So viele Kammern als möglich so voll wie möglich machen
        foreach (var transporterRange in rangeCollection)
        {
            long maxCellsFullTemp = 0;
            long cellVolumeTemp = 0;
            for (short j = 0; j < transporterRange.Count; j++)
            {
                // Gesamtvolumen wird ausgelesen
                if (transporterRange[j].UpperLimit == validator.LowerMaxLimit(transporter.Cell(j)) ||
                    transporterRange[j].UpperLimit == validator.UpperMaxLimit(transporter.Cell(j)))
                {
                    cellVolumeTemp += transporterRange[j].UpperLimit;
                    maxCellsFullTemp += 1;
                }
            }
            maxCellsFull = Math.Max(maxCellsFullTemp, maxCellsFull);
            if (maxCellsFullTemp >= maxCellsFull)
            {
                cellVolume = Math.Max(cellVolumeTemp, cellVolume);
            }
        }

        // Jetzt alle Bereiche speichern die minimale Kammernanzahl verwenden
        foreach (var transporterRange in rangeCollection)
        {
            long maxCellsFullTemp = 0;
            long cellVolumeTemp = 0;
            for (short j = 0; j < transporterRange.Count; j++)
            {
                if (transporterRange[j].UpperLimit == validator.LowerMaxLimit(transporter.Cell(j)) ||
                    transporterRange[j].UpperLimit == validator.UpperMaxLimit(transporter.Cell(j)))
                {
                    cellVolumeTemp += transporterRange[j].UpperLimit;
                    maxCellsFullTemp += 1;
                }
            }
            if (maxCellsFull == maxCellsFullTemp && cellVolume == cellVolumeTemp)
            {
                newRangeCollection.Add(transporterRange);
            }
        }
        rangeCollection = newRangeCollection;
        newRangeCollection = null;
        return 0;
    }

    /// <summary>
    /// Zielfunktion wird bezüglich des kleinsten nutzbaren Bereichs optimiert
    /// </summary>
    private long OptimizeMinimumVolume(ref RangeCollection rangeCollection)
    {
        // Initialisieren
        var newRangeCollection = new RangeCollection();
        long minRange = int.MaxValue;
        // Minimum suchen -> Minimales KammerVolumen suchen
        foreach (var transporterRange in rangeCollection)
        {
            if (Math.Min(transporterRange.Volume(), minRange) > 0)
            {
                minRange = (long)Math.Min(transporterRange.Volume(), minRange);
            }
        }
        // Jetzt alle Bereiche speichern die minimale Kammernanzahl verwenden
        foreach (var transporterRange in rangeCollection)
        {
            if (transporterRange.Volume() == minRange)
            {
                newRangeCollection.Add(transporterRange);
            }
        }
        rangeCollection = newRangeCollection;
        newRangeCollection = null;
        return 0;
    }

    /// <summary>
    /// Berechnet ein Zahlensystem und gibt dessen Werte zurück
    /// </summary>
    /// <param name="digits">Stellenanzahl</param>
    /// <param name="number">Ordung des Zahlensystems</param>
    /// <returns>2 dim Array mit allen Kombinationen des entsprechenden Zahlensystems</returns>
    private long[,] GetNumbersystem(long digits, long number)
    {
        long[,] arrTemp;
        long i, l, j;
        l = 0;
        arrTemp = new long[(int)Math.Pow(number, digits), digits];
        for (j = 0; j < digits; j++)
        {
            l = 0;
            for (i = 0; i < Math.Pow(number, digits); i++)
            {
                if (l >= number)
                {
                    l = 0;
                }
                arrTemp[i, j] = l;
                if ((i + 1) % Math.Pow(number, j) == 0)
                {
                    l = l + 1;
                }
            }
        }
        return arrTemp;
    }

    /// <summary>
    /// Rundet den übergebenen Wert entweder ab, oder auf!!!
    /// Werte werden niemals negativ
    /// </summary>
    /// <param name="d">Zahl die gerundet werden soll</param>
    /// <param name="digits">Anzahl der ganzzahligen Stellen auf die gerunden werden soll</param>
    /// <param name="mode">Aufrunden oder Abrunden</param>
    /// <returns>Gerundeter Wert</returns>
    private long Round(double d, long digits, RoundMode mode)
    {
        if (mode == RoundMode.Up)
        {
            return Math.Max(0, (long)Math.Ceiling(d / Math.Pow(10, digits)) * (long)Math.Pow(10, digits));
        }
        else if (mode == RoundMode.Down)
        {
            return Math.Max(0, (long)Math.Floor(d / Math.Pow(10, digits)) * (long)Math.Pow(10, digits));
        }
        return 0;
    }

    /// <summary>
    /// Rundet die zu verladende Menge, falls es nötig ist, auf einen
    /// zulässigen Wert innerhalb der Toleranzen
    /// Zulässige Menge wird niemals negativ!!!
    /// </summary>
    /// <param name="amount"></param>
    /// <param name="digits">Anzahl der ganzzahligen Stellen auf die gerunden werden soll</param>
    /// <param name="min">Minimal zulässiger wert</param>
    /// <param name="max">Maximal zulässiger Wert</param>
    /// <returns>XML String mit dem Status</returns>
    private string RoundAmount(ref long amount, long digits, long min, long max)
    {
        long lRoundUp, lRoundDown, lMinTemp = -1, lMaxTemp = -1;
        string ret = "";

        if (amount < min || amount > max)
        {
            ret = $"<Round Status=\"Bad\" Amount=\"{amount}\" Text=\"Amount is not within loading tolerance\"/>";
        }
        else
        {
            lRoundDown = Math.Max(0, Round(amount, digits, RoundMode.Down));
            lRoundUp = Math.Max(0, Round(amount, digits, RoundMode.Up));
            lMinTemp = lRoundDown - min;
            lMaxTemp = max - lRoundUp;

            //Es wird geprüft, ob überhaupt gerundet werden muss!!!
            if ((Math.Round((double)amount / Math.Pow(10, digits)) * Math.Pow(10, digits)) - amount != 0)
            {
                //Werte müssen positiv sein!!!
                if (lMinTemp >= 0 && lMaxTemp >= 0 && Math.Abs(amount - lRoundDown) < Math.Abs(lRoundUp - amount))
                {
                    //Runden auf Minimalwert
                    amount = lRoundDown;
                    ret = $"<Round Status=\"Good\" Mode=\"Down\" Amount=\"{amount}\"/>";
                }
                else if (lMinTemp >= 0 && lMaxTemp >= 0 && Math.Abs(amount - lRoundDown) >= Math.Abs(lRoundUp - amount))
                {
                    //Runden auf Maximalwert
                    amount = lRoundUp;
                    ret = $"<Round Status=\"Good\" Mode=\"Up\" Amount=\"{amount}\"/>";
                }
                else if (lMinTemp >= 0 && lMaxTemp < 0)
                {
                    //Runden auf Minimalwert
                    amount = lRoundDown;
                    ret = $"<Round Status=\"Good\" Mode=\"Down\" Amount=\"{amount}\"/>";
                }
                else if (lMaxTemp >= 0 && lMinTemp < 0)
                {
                    //Runden auf Maximalwert
                    amount = lRoundUp;
                    ret = $"<Round Status=\"Good\" Mode=\"Up\" Amount=\"{amount}\"/>";
                }
                else
                {
                    //Runden nicht möglich
                    amount = amount;
                    ret = $"<Round Status=\"Bad\" Amount=\"{amount}\" Text=\"Rounding not possible\"/>";
                }
            }
            else
            {
                //Runden nicht nötig
                amount = amount;
                ret = $"<Round Status=\"Good\" Mode=\"NoRound\" Amount=\"{amount}\"/>";
            }
        }
        return ret;
    }

    /// <summary>
    /// Gibt die Anzahl der Rundbaren Größen innerhalb des Bereiches zurück
    /// </summary>
    private long IsRoundable(long min, long max, long digits)
    {
        return (long)((Math.Floor(max / Math.Pow(10, digits)) - Math.Ceiling(min / Math.Pow(10, digits))) + 1);
    }
}
