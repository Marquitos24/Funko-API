using Funkos.Dto.Categoria;
using Swashbuckle.AspNetCore.Filters;

namespace Funkos.EjemplosSwagger.CategoriaSwagger;

public class CreateCategoriaExample: IExamplesProvider<CategoriaCreateDto>
{
    public CategoriaCreateDto GetExamples()
    {
        return new CategoriaCreateDto
        {
            Name = "Marvel",
            Descripcion = "Figuras coleccionables de superhéroes del universo Marvel"
        };
    }
}

