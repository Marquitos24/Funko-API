using Funkos.Dto.Auth;
using Swashbuckle.AspNetCore.Filters;

namespace Funkos.EjemplosSwagger.AuthSwagger;

public class SignupRequestExample: IExamplesProvider<AuthCreateDtoRequest>
{
    public AuthCreateDtoRequest GetExamples()
    {
        return new AuthCreateDtoRequest
        {
            Username = "jose_developer",
            Email = "jose.gonzalez@example.com",
            Password = "PasswordSecure123*"
        };
    }
}