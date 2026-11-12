using System.ComponentModel.DataAnnotations;
using Funkos.Models;
using Funkos.Validations;
using MongoDB.Bson;
using Swashbuckle.AspNetCore.Annotations;

namespace Funkos.Dto.Proveedor;

[SwaggerSchema(Description = "Datos para actualizar un proveedor")]
public record ProveedorUpdateDto
{
    [RegularExpression(@"^[0-9a-fA-F]{24}$", ErrorMessage =  "Formato invalido de ID")]
    public ObjectId Id { get; set; }
    
    [Required (ErrorMessage =  "El nombre de la categoria es obligatoria")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "El nombre del proveedor no puede superar los 200 caracteres y debe ser superior a 1")]
    [NoLink]
    [SwaggerSchema(Description = "Nombre del proveedor", Nullable = false)]
    public required string Name { get; set; } = string.Empty;
    
    [Required (ErrorMessage =  "La descripción de la categoria es obligatoria")]
    [StringLength(300, MinimumLength = 1, ErrorMessage = "La descripción del proveedor no puede superar los 300 caracteres y debe ser superior a 1")]
    [NoLink]
    [SwaggerSchema(Description = "Descripción del proveedor", Nullable = false)]
    public required string Descripcion  { get; set; }
}