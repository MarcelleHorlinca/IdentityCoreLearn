using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdentityManager.Core.Data.ViewModel
{
    public class TwoFactorAuthenticationViewModel
    { 
        //use to login
        public string? Code { get; set; }

        //use to register/ sign in
        public string? Token { get; set; }

    }
}
