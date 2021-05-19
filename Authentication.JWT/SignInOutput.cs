using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Authentication.JWT
{
    public class SignInOutput
    {
        public string AccountToken { get; set; }
        public string RefreshToken { get; set; }
    }
}
