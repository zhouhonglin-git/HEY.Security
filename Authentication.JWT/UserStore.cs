using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Authentication.JWT
{
    public class UserStore
    {
        private static List<User> _users = new List<User>() {
            new User {  Id=1, Name="alice", Password="alice", Email="alice@gmail.com", PhoneNumber="18800000001", Role="admin" },
            new User {  Id=2, Name="bob", Password="bob", Email="bob@gmail.com",
                PhoneNumber="18800000002",
                Permissions=new List<UserPermission>{ new UserPermission {
                     UserId=2,
                     PermissionName="User.Read"
            } }
            }  };

        public User FindUser(string userName, string password)
        {
            return _users.FirstOrDefault(_ => _.Name == userName && _.Password == password);
        }
        public User FindUser(int id)
        {
            return _users.FirstOrDefault(_ => _.Id == id);
        }
        public bool CheckPermission(int userId, string permissionName)
        {
            var user = FindUser(userId);
            if (user == null) return false;
            return user.Permissions.Any(p => permissionName.StartsWith(p.PermissionName));
        }
    }
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Role { get; set; }
        public List<UserPermission> Permissions { get; set; }
    }
    public class UserPermission
    {
        public int UserId { get; set; }

        public string PermissionName { get; set; }

        public User User { get; set; }
    }
}
