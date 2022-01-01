namespace Chuech.ProjectSce.Core.API.Infrastructure;

/// <summary>
/// Represents a domain error in response to a MassTransit request.
/// </summary>
/// <param name="Error">The error that happened.</param>
public record ProjectSceFailure(Error Error);
