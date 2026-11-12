using Funkos.Models;
using MongoDB.Bson;
using Swashbuckle.AspNetCore.Annotations;

namespace Funkos.Dto.Proveedor;

[SwaggerSchema(Description = "Datos de proveedor")]
public record ProveedorDtoResponse(
    [property: SwaggerSchema(Description = "ID único del proveedor")]
    string Id,
    
    [property: SwaggerSchema(Description = "Nombre del proveedor")]
    string Name,
    
    [property: SwaggerSchema(Description = "Descripción del proveedor")]
    string Descripcion,
    
    [property: SwaggerSchema(Description = "Fecha de registro en formato UTC de la creación del proveedor")]
    DateTime CreateAt,
    
    [property: SwaggerSchema(Description = "Fecha de registro en formato UTC de la actualización del proveedor")]
    DateTime? UpdateAt,
    
    [property: SwaggerSchema(Description = "Fecha de registro en formato UTC de la eliminación del proveedor")]
    DateTime? DeleteAt
);

    
