using System.ComponentModel.DataAnnotations;
using Funkos.idGenerators;
using Funkos.Models;
using Funkos.Validations;
using Swashbuckle.AspNetCore.Annotations;

namespace Funkos.Dto.Funko;

[SwaggerSchema(Description = "Filtros para obtener una lista de funkos")]
public record FunkoFilterDto
{
    [StringLength(100, MinimumLength = 1, ErrorMessage = "El nombre del Funko no puede superar los 100 caracteres y debe ser superior a 1")]
    [NoLink]
    [SwaggerSchema(Description = "Nombre del funko", Nullable = true)]
    public string? Name { get; init; } 
    
    [Range(1, 10000, ErrorMessage = "El precio maximo debe ser mayor a 0 con limite de 10000 €")]
    [SwaggerSchema(Description = "Precio maximos del funko en euros", Format = "decimal", Nullable = true)]
    public decimal? PrecioMax  { get; init; } 
    
    [Range(1, 10000, ErrorMessage = "El precio minimo debe ser mayor a 0 €")]
    [SwaggerSchema(Description = "Precio minimo del funko en euros", Format = "decimal", Nullable = true)]
    public decimal? PrecioMin  { get; init; } 
    
    [StringLength(200, MinimumLength = 1, ErrorMessage = "La categoria del Funko no puede superar los 200 caracteres y debe ser superior a 1")]
    [SwaggerSchema(Description = "Idenificador de la categoria", Nullable = true)]
    public string? Categoria { get; init; }
    
    [SwaggerSchema(Description = "Marcador de borrado logico de funko", Format = "bool", Nullable = true)]
    public bool? IsDeleted { get; init; }

    [Range(1, 10000000000000, ErrorMessage = "El precio debe ser mayor a 0")]
    [SwaggerSchema(Description = "Stock minimo disponible del funko", Format = "int", Nullable = true)]
    public int? StockMin { get; init; } 

    [Required (ErrorMessage =  "El número de pagina es obligatorio")]
    [Range(0, int.MaxValue, ErrorMessage = "La página no puede ser negativa")]
    [SwaggerSchema(Description = "Número de pagina", Format = "int", Nullable = true)]
    public int Page { get; init; } = 0;

    [Required (ErrorMessage =  "El número de elementos en la pagina es obligatorio")]
    [Range(1, 100, ErrorMessage = "El tamaño de página debe estar entre 1 y 100")]
    [SwaggerSchema(Description = "Número de funkos en una pagina", Format = "int", Nullable = true)]
    public int Size { get; init; } = 10;
    
    // <summary>
    // Campo para ordenar resultados.
    // </summary>
    // <default>id</default>
    // <example>precio</example>
    public string SortBy = "id";

    // <summary>
    // Dirección de ordenamiento (asc/desc).
    // </summary>
    // <default>asc</default>
    // <example>desc</example>
    public string Direction = "asc";
}