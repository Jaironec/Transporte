# Transporte Premium - Sistema de Gestión Logística y Contable

Aplicación de escritorio Windows Premium desarrollada en .NET 8 con WPF y Material Design para la gestión logística y contable de una empresa de transporte de carga líquida.

## 🚀 Características

- **Dashboard Moderno**: Interfaz elegante con tarjetas KPI y visualización de métricas en tiempo real
- **Gestión Completa**: Administración de vehículos, conductores, clientes y viajes
- **Control Financiero**: Cálculo automático de utilidades, gastos y cuenta corriente de conductores
- **Material Design**: Interfaz moderna con MaterialDesignInXaml
- **Modo Oscuro/Claro**: Soporte para cambio de temas
- **Base de Datos PostgreSQL**: Optimizada con índices y relaciones bien definidas

## 📋 Requisitos

- .NET 8 SDK
- PostgreSQL 12 o superior
- Visual Studio 2022 o Visual Studio Code

## 🛠️ Instalación

1. **Clonar o descargar el proyecto**

2. **Configurar la base de datos**:
   - Ejecutar el script `Database/Script_PostgreSQL.sql` en PostgreSQL
   - Verificar que las credenciales en `appsettings.json` sean correctas
   - Por defecto: Usuario `postgres`, Puerto `5432`

3. **Restaurar paquetes NuGet**:
   ```bash
   dotnet restore
   ```

4. **Compilar el proyecto**:
   ```bash
   dotnet build
   ```

5. **Ejecutar la aplicación**:
   ```bash
   dotnet run --project TransporteApp/TransporteApp.csproj
   ```

## 📁 Estructura del Proyecto

```
TransporteApp/
├── Models/              # Modelos de dominio (Records con C# moderno)
├── Data/                # DbContext y configuración de EF Core
├── ViewModels/          # ViewModels MVVM
├── Views/               # Vistas XAML
├── Services/            # Servicios auxiliares
└── appsettings.json     # Configuración de conexión
```

## 🎨 Diseño

La aplicación utiliza:
- **Material Design**: Librería MaterialDesignInXaml
- **Paleta de Colores**: Azul oscuro profesional (#1E3A5F) con acentos en Naranja (#FF6B35) y Turquesa (#00CED1)
- **Tipografía**: Segoe UI Variable
- **Efectos Visuales**: Sombras suaves, bordes redondeados (12-16px)

## 📊 Funcionalidades Principales

### Dashboard
- Ganancia del mes actual
- Viajes en curso
- Litros transportados hoy
- Lista de viajes recientes

### Gestión de Viajes
- Registro completo de viajes con origen/destino
- Cálculo automático de utilidad (Flete - Pago Conductor - Gastos)
- Gestión de gastos por categorías (Combustible, Peaje, Comida, etc.)

### Vehículos
- Control de mantenimiento
- Alertas de vencimiento de seguro y SOAT
- Capacidad de carga en litros

### Conductores
- Gestión de perfiles con foto
- Control de licencias con alertas de vencimiento
- Cuenta corriente (rastreo de viáticos)

## 🔧 Configuración

Editar `appsettings.json` para configurar:
- Cadena de conexión a PostgreSQL
- Nombre de la empresa
- Tema por defecto (Dark/Light)

## 📝 Notas Técnicas

- **.NET 8**: Utiliza las últimas características de C# con soporte LTS
- **Records**: Modelos implementados como records para inmutabilidad
- **Required Properties**: Uso de propiedades `required` para validación en tiempo de compilación
- **MVVM**: Patrón Model-View-ViewModel con CommunityToolkit.Mvvm
- **Async/Await**: Operaciones asíncronas para mejor rendimiento

## 📄 Licencia

Este proyecto es de uso interno para la empresa de transporte.

