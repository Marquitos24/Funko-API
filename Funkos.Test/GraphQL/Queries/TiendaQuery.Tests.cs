using FluentAssertions;
using Funkos.DbContext;
using Funkos.Dto.Categoria;
using Funkos.Dto.Funko;
using Funkos.Errors;
using Funkos.GraphQL.Queries;
using Funkos.GraphQL.Types;
using Funkos.Models;
using Funkos.Service;
using Funkos.Service.Categoria;
using HotChocolate;
using HotChocolate.Execution;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Funkos.Test.GraphQL.Queries;

[TestFixture]
public class TiendaQueryTests
{
    private IRequestExecutor _executor = null!;
    private Mock<IFunkoService> _funkoServiceMock = null!;
    private AppDbContext _dbContext = null!;

    [SetUp]
    public async Task SetUp()
    {
        _funkoServiceMock = new Mock<IFunkoService>();

        // Configuramos EF Core InMemory para las consultas de IQueryable (GetFunkosPaged / GetCategorias)
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _dbContext = new AppDbContext(options);

        var services = new ServiceCollection();
        services.AddSingleton(_funkoServiceMock.Object);
        services.AddSingleton(_dbContext);

        _executor = await services
            .AddGraphQL()
            .AddQueryType<TiendaQuery>()
            .AddType<FunkoType>()
            .AddType<CategoriaType>()
            .AddType<ProveedorType>()
            .BuildRequestExecutorAsync();
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    [Test]
    public async Task GetFunko_DeberiaDevolverDto_CuandoElFunkoExiste()
    {
        // Arrange
        long funkoId = 1L;
        var dtoResponse = new FunkosDtoResponse(
            Id: funkoId,
            Name: "Goku Super Saiyan",
            Precio: 29.99m,
            CategoriaId: "CAT01",
            ProveedorId: "650f1f1f1f1f1f1f1f1f1f1f",
            Stock: 15,
            image: "https://example.com/goku.jpg",
            CreateAt: DateTime.UtcNow,
            UpdateAt: null,
            DeleteAt: null
        );

        _funkoServiceMock
            .Setup(s => s.GetByIdAsync(funkoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(dtoResponse);

        string query = @"{
            getFunko(id: 1) {
                id
                name
                precio
                stock
                image
            }
        }";

        // Act
        var result = await _executor.ExecuteAsync(query);

        // Assert
        result.Should().NotBeNull();
        var jsonResult = result.ToJson();
        jsonResult.Should().Contain("Goku Super Saiyan");
        jsonResult.Should().Contain("29.99");
    }

    [Test]
    public async Task GetFunko_DeberiaDevolverNull_CuandoElFunkoNoExiste()
    {
        // Arrange
        long funkoId = 999L;
        _funkoServiceMock
            .Setup(s => s.GetByIdAsync(funkoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((FunkosDtoResponse?)null);

        string query = @"{
            getFunko(id: 999) {
                id
                name
            }
        }";

        // Act
        var result = await _executor.ExecuteAsync(query);

        // Assert
        result.Should().NotBeNull();
        var jsonResult = result.ToJson();
        jsonResult.Should().Contain("\"getFunko\": null");
    }

    [Test]
    public async Task GetFunkosPaged_DeberiaRetornarListaDeFunkos_DesdeDbContext()
    {
        // Arrange
        _dbContext.Funkos.Add(new Funko
        {
            Id = 1,
            Name = "Vegeta",
            Precio = 35.00m,
            CategoriaId = "CAT01",
            ProveedorId = "650f1f1f1f1f1f1f1f1f1f1f",
            Stock = 5,
            Imagen = "url",
            CreateAt = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();

        string query = @"{
            funkosPaged {
                nodes {
                    id
                    name
                    precio
                }
            }
        }";

        // Act
        var result = await _executor.ExecuteAsync(query);

        // Assert
        result.Should().NotBeNull();
        var jsonResult = result.ToJson();
        jsonResult.Should().Contain("Vegeta");
    }
}