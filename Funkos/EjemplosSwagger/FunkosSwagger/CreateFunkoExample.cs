using Funkos.Dto.Funko;
using Swashbuckle.AspNetCore.Filters;

namespace Funkos.EjemplosSwagger.FunkosSwagger;

public class CreateFunkoExample : IExamplesProvider<FunkoCreateDto>
{
    public FunkoCreateDto GetExamples()
    {
        return new FunkoCreateDto
        {
            Name = "Iron Man Mark 85",
            Precio = 24.99m,
            CategoriaId = "drUKZ2KCd7J",
            ProveedorId = "64a2f1e4bcf1a2b3c4d5e6f7", // ObjectId válido de MongoDB como string
            Stock = 15,
            Image = null // En formularios multipart/form-data el archivo de ejemplo se prueba directamente en Swagger UI
        };
    }
}