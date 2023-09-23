using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Application.Common;

public class AuthenticationOptions
{
    public const string ISSUER = "Project5GBack"; 
    public const string AUDIENCE = "Prooject5GFront";
    const string KEY = "arthursuperSecretKey@1995";  
    public const int LIFETIME = 60;
    public const int LIFETIMEREFRESHTOKEN = 4800;
    public static SymmetricSecurityKey GetSymmetricSecurityKey()
    {
        return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(KEY));
    }
}