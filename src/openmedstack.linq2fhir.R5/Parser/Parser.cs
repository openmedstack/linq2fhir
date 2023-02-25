namespace OpenMedStack.Linq2Fhir.Parser;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Hl7.Fhir.Model;
using Expression = System.Linq.Expressions.Expression;

internal class Parser
{
    private static readonly Dictionary<string, PropertyInfo> PropertyInfos = new();

    public static Expression<Func<T, bool>> Parse<T>(IReadOnlyList<DslToken> tokens) where T: Resource
    {
        var tokenSequence = LoadSequenceStack(tokens);

        var parameter = Expression.Parameter(typeof(T), "x");
        var body = CreateExpression<T>(tokenSequence, parameter);
        return Expression.Lambda<Func<T, bool>>(body, false, parameter);
    }

    private static Expression CreateExpression<T>(Stack<DslToken> tokens, ParameterExpression parameter)
    {
        var first = tokens.Pop();

        var op = tokens.Pop();
        if (op.TokenType != TokenType.AssignedValue)
        {
            throw new Exception($"Malformed expression: {first.Value} {op.Value}");
        }

        var second = tokens.Pop();


        var (property, firstExpression) = CreatePropertyExpression<T>(parameter, first);


        if (!IsPrefix(second.TokenType))
        {
            var secondExpression = Expression.Constant(
                Convert.ChangeType(second.Value, property.PropertyType),
                property.PropertyType);
            return first.TokenType == TokenType.NotValue
                ? Expression.NotEqual(firstExpression, secondExpression)
                : Expression.Equal(firstExpression, secondExpression);
        }
        else
        {
            var prefix = second.TokenType;
            second = tokens.Pop();
            var secondExpression = Expression.Constant(
                Convert.ChangeType(second.Value, property.PropertyType),
                property.PropertyType);
            return BuildExpression(prefix, first.TokenType, firstExpression, secondExpression);
        }
    }

    private static (PropertyInfo property, Expression expression) CreatePropertyExpression<T>(
        ParameterExpression parameter,
        DslToken first)
    {
        var i = first.Value.IndexOf(':');
        var propertyName = i > 0 ? first.Value[..i] : first.Value;
        lock (PropertyInfos)
        {
            var key = $"{typeof(T).AssemblyQualifiedName}-{propertyName}";
            if (!PropertyInfos.TryGetValue(key, out var info))
            {
                info = typeof(T).GetProperties()
                    .First(
                        p => string.Equals(p.Name, propertyName, StringComparison.InvariantCultureIgnoreCase));
                PropertyInfos[key] = info;
            }

            return (info, Expression.Property(parameter, info));
        }
    }

    private static Expression BuildExpression(TokenType prefix, TokenType suffix, Expression first, Expression second)
    {
        return prefix switch
        {
            TokenType.NotEquals when suffix == TokenType.NotValue => Expression.Equal(first, second),
            TokenType.NotEquals => Expression.NotEqual(first, second),
            TokenType.GreaterThan when suffix == TokenType.NotValue => Expression.LessThanOrEqual(first, second),
            TokenType.GreaterThan => Expression.GreaterThan(first, second),
            TokenType.GreaterThanOrEqual when suffix == TokenType.NotValue => Expression.LessThan(first, second),
            TokenType.GreaterThanOrEqual => Expression.GreaterThanOrEqual(first, second),
            TokenType.LessThan when suffix == TokenType.NotValue => Expression.GreaterThanOrEqual(first, second),
            TokenType.LessThan => Expression.LessThan(first, second),
            TokenType.LessThanOrEqual when suffix == TokenType.NotValue => Expression.GreaterThan(first, second),
            TokenType.LessThanOrEqual => Expression.LessThanOrEqual(first, second),
            _ => throw new ArgumentOutOfRangeException(nameof(prefix))
        };
    }

    private static bool IsPrefix(TokenType tokenType)
    {
        return tokenType is TokenType.NotEquals or TokenType.GreaterThan or TokenType.LessThan
            or TokenType.GreaterThanOrEqual or TokenType.LessThanOrEqual or TokenType.StartsAfter
            or TokenType.EndsBefore or TokenType.Approximately;
    }

    private static Stack<DslToken> LoadSequenceStack(IReadOnlyList<DslToken> tokens)
    {
        var tokenSequence = new Stack<DslToken>();
        var count = tokens.Count;
        for (var i = count - 1; i >= 0; i--)
        {
            tokenSequence.Push(tokens[i]);
        }

        return tokenSequence;
    }

}