using System.ComponentModel.DataAnnotations;
using Funkos.Validations;
using Swashbuckle.AspNetCore.Annotations;

namespace Funkos.Dto.Categoria;

[SwaggerSchema(Description = "Datos para crear una categoria")]
public record CategoriaCreateDto
{
    [Required (ErrorMessage =  "El nombre de la categoria es obligatoria")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "El nombre de la categoria no puede superar los 200 caracteres y debe ser superior a 1")]
    [NoLink]
    [SwaggerSchema(Description = "Nombre de la categoria", Nullable = false)]
    public required string Name { get; set; }
    
    [Required (ErrorMessage =  "La descripción de la categoria es obligatoria")]
    [StringLength(300, MinimumLength = 1, ErrorMessage = "La descripción de la categoria no puede superar los 300 caracteres y debe ser superior a 1")]
    [NoLink]
    [SwaggerSchema(Description = "Descripción de la categoria", Nullable = false)]
    public required string Descripcion  { get; set; }
}