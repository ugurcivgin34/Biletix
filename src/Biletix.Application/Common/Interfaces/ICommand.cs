using MediatR;

namespace Biletix.Application.Common.Interfaces;

/// <summary>
/// Sistemde durum degisikligi yapan ve sonuc donduren MediatR istegini temsil eder.
/// </summary>
/// <typeparam name="TResult">Komutun dondurecegi sonuc tipi.</typeparam>
public interface ICommand<out TResult> : IRequest<TResult>
{
}

/// <summary>
/// Sistemde durum degisikligi yapan ancak sonuc dondurmeyen MediatR istegini temsil eder.
/// </summary>
public interface ICommand : IRequest
{
}
