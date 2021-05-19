using Authentication.JWT.Permission;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Authentication.JWT.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly UserStore _userStore;
        public UserController(UserStore userStore)
        {
            _userStore = userStore;
        }
        [HttpGet]
        [PermissionFilter(Permissions.UserRead)]
        public User Get(int id)
        {
            return _userStore.FindUser(id);
        }
        [HttpPost]
        public bool Post()
        {
            return true;
        }
    }
}
