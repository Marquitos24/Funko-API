using Funkos.Models;
using Swashbuckle.AspNetCore.Annotations;

namespace Funkos.Dto.Categoria;

[SwaggerSchema(Description = "Datos de categoria.")]
public record CategoriaDtoResponse(
    [property: SwaggerSchema(Description = "ID único de la categoria")]
    string IdY,
    
    [property: SwaggerSchema(Description = "Nombre de la categoria")]
    string Name,
    
    [property: SwaggerSchema(Description = "Descripción de la categoria")]
    string Descripcion,
    
    [property: SwaggerSchema(Description = "Fecha de registro en formato UTC de la creación de categoria")]
    DateTime CreateAt,
    
    [property: SwaggerSchema(Description = "Fecha de registro en formato UTC de la actualización de la categoria")]
    DateTime? UpdateAt
);

    
