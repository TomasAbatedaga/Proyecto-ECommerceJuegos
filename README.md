# ECommerce Juegos

## Descripción

Proyecto académico de comercio electrónico de claves digitales de videojuegos y software, con interfaz StoreJuegos. Usa ASP.NET Core MVC y compras simuladas: no integra una pasarela ni procesa pagos reales.

El Cliente puede registrarse, explorar el catálogo paginado, consultar stock, realizar compras simuladas, consultar las claves asignadas en su historial y reseñar productos adquiridos. El Administrador gestiona usuarios, categorías, productos y stock de claves mediante un módulo Vue con AJAX.

## Integrantes

- Tomas Abatedaga - abatedagatomas@gmail.com - @TomasAbatedaga - Discord: ztomy1
- Facundo Calderon - facu.eze.calderon@hotmail.com - @FacundoC2013 - Discord: facundoc2013

## Tecnologías y requisitos

- .NET SDK 10; ASP.NET Core MVC, C# y Razor.
- MySQL 8.0.16 o superior, con MySqlConnector y ADO.NET (sin Entity Framework).
- Bootstrap y Vue 3 Options API en Claves Digitales.
- Cookies para el sitio MVC; JWT para la API REST.
- Postman para probar la API.
- Acceso a Internet para cargar Vue y Bootstrap Icons desde sus CDN.

## Base de datos y DER

El esquema y los datos académicos están en [database/script.sql](database/script.sql). Abrir ese archivo desde MySQL Workbench o DBeaver y ejecutarlo en un servidor local para una base nueva. No es una migración ni un reinicio: no elimina tablas y fallará si ya existen. No ejecutarlo nuevamente sobre una base en uso.

Las siete entidades son Usuario, Categoria, Producto, ClaveDigital, Orden, OrdenDetalle y Resena. Sus tablas son Usuarios, Categorias, Productos, ClavesDigitales, Ordenes, OrdenDetalles y Resenas.

Hay relaciones 1:N, por ejemplo Categorías → Productos y Usuarios → Órdenes. La relación N:M entre Órdenes y Productos se resuelve mediante OrdenDetalle, con precio histórico y clave asignada.

![DER de las siete entidades](assets/diagrama.png)

## CONFIGURACIÓN LOCAL

Ejecutar los comandos desde la carpeta que contiene Proyecto-ECommerceJuegos.csproj. El proyecto ya tiene UserSecretsId: no hace falta ejecutar user-secrets init. Los siguientes valores son marcadores de ejemplo, no secretos reales.

~~~powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=ECommerceJuegos;User=root;Password=TU_PASSWORD;SslMode=none"
dotnet user-secrets set "TokenAuthentication:SecretKey" "TU_CLAVE_SEGURA"
~~~

Reemplazar TU_PASSWORD por la contraseña local y TU_CLAVE_SEGURA por una clave privada de al menos 32 bytes UTF-8. El marcador TU_CLAVE_SEGURA no cumple ese mínimo y debe sustituirse. Ajustar usuario, servidor y puerto de MySQL según la instalación. SslMode=none corresponde al ejemplo de conexión local.

No escribir contraseñas ni la clave JWT en appsettings.json. El archivo versionado conserva Password= vacío y no contiene SecretKey. User Secrets se carga en Development; en otro entorno se requiere configuración externa equivalente. La aplicación exige la clave JWT al iniciar, incluso para usar las vistas MVC.

## Ejecución

Con la base inicializada y la configuración local completa:

~~~powershell
dotnet restore
dotnet run --launch-profile https
~~~

El perfil https usa Development y declara https://localhost:7195 y http://localhost:5228. Abrir la URL HTTPS indicada en consola. Si el certificado local todavía no está confiado, ejecutar dotnet dev-certs https --trust. También se puede iniciar con dotnet run, usando la URL y el entorno indicados por el perfil seleccionado.

Para comprobar compilación después de restaurar dependencias:

~~~powershell
dotnet build --no-restore
~~~

## Roles y cuentas de demostración

- **Administrador:** administra usuarios, categorías, productos y claves. Puede cargar portadas y su avatar; no es el rol de compra.
- **Cliente:** consulta catálogo, compra de forma simulada, ve únicamente su historial, reseña productos adquiridos y carga su avatar. No administra entidades.

Las cuentas siguientes son exclusivamente de demostración y están definidas en el script SQL; no son credenciales de servicios reales:

| Rol | Email | Contraseña demo |
|---|---|---|
| Administrador | admin@example.test | AdminPrueba123! |
| Cliente | cliente@example.test | ClientePrueba123! |

Los INSERT guardan hashes de contraseña. El registro público crea Clientes. El cierre de sesión MVC se realiza por POST con antiforgery; no es un endpoint de revocación de JWT.

## Imágenes

- **Avatar:** Mi perfil → seleccionar imagen → Guardar avatar. JPG, JPEG, PNG o WebP, máximo 2 MB.
- **Portada:** un Administrador puede cargarla al crear o editar un producto. Mismos formatos, máximo 5 MB. Al editar sin archivo nuevo se conserva la portada actual.
- Los archivos se guardan localmente en wwwroot/Uploads/Avatares y wwwroot/Uploads/Productos, carpetas ignoradas por Git. Las imágenes subidas no se distribuyen con el repositorio.

## API REST

| Método | Ruta | Acceso |
|---|---|---|
| POST | /api/auth/login | Público; recibe email y contrasenia en JSON y devuelve token, expiración y datos de usuario. |
| GET | /api/productos?pagina=1 | Público; productos activos paginados. |
| GET | /api/productos/{id} | Público; detalle de un producto activo. |
| GET | /api/mis-compras?pagina=1 | JWT Bearer y rol Cliente; historial del usuario autenticado. |

Ejemplo de cuerpo para login, completando los marcadores localmente:

~~~json
{ "email": "EMAIL_CLIENTE_LOCAL", "contrasenia": "PASSWORD_CLIENTE_LOCAL" }
~~~

Las respuestas paginadas incluyen items, paginaActual, totalPaginas y totalRegistros. La cookie MVC no reemplaza el JWT de /api/mis-compras. Sin token válido se espera 401; un JWT de Administrador no cumple el rol Cliente y se espera 403. Los tokens emitidos vencen a las dos horas.

## Postman

Colección: [ECommerceJuegos.postman_collection.json](docs/postman/ECommerceJuegos.postman_collection.json).

1. Importar la colección en Postman.
2. Configurar baseUrl con la URL local real, por ejemplo https://localhost:7195, sin barra final.
3. Completar clienteEmail y clientePassword localmente con una cuenta Cliente de prueba.
4. Ejecutar **Login Cliente**.
5. Su script guarda el token devuelto en la variable token de la colección.
6. Ejecutar **Mis Compras con JWT** y las demás peticiones de la colección.

Probar también credenciales incorrectas, acceso sin token, token inválido o vencido, rol incorrecto y separación de historial entre clientes. La colección describe pruebas adicionales; sus resultados finales todavía deben producirse. No versionar tokens ni exportar credenciales o respuestas sensibles junto a las evidencias.

## Evidence Matrix

La implementación indicada se referencia al código. Las evidencias finales pendientes no se consideran producidas ni verificadas por esta tabla.

