namespace mitoSoft.PayloadCalculation;

public class LSCLCalculatorException : Exception
{
    public LSCLCalculatorException(string message) : base(message)
    {
    }
}

public class MaxCellException : LSCLCalculatorException
{
    public MaxCellException() : this("Zu viele Kammer angelegt!")
    {
    }

    public MaxCellException(string message) : base(message)
    {
    }
}
