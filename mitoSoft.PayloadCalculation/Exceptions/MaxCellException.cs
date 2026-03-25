namespace mitoSoft.PayloadCalculation.Exceptions;

public class MaxCellException : PayloadCalculationException
{
    public MaxCellException() : base("Zu viele Kammer angelegt!")
    {
    }
}