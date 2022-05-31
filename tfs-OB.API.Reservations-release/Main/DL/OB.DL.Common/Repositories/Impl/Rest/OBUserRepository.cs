using OB.DL.Common.Impl;
using OB.DL.Common.Infrastructure;
using OB.DL.Common.Repositories.Interfaces.Rest;
using System.Collections.Generic;
using System.Linq;

namespace OB.DL.Common.Repositories.Impl.Rest
{
    internal class OBUserRepository : RestRepository<OB.BL.Contracts.Data.General.User>, IOBUserRepository
    {
        public List<OB.BL.Contracts.Data.General.User> ListUsers(OB.BL.Contracts.Requests.ListUserRequest request)
        {
            var data = new List<OB.BL.Contracts.Data.General.User>();
            var response = RESTServicesFacade.Call<OB.BL.Contracts.Responses.ListUserResponse>(request, "General", "ListUsers");

            if (response.Status == OB.BL.Contracts.Responses.Status.Success)
                data = response.Result.ToList();

            return data;
        }
    }
}