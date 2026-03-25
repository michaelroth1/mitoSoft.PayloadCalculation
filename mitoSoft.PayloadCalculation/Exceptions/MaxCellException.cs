namespace mitoSoft.PayloadCalculation.Exceptions;

public class MaxCellException : PayloadCalculationException
{
    public MaxCellException() : this("Zu viele Kammer angelegt!")
    {
    }

    public MaxCellException(string message) : base(message)
    {
    }
}