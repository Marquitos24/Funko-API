using System.Text.Json.Serialization;
using Swashbuckle.AspNetCore.Annotations;

namespace Funkos.Dto.Auth;

[SwaggerSchema(Description = "Datos devueltos tras revisar el perfil propio.")]
public record AuthDtoResponse(
    
    [property: SwaggerSchema(Description = "ID único del usuario en el sistema")]
    long Id,
    
    [property: SwaggerSchema(Description = "Nombre de usuario")]
    string Username,
    
    [property: SwaggerSchema(Description = "Correo electrónico del usuario")]
    string Email,
    
    [property: SwaggerSchema(Description = "Roles de seguridad asignados")]
    IList<string> Roles,
    // Si es null, se elimina por completo del JSON gracias a esta línea
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [property: SwaggerSchema(Description = "Departamento corporativo")]
    string? Departament,
    
    [property: SwaggerSchema(Description = "Fecha de registro en formato UTC")]
    DateTime CreatedAt,
    
    [property: SwaggerSchema(Description = "Fecha del último login en formato UTC")]
    DateTime? LastLoginAt
    );