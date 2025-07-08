# Sistema de Monitoreo de Gasolineras

Sistema de monitoreo para estaciones de combustible que permite gestionar inventarios, descargas y comunicación con dispositivos seriales para el control de tanques de combustible.

## 🏗️ Arquitectura del Proyecto

El proyecto sigue una arquitectura de capas basada en **Clean Architecture** con los siguientes proyectos:

### Proyectos Ejecutables
- **`API/`** - API REST con documentación Swagger, autenticación JWT y sistema de trabajos programados (Hangfire)
- **`Web/`** - Aplicación web Blazor Server para interfaz de usuario
- **`Presentation/`** - Aplicación de consola para comunicación directa con dispositivos seriales

### Proyectos de Librerías
- **`Domain/`** - Entidades del dominio y contratos principales
- **`Application/`** - Casos de uso, interfaces y servicios de aplicación
- **`Infrastructure/`** - Implementación de repositorios, comunicación serial, base de datos y servicios externos
- **`Test/`** - Proyecto de pruebas unitarias

## 🚀 Tecnologías Utilizadas

- **.NET 9.0**
- **ASP.NET Core** (API REST)
- **Blazor Server** (Interfaz web)
- **Entity Framework Core** (ORM)
- **SQL Server** (Base de datos)
- **Hangfire** (Trabajos en segundo plano)
- **JWT** (Autenticación)
- **Swagger/OpenAPI** (Documentación de API)
- **ASP.NET Core Identity** (Gestión de usuarios)

## ⚙️ Configuración y Requisitos

### Requisitos del Sistema
- **.NET 9.0 SDK**
- **SQL Server** (LocalDB, Express o completo)
- **Puerto serial** disponible para comunicación con dispositivos
- **Windows x86 o x64** (para despliegue)

### Configuración de Base de Datos

El sistema utiliza dos bases de datos:
- **`DBInventarioGasolineras`** - Base de datos principal
- **`Hangfire`** - Base de datos para trabajos programados

#### Cadenas de Conexión por Ambiente

**Desarrollo** (`appsettings.Development.json`):{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=DBInventarioGasolineras;User Id=sa;Password=Admin!90;TrustServerCertificate=True;",
    "HangfireConnection": "Server=localhost;Database=Hangfire;User Id=sa;Password=Admin!90;TrustServerCertificate=True;"
  }
}
**Producción** (`appsettings.json`):{
  "ConnectionStrings": {
    "DefaultConnection": "Server=SERVIDOR\\INSTANCIA;Database=DBInventarioGasolineras;User Id=usuario;Password=contraseña;TrustServerCertificate=True;",
    "HangfireConnection": "Server=SERVIDOR\\INSTANCIA;Database=Hangfire;User Id=usuario;Password=contraseña;TrustServerCertificate=True;"
  }
}
### Configuración de Puerto Serial
{
  "SerialPort": {
    "PortName": "COM3",
    "BaudRate": 2400,
    "DataBits": 8,
    "Parity": "None",
    "StopBits": "One",
    "Handshake": "None",
    "ReadTimeout": 500,
    "WriteTimeout": 500
  },
  "Estacion": {
    "Id": 1946
  }
}
## 🔧 Compilación y Ejecución

### Modo Desarrollo

#### API RESTcd API
dotnet run --launch-profile "Test-https"- **URL HTTPS**: https://localhost:7018
- **URL HTTP**: http://localhost:5225
- **Swagger**: https://localhost:7018/swagger

#### Aplicación Web (Blazor)cd Web
dotnet run --launch-profile "Test-https"- **URL HTTPS**: https://localhost:7232
- **URL HTTP**: http://localhost:5150

#### Aplicación de Consolacd Presentation
dotnet run
### Modo Producción

#### API RESTcd API
dotnet run --launch-profile "Production"
#### Aplicación Webcd Web
dotnet run --launch-profile "Production"
## 📦 Despliegue para Equipos de 32 bits

Para preparar el proyecto para despliegue en equipos de 32 bits, ejecutar los siguientes comandos:

### Publicación de la APIdotnet publish API/API.csproj -c Release -o ./publish/API --runtime win-x86 --self-contained true
### Publicación de la Aplicación Webdotnet publish Web/Web.csproj -c Release -o ./publish/WEB --runtime win-x86 --self-contained true
### Publicación de la Aplicación de Consoladotnet publish Presentation/Presentation.csproj -c Release -o ./publish/Presentation --runtime win-x86 --self-contained true
### Ejecución en Producción

Una vez publicados, los ejecutables se encuentran en las carpetas correspondientes:

- **API**: `./publish/API/API.exe`
- **Web**: `./publish/WEB/Web.exe`

## 🔐 Configuración de Seguridad

### JWT (API){
  "Jwt": {
    "Key": "EstaEsMiClaveSecretaSuperSegura1234567890!!",
    "Issuer": "MonitoringSystem.API",
    "Audience": "MonitoringSystem.Client",
    "ExpiryMinutes": 60
  }
}
### Configuración de Identity (Web)
- Requiere confirmación de email
- Contraseñas con requisitos de seguridad (8 caracteres mínimo, mayúsculas, minúsculas, números y caracteres especiales)
- Bloqueo de cuenta después de 5 intentos fallidos

## 🔌 Servicios Externos

### Servicio SOAP Dagal{
  "Dagal": {
    "Username": "DAGALADMIN",
    "Password": "DAGALSISTEMAS",
    "Endpoint": "http://www.dagal.mx/Servicios/ContadosDagal/ContadosService.asmx"
  }
}
## 📋 Comandos de la Aplicación de Consola

La aplicación de consola (`Presentation`) incluye los siguientes comandos:

- **`I201XX`** - Muestra inventario de tanques (ej. `I20100` - todos, `I20101` - tanque 1)
- **`I202XX`** - Muestra últimas 10 descargas (ej. `I20200` - todos, `I20201` - tanque 1)
- **`clean`** - Limpia la consola
- **`exit`** - Salir del programa

## 🏃‍♂️ Trabajos Programados (Hangfire)

El sistema incluye trabajos automáticos para:
- **Lecturas de inventario** - Consulta periódica del estado de tanques
- **Registro de descargas** - Monitoreo automático de transferencias de combustible
- **Sincronización con servicios externos** - Integración con sistema Dagal

### Dashboard de Hangfire
Accesible en: `https://localhost:7018/hangfire` (solo en desarrollo)

## 🗄️ Migraciones de Base de Datos

Para aplicar migraciones:
# Desde el directorio Infrastructure
dotnet ef database update --startup-project ../API/API.csproj

# Para crear nuevas migraciones
dotnet ef migrations add NombreMigracion --startup-project ../API/API.csproj
## 📊 Monitoreo y Logs

### Niveles de Log por Ambiente

**Desarrollo**:
- Default: Debug
- Hangfire: Information
- Microsoft.AspNetCore: Warning

**Producción**:
- Default: Warning
- Hangfire: Warning
- Microsoft.AspNetCore: Warning

## 🚨 Solución de Problemas Comunes

1. **Error de conexión serial**: Verificar que el puerto COM esté disponible y configurado correctamente
2. **Error de base de datos**: Verificar cadenas de conexión y que SQL Server esté ejecutándose
3. **Error en trabajos de Hangfire**: Verificar configuración de base de datos Hangfire
4. **Problemas de autenticación**: Verificar configuración JWT y claves secretas

## 👥 Contacto

**Desarrollador**: Gilberto Alvarez  
**Email**: gilbertoalvarez514@hotmail.com  
**GitHub**: https://github.com/gdegilberto

---

Para más información sobre el uso de la API, consulte la documentación Swagger disponible en `/swagger` cuando la aplicación esté ejecutándose en modo desarrollo.