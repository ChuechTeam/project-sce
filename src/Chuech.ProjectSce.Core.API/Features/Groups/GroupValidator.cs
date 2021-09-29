
using System.Linq.Expressions;

namespace Chuech.ProjectSce.Core.API.Features.Groups;
public class GroupValidator<T> : AbstractValidator<T>
{
    protected void AddNameRule(Expression<Func<T, string>> expression)
    {
        RuleFor(expression)
            .Must(x => !string.IsNullOrEmpty(x) && x.Length <= 60)
            .WithErrorCode("group.name.invalid")
            .Configure(r => r.MessageBuilder = _ => "The group name is required and has a maximum of 60 characters.");
    }

    protected void AddUserIdsRule(Expression<Func<T, int[]?>> expression)
    {
        RuleFor(expression)
            .Must(x => (x?.Length ?? 0) < 50)
            .WithErrorCode("group.users.maximumCountExceeded")
            .WithMessage("Too much users (max 50).");
    }
}
