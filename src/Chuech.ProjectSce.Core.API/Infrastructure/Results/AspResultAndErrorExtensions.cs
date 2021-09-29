using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Chuech.ProjectSce.Core.API.Infrastructure.Results;

public static class AspResultAndErrorExtensions
{
    public static IActionResult AsAspResult<T>(this in OperationResult<T> result, ControllerBase controller)
        => AsAspResult(result, controller.ProblemDetailsFactory, controller.HttpContext);

    public static IActionResult AsAspResult<T>(this in OperationResult<T> result,
        ProblemDetailsFactory problemDetailsFactory,
        HttpContext httpContext)
    {
        if (result.TryGetValue(out var value, out var error))
        {
            return new OkObjectResult(value);
        }
        else
        {
            return error.AsAspResult(problemDetailsFactory, httpContext);
        }
    }

    public static IActionResult AsAspResult(this Error error,
        ProblemDetailsFactory problemDetailsFactory,
        HttpContext httpContext)
    {
        var statusCode = (int)error.Kind.GetStatusCode();
        var problemDetails = problemDetailsFactory.CreateProblemDetails(
            httpContext,
            title: error.Message,
            type: error.ErrorCode,
            statusCode: statusCode);
        if (error.AdditionalInfo is not null)
        {
            problemDetails.Extensions["info"] = error.AdditionalInfo;
        }
        return new ObjectResult(problemDetails) { StatusCode = statusCode };
    }
}
