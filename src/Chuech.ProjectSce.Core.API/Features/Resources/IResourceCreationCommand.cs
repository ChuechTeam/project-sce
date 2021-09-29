using Chuech.ProjectSce.Core.API.Features.Institutions;

namespace Chuech.ProjectSce.Core.API.Features.Resources
{
    public interface IResourceCreationCommand : IInstitutionRequest
    {
        string Name { get; }
    }

    public abstract class ResourceCreationCommandValidator<T> : AbstractValidator<T> where T : IResourceCreationCommand
    {
        public ResourceCreationCommandValidator()
        {
            RuleFor(x => x.Name)
                .MaximumLength(100)
                .WithMessage("The resource name must not exceed the max length.")
                .WithErrorCode("resource.name.maxLengthExceeded");
        }
    }
}