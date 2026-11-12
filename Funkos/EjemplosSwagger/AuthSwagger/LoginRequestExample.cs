using Funkos.Dto.Auth;
using Swashbuckle.AspNetCore.Filters;

namespace Funkos.EjemplosSwagger.AuthSwagger;

public class LoginRequestExample: IExamplesProvider<LoginRequest>
{
    public LoginRequest GetExamples()
    {
        return new LoginRequest
        {
            Username = "jose_developer",
            Password = "PasswordSecure123*"
        };
    }
}