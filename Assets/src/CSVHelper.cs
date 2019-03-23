using System;
using System.Text;

public class CSVHelper {
    private static readonly string SEPARATOR = ";";
    private static readonly string NEW_LINE = Environment.NewLine;

    private StringBuilder csv;

    public CSVHelper()
    {
        csv = new StringBuilder();
    }

    public CSVHelper Append_Cell(string cell)
    {
        csv.Append(cell).Append(SEPARATOR);
        return this;
    }

    public CSVHelper New_Line()
    {
        csv.Append(NEW_LINE);
        return this;
    }

    public CSVHelper Append_Row(string cell)
    {
        Append_Cell(cell);
        New_Line();
        return this;
    }

    public CSVHelper Append_Row(string[] row)
    {
        foreach(string cell in row) {
            Append_Cell(cell);
        }
        New_Line();
        return this;
    }

    public override string ToString()
    {
        return csv.ToString();
    }
}
