namespace OpenMedStack.Linq2Fhir.Parser;

internal record DslToken
{
    public DslToken(TokenType tokenType)
    {
        TokenType = tokenType;
        Value = string.Empty;
    }

    public DslToken(TokenType tokenType, string value)
    {
        TokenType = tokenType;
        Value = value;
    }

    public TokenType TokenType { get; }

    public string Value { get; }
}