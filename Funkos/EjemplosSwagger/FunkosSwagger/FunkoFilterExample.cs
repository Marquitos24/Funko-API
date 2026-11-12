using Funkos.Dto.Funko;
using Swashbuckle.AspNetCore.Filters;

namespace Funkos.EjemplosSwagger.FunkosSwagger;

public class FunkoFilterExample : IExamplesProvider<FunkoFilterDto>
{
    public FunkoFilterDto GetExamples()
    {
        return new FunkoFilterDto
        {
            Name = "Iron Man",
            PrecioMin = 10.00m,
            PrecioMax = 50.00m,
            Page = 0,
            Size = 10,
            SortBy = "precio",
            Direction = "asc"
        };
    }
}