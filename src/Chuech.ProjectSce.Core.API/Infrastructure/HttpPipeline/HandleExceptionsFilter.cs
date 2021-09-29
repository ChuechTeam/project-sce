using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Chuech.ProjectSce.Core.API.Infrastructure.HttpPipeline
{
    public class HandleExceptionsFilter : IActionFilter, IOrderedFilter
    {
        private readonly ProblemDetailsFactory _problemDetailsFactory;

        public HandleExceptionsFilter(ProblemDetailsFactory problemDetailsFactory)
        {
            _problemDetailsFactory = problemDetailsFactory;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            var exception = context.Exception;
            if (exception is not ProjectSceException domainException)
            {
                // The DeveloperExceptionPage will handle the exception for us, 
                // or, in production, ASP.NET Core's default error handling will
                // do the job
                return;
            }

            context.Result = domainException.Error.AsAspResult(_problemDetailsFactory, context.HttpContext);
            context.ExceptionHandled = true;
        }

        public int Order => int.MaxValue - 25;
    }
}