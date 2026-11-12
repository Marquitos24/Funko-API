using Swashbuckle.AspNetCore.Annotations;

namespace Funkos.Dto.Funko;

[SwaggerSchema(Description = "Datos de funko para añadir en categoria")]
public record FunkoParaCategoriaDto
    (
        [property: SwaggerSchema(Description = "Identificador de un funko")]
    long Id,
    
    [property: SwaggerSchema(Description = "Nombre de un funko")]
    string Name,
    
    [property: SwaggerSchema(Description = "Precio de un funko en euros", Format = "decimal")]
    decimal Precio
    );