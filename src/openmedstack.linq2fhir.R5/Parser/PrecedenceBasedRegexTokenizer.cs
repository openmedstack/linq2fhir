namespace OpenMedStack.Linq2Fhir.Parser;

using System.Collections.Generic;
using System.Linq;

internal class PrecedenceBasedRegexTokenizer
{
    private readonly List<TokenDefinition> _tokenDefinitions;

    public PrecedenceBasedRegexTokenizer()
    {
        _tokenDefinitions = new List<TokenDefinition>
        {
                new (TokenType.OpenParenthesis, "\\(", 1),
                new (TokenType.CloseParenthesis, "\\)", 1),
                new (TokenType.AssignedValue, "=", 1),
                new (TokenType.Equals, "eq", 1),
                new (TokenType.NotEquals, "ne", 1),
                new (TokenType.GreaterThan, "gt", 1),
                new (TokenType.LessThan, "lt", 1),
                new (TokenType.GreaterThanOrEqual, "ge", 1),
                new (TokenType.LessThanOrEqual, "le", 1),
                new (TokenType.StartsAfter, "sa", 1),
                new (TokenType.EndsBefore, "eb", 1),
                new (TokenType.Approximately, "ap", 1),
                new (TokenType.StringValue, "'([^']*)'", 2),
                new (TokenType.Value, "((?<!:)\\b\\w+\\b(?!:(not|missing|exact|contains)))", 2),
                new (TokenType.NotValue, "\\b\\w+\\b:not", 2),
                new (TokenType.MissingValue, "\\b\\w+\\b:missing", 2),
                new (TokenType.ExactValue, "\\b\\w+\\b:exact", 2),
                new (TokenType.ContainsValue, "\\b\\w+\\b:contains", 2),
                new (TokenType.Number, "\\d+", 2)
            };
    }

    public IEnumerable<DslToken> Tokenize(string input)
    {
        var tokenMatches = FindTokenMatches(input);

        var groupedByIndex = tokenMatches.GroupBy(x => x.StartIndex)
            .OrderBy(x => x.Key)
            .ToArray();

        TokenMatch? lastMatch = null;
        foreach (var group in groupedByIndex)
        {
            var bestMatch = group.OrderBy(x => x.Precedence).First();
            if (lastMatch != null && bestMatch.StartIndex < lastMatch.EndIndex)
                continue;

            yield return new DslToken(bestMatch.TokenType, bestMatch.Value);

            lastMatch = bestMatch;
        }

        yield return new DslToken(TokenType.SequenceTerminator);
    }

    private IEnumerable<TokenMatch> FindTokenMatches(string lqlText)
    {
        return _tokenDefinitions.SelectMany(tokenDefinition => tokenDefinition.FindMatches(lqlText));
    }
}