using System.Text;

namespace Nestor.SourceGenerator;

public class CSharpStringBuilder
{
    private readonly StringBuilder _stringBuilder = new();
    private readonly int _indentSize;
    private int _currentindent;

    public CSharpStringBuilder(int indentSize)
    {
        _indentSize = indentSize;
    }

    public void AppendLine(string line)
    {
        var normalizeLine = line.Trim();
        _stringBuilder.AppendLine($"{GetCurrentIndent()}{normalizeLine}");

        if (normalizeLine.Contains('{'))
        {
            _currentindent++;
        }
        else if (normalizeLine.Contains('}'))
        {
            _currentindent--;
        }
    }

    public void AppendLine()
    {
        _stringBuilder.AppendLine();
    }

    public override string ToString()
    {
        return _stringBuilder.ToString();
    }

    private string GetCurrentIndent()
    {
        if (_currentindent == 0)
        {
            return string.Empty;
        }

        return new(' ', _indentSize * _currentindent);
    }
}
