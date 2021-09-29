using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Chuech.ProjectSce.Identity.Data;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;

namespace Chuech.ProjectSce.Identity.Grpc
{
    public class GrpcUserService : UserService.UserServiceBase
    {
        private readonly AppIdentityContext _context;

        public GrpcUserService(AppIdentityContext context)
        {
            _context = context;
        }

        public override async Task<UsersResponse> GetUsers(UsersRequest request, ServerCallContext context)
        {
            var idsToSearch = request.Ids.ToArray();

            var users = await _context.UserProfiles
                .Where(x => idsToSearch.Contains(x.Id))
                .Select(x => new User
                {
                    Id = x.Id,
                    DisplayName = x.DisplayName
                })
                .ToListAsync(context.CancellationToken);

            return MapToResponse(users);
        }

        private static UsersResponse MapToResponse(IEnumerable<User> users)
        {
            var usersResponse = new UsersResponse();
            foreach (var user in users)
            {
                usersResponse.Users[user.Id] = user;
            }

            return usersResponse;
        }

        public override async Task<UsersResponse> GetAllUsers(Empty request, ServerCallContext context)
        {
            var users = await _context.UserProfiles
                .Select(x => new User
                {
                    Id = x.Id,
                    DisplayName = x.DisplayName
                })
                .ToListAsync(context.CancellationToken);

            return MapToResponse(users);
        }
    }
}