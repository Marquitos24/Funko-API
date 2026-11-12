using Funkos.Dto.Proveedor;
using Swashbuckle.AspNetCore.Filters;

namespace Funkos.EjemplosSwagger.ProveedorSwagger;

public class CreateProveedorExample: IExamplesProvider<ProveedorCreateDto>
{
    public ProveedorCreateDto GetExamples()
    {
        return new ProveedorCreateDto
        {
            Name = "Funko Distribution Europe",
            Descripcion = "Distribuidor oficial de figuras coleccionables para la región de Europa"
        };
    }
}