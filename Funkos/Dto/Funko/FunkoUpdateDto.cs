using System.ComponentModel.DataAnnotations;
using Funkos.idGenerators;
using Funkos.Models;
using Funkos.Validations;
using Swashbuckle.AspNetCore.Annotations;

namespace Funkos.Dto;

[SwaggerSchema(Description = "Datos para actualizar un funko")]
public record FunkoUpdateDto
{
    public long Id { get; set; }
    
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
    
    [Range(1, int.MaxValue, ErrorMessage = "Inserte un stock mayor que 0")]
    [SwaggerSchema(Description = "Stock disponible del funko", Format = "int", Nullable = false)]
    public required int Stock { get; set; }
    
    [MaxLength(500, ErrorMessage = "La URL de la imagen no puede exceder 500 caracteres")]
    [Url(ErrorMessage = "Debe ser una URL válida")]
    [SwaggerSchema(Description = "Imagen del funko", Format = "IFormFile", Nullable = true)]
    public IFormFile? Image { get; set; }
}
[SwaggerSchema(Description = "Datos para actualizar un funko parcialmente")]
public record FunkoPatchDto
{
    [SwaggerSchema(Description = "Identificador del funko", Nullable = false)]
    public long Id { get; set; }
    
    [StringLength(100, MinimumLength = 1, ErrorMessage = "El nombre del Funko no puede superar los 100 caracteres y debe ser superior a 1")]
    [NoLink]
    [SwaggerSchema(Description = "Nombre del funko", Nullable = true)]
    public string? Name { get; set; } 
    
    [Range(1, 10000, ErrorMessage = "El precio debe ser mayor a 0 con limite de 10000 €")]
    [NoLink]
    [SwaggerSchema(Description = "Precio del funko en euros", Format = "decimal", Nullable = true)]
    public decimal? Precio  { get; set; } 
    
    [IdYoutube]
    [SwaggerSchema(Description = "Idenificador de la categoria", Nullable = true)]
    public string? CategoriaId { get; set; }
    
    [RegularExpression(@"^[0-9a-fA-F]{24}$", ErrorMessage =  "Formato invalido de ID")]
    [SwaggerSchema(Description = "Identificador del proveedor", Nullable = true)]
    public string? ProveedorId { get; set; }
    
    [Range(1, int.MaxValue, ErrorMessage = "Inserte un stock mayor que 0")]
    [SwaggerSchema(Description = "Stock disponible del funko", Format = "int", Nullable = true)]
    public int? Stock { get; set; }
    
    [SwaggerSchema(Description = "Imagen del funko", Format = "IFormFile", Nullable = true)]
    public IFormFile? Image { get; set; }
}