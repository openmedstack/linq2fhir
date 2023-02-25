// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FhirQueryProvider.cs" company="Reimers.dk">
//   Copyright © Reimers.dk 2014-2021
//   This source is subject to the Microsoft Public License (Ms-PL).
//   Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
//   All other rights reserved.
// </copyright>
// <summary>
//   Defines the FhirQueryProvider type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenMedStack.Linq2Fhir.Provider;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Expression = System.Linq.Expressions.Expression;

internal class FhirQueryProvider<T> : IAsyncQueryProvider, IDisposable where T : Resource, new()
{
    private readonly FhirClient _client;

    public FhirQueryProvider(FhirClient client)
    {
        _client = client;
    }

    public IAsyncQueryable<TResult> CreateQuery<TResult>(Expression expression)
    {
        if (expression == null)
        {
            throw new ArgumentNullException(nameof(expression));
        }

        var methodInfo = GetType().GetMethod(nameof(InnerCreateQueryable), BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Static);
        var method = methodInfo!.MakeGenericMethod(typeof(TResult));
        return (IAsyncQueryable<TResult>)method.Invoke(null, new object?[] { _client, expression })!;
    }

    /// <inheritdoc />
    public async ValueTask<TResult> ExecuteAsync<TResult>(Expression expression, CancellationToken token)
    {
        var visitor = new QueryExpressionVisitor();
        visitor.Visit(expression);
        var p = visitor.GetParams();
        var bundle = await GetResults(p).ConfigureAwait(false);
        if (typeof(TResult) == typeof(Bundle))
        {
            return (TResult)(bundle as object)!;
        }
        var enumerable = bundle.GetResources().OfType<T>();
        object? o = typeof(TResult) switch
        {
            not null when typeof(TResult) == typeof(T) && Nullable.GetUnderlyingType(typeof(TResult)) != null => enumerable.FirstOrDefault(),
            not null when typeof(TResult) == typeof(T) && Nullable.GetUnderlyingType(typeof(TResult)) == null => enumerable.First(),
            not null when typeof(TResult).IsAssignableTo(typeof(List<T>)) => enumerable.ToList(),
            not null when typeof(TResult).IsAssignableTo(typeof(T[])) => enumerable.ToArray(),
            not null when typeof(TResult).IsAssignableTo(typeof(IEnumerable<T>)) => enumerable.AsEnumerable(),
            _ => throw new Exception($"Unexpected type {nameof(TResult)}")
        };
        return (TResult)o!;
    }

    public void Dispose()
    {
        _client.Dispose();
        GC.SuppressFinalize(this);
    }

    private static IAsyncQueryable<TResult> InnerCreateQueryable<TResult>(
        FhirClient client,
        Expression expression) where TResult : Resource, new()
    {
        return new FhirQueryable<TResult>(client, expression);
    }

    private Task<Bundle?> GetResults(SearchParams builder)
    {
        return _client.SearchAsync<T>(builder);
    }
}