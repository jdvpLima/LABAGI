using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts
{
    // AuthContext.cs
    public static class AuthContext
    {
        public static string SessionToken { get; private set; }
        public static long UserId { get; private set; }
        public static string Handle { get; private set; }
        public static string DisplayName { get; private set; }

        public static bool IsLoggedIn => !string.IsNullOrEmpty(SessionToken) && UserId > 0;

        public static void SetSession(string token, MeResponseDTO me)
        {
            SessionToken = token;
            UserId = me.id;
            Handle = me.handle;
            DisplayName = me.displayName;
        }

        public static void Clear()
        {
            SessionToken = null;
            UserId = 0;
            Handle = null;
            DisplayName = null;
        }
    }

}
