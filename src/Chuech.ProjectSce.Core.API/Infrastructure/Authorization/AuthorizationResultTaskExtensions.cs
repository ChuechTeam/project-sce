namespace Chuech.ProjectSce.Core.API.Infrastructure.Authorization;

public static class AuthorizationResultTaskExtensions
{
    public static async Task ThrowIfUnsuccessful<T>(this Task<T> task) where T : AuthorizationResult
    {
        (await task).ThrowIfFailed();    
    }
}