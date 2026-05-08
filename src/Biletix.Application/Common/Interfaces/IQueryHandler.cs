using MediatR;

namespace Biletix.Application.Common.Interfaces;

/// <summary>
/// Veri okuma amacli sorgulari isleyen handler sozlesmesidir.
/// </summary>
/// <typeparam name="TQuery">Islenecek sorgu tipi.</typeparam>
/// <typeparam name="TResult">Sorgu sonucunun tipi.</typeparam>
public interface IQueryHandler<in TQuery, TResult> : IRequestHandler<TQuery, TResult>
    where TQuery : IQuery<TResult>
{
}
