using System.Collections.Generic;

namespace OB.DL.Common.Repositories.Interfaces.Rest
{
    public interface IOBUserRepository : IRestRepository<OB.BL.Contracts.Data.General.User>
    {

        List<OB.BL.Contracts.Data.General.User> ListUsers(OB.BL.Contracts.Requests.ListUserRequest request);
    }
}
