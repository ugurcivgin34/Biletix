using MediatR;

namespace Biletix.Application.Common.Interfaces;

/// <summary>
/// Sistemde veri okuyan ve durum degistirmemesi beklenen MediatR istegini temsil eder.
/// </summary>
/// <typeparam name="TResult">Sorgunun dondurecegi sonuc tipi.</typeparam>
public interface IQuery<out TResult> : IRequest<TResult>
{
}
