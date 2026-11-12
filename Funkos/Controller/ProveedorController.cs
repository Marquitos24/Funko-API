using Funkos.Dto.Proveedor;
using Funkos.EjemplosSwagger.ProveedorSwagger;
using Funkos.Errors;
using Funkos.Helper;
using Funkos.Service.Proveedor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;

namespace Funkos.Controller;

[ApiController]
[Route("api/[controller]")]
[SwaggerTag("Gestion de proveedores")]
public class ProveedorController(IProveedorService service) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = "RequireAdmin")]
    [SwaggerOperation(
        Summary = "Obtener todos los proveedores",
        Description = "Devuelve una lista de proveedores",
        OperationId = "GetAllAsync",
        Tags = new[] { "Proveedor" }
    )]
    [SwaggerResponse(200, "Lista de proveedores", typeof(IEnumerable<ProveedorDtoResponse>))]
    [SwaggerResponse(204, "No hay proveedores")]
    [SwaggerResponse(404, "No se ha podido realizar la acción")]
    public async Task<ActionResult<IEnumerable<ProveedorDtoResponse>>> GetAllAsync(CancellationToken cancellationToken)
    {
       var resultado = await service.GetAllAsync(cancellationToken);
       if (resultado.IsSuccess && !resultado.Value.Any()) return Ok("Aún no hay Proveedores");
       
       return resultado.IsSuccess 
           ? Ok(resultado.Value)
           : resultado.Error.ToHttpResult<IEnumerable<ProveedorDtoResponse>>();
    }
    
    [HttpGet("{id}")]
    [Authorize(Policy = "RequireAdmin")]
    [SwaggerOperation(
        Summary = "Obtener un único proveedor",
        Description = "Devuelve los datos de un proveedor",
        OperationId = "GetByIdAync",
        Tags = new[] { "Proveedor" }
    )]
    [SwaggerResponse(200, "Datos del proveedor", typeof(ProveedorDtoResponse))]
    [SwaggerResponse(404, "Id no encontrado")]
    [SwaggerResponse(404, "Proveedor no encontrado")]
    public async Task<ActionResult<ProveedorDtoResponse>> GetByIdAync([FromRoute, SwaggerRequestBody("ID del proveedor", Required = true)] string id, CancellationToken cancellationToken)
    {
        if (!ObjectId.TryParse(id, out var objectId))
            return BadRequest("El identificador no tiene un formato válido.");
        
        var resultado = await service.GetByIdAsync(objectId, cancellationToken);
                
        var actionLinks = ProveedorLinkHelper.GenerateLinks(id,"/api/proveedor");
        Response.Headers.Append("Link", actionLinks);
        
        return resultado.IsSuccess 
            ? Ok(resultado.Value)
            : resultado.Error.ToHttpResult<ProveedorDtoResponse>();
    }
    
    [HttpPost]
    [Authorize(Policy = "RequireAdmin")]
    [SwaggerOperation(
        Summary = "Cración de nuevo proveedor",
        Description = "Devuelve el proveedor creado",
        OperationId = "CreateAsync",
        Tags = new[] { "Proveedor" }
    )]
    [SwaggerRequestExample(typeof(ProveedorCreateDto), typeof(CreateProveedorExample))] // <-- Ejemplo añadido
    [SwaggerResponse(200, "Proveedor creado correctamente", typeof(ProveedorDtoResponse))]
    [SwaggerResponse(400, "Errores de validación")]
    [SwaggerResponse(404, "Proveedor no creada")]
    [SwaggerResponse(409, "Proveedor con nombre que ya existe")]
    public async Task<ActionResult<ProveedorDtoResponse>> CreateAsync([FromBody, SwaggerRequestBody("Datos del nuevo proveedor")] ProveedorCreateDto dto, CancellationToken cancellationToken)
    {
        var resultado = await service.AddAsync(dto, cancellationToken);
        if (resultado.IsFailure) { return resultado.Error.ToHttpResult<ProveedorDtoResponse>(); }
        //HETOAS
        var id = resultado.Value.Id;
        
        var actionLinks = ProveedorLinkHelper.GenerateLinks(id,"/api/proveedor");
        Response.Headers.Append("Link", actionLinks);
        
            return resultado.IsSuccess 
                ? CreatedAtAction(nameof(GetByIdAync), new { id}, resultado.Value)
                : resultado.Error.ToHttpResult<ProveedorDtoResponse>();
    }
    
    [HttpPut("{id}")]
    [Authorize(Policy = "RequireAdmin")]
    [SwaggerOperation(
        Summary = "Actualización de datos de un proveedor",
        Description = "Devuelve los datos de un proveedor actualizado",
        OperationId = "UpdateAsync",
        Tags = new[] { "Proveedor" }
    )]
    [SwaggerRequestExample(typeof(ProveedorUpdateDto), typeof(UpdateProveedorExample))] // <-- Ejemplo añadido
    [SwaggerResponse(200, "Proveedor actualizado correctamente", typeof(ProveedorDtoResponse))]
    [SwaggerResponse(400, "Errores de validación")]
    [SwaggerResponse(404, "No se encontro el proveedor")]
    [SwaggerResponse(409, "Proveedor con nombre que ya existe")]
    [SwaggerResponse(409, "Conflicto de concurrencia")]
    public async Task<ActionResult<ProveedorDtoResponse>> UpdateAsync([FromRoute, SwaggerParameter("ID del proveedor", Required = true)] string id,
    [FromBody, SwaggerRequestBody("Datos actualizados del proveedor", Required = true)] ProveedorUpdateDto dto, CancellationToken cancellationToken)
    {
        if (!ObjectId.TryParse(id, out var objectId))
            return BadRequest("El identificador no tiene un formato válido.");
        
        var actualizado = await service.UpdateAsync(objectId, dto, cancellationToken);
        //HETOAS
        var actionLinks = ProveedorLinkHelper.GenerateUpdateLinks(id,"/api/proveedor");
        Response.Headers.Append("Link", actionLinks);
        return actualizado.IsSuccess 
            ? Ok(actualizado.Value)
            : actualizado.Error.ToHttpResult<ProveedorDtoResponse>();
    }
    
    [HttpDelete("{id}")]
    [Authorize(Policy = "RequireAdmin")]
    [SwaggerOperation(
        Summary = "Eliminación de un proveedor",
        Description = "Devuelve not content",
        OperationId = "DeleteAsync",
        Tags = new[] { "Proveedor" }
    )]
    [SwaggerResponse(204, "Proveedor eliminado correctamente", typeof(bool))]
    [SwaggerResponse(400, "Proveedor nula")]
    [SwaggerResponse(422, "el proveedor tiene funkos asociados")]
    [SwaggerResponse(404, "No se pudo realiazar la acción")]
    [SwaggerResponse(409, "Conflicto de concurrencia")]
    public async Task<ActionResult<bool>> DeleteAsync([FromRoute, SwaggerParameter("ID del proveedor", Required = true)] string id, CancellationToken cancellationToken)
    {
        if (!ObjectId.TryParse(id, out var objectId))
            return BadRequest("El identificador no tiene un formato válido.");
        
        var deleted = await service.DeleteAsync(objectId, cancellationToken);
        //HETOAS de Delete
        var actionLinks = ProveedorLinkHelper.GenerateDeleteLinks(id,"/api/proveedor");
        Response.Headers.Append("Link", actionLinks);
        if (deleted.IsFailure) { return deleted.Error.ToHttpResult<bool>(); }
        return NoContent();
    }
}