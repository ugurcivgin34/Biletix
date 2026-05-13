using Biletix.Application.Common.Behaviours;
using FluentAssertions;
using FluentValidation;
using MediatR;

namespace Biletix.Application.Tests.Tests.Common;

public class ValidationBehaviourTests
{
    [Fact]
    public async Task Should_ThrowValidationException_WhenValidationFails()
    {
        var validator = new TestRequestValidator();
        var behaviour = new ValidationBehaviour<TestRequest, string>(new[] { validator });
        var request = new TestRequest(string.Empty);

        var act = async () => await behaviour.Handle(
            request,
            _ => Task.FromResult("next"),
            CancellationToken.None);

        var exception = await act.Should().ThrowAsync<ValidationException>();
        exception.Which.Errors.Should().Contain(error => error.PropertyName == nameof(TestRequest.Name));
    }

    [Fact]
    public async Task Should_CallNext_WhenValidationPasses()
    {
        var validator = new TestRequestValidator();
        var behaviour = new ValidationBehaviour<TestRequest, string>(new[] { validator });
        var nextCalled = false;
        RequestHandlerDelegate<string> next = _ =>
        {
            nextCalled = true;
            return Task.FromResult("next");
        };

        var result = await behaviour.Handle(new TestRequest("valid"), next, CancellationToken.None);

        result.Should().Be("next");
        nextCalled.Should().BeTrue();
    }

    private sealed record TestRequest(string Name) : IRequest<string>;

    private sealed class TestRequestValidator : AbstractValidator<TestRequest>
    {
        public TestRequestValidator()
        {
            RuleFor(request => request.Name).NotEmpty();
        }
    }
}
