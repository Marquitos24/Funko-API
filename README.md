# Funkos API

API backend para gestionar una tienda de figuras Funko. El proyecto combina una API REST y una API GraphQL, autenticación mediante JWT, persistencia poliglota (PostgreSQL y MongoDB), caché distribuida con Redis, notificaciones en tiempo real y almacenamiento de imágenes en el sistema de archivos.

> **Estado de la documentación:** este README se ha elaborado a partir de la estructura y el código fuente facilitados. Describe lo que aparece implementado en el proyecto. Los valores de configuración, credenciales y direcciones deben adaptarse al entorno donde se despliegue.

---

## Índice

- [1. Descripción general](#1-descripción-general)
- [2. Tecnologías](#2-tecnologías)
- [3. Arquitectura](#3-arquitectura)
- [4. Organización de la solución](#4-organización-de-la-solución)
- [5. Persistencia y almacenamiento de datos](#5-persistencia-y-almacenamiento-de-datos)
- [6. Caché con Redis](#6-caché-con-redis)
- [7. Autenticación y autorización](#7-autenticación-y-autorización)
- [8. API REST](#8-api-rest)
- [9. API GraphQL](#9-api-graphql)
- [10. Comunicación en tiempo real](#10-comunicación-en-tiempo-real)
  - [10.4. Diseño de notificaciones por email](#104-diseño-de-notificaciones-por-email)
- [11. Gestión de imágenes y archivos](#11-gestión-de-imágenes-y-archivos)
- [12. Validación y gestión de errores](#12-validación-y-gestión-de-errores)
- [13. Seguridad y control del tráfico](#13-seguridad-y-control-del-tráfico)
- [14. Ejecución con Docker Compose](#14-ejecución-con-docker-compose)
- [15. Configuración](#15-configuración)
- [16. Pruebas](#16-pruebas)
- [17. Flujo completo de una petición](#17-flujo-completo-de-una-petición)
- [18. Consideraciones para producción](#18-consideraciones-para-producción)

---

## 1. Descripción general

Funkos API es un backend desarrollado con **ASP.NET Core sobre .NET 10**. Su objetivo es gestionar el catálogo de una tienda de Funkos, incluyendo:

- Funkos: nombre, precio, stock, imagen, categoría y proveedor.
- Categorías: clasificación de los productos.
- Proveedores: información de las empresas que suministran los productos.
- Usuarios: registro, inicio de sesión, roles y permisos.
- Consultas y modificaciones mediante REST y GraphQL.
- Notificaciones de cambios en tiempo real.
- Caché de consultas para reducir accesos repetidos a las bases de datos.

La solución utiliza distintos mecanismos de almacenamiento según el tipo de información. PostgreSQL almacena los datos relacionales y de identidad; MongoDB almacena los proveedores; Redis se utiliza como caché y como backplane de SignalR; las imágenes se guardan en el sistema de archivos.

## 2. Tecnologías

| Tecnología | Uso en el proyecto |
|---|---|
| C# 14 / .NET 10 | Lenguaje y plataforma de desarrollo |
| ASP.NET Core | Servidor HTTP, API REST, middleware e inyección de dependencias |
| Entity Framework Core 10 | ORM y acceso a PostgreSQL |
| PostgreSQL 16 | Datos relacionales, categorías, Funkos e identidad |
| Npgsql | Proveedor de Entity Framework Core para PostgreSQL |
| MongoDB 8 | Persistencia de proveedores |
| MongoDB.Driver | Acceso a MongoDB desde C# |
| Redis 7 | Caché distribuida y backplane de SignalR |
| StackExchange.Redis | Cliente de Redis |
| ASP.NET Core Identity | Gestión de usuarios, contraseñas y roles |
| JWT Bearer | Autenticación mediante tokens |
| Hot Chocolate 16 | Servidor GraphQL |
| SignalR | Comunicación bidireccional y notificaciones en tiempo real |
| FluentValidation | Validación de reglas de entrada |
| CSharpFunctionalExtensions | Resultados de operaciones y errores de dominio |
| Scrutor | Herramientas para decoración y registro de servicios |
| Serilog | Registro estructurado de eventos y errores |
| Swagger / Swashbuckle | Documentación y exploración de la API REST |
| Docker / Docker Compose | Contenedorización y ejecución de servicios |
| NUnit, Moq y FluentAssertions | Pruebas automatizadas |

Las versiones exactas de los paquetes NuGet se encuentran en `Funkos/Funkos.csproj`.

## 3. Arquitectura

El backend sigue una **arquitectura por capas**, separando la entrada HTTP, la lógica de negocio, el acceso a datos y los servicios de infraestructura. No es una arquitectura de microservicios: la API es una aplicación ASP.NET Core que se conecta a varios servicios de almacenamiento y soporte.

### 3.1. Capas principales

```text
Cliente HTTP / Swagger / Cliente GraphQL
                  |
                  v
       Controllers REST / GraphQL
                  |
                  v
             Services
       (lógica de negocio)
                  |
          +-------+-------+
          |               |
          v               v
     Repositories    Storage Service
          |               |
     +----+----+          v
     |         |      Sistema de archivos
     v         v
 PostgreSQL  MongoDB

     Services Decorators
             |
             v
           Redis
```

El esquema representa las responsabilidades principales. GraphQL también dispone de consultas que utilizan directamente `IQueryable` del contexto de Entity Framework para que Hot Chocolate pueda componer filtros, ordenaciones, proyecciones y paginación en la consulta.

### 3.2. Controllers

Los controladores reciben las peticiones HTTP, leen los parámetros y el cuerpo de la petición, aplican la autorización correspondiente y delegan el trabajo en los servicios.

No deberían concentrar la lógica de persistencia ni las reglas de negocio.

Controladores principales:

- `FunkoController`
- `CategoriaController`
- `ProveedorController`
- `AuthIdentityController`

### 3.3. Services

Los servicios contienen la lógica de negocio. Se encargan, entre otras cosas, de:

- Comprobar que los registros existen.
- Ejecutar validaciones.
- Coordinar repositorios y almacenamiento de imágenes.
- Convertir los resultados en DTOs.
- Devolver resultados de éxito o errores de dominio.
- Emitir notificaciones cuando se producen determinadas operaciones.

Existen interfaces como `IFunkoService`, `ICategoriaService` e `IProveedorService`, que permiten desacoplar a los controladores de las implementaciones concretas.

### 3.4. Repositories

Los repositorios encapsulan las operaciones de acceso a los datos.

- `FunkoRepository` y `CategoriaRepository` trabajan con Entity Framework Core y PostgreSQL.
- `ProveedorRepository` trabaja con MongoDB.

Los servicios no necesitan conocer los detalles internos de cada motor de base de datos.

### 3.5. DTOs y mappers

Los DTO (*Data Transfer Objects*) definen los datos que entran y salen de la API. Evitan exponer directamente las entidades de persistencia y permiten tener contratos específicos para crear, actualizar, filtrar y responder.

Los mappers convierten entre entidades y DTOs:

- `FunkoMapper`
- `CategoriaMapper`
- `ProveedorMapper`

### 3.6. Decorator Pattern para la caché

Los servicios se registran mediante decoradores:

- `CachedFunkoServiceDecorator`
- `CachedCategoriaServiceDecorator`
- `CachedProveedorServiceDecorator`

El controlador recibe la interfaz del servicio, pero la implementación registrada añade la lógica de caché antes o después de delegar en el servicio real.

Así se incorpora caché sin introducir esa responsabilidad en los controladores ni mezclarla con la lógica de negocio.

### 3.7. Inyección de dependencias

El registro de servicios y dependencias se centraliza principalmente en:

- `Program.cs`
- `Infrastructure/ApplicationInfraestructure.cs`

Esta separación permite mantener el arranque de la aplicación más limpio y agrupar la configuración de PostgreSQL, MongoDB, Redis, Identity, JWT, GraphQL, SignalR y Swagger.

---

## 4. Organización de la solución

La solución contiene los siguientes proyectos principales:

```text
marquitos24-funko-api/
├── Funkos.slnx
├── compose.yaml
├── Funkos/
│   ├── Controller/
│   ├── Dto/
│   ├── Errors/
│   ├── Exceptions/
│   ├── GraphQL/
│   ├── Helper/
│   ├── idGenerators/
│   ├── Infrastructure/
│   ├── Mappers/
│   ├── Middleware/
│   ├── Migrations/
│   ├── Models/
│   ├── RealTime/
│   ├── Repositories/
│   ├── Service/
│   └── Validations/
├── Funkos.Client/
├── Funkos.Clients/
├── Funko.Client/
└── Funkos.Test/
```

### Proyecto `Funkos`

Es el backend principal. Contiene los controladores, servicios, repositorios, modelos, DTOs, configuración de infraestructura, GraphQL, SignalR y middleware.

### Proyecto `Funkos.Test`

Contiene pruebas de repositorios, servicios, validaciones, mappers, caché, middleware, almacenamiento, GraphQL, SignalR y pruebas de integración de controladores.

### Proyectos cliente

La solución incluye proyectos cliente Blazor WebAssembly y páginas de demostración para consumir GraphQL y SignalR. Son clientes separados del backend: no forman parte de la capa de persistencia ni de la lógica interna de la API.

---

## 5. Persistencia y almacenamiento de datos

La aplicación utiliza persistencia poliglota: combina más de un motor de base de datos para aprovechar las características de cada uno.

### 5.1. PostgreSQL: datos relacionales e identidad

PostgreSQL es la base de datos relacional principal. Se accede mediante Entity Framework Core 10 y el proveedor Npgsql.

El contexto principal es `AppDbContext`, que hereda de `IdentityDbContext<User, Role, long>`.

En PostgreSQL se almacenan:

- **Funkos:** productos de la tienda.
- **Categorías:** clasificación de los productos.
- **Usuarios y roles:** tablas gestionadas por ASP.NET Core Identity.

La relación entre Funkos y categorías es relacional: cada Funko contiene un `CategoriaId` y una navegación a `Categoria`; una categoría puede tener varios Funkos.

El identificador de Funko es de tipo `long`. El identificador de categoría es una cadena `IdY` de longitud 11, generada con el mecanismo propio `YouTubeIdValueGenerator`.

Las migraciones de Entity Framework Core, ubicadas en `Funkos/Migrations`, permiten versionar la estructura de la base de datos y aplicar cambios de esquema.

### 5.2. MongoDB: proveedores

Los proveedores se almacenan en MongoDB, no en PostgreSQL.

La conexión se configura mediante `MongoDbSettings` y el contexto `MongoDbContext`. La colección utilizada es `Proveedores`.

Características:

- Los documentos utilizan `ObjectId` como identificador.
- Se crea un índice único ascendente sobre `Name`.
- El modelo utiliza atributos BSON para definir los nombres de los campos.
- El acceso se realiza mediante `MongoDB.Driver`.
- `ProveedorRepository` encapsula las operaciones sobre la colección.

### 5.3. ¿Cómo se relaciona un Funko con un proveedor de MongoDB?

Esta es una diferencia importante respecto a una relación relacional tradicional.

El modelo `Funko` almacena `ProveedorId` como una cadena con el identificador `ObjectId` del proveedor. No existe una clave foránea SQL entre PostgreSQL y MongoDB.

Por tanto, la relación se coordina desde la aplicación:

1. Se recibe el identificador del proveedor.
2. Se valida su formato y se comprueba que el proveedor exista en MongoDB.
3. Se guarda el identificador en el documento relacional del Funko en PostgreSQL.
4. Cuando es necesario mostrar los datos del proveedor, la aplicación debe resolver esa referencia mediante la lógica correspondiente.

PostgreSQL no puede garantizar por sí mismo la integridad referencial de un identificador que apunta a un documento MongoDB.

### 5.4. Esquema de persistencia

```text
                 PostgreSQL
              ┌───────────────┐
              │     users     │
              │     roles     │
              │   Categoria   │
              │    Funkos     │
              └───────┬───────┘
                      │
          Funko.ProveedorId (ObjectId como string)
                      │
                      v
                 MongoDB
              ┌───────────────┐
              │  Proveedores  │
              └───────────────┘

                 Redis
              ┌───────────────┐
              │ Caché de datos│
              │ Backplane     │
              └───────────────┘

              Sistema de archivos
              ┌───────────────┐
              │ Imágenes      │
              └───────────────┘
```

### 5.5. Borrado lógico y control de cambios

Los modelos Funko y Proveedor incluyen campos como:

- `IsDeleted`: indica si el registro está eliminado lógicamente.
- `DeleteAt`: fecha de eliminación.
- `CreateAt`: fecha de creación.
- `UpdateAt`: fecha de última actualización.
- `Version`: número de versión del registro.

En Funko también se contempla el control de concurrencia mediante su campo `Version` y el tratamiento de `DbUpdateConcurrencyException` en la lógica de actualización.

La presencia de estos campos no significa que todos los repositorios tengan necesariamente el mismo comportamiento: las operaciones concretas deben consultarse en cada implementación.

---

## 6. Caché con Redis

Redis se utiliza como sistema de caché distribuida. Su objetivo es evitar consultar repetidamente PostgreSQL o MongoDB cuando se solicitan datos que ya se han obtenido recientemente.

La abstracción propia es `ICacheService`, implementada por `RedisCacheService`.

### 6.1. Funcionamiento

El proceso habitual de lectura es:

1. El cliente solicita un recurso.
2. El decorador calcula la clave de caché.
3. Busca el dato en Redis.
4. Si existe, lo devuelve sin consultar el servicio real (*cache hit*).
5. Si no existe, delega en el servicio.
6. Si la operación tiene éxito, almacena el resultado en Redis.
7. Devuelve el resultado al controlador.

Los objetos se serializan como JSON antes de guardarse y se deserializan al recuperarlos.

### 6.2. Claves de caché

Se utilizan claves diferenciadas por recurso, por ejemplo:

| Recurso | Ejemplo de clave |
|---|---|
| Lista de categorías | `categoria:all` |
| Categoría individual | `categoria:{id}` |
| Lista de proveedores | `proveedor:all` |
| Proveedor individual | `proveedor:{id}` |
| Funko individual | `funko:{id}` |
| Listado paginado de Funkos | `funkos:v{version}:{filtros}` |

Los listados paginados de Funkos utilizan una versión de caché. Cuando se crea, actualiza o elimina un Funko, se incrementa esa versión. Las siguientes consultas utilizan nuevas claves, por lo que dejan de leer los listados asociados a la versión anterior.

### 6.3. Invalidación

La caché no debe devolver información antigua después de modificar los datos.

Por ello, los decoradores invalidan las claves correspondientes cuando una operación de escritura termina correctamente:

- Crear: invalida los listados afectados.
- Actualizar: invalida el listado y el elemento individual.
- Eliminar: invalida el listado y el elemento individual.

Las entradas de lectura se guardan con una expiración de 30 minutos en los decoradores que especifican ese tiempo. También existe una configuración general mediante `CacheOptions`.

### 6.4. Protección frente a consultas simultáneas

`RedisCacheService` incorpora un `SemaphoreSlim` para mitigar el *cache stampede* dentro de una misma instancia de la aplicación. Es un bloqueo local al proceso, no un bloqueo distribuido compartido por todas las instancias.

### 6.5. Redis y SignalR

Además de la caché, Redis se configura como *backplane* de SignalR. Esto permite distribuir mensajes entre instancias de la aplicación que compartan ese backplane.

---

## 7. Autenticación y autorización

La seguridad de usuarios se implementa mediante **ASP.NET Core Identity y JWT Bearer**.

### 7.1. Registro de usuarios

El endpoint de registro es:

```http
POST /api/auth/signup
```

El usuario se crea con `UserManager<User>`. Identity aplica las reglas de contraseña y el nuevo usuario recibe el rol `USER`.

### 7.2. Inicio de sesión

El endpoint de login es:

```http
POST /api/auth/login
```

El flujo es:

1. Se busca al usuario por su nombre.
2. Se comprueba la contraseña mediante `SignInManager`.
3. Se aplican los mecanismos de bloqueo de Identity ante intentos fallidos.
4. Se actualiza `LastLoginAt`.
5. Se consultan los roles del usuario.
6. Se genera un JWT firmado con la clave configurada.
7. Se devuelve el token al cliente.

El código de generación establece una duración de **15 minutos** para el token.

El JWT incluye información en forma de *claims*, como el identificador, el correo, el nombre de usuario, el rol y el identificador único del token. Para el administrador también se incorpora el claim `department` con el valor `IT`.

### 7.3. Envío del token

Las peticiones protegidas deben incluir:

```http
Authorization: Bearer <token>
```

El middleware JWT valida:

- La firma del token.
- El emisor (`Issuer`).
- La audiencia (`Audience`).
- La fecha de expiración.
- El claim utilizado para los roles.

### 7.4. Roles y políticas

Se definen dos roles:

- `ADMIN`
- `USER`

Y dos políticas:

| Política | Requisito |
|---|---|
| `RequireAdmin` | Rol `ADMIN` y claim `department=IT` |
| `RequireUser` | Rol `USER` o `ADMIN` |

La política predeterminada exige un usuario autenticado.

El registro inicial de datos mediante `SeedIdentityData` crea los roles y usuarios de demostración si todavía no existen. Las credenciales de demostración no deben utilizarse en producción.

### 7.5. Perfil del usuario

El endpoint:

```http
GET /api/auth/me
```

requiere autenticación y devuelve los datos del usuario actual, sus roles y datos de registro e inicio de sesión.

---

## 8. API REST

La API REST está construida con controladores de ASP.NET Core MVC.

Los controladores principales utilizan rutas basadas en el nombre del controlador:

| Recurso | Ruta base |
|---|---|
| Autenticación | `/api/auth` |
| Funkos | `/api/Funko` |
| Categorías | `/api/Categoria` |
| Proveedores | `/api/Proveedor` |

Las rutas respetan los atributos HTTP definidos en cada acción. Consulta Swagger para ver las rutas, parámetros y respuestas exactas.

### 8.1. Operaciones

Los controladores incluyen operaciones de consulta, creación, actualización y eliminación de los recursos correspondientes.

En Funkos se contemplan operaciones de consulta paginada y filtrada, actualización completa y parcial, además de la gestión de imágenes.

Las operaciones de escritura y las consultas de proveedores están protegidas con la política de administrador allí donde se ha aplicado `[Authorize(Policy = "RequireAdmin")]`.

### 8.2. Paginación y filtros

El proyecto define DTOs para filtrar Funkos y estructuras de respuesta paginada.

La paginación permite devolver conjuntos de resultados controlados en lugar de cargar todos los registros de una vez. Los filtros permiten restringir los resultados por los campos contemplados en `FunkoFilterDto`.

### 8.3. Respuestas y enlaces

Los controladores utilizan códigos HTTP y un sistema común de errores de dominio.

También existen helpers que generan enlaces en la cabecera HTTP `Link`, con relaciones para consultar, actualizar, eliminar o volver al listado. Es una forma de ofrecer navegación entre operaciones desde las respuestas de la API.

### 8.4. Documentación

Swagger se configura mediante Swashbuckle. Incluye anotaciones, descripciones, respuestas documentadas, ejemplos de peticiones y esquema de seguridad Bearer.

En el arranque actual, Swagger y Swagger UI se habilitan únicamente cuando el entorno es `Development`.

---

## 9. API GraphQL

Además de REST, el backend expone un servidor GraphQL mediante **Hot Chocolate 16**.

El endpoint es:

```http
POST /graphql
```

GraphQL permite que el cliente especifique qué campos necesita, evitando recibir siempre una estructura fija.

### 9.1. Queries

Las consultas de lectura se encuentran en `TiendaQuery`.

Entre las operaciones implementadas se encuentran:

- Obtener Funkos paginados.
- Obtener un Funko por identificador.
- Obtener categorías.
- Obtener una categoría por identificador.

Las consultas paginadas incorporan middleware de Hot Chocolate para paginación, proyecciones, filtrado y ordenación.

En las consultas que devuelven `IQueryable`, Hot Chocolate puede componer la consulta antes de ejecutarla en la base de datos. Esto permite aplicar selección de campos, filtros y ordenación en el origen de datos.

### 9.2. Mutations

Las modificaciones se encuentran en `TiendaMutation`.

Incluyen operaciones para crear, actualizar y eliminar Funkos y las operaciones de categoría que aparecen definidas en el esquema.

Las mutaciones administrativas están protegidas con `RequireAdmin`.

Las operaciones utilizan los servicios de aplicación para mantener la lógica de negocio centralizada y, en las operaciones correspondientes, publican eventos para las suscripciones.

### 9.3. Subscriptions

Las suscripciones permiten que un cliente reciba eventos cuando se producen cambios.

Se definen eventos relacionados con:

- `FunkoCreado`
- `FunkoActualizado`
- `FunkoEliminado`

El servidor utiliza el mecanismo de suscripciones de Hot Chocolate. En la configuración actual se registra `AddInMemorySubscriptions`, por lo que el proveedor de eventos de GraphQL es en memoria.

### 9.4. Seguridad

GraphQL incorpora autorización mediante `AddAuthorization` y atributos de autorización en las operaciones protegidas.

En producción se deshabilita la introspección del esquema y los detalles de excepción se limitan según el entorno.

---

## 10. Comunicación en tiempo real

El proyecto incorpora **SignalR** mediante `NotificacionesHub`.

El Hub se publica en:

```text
/hubs/notificaciones
```

La conexión requiere autenticación.

### 10.1. Grupos

Al conectarse, el usuario se incorpora a grupos según su identidad y rol:

- `user-{userId}`: grupo privado asociado al usuario.
- `admins`: administradores.
- `users`: usuarios estándar.

Esto permite enviar notificaciones a todos los clientes, a un grupo o a una conexión concreta.

### 10.2. Métodos del Hub

El Hub contiene operaciones como:

- `SendMessage`: envía un mensaje a todos los clientes conectados y controla el número de mensajes por conexión.
- `SendToCaller`: responde únicamente al cliente que invocó el método.
- `SendToGroup`: envía un mensaje a un grupo determinado.

También gestiona los eventos de conexión y desconexión para asignar grupos y limpiar el contador de mensajes.

### 10.3. Notificaciones de negocio

Los servicios pueden emitir eventos cuando se crea o actualiza un Funko. Por ejemplo, una actualización puede notificar al grupo `admins` mediante el evento `FunkoActualizado`.

### 10.4. Escalabilidad

SignalR se configura con Redis como backplane y el prefijo de canal `FunkosApp`. Esto permite propagar mensajes entre diferentes instancias del backend que utilicen el mismo Redis.

---

### 10.4. Diseño de notificaciones por email

La arquitectura contempla un sistema desacoplado de envío de correos para comunicar eventos relevantes de la aplicación, como la creación o eliminación de Funkos. El diseño se basa en **MailKit/SMTP**, una abstracción `IEmailService`, plantillas HTML reutilizables y procesamiento asíncrono mediante una cola y un `BackgroundService`.

El flujo previsto es:

1. Un servicio de negocio genera una notificación cuando se produce un evento.
2. El mensaje se añade a `EmailQueueService`, evitando que la petición HTTP tenga que esperar al envío.
3. `EmailBackgroundWorker` procesa la cola en segundo plano.
4. `IEmailService` delega el envío en el proveedor configurado, con MailKit para SMTP.
5. Las plantillas permiten mantener separado el contenido HTML de la lógica de negocio.

La abstracción permite utilizar una implementación en memoria durante desarrollo y pruebas, sin depender de un servidor SMTP real. La configuración SMTP se gestiona mediante opciones tipadas (`IOptions<SmtpSettings>`), manteniendo las credenciales fuera del código fuente.

> **Nota de arquitectura:** este apartado documenta el diseño previsto para ampliar las notificaciones de la API por correo. No forma parte de los componentes verificados en el código fuente actual.

---


## 11. Gestión de imágenes y archivos

Las imágenes no se guardan dentro de PostgreSQL ni MongoDB. Se almacenan físicamente en el servidor mediante `FileSystemStorageService`.

El servicio implementa `IStorageService` y utiliza la configuración `StorageSettings`.

### 11.1. Directorios

La ruta raíz configurada por defecto es:

```text
wwwroot/uploads
```

El servicio prepara directorios para imágenes, documentos y archivos temporales. Las imágenes de Funkos se almacenan en una subcarpeta `funkos`.

### 11.2. Proceso de subida

Cuando se crea o actualiza un Funko con imagen:

1. Se recibe el archivo mediante `IFormFile`.
2. Se comprueba que no esté vacío.
3. Se comprueba el tamaño máximo.
4. Se valida la extensión.
5. Se valida el tipo MIME declarado.
6. Se genera un nombre único para evitar colisiones.
7. Se guarda el contenido en el disco.
8. Se almacena en el Funko la URL relativa de la imagen.

La configuración y las validaciones contemplan formatos de imagen como JPG, JPEG, PNG, GIF y WebP, aunque las reglas concretas del validador de Funko pueden ser más restrictivas.

El tamaño máximo configurado para el almacenamiento es de 5 MB. Kestrel y el formulario multipart permiten un límite de petición de hasta 10 MB.

### 11.3. Actualización y eliminación

Cuando se actualiza un Funko con una nueva imagen, se guarda la nueva y se intenta eliminar la anterior, excepto si es la imagen por defecto.

Si no se adjunta una imagen nueva, se conserva la que ya tenía el producto.

La API sirve los archivos estáticos mediante `UseStaticFiles`.

> **Importante para despliegues:** si se ejecutan contenedores efímeros, el directorio de imágenes debe persistirse mediante un volumen o un almacenamiento externo. El `compose.yaml` facilitado define volúmenes para PostgreSQL, MongoDB y Redis, pero no muestra un volumen específico para las imágenes.

---

## 12. Validación y gestión de errores

### 12.1. FluentValidation

Los validadores se encuentran organizados por recurso:

- `Validations/Funko`
- `Validations/Categoria`
- `Validations/Proveedor`

Comprueban reglas como campos obligatorios, longitudes, rangos numéricos, formato de identificadores, existencia de categorías y proveedores y unicidad de determinados datos.

También se utiliza `DataAnnotations` en DTOs y existe el atributo personalizado `NoLink` para impedir determinados contenidos en campos de texto.

### 12.2. Errores de dominio

El proyecto utiliza tipos de error específicos y una jerarquía común:

- `DomainErrors`
- `FunkoError`
- `CategoriaError`
- `ProveedorError`
- `StorageError`

Se contemplan situaciones como:

- Validación incorrecta.
- Recurso no encontrado.
- Conflicto por datos duplicados.
- Operación no permitida.
- Regla de negocio incumplida.
- Error de almacenamiento.
- Error interno.

### 12.3. Resultados funcionales

Los servicios utilizan `Result<T, DomainErrors>` de CSharpFunctionalExtensions para representar el éxito o el fallo de una operación sin depender exclusivamente de excepciones.

Los controladores convierten los errores de dominio a respuestas HTTP mediante las extensiones definidas en el proyecto.

### 12.4. Excepciones

Las excepciones se reservan para situaciones excepcionales, como determinados errores de almacenamiento o conflictos de concurrencia. Existe un manejador global de excepciones y se registra `ProblemDetails`.

---

## 13. Seguridad y control del tráfico

El backend incorpora diferentes medidas:

- Autenticación JWT.
- Autorización por roles y políticas.
- Reglas de contraseñas de Identity.
- Bloqueo temporal tras varios intentos fallidos de inicio de sesión.
- CORS mediante políticas configuradas para desarrollo y producción.
- Límites del tamaño de las peticiones.
- Validación de archivos.
- Rate limiting.
- Desactivación de introspección GraphQL en producción.
- Restricción de detalles de excepciones según el entorno.

### 13.1. Rate limiting

Hay dos mecanismos configurados en el proyecto:

1. El rate limiter integrado de ASP.NET Core, con políticas por IP y por usuario.
2. Un middleware propio (`RateLimitMiddleware`) que mantiene particiones por IP, método HTTP y ruta.

El middleware propio define una ventana general de 100 peticiones cada 15 segundos, y ventanas adicionales para rutas de autenticación y operaciones de escritura. Los límites concretos se encuentran en `Infrastructure/RateLimit/RateLimitConfig.cs` y `Middleware/RateLimitMiddleware.cs`.

Cuando se supera un límite, el sistema puede responder con HTTP `429 Too Many Requests`.

---

## 14. Ejecución con Docker Compose

El archivo `compose.yaml` levanta los servicios principales:

| Servicio | Imagen | Puerto publicado |
|---|---|---|
| API | Construida desde `Funkos/Dockerfile` | `5000` → `8080` |
| PostgreSQL | `postgres:16` | `5432` |
| MongoDB | `mongo:8` | `27017` |
| Redis | `redis:7-alpine` | `127.0.0.1:6379` |

PostgreSQL, MongoDB y Redis disponen de volúmenes para conservar sus datos. También tienen comprobaciones de salud (*healthchecks*), y la API declara dependencias de esos servicios.

### 14.1. Arrancar el entorno

Desde la carpeta raíz del repositorio:

```bash
docker compose up -d --build
```

Este comando construye la imagen de la API y arranca los contenedores en segundo plano.

### 14.2. Consultar los contenedores

```bash
docker compose ps
```

Muestra el estado de los servicios.

### 14.3. Consultar los logs

```bash
docker compose logs -f funkos
```

Muestra los registros de la API y continúa mostrando los nuevos mensajes.

### 14.4. Detener el entorno

```bash
docker compose down
```

Detiene y elimina los contenedores, pero no elimina los volúmenes nombrados.

Para eliminar también los datos persistidos:

```bash
docker compose down -v
```

> **Advertencia:** `-v` elimina los volúmenes asociados y, por tanto, los datos persistidos de PostgreSQL, MongoDB y Redis. No lo utilices si necesitas conservarlos.

### 14.5. Puertos y conexiones

El puerto `5000` del host se dirige al puerto `8080` del contenedor de la API. Las cadenas de conexión utilizadas desde el contenedor deben apuntar a los nombres de servicio de Docker Compose (`posgresql`, `mongo` y `redis`), no a `localhost`, porque dentro del contenedor `localhost` hace referencia al propio contenedor.

Comprueba que las cadenas de conexión de los `appsettings` o variables de entorno coinciden con esta topología antes de arrancar el entorno.

---

## 15. Configuración

La aplicación utiliza el sistema de configuración de ASP.NET Core:

- `appsettings.json`
- `appsettings.Development.json`
- `appsettings.Production.json`
- Variables de entorno

Entre las secciones de configuración relevantes se encuentran:

| Sección | Finalidad |
|---|---|
| `ConnectionStrings` | Conexión con PostgreSQL |
| `MongoDbSettings` | Conexión y nombre de base de datos MongoDB |
| `Redis` | Conexión con Redis |
| `Jwt` | Clave, emisor y audiencia de los tokens |
| `Cache` | Duración y opciones de caché |
| `Storage` | Directorios, extensiones y límites de archivos |

No se deben guardar contraseñas reales, claves JWT ni credenciales de producción en el repositorio. En despliegues reales deben inyectarse mediante variables de entorno o un gestor de secretos.

---

## 16. Pruebas

El proyecto de pruebas es `Funkos.Test`.

La estructura incluye pruebas para:

- Repositorios PostgreSQL y MongoDB.
- Servicios de Funkos, categorías y proveedores.
- Decoradores de caché.
- Servicio Redis.
- Mappers.
- Validaciones.
- Helpers de enlaces.
- Middleware de rate limiting.
- Almacenamiento de archivos.
- Consultas y mutaciones GraphQL.
- Hub de SignalR.
- Integración de controladores.

Se utilizan NUnit, Moq y FluentAssertions. También se incluye un directorio `CoverageReport` con informes de cobertura generados.

Para ejecutar las pruebas desde la raíz de la solución:

```bash
dotnet test Funkos.Test/Funkos.Test.csproj
```

Para ejecutar la solución completa:

```bash
dotnet test Funkos.slnx
```

La disponibilidad de PostgreSQL, MongoDB o Redis puede ser necesaria para determinadas pruebas de integración, según cómo estén configuradas.

---

## 17. Flujo completo de una petición

Ejemplo: un administrador crea un Funko mediante REST.

```text
1. Cliente
   |
   | POST /api/Funko
   | Authorization: Bearer <JWT>
   | Datos del Funko + imagen opcional
   v
2. ASP.NET Core
   |
   | Comprueba autenticación JWT
   | Comprueba la política RequireAdmin
   v
3. FunkoController
   |
   | Recibe el DTO
   | Delega en IFunkoService
   v
4. CachedFunkoServiceDecorator
   |
   | Delega la escritura en el servicio real
   v
5. FunkoService
   |
   | Ejecuta validaciones
   | Comprueba categoría en PostgreSQL
   | Comprueba proveedor en MongoDB
   | Guarda imagen si se ha enviado
   v
6. FunkoRepository
   |
   | Persiste el Funko en PostgreSQL
   v
7. Decorador de caché
   |
   | Invalida el listado paginado afectado
   v
8. Servicio / SignalR
   |
   | Emite la notificación correspondiente
   v
9. Controller
   |
   | Devuelve el DTO y el código HTTP
   v
10. Cliente
```

El flujo es ilustrativo: el orden exacto de cada operación depende de la acción concreta y de su implementación. La validación y la persistencia de una imagen, por ejemplo, pueden producir efectos independientes que deben tenerse en cuenta si falla una operación posterior.

---

## 18. Consideraciones para producción

Antes de desplegar la API en un entorno real, conviene revisar especialmente:

1. **Secretos:** sustituir todas las credenciales de ejemplo y proteger la clave JWT.
2. **HTTPS:** servir la API exclusivamente mediante HTTPS y configurar correctamente los certificados y proxies.
3. **CORS:** utilizar la política de producción y permitir únicamente los orígenes reales.
4. **Persistencia de imágenes:** montar un volumen persistente o utilizar almacenamiento de objetos.
5. **MongoDB:** configurar usuarios con permisos mínimos y no utilizar credenciales administrativas para la aplicación.
6. **PostgreSQL:** utilizar un usuario específico para la API y copias de seguridad periódicas.
7. **Redis:** proteger el acceso, configurar autenticación y revisar la persistencia y la política de memoria.
8. **Migraciones:** aplicar las migraciones de forma controlada durante el despliegue.
9. **Observabilidad:** revisar logs, métricas, trazas y alertas.
10. **Escalabilidad:** revisar los mecanismos de invalidación de caché y los proveedores de suscripciones GraphQL si se ejecutan varias instancias.

### Limitaciones y decisiones arquitectónicas que conviene conocer

- La relación entre Funkos y proveedores es una referencia entre dos bases de datos distintas; no existe una clave foránea que garantice la integridad entre PostgreSQL y MongoDB.
- El bloqueo de `RedisCacheService` contra *cache stampede* es local a cada proceso.
- Las suscripciones GraphQL están configuradas con almacenamiento en memoria; Redis se utiliza como backplane de SignalR, pero eso no convierte automáticamente las suscripciones GraphQL en distribuidas.
- Los archivos se almacenan en disco local. Para varias instancias de API, todas deben compartir el mismo almacenamiento o utilizar un servicio externo.
- El campo `Version` existe en los modelos y se contempla concurrencia en la actualización de Funkos; debe comprobarse la configuración de EF Core para confirmar cómo se aplica el control optimista en cada entidad.
- El `compose.yaml` debe revisarse junto con las cadenas de conexión de cada entorno. Los nombres de servicio, puertos y credenciales deben coincidir.
- El proyecto contiene varios mecanismos de rate limiting; conviene comprobar qué políticas están asociadas a cada endpoint y evitar aplicar límites duplicados accidentalmente.

---

## Resumen

Funkos API es un backend monolítico modular construido con ASP.NET Core y .NET 10. Separa controladores, servicios, repositorios y DTOs; utiliza PostgreSQL para datos relacionales e identidad, MongoDB para proveedores, Redis para caché y SignalR, y el sistema de archivos para imágenes.

Ofrece dos formas de acceso a los datos —REST y GraphQL—, autenticación JWT con Identity, autorización por roles y políticas, notificaciones en tiempo real, validaciones, gestión común de errores, documentación Swagger, pruebas automatizadas y un entorno de ejecución basado en Docker Compose.
