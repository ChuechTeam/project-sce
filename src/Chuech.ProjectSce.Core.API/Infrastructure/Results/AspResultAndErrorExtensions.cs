using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Chuech.ProjectSce.Core.API.Infrastructure.Results;

public static class AspResultAndErrorExtensions
{
    public static IActionResult AsAspResult(this Error error, ControllerBase controller) 
        => AsAspResult(error, controller.ProblemDetailsFactory, controller.HttpContext);

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