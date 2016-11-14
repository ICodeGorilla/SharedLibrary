using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;

namespace Shared.Helper.Test
{
    public static class UserHelper
    {
        public static IDisposable Context { get; set; }

        public static IPrincipal CreateTestCurrentUser(string username = "Test")
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.NameIdentifier, "TestUserIdentifier"),
                new Claim(ClaimTypes.Role, "TestRole")
            };

            var claimsIdentity = new ClaimsIdentity(claims, "NeededToSetAuthenticated");
            var principal = new ClaimsPrincipal(new[] { claimsIdentity });
            return principal;
        }
    }
}