using mitoSoft.PayloadCalculation;
using mitoSoft.PayloadCalculation.Models;

Console.WriteLine("Ladestellencheckliste Befüllung:");

var transporter = new Transporter();

transporter.Add(new Cell("Cell1", 10000, 0, 0));
transporter.Add(new Cell("Cell2", 5000, 0, 0));
transporter.Add(new Cell("Cell3", 5000, 0, 0));
transporter.Add(new Cell("Cell4", 2000, 0, 0));
transporter.Add(new Cell("Cell5", 1000, 0, 0));

var filler = new Filler();
var result =  filler.Fill(15000, ref transporter);

Console.WriteLine(result);