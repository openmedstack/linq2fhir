namespace OpenMedStack.Linq2Fhir.Parser;

internal enum TokenType
{
    CloseParenthesis,
    Equals,
    NotEquals,
    Number,
    OpenParenthesis,
    StringValue,
    SequenceTerminator,
    Value,
    NotValue,
    MissingValue,
    ExactValue,
    ContainsValue,
    AssignedValue,
    GreaterThan,
    LessThan,
    GreaterThanOrEqual,
    LessThanOrEqual,
    StartsAfter,
    EndsBefore,
    Approximately
}