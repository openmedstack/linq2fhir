namespace OpenMedStack.Linq2Fhir.Parser;

internal class TokenMatch
{
    public TokenType TokenType { get; init; }
    public string Value { get; init; } = null!;
    public int StartIndex { get; init; }
    public int EndIndex { get; init; }
    public int Precedence { get; init; }
}