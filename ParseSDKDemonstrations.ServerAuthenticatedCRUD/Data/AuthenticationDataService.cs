using Parse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ParseSDKDemonstrations.ServerAuthenticatedCRUD.Data
{
    public class AuthenticationDataService
    {
        public string Handle { get; set; }

        public string Password { get; set; }

        public bool Fresh { get; set; }
    }
}
