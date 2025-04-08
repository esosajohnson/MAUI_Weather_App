using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Firebase.Auth;

namespace WeatherApp_CW.NVVM.Models
{
    public class AuthService
    {
        public FirebaseAuthProvider AuthProvider { get; set; }
        public FirebaseAuthLink AuthLink { get; set; }

        public bool IsAuthenticated => AuthLink != null;
    }

}
