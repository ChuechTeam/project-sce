namespace Chuech.ProjectSce.Core.API.Features.Resources.Documents
{
    public static class DeleteDocumentResource
    {
        public record Command(int ResourceId) : IRequest;

        public class Handler : IRequestHandler<Command>
        {
            public async Task<Unit> Handle(Command request, CancellationToken cancellationToken)
            {
                throw new NotImplementedException(); // Do something!
            }
        }
    }
}