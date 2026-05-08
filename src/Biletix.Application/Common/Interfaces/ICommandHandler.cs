using MediatR;

namespace Biletix.Application.Common.Interfaces;

/// <summary>
/// Sonuc donduren komutlari isleyen handler sozlesmesidir.
/// </summary>
/// <typeparam name="TCommand">Islenecek komut tipi.</typeparam>
/// <typeparam name="TResult">Komut sonucunun tipi.</typeparam>
public interface ICommandHandler<in TCommand, TResult> : IRequestHandler<TCommand, TResult>
    where TCommand : ICommand<TResult>
{
}

/// <summary>
/// Sonuc dondurmeyen komutlari isleyen handler sozlesmesidir.
/// </summary>
/// <typeparam name="TCommand">Islenecek komut tipi.</typeparam>
public interface ICommandHandler<in TCommand> : IRequestHandler<TCommand>
    where TCommand : ICommand
{
}
