using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using web_server.Database.Repositories;
using web_server.Models.DBModels;

namespace web_server
{
    public class TokenProvider
    {
        UserRepository _userRepository;
        public TokenProvider(UserRepository userRepository)
        {
            _userRepository = userRepository;
        }
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
        public Dictionary<string, string> LoginUser(string email, string Password, User user)
        {
       

            if (Password == user.Password)
            {

                var key = Encoding.ASCII.GetBytes
                          ("YourKey-2374-OFFKDI940NG7:56753253-tyuw-5769-0921-kfirox29zoxv");

                var JWToken = new JwtSecurityToken(
                    issuer: "http://localhost:23571/",
                    audience: Program.web_app_ip + "/",
                    claims: GetUserClaims(user).Claims,
                    notBefore: DateTime.Now,
                    expires: new DateTimeOffset(DateTime.Now.AddMinutes(1160)).DateTime,
                    signingCredentials: new SigningCredentials(new SymmetricSecurityKey(key),
                                        SecurityAlgorithms.HmacSha256)
                );

                var token = new JwtSecurityTokenHandler().WriteToken(JWToken);
                var res = new Dictionary<string, string>
                {
                    { token, user.Role }
                };
                return res;
            }
            else
            {
                return null;
            }
        }

    }

}