| Requisito | Implementación | Ubicación | Evidencia |
|---|---|---|---|
| 1. Siete entidades | Siete tablas con claves y restricciones. | [SQL](database/script.sql), [Models](Models/) | DER disponible; PENDIENTE — captura final del esquema. |
| 2. Relaciones 1:N | Categorías/Productos, Usuarios/Órdenes y otras FK. | [SQL](database/script.sql) | [DER disponible](assets/diagrama.png). |
| 3. N:M mediante OrdenDetalle | Órdenes y Productos relacionados por OrdenDetalles. | [OrdenDetalle](Models/OrdenDetalle.cs), [SQL](database/script.sql) | DER disponible; PENDIENTE — captura final del detalle relacionado. |
| 4. Login | Cookies MVC y validación de credenciales. | [UsuariosController](Controllers/UsuariosController.cs) | PENDIENTE — captura final. |
| 5. Logout | POST con antiforgery y cierre de cookie. | [UsuariosController](Controllers/UsuariosController.cs), [Layout](Views/Shared/_Layout.cshtml) | PENDIENTE — GIF/video final. |
| 6. Roles | Administrador y Cliente. | [UsuariosController](Controllers/UsuariosController.cs), [Program](Program.cs) | PENDIENTE — captura final de ambos menús. |
| 7. Restricciones backend | Autorización por rol, validación y protección de claves vendidas. | [ClavesDigitalesController](Controllers/ClavesDigitalesController.cs), [ComprasController](Controllers/ComprasController.cs) | PENDIENTE — GIF/video final de accesos y rechazos. |
| 8. Perfil | Consulta del perfil del usuario autenticado. | [Perfil](Views/Usuarios/Perfil.cshtml), [UsuariosController](Controllers/UsuariosController.cs) | PENDIENTE — captura final. |
| 9. Avatar | Carga de imagen de hasta 2 MB. | [UsuariosController](Controllers/UsuariosController.cs) | [Captura de perfil con avatar](assets/perfil%20con%20avatar.png). |
| 10. Portada | Carga administrativa de imagen de hasta 5 MB. | [ProductosController](Controllers/ProductosController.cs) | [Video de carga de portada](assets/subida%20de%20imagen%20de%20portada.mp4). |
| 11. CRUD Vue | Alta, listado, edición y eliminación de claves disponibles. | [Vista Vue](Views/ClavesDigitales/Index.cshtml), [Controlador](Controllers/ClavesDigitalesController.cs) | PENDIENTE — GIF/video final. |
| 12. AJAX búsqueda de productos | Búsqueda por nombre desde dos caracteres. | [Vista Vue](Views/ClavesDigitales/Index.cshtml), [RepositorioProducto](Repositories/RepositorioProducto.cs) | PENDIENTE — GIF/video final. |
| 13. Paginación servidor | Usuarios, Productos, Catálogo, Claves y Mis Compras. | [Partial](Views/Shared/_Paginacion.cshtml), [Repositorios](Repositories/) | PENDIENTE — captura final de navegación y solicitud de página. |
| 14. API REST | Login, productos y compras del usuario. | [Controllers/Api](Controllers/Api/) | PENDIENTE — Postman final. |
| 15. JWT | Emisión HS256 y validación de firma, emisor, audiencia y expiración. | [AuthController](Controllers/Api/AuthController.cs), [Program](Program.cs) | PENDIENTE — Postman final. |
| 16. Endpoint protegido | Mis Compras requiere Bearer y Cliente. | [MisComprasApiController](Controllers/Api/MisComprasApiController.cs) | PENDIENTE — Postman final de 200/401/403 y aislamiento de usuarios. |
| 17. Postman | Colección importable con variables y algunas aserciones. | [Colección](docs/postman/ECommerceJuegos.postman_collection.json) | PENDIENTE — Postman final. |
| 18. Script SQL | Esquema y datos académicos para una base nueva. | [database/script.sql](database/script.sql) | Archivo disponible; PENDIENTE — captura final de inicialización. |
| 19. DER | Imagen de las siete entidades y relaciones. | [assets/diagrama.png](assets/diagrama.png) | DER disponible. |
| 20. Documentación | Requisitos, configuración, ejecución, roles, imágenes y API. | [README](README.md) | Documento disponible; PENDIENTE — captura final de ejecución local. |
