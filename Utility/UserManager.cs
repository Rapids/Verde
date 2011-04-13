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

        public User(string strNameCandidate)
        {
            this.Name = strNameCandidate.Trim();
        }
    }

    class UserManager
    {
    }
}
