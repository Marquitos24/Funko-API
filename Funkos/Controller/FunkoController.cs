using Funkos.Dto.Funko;
using Funkos.Service;
using Microsoft.AspNetCore.Mvc;
using Funkos.Dto;
using Funkos.EjemplosSwagger.FunkosSwagger;
using Funkos.Errors;
using Funkos.Helper;
using Funkos.Service.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;

namespace Funkos.Controller;

[ApiController]
[Route("api/[controller]")]
[SwaggerTag("Gestion de funkos")]
public class FunkoController(IFunkoService service, ILogger<FunkoController> _logger)
    : ControllerBase
{
    [HttpGet]
    [EnableRateLimiting("api")]
    [SwaggerOperation(
        Summary = "Obtener todos los funkos",
        Description = "Devuelve una lista paginada de funkos con filtros opcionales",
        OperationId = "GetAllAsync",
        Tags = new[] { "Funkos" }
    )]
    [SwaggerRequestExample(typeof(FunkoFilterDto), typeof(FunkoFilterExample))] 
    [SwaggerResponse(200, "Lista de funkos obtenida", typeof(List<FunkosDtoResponse>))]
    [SwaggerResponse(400, "Parametros invalidos")]
    [SwaggerResponse(404, "No se ha podido realizar la acción")]
    public async Task<ActionResult<List<FunkosDtoResponse>>> GetAllAsync([FromQuery, SwaggerParameter("Filtros opcionales", Required = false)] FunkoFilterDto filter,
        CancellationToken cancellationToken)
    {
        var resultado = await service.FindAllPagedAsync(filter, cancellationToken);
        //if (resultado.Value.Count <= 0) { return Ok("Aún no hay Funkos"); }

        return resultado.IsSuccess
            ? Ok(resultado.Value)
            : resultado.Error.ToHttpResult<List<FunkosDtoResponse>>();
    }

    [HttpGet("{id:long}")]
    [Authorize(Policy = "RequireAdmin")]
    [SwaggerOperation(
        Summary = "Obtener un único funko",
        Description = "Devuelve los datos de un funko",
        OperationId = "GetByIdAync",
        Tags = new[] { "Funkos" }
    )]
    [SwaggerResponse(200, "Datos del funko", typeof(FunkosDtoResponse))]
    [SwaggerResponse(404, "Id no encontrado")]
    [SwaggerResponse(404, "Funko no encontrado")]
    public async Task<ActionResult<FunkosDtoResponse>> GetByIdAync([FromRoute, SwaggerRequestBody("ID del funko", Required = true)] long id,
        CancellationToken cancellationToken)
    {
        var resultado = await service.GetByIdAsync(id, cancellationToken);

        var actionLinks = LinkHelper.GenerateLinks(id, "/api/funkos");
        Response.Headers.Append("Link", actionLinks);

        return resultado.IsSuccess
            ? Ok(resultado.Value)
            : resultado.Error.ToHttpResult<FunkosDtoResponse>();
    }

    [HttpPost]
    [Authorize(Policy = "RequireAdmin")]
    [SwaggerOperation(
        Summary = "Creación de un funko",
        Description = "Devuelve los datos del funko creado",
        OperationId = "CreateAsync",
        Tags = new[] { "Funkos" }
    )]
    [SwaggerRequestExample(typeof(FunkoCreateDto), typeof(CreateFunkoExample))] 
    [SwaggerResponse(201, "Funko creado correctamente", typeof(FunkosDtoResponse))]
    [SwaggerResponse(400, "Errores de validación")]
    [SwaggerResponse(404, "Funko no creadp")]
    [SwaggerResponse(409, "Ya hay un funko con ese nombre")]
    [SwaggerResponse(404, "Error al guardar la imagen")]
    public async Task<ActionResult<FunkosDtoResponse>> CreateAsync([FromForm, SwaggerRequestBody("Datos para crear el funko", Required = true)] FunkoCreateDto dto,
        CancellationToken cancellationToken)
    {
        var resultado = await service.AddAsync(dto, cancellationToken);
        if (resultado.IsFailure)
        {
            return resultado.Error.ToHttpResult<FunkosDtoResponse>();
        }

        //HETOAS
        var actionLinks = LinkHelper.GenerateLinks(resultado.Value.Id, "/api/funkos");
        Response.Headers.Append("Link", actionLinks);

        return resultado.IsSuccess
            ? CreatedAtAction(nameof(GetByIdAync), new { id = resultado.Value.Id }, resultado.Value)
            : resultado.Error.ToHttpResult<FunkosDtoResponse>();
    }

    [HttpPut("{id:long}")]
    [Authorize(Policy = "RequireAdmin")]
    [SwaggerOperation(
        Summary = "Actualización de un funko",
        Description = "Devuelve los datos del funko actualizado",
        OperationId = "UpdateAsync",
        Tags = new[] { "Funkos" }
    )]
    [SwaggerRequestExample(typeof(FunkoUpdateDto), typeof(UpdateFunkoExample))] 
    [SwaggerResponse(200, "Funko creado actualizado correctamente", typeof(FunkosDtoResponse))]
    [SwaggerResponse(400, "Errores de validación")]
    [SwaggerResponse(404, "No se encontro el funko")]
    [SwaggerResponse(404, "No se guardo el funko")]
    [SwaggerResponse(409, "Ya existe un funko con ese nombre")]
    [SwaggerResponse(409, "Conflicto de concurrencia")]
    public async Task<ActionResult<FunkosDtoResponse>> UpdateAsync([FromRoute, SwaggerParameter("ID del funko", Required = true)] long id,
        [FromForm, SwaggerRequestBody("Datos actualizados del funko", Required = true)] FunkoUpdateDto dto,
        CancellationToken cancellationToken)
    {
        var actualizado = await service.UpdateAsync(id, dto, cancellationToken);
        //HETOAS
        var actionLinks = LinkHelper.GenerateUpdateLinks(id, "/api/funkos");
        Response.Headers.Append("Link", actionLinks);
        return actualizado.IsSuccess
            ? Ok(actualizado.Value)
            : actualizado.Error.ToHttpResult<FunkosDtoResponse>();
    }

    [HttpDelete("{id:long}")]
    [Authorize(Policy = "RequireAdmin")]
    [SwaggerOperation(
        Summary = "Eliminación de un funko",
        Description = "Devuelve respuesta vacia",
        OperationId = "DeleteAsync",
        Tags = new[] { "Funkos" }
    )]
    [SwaggerResponse(204, "Funko eliminado", typeof(bool))]
    [SwaggerResponse(400, "Funko nulo")]
    [SwaggerResponse(404, "No se encontro el ID")]
    [SwaggerResponse(404, "No se pudo realiazar la acción")]
    [SwaggerResponse(409, "Conflicto de concurrencia")]
    public async Task<ActionResult<bool>> DeleteAsync([FromRoute, SwaggerParameter("ID del funko", Required = true)] long id, CancellationToken cancellationToken)
    {
        var deleted = await service.DeleteAsync(id, cancellationToken);
        //HETOAS de Delete
        var actionLinks = LinkHelper.GenerateDeleteLinks(id, "/api/funkos");
        Response.Headers.Append("Link", actionLinks);
        if (deleted.IsFailure)
        {
            return deleted.Error.ToHttpResult<bool>();
        }

        return NoContent();
    }
}