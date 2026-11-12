using Funkos.Models;
using Swashbuckle.AspNetCore.Annotations;

namespace Funkos.Dto.Funko;

[SwaggerSchema(Description = "Datos de funko.")]
public record FunkosDtoResponse(
    [property: SwaggerSchema(Description = "Identificador de un funko")]
    long Id,
    
    [property: SwaggerSchema(Description = "Nombre del funko")]
    string Name,
    
    [property: SwaggerSchema(Description = "Nombre de la categoria", Format = "decimal")]
    decimal Precio,
    
    [property: SwaggerSchema(Description = "Identificador de la categoria asociada", Nullable = true)]
    string? CategoriaId,
    
    [property: SwaggerSchema(Description = "Identificador del proveedor asociado")]
    string ProveedorId,
    
    [property: SwaggerSchema(Description = "Stock disponible de funkos", Format = "int")]
    int Stock,
    
    [property: SwaggerSchema(Description = "Imagen del funko", Nullable = true)]
    string? image,
    
    [property: SwaggerSchema(Description = "Fecha de registro en formato UTC de creación del funko", Format = "DateTime")]
    DateTime CreateAt,
    
    [property: SwaggerSchema(Description = "Fecha de registro en formato UTC de actualización del funko", Format = "DateTime", Nullable = true)]
    DateTime? UpdateAt,
    
    [property: SwaggerSchema(Description = "Fecha de registro en formato UTC de eliminación del funko", Format = "DateTime" , Nullable = true)]
    DateTime? DeleteAt
);

    
