namespace mitoSoft.PayloadCalculation;

public class RangeCollection : List<TransporterRange>
{
    /// <summary>
    /// Äquivalent zu 'RemoveAt'
    /// Hält die Schnittstelle konform
    /// </summary>
    public long DeleteItem(long index)
    {
        RemoveAt((int)index);
        return 0;
    }

    public new bool Contains(TransporterRange transporterRange)
    {
        foreach (var range in this)
        {
            bool contain = true;
            for (short j = 0; j < range.Count; j++)
            {
                if (transporterRange[j].LowerLimit != range[j].LowerLimit ||
                    transporterRange[j].UpperLimit != range[j].UpperLimit)
                {
                    contain = false;
                }
            }
            if (contain)
            {
                return true;
            }
        }
        return false;
    }
}