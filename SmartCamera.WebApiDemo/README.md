```
SmartCamera.WebApiDemo/
│
├── Controllers/
│   └── CameraController.cs
│
├── Data/
│   └── AppDbContext.cs
│
├── DTOs/
│   ├── CameraDto.cs
│   └── CreateCameraRequest.cs
│
├── Messaging/
│   ├── IMessageProducer.cs
│   └── RabbitMQProducer.cs
│
├── Models/
│   └── Camera.cs
│
├── Services/
│   ├── ICameraService.cs
│   └── CameraService.cs
│
├── Migrations/
│   └── ... (Các file migration của EF Core)
│
├── Properties/
│   └── launchSettings.json
│
├── Program.cs
├── appsettings.json
├── SmartCamera.WebApiDemo.csproj
└── README.md
```
## Tiếp theo, bạn cần cài đặt NuGet packages:

**Package Manager Console:**
```
Install-Package Microsoft.EntityFrameworkCore
Install-Package Microsoft.EntityFrameworkCore.SqlServer
Install-Package Microsoft.EntityFrameworkCore.Tools
Install-Package Microsoft.EntityFrameworkCore.Design
Install-Package BCrypt.Net-Next
Install-Package AutoMapper
Install-Package AutoMapper.Extensions.Microsoft.DependencyInjection
Install-Package Microsoft.AspNetCore.Authentication.JwtBearer
Install-Package System.IdentityModel.Tokens.Jwt
```
#Chạy lại lệnh Add-Migration::
Add-Migration InitialCreate
Update-Database