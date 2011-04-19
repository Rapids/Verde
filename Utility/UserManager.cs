using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Verde.Utility
{
    class User
    {
        public string Name { set; get; }
        public int WriteCount { set; get; }
        public int PickedupCount { set; get; }
        public string IconUrl { set; get; }

        public User()
        {
        }
    }

    class UserManager
    {
        private Dictionary<string, User> listUsers;

        public static UserManager GlobalUserManager = new UserManager();

        public UserManager()
        {
            this.listUsers = new Dictionary<string, User>();
        }

        public User Register(string strNameCandidate, string strIconUrl)
        {
            User user = null;
            lock (this) {
                string strName = strNameCandidate.Trim();

                if (!this.listUsers.ContainsKey(strName)) {
                    user = new User();
                    user.Name = strName;
                    user.IconUrl = strIconUrl;
                    this.listUsers.Add(strName, user);
                } else {
                    user = this.listUsers[strName];
                }
            }

            return user;
        }
    }
}
