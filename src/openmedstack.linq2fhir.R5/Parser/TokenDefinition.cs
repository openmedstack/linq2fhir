namespace OpenMedStack.Linq2Fhir.Parser;

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

internal class TokenDefinition
{
    private readonly Regex _regex;
    private readonly TokenType _returnsToken;
    private readonly int _precedence;

#if NET7_0
    public TokenDefinition(TokenType returnsToken, [StringSyntax(StringSyntaxAttribute.Regex)] string regexPattern, int precedence)
#else
    public TokenDefinition(TokenType returnsToken, string regexPattern, int precedence)
#endif
    {
        _regex = new Regex(regexPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
        _returnsToken = returnsToken;
        _precedence = precedence;
    }

    public IEnumerable<TokenMatch> FindMatches(string inputString)
    {
        var matches = _regex.Matches(inputString);
        for (var i = 0; i < matches.Count; i++)
        {
            yield return new TokenMatch()
            {
                StartIndex = matches[i].Index,
                EndIndex = matches[i].Index + matches[i].Length,
                TokenType = _returnsToken,
                Value = matches[i].Value,
                Precedence = _precedence
            };
        }
    }
}