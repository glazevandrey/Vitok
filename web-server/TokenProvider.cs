using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using web_server.DbContext;
using web_server.Models;

namespace web_server
{
    public class TokenProvider
    {

        private ClaimsIdentity GetUserClaims(User user)
        {
            var claims = new List<Claim>
            {
            new Claim(ClaimTypes.Name, user.FirstName + " " + user.LastName),
            new Claim("UserId", user.UserId.ToString()),
            new Claim("Email", user.Email),
            new Claim("Phone", user.Phone),
            new Claim("Role", user.Role),
            };

            return new ClaimsIdentity(claims, "token");
        }
        public string LoginUser(string email, string Password)
        {
            var user = TestData.UserList.SingleOrDefault(x => x.Email == email);

            if (user == null)
                return null;


            if (Password == user.Password)
            {

                var key = Encoding.ASCII.GetBytes
                          ("YourKey-2374-OFFKDI940NG7:56753253-tyuw-5769-0921-kfirox29zoxv");

                var JWToken = new JwtSecurityToken(
                    issuer: "http://localhost:35944/",
                    audience: Program.web_app_ip + "/",
                    claims: GetUserClaims(user).Claims,
                    notBefore: DateTime.Now,
                    expires: new DateTimeOffset(DateTime.Now.AddMinutes(1160)).DateTime,
                    signingCredentials: new SigningCredentials(new SymmetricSecurityKey(key),
                                        SecurityAlgorithms.HmacSha256)
                );

                var token = new JwtSecurityTokenHandler().WriteToken(JWToken);
                return token;
            }
            else
            {
                return null;
            }
        }

    }

}
