using Funkos.Dto;
using Swashbuckle.AspNetCore.Filters;

namespace Funkos.EjemplosSwagger.FunkosSwagger;

public class UpdateFunkoExample : IExamplesProvider<FunkoUpdateDto>
{
    public FunkoUpdateDto GetExamples()
    {
        return new FunkoUpdateDto
        {
            Id = 1,
            Name = "Iron Man Mark 85 (Battle Damaged)",
            Precio = 29.99m,
            CategoriaId = "drUKZ2KCd7J",
            ProveedorId = "64a2f1e4bcf1a2b3c4d5e6f7",
            Stock = 10,
            Image = null
        };
    }
}