namespace mitoSoft.PayloadCalculation;

internal class RangeCollection
{
    /// <summary>
    /// Collection of transporter ranges
    /// </summary>
    public List<TransporterRange> TransporterRanges { get; } = [];

    /// <summary>
    /// Indexer for accessing transporter ranges by index
    /// </summary>
    public TransporterRange this[int index]
    {
        get => TransporterRanges[index];
        set => TransporterRanges[index] = value;
    }
            
    /// <summary>
    /// Adds a transporter range to the collection
    /// </summary>
    public void Add(TransporterRange transporterRange)
    {
        TransporterRanges.Add(transporterRange);
    }

    public bool Contains(TransporterRange transporterRange)
    {
        foreach (var range in TransporterRanges)
        {
            bool contain = true;
            for (short j = 0; j < range.CellRanges.Count; j++)
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