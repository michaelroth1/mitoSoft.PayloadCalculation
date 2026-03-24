using mitoSoft.PayloadCalculation;

Console.WriteLine("Ladestellencheckliste Befüllung:");

var transporter = new Transporter();

transporter.Add(new Cell("Kammer1", 10000, 0, 0, ref transporter));
transporter.Add(new Cell("Kammer2", 5000, 0, 0, ref transporter));
transporter.Add(new Cell("Kammer3", 5000, 0, 0, ref transporter));
transporter.Add(new Cell("Kammer4", 2000, 0, 0, ref transporter));
transporter.Add(new Cell("Kammer5", 1000, 0, 0, ref transporter));

var filler = new Filler();
var result =  filler.Fill(15000, ref transporter);

Console.WriteLine(result);