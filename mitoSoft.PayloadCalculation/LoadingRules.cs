namespace mitoSoft.PayloadCalculation;

/// <summary>
/// Legal rules and limits for chamber filling
/// </summary>
/// <remarks>
/// Constructor with custom values
/// </remarks>
public class LoadingRules(
    double chamberLowerMin,
    double chamberLowerMax,
    double chamberUpperMin,
    double chamberUpperMax,
    long chamberSplitLimit)
{
    /// <summary>
    /// Constructor -> sets properties with default values
    /// </summary>
    public LoadingRules() : this(0.0, 0.25, 0.8, 0.9, 7500) { }

    public double ChamberLowerMin { get; set; } = chamberLowerMin;
    public double ChamberLowerMax { get; set; } = chamberLowerMax;
    public double ChamberUpperMin { get; set; } = chamberUpperMin;
    public double ChamberUpperMax { get; set; } = chamberUpperMax;

    /// <summary>
    /// Chamber size from which the calculation no longer uses 2 ranges ('default value' = 7500)
    /// </summary>
    public long ChamberSplitLimit { get; set; } = chamberSplitLimit;
}
