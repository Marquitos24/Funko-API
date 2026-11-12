using FluentAssertions;
using Funkos.Dto;
using Funkos.Dto.Funko;
using Funkos.Errors;
using Funkos.GraphQL.Mutations;
using Funkos.GraphQL.Types;
using Funkos.Service;
using HotChocolate;
using HotChocolate.Execution;
using HotChocolate.Subscriptions;
using HotChocolate.Types;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Resu = CSharpFunctionalExtensions;

namespace Funkos.Test.GraphQL.Mutations;

[TestFixture]
public class TiendaMutationTests
{
   private IRequestExecutor _executor = null!;
    private Mock<IFunkoService> _funkoServiceMock = null!;
    private Mock<ITopicEventSender> _eventSenderMock = null!;

    [SetUp]
    public async Task SetUp()
    {
        _funkoServiceMock = new Mock<IFunkoService>();
        _eventSenderMock = new Mock<ITopicEventSender>();

        _eventSenderMock
            .Setup(s => s.SendAsync(It.IsAny<string>(), It.IsAny<object?>(), It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        var services = new ServiceCollection();
        services.AddSingleton(_funkoServiceMock.Object);
        services.AddSingleton(_eventSenderMock.Object);

        _executor = await services
            .AddGraphQL()
            .AddMutationType<TiendaMutation>()
            .AddType<FunkoType>()
            .AddType<CategoriaType>()
            .AddType<ProveedorType>()
            .AddType<UploadType>()
            .BuildRequestExecutorAsync();
    }

    [Test]
    public async Task CrearProducto_DeberiaRetornarDtoYNotificar_CuandoDatosSonValidos()
    {
        // Arrange
        var responseDto = new FunkosDtoResponse(
            Id: 10L,
            Name: "Naruto Uzumaki",
            Precio: 24.50m,
            CategoriaId: "CAT02",
            ProveedorId: "650f1f1f1f1f1f1f1f1f1f1f",
            Stock: 20,
            image: null,
            CreateAt: DateTime.UtcNow,
            UpdateAt: null,
            DeleteAt: null
        );

        _funkoServiceMock
            .Setup(s => s.AddAsync(It.IsAny<FunkoCreateDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Resu.Result.Success<FunkosDtoResponse, DomainErrors>(responseDto));
        string mutation = @"
            mutation {
                crearProducto(input: {
                    name: ""Naruto Uzumaki"",
                    precio: 24.50,
                    categoriaId: ""CAT02"",
                    proveedorId: ""650f1f1f1f1f1f1f1f1f1f1f"",
                    stock: 20
                }) {
                    id
                    name
                    precio
                }
            }
        ";

        // Act
        var result = await _executor.ExecuteAsync(mutation);

        // Assert
        result.Should().NotBeNull();
        var jsonResult = result.ToJson();
        jsonResult.Should().Contain("Naruto Uzumaki");
        jsonResult.Should().Contain("24.50");

        _eventSenderMock.Verify(s => s.SendAsync("FunkoCreado", It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task CrearProducto_DeberiaLanzarGraphQLException_CuandoServicioFalla()
    {
        // Arrange
        var domainError = ConflictError.Duplicate("Funko", "Naruto Uzumaki");
        _funkoServiceMock
            .Setup(s => s.AddAsync(It.IsAny<FunkoCreateDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Resu.Result.Failure<FunkosDtoResponse, DomainErrors>(domainError));

        string mutation = @"
            mutation {
                crearProducto(input: {
                    name: ""Naruto Uzumaki"",
                    precio: 24.50,
                    categoriaId: ""CAT02"",
                    proveedorId: ""650f1f1f1f1f1f1f1f1f1f1f"",
                    stock: 20
                }) {
                    id
                }
            }
        ";

        // Act
        var result = await _executor.ExecuteAsync(mutation);

        // Assert
        result.Should().NotBeNull();
        var jsonResponse = result.ToJson();
        jsonResponse.Should().Contain("errors");
        jsonResponse.Should().Contain("Ya existe");
    }

    [Test]
    public async Task ActualizarFunko_DeberiaRetornarDtoActualizado_CuandoExiste()
    {
        // Arrange
        long funkoId = 1L;
        var responseDto = new FunkosDtoResponse(
            Id: funkoId,
            Name: "Kakashi Hatake (Updated)",
            Precio: 30.00m,
            CategoriaId: "CAT02",
            ProveedorId: "650f1f1f1f1f1f1f1f1f1f1f",
            Stock: 8,
            image: null,
            CreateAt: DateTime.UtcNow.AddDays(-1),
            UpdateAt: DateTime.UtcNow,
            DeleteAt: null
        );

        _funkoServiceMock
            .Setup(s => s.UpdatePartialAsync(funkoId, It.IsAny<FunkoPatchDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Resu.Result.Success<FunkosDtoResponse, DomainErrors>(responseDto));

        string mutation = $@"
            mutation {{
                actualizarFunko(id: {funkoId}, input: {{
                    name: ""Kakashi Hatake (Updated)"",
                    precio: 30.00
                }}) {{
                    id
                    name
                    precio
                }}
            }}
        ";

        // Act
        var result = await _executor.ExecuteAsync(mutation);

        // Assert
        result.Should().NotBeNull();
        var jsonResult = result.ToJson();
        jsonResult.Should().Contain("Kakashi Hatake (Updated)");
        
        _eventSenderMock.Verify(s => s.SendAsync("FunkoActualizado", It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task EliminarFunko_DeberiaRetornarTrue_CuandoSeEliminaCorrectamente()
    {
        // Arrange
        long funkoId = 5L;
        _funkoServiceMock
            .Setup(s => s.DeleteAsync(funkoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Resu.Result.Success<bool, DomainErrors>(true));

        string mutation = $@"
            mutation {{
                eliminarFunko(id: {funkoId})
            }}
        ";

        // Act
        var result = await _executor.ExecuteAsync(mutation);

        // Assert
        result.Should().NotBeNull();
        var jsonResult = result.ToJson();
        jsonResult.Should().Contain("true");

        _eventSenderMock.Verify(s => s.SendAsync("ProductoEliminado", funkoId, It.IsAny<CancellationToken>()), Times.Once);
    }
}