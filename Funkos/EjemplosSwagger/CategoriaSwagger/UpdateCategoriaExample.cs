using Funkos.Dto.Categoria;
using Swashbuckle.AspNetCore.Filters;

namespace Funkos.EjemplosSwagger.CategoriaSwagger;

public class UpdateCategoriaExample : IExamplesProvider<CategoriaUpdateDto>
{
    public CategoriaUpdateDto GetExamples()
    {
        return new CategoriaUpdateDto
        {
            IdY = "drUKZ2KCd7J",
            Name = "Marvel & Avengers",
            Descripcion = "Figuras coleccionables actualizadas del universo cinematográfico de Marvel"
        };
    }
}