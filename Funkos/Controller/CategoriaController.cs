using Funkos.Dto.Categoria;
using Funkos.EjemplosSwagger.CategoriaSwagger;
using Funkos.Errors;
using Funkos.Helper;
using Funkos.Service.Categoria;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;

namespace Funkos.Controller;


[ApiController]
[Route("api/[controller]")]
[SwaggerTag("Gestion de categorias de funkos")]
public class CategoriaController(ICategoriaService service, ILogger<CategoriaController> logger) : ControllerBase
{
    /// <summary>
    /// Obtener todos las categorias
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    [EnableRateLimiting("api")]
    [SwaggerOperation(
        Summary = "Obtener todos las categorias",
        Description = "Devuelve una lista de categorias",
        OperationId = "GetAllAsync",
        Tags = new[] { "Categorias" }
    )]
    [SwaggerResponse(200, "Lista de categorias", typeof(List<CategoriaDtoResponse>))]
    [SwaggerResponse(204, "No hay categorias")]
    [SwaggerResponse(404, "No se ha podido realizar la acción")]
    public async Task<ActionResult<List<CategoriaDtoResponse>>> GetAllAsync(CancellationToken cancellationToken)
    {
        var resultado = await service.GetAllAsync(cancellationToken);
        if (!resultado.IsSuccess)
        {
            return resultado.Error.ToHttpResult<List<CategoriaDtoResponse>>();
        }
        
        if (resultado.Value.Count <= 0) return Ok("Aún no hay Categorias");
        return resultado.IsSuccess
            ? Ok(resultado.Value)
            : resultado.Error.ToHttpResult<List<CategoriaDtoResponse>>();
    }

    [HttpGet("{id}")]
    [Authorize(Policy = "RequireAdmin")]
    [SwaggerOperation(
        Summary = "Obtener una única categoria",
        Description = "Devuelve una categoria",
        OperationId = "GetByIdAync",
        Tags = new[] { "Categorias" }
    )]
    [SwaggerResponse(200, "Información de la categoria y sus funkos asociados", typeof(CategoriaWithFunkosResponseDto))]
    [SwaggerResponse(404, "Id no encontrado")]
    [SwaggerResponse(404, "Categoria no encontrado")]
    public async Task<ActionResult<CategoriaWithFunkosResponseDto>> GetByIdAync([FromRoute, SwaggerRequestBody("ID de la categoria", Required = true)] string id, CancellationToken cancellationToken)
    {
        
        var resultado = await service.GetByIdAsync(id, cancellationToken);

        var actionLinks = CategoriaLinkHelper.GenerateLinks(id, "/api/categorias");
        Response.Headers.Append("Link", actionLinks);

        return resultado.IsSuccess
            ? Ok(resultado.Value)
            : resultado.Error.ToHttpResult<CategoriaWithFunkosResponseDto>();
    }

    [HttpPost]
    [Authorize(Policy = "RequireAdmin")]
    [SwaggerOperation(
        Summary = "Cración de nueva categoria",
        Description = "Devuelve la categoria creada",
        OperationId = "CreateAsync",
        Tags = new[] { "Categorias" }
    )]
    [SwaggerRequestExample(typeof(CategoriaCreateDto), typeof(CreateCategoriaExample))] // <-- Ejemplo añadido
    [SwaggerResponse(200, "Información de la categoria creada", typeof(CategoriaDtoResponse))]
    [SwaggerResponse(400, "Errores de validación")]
    [SwaggerResponse(404, "Categoria no creada")]
    [SwaggerResponse(409, "Categoria con nombre que ya existe")]
    public async Task<ActionResult<CategoriaDtoResponse>> CreateAsync([FromBody, SwaggerRequestBody("Datos de la categoria nueva")] CategoriaCreateDto dto, CancellationToken cancellationToken)
    {
        var resultado = await service.AddAsync(dto, cancellationToken);
        if (resultado.IsFailure)
        {
            return resultado.Error.ToHttpResult<CategoriaDtoResponse>();
        }

        //HETOAS
        var actionLinks = CategoriaLinkHelper.GenerateLinks(resultado.Value.IdY, "/api/categorias");
        Response.Headers.Append("Link", actionLinks);

        return resultado.IsSuccess
            ? CreatedAtAction(nameof(GetByIdAync), new { id = resultado.Value.IdY }, resultado.Value)
            : resultado.Error.ToHttpResult<CategoriaDtoResponse>();
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "RequireAdmin")]
    [SwaggerOperation(
        Summary = "Actualización de datos de una categoria",
        Description = "Devuelve los datos de la categoria actualizada",
        OperationId = "UpdateAsync",
        Tags = new[] { "Categorias" }
    )]
    [SwaggerRequestExample(typeof(CategoriaUpdateDto), typeof(UpdateCategoriaExample))] // <-- Ejemplo añadido
    [SwaggerResponse(200, "Datos de la categoria actualizada", typeof(CategoriaDtoResponse))]
    [SwaggerResponse(400, "Errores de validación")]
    [SwaggerResponse(404, "No se encontro la categoria")]
    [SwaggerResponse(404, "No se guardo la categoria")]
    [SwaggerResponse(409, "Categoria con nombre que ya existe")]
    [SwaggerResponse(409, "Conflicto de concurrencia")]
    public async Task<ActionResult<CategoriaDtoResponse>> UpdateAsync([FromRoute, SwaggerParameter("ID de la categoria", Required = true)] string id,
        [FromBody, SwaggerRequestBody("Datos actualizados de la categoria", Required = true)] CategoriaUpdateDto dto, CancellationToken cancellationToken)
    {
        var actualizado = await service.UpdateAsync(id, dto, cancellationToken);
        //HETOAS
        var actionLinks = CategoriaLinkHelper.GenerateUpdateLinks(id, "/api/categorias");
        Response.Headers.Append("Link", actionLinks);
        return actualizado.IsSuccess
            ? Ok(actualizado.Value)
            : actualizado.Error.ToHttpResult<CategoriaDtoResponse>();

    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "RequireAdmin")]
    [SwaggerOperation(
        Summary = "Eliminación de una categoria",
        Description = "Devuelve not content",
        OperationId = "DeleteAsync",
        Tags = new[] { "Categorias" }
    )]
    [SwaggerResponse(204, "Categoria eliminada", typeof(bool))]
    [SwaggerResponse(400, "Categoria nula")]
    [SwaggerResponse(422, "La categoria tiene funkos asociados")]
    [SwaggerResponse(404, "No se pudo realiazar la acción")]
    [SwaggerResponse(409, "Conflicto de concurrencia")]
    public async Task<ActionResult<bool>> DeleteAsync([FromRoute, SwaggerParameter("ID de la categoria", Required = true)] string id, CancellationToken cancellationToken)
    {
        var deleted = await service.DeleteAsync(id, cancellationToken);
        //HETOAS de Delete
        var actionLinks = CategoriaLinkHelper.GenerateDeleteLinks(id, "/api/categorias");
        Response.Headers.Append("Link", actionLinks);
        if (deleted.IsFailure)
        {
            return deleted.Error.ToHttpResult<bool>();
        }

        return NoContent();
    }
}