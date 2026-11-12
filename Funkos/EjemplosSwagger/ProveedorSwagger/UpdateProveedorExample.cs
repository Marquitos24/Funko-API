using Funkos.Dto.Proveedor;
using Swashbuckle.AspNetCore.Filters;

namespace Funkos.EjemplosSwagger.ProveedorSwagger;

public class UpdateProveedorExample : IExamplesProvider<ProveedorUpdateDto>
{
    public ProveedorUpdateDto GetExamples()
    {
        return new ProveedorUpdateDto
        {
            Id = MongoDB.Bson.ObjectId.GenerateNewId(),
            Name = "Funko Global Distribution S.L.",
            Descripcion = "Distribuidor global actualizado con soporte logístico internacional"
        };
    }
}