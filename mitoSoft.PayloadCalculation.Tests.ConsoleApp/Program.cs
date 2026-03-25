using mitoSoft.PayloadCalculation.Extensions;
using mitoSoft.PayloadCalculation.Models;

Console.WriteLine("Payload Test:");
Console.WriteLine("");

var transporter = new Transporter();

transporter.Cells.Add(new Cell("Cell1", 10000, 0, 0));

var result = transporter.Fill(5000);
Console.WriteLine(result);

Console.WriteLine("");
Console.WriteLine("");

transporter.Cells.Add(new Cell("Cell2", 5000, 0, 0));
transporter.Cells.Add(new Cell("Cell3", 5000, 0, 0));
transporter.Cells.Add(new Cell("Cell4", 2000, 0, 0));
transporter.Cells.Add(new Cell("Cell5", 1000, 0, 0));

result = transporter.Fill(15000);
Console.WriteLine(result);