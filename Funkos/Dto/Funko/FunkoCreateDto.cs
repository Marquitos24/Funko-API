using System.ComponentModel.DataAnnotations;
using Funkos.idGenerators;
using Funkos.Models;
using Funkos.Validations;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace Funkos.Dto.Funko;

[SwaggerSchema(Description = "Datos para crear un funko")]
public record FunkoCreateDto
{
    [Required (ErrorMessage =  "El nombre del funko es obligatorio")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "El nombre del Funko no puede superar los 100 caracteres y debe ser superior a 1")]
    [NoLink]
    [SwaggerSchema(Description = "Nombre del funko", Nullable = false)]
    public required string Name { get; set; } = string.Empty;
    
    [Required (ErrorMessage =  "El precio del funko es obligatorio")]
    [Range(1, 10000, ErrorMessage = "El precio debe ser mayor a 0 con limite de 10000 €")]
    [NoLink]
    [SwaggerSchema(Description = "Precio del funko en euros", Format = "decimal", Nullable = false)]
    public required decimal Precio  { get; set; } = 0;
 
    [IdYoutube]
    [SwaggerSchema(Description = "Idenificador de la categoria", Nullable = false)]
    public required string CategoriaId { get; set; }
    
    [RegularExpression(@"^[0-9a-fA-F]{24}$", ErrorMessage =  "Formato invalido de ID")]
    [SwaggerSchema(Description = "Identificador del proveedor", Nullable = false)]
    public required string ProveedorId { get; set; }
    
    [SwaggerSchema(Description = "Stock disponible del funko", Format = "int", Nullable = false)]
    public required int Stock { get; set; }
    
    [SwaggerSchema(Description = "Imagen del funko", Format = "IFormFile", Nullable = true)]
    public IFormFile? Image { get; set; }
}