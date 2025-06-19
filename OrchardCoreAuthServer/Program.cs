// 如果之前删除了 using OrchardCore.Logging; 导致不报错，可以不加这行。
// 如果报错，说明你需要它，或者有其他配置问题。
// using OrchardCore.Logging; 
var builder = WebApplication.CreateBuilder(args);


// ****** 1. 在 ConfigureServices (builder.Services) 中添加 CORS 服务 ******
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontendApp", // 定义一个 CORS 策略名称
        builder => builder.WithOrigins("http://localhost:8080") // <--- 关键：您的 Vue 前端应用的完整 URL
            .AllowAnyHeader()                     // 允许所有头部
            .AllowAnyMethod()                     // 允许所有 HTTP 方法 (GET, POST, PUT, DELETE等)
            .AllowCredentials());                 // 如果您的前端需要发送 cookie 或认证头部，则需要此项
});



// 配置 Orchard Core 服务
// 这是将你的 ASP.NET Core 项目转换为 Orchard Core 的核心方法
builder.Services.AddOrchardCms(); 



// // --- 开始添加这些行，用于显式注册 UserManager ---
// builder.Services.AddScoped<Microsoft.AspNetCore.Identity.UserManager<OrchardCore.Users.Models.User>>();
// builder.Services.AddScoped<Microsoft.AspNetCore.Identity.SignInManager<OrchardCore.Users.Models.User>>();
//
// // !!! 关键修复 !!!
// // 显式注册 Orchard Core 的 IUserStore 实现
// builder.Services.AddScoped<
//     Microsoft.AspNetCore.Identity.IUserStore<OrchardCore.Users.Models.User>,
//     OrchardCore.Users.Services.UserStore
// >();
// // --- 结束添加这些行 ---




// !!! 关键修复 !!!
// 在使用 app.UseAuthorization() 之前，必须先添加授权服务
builder.Services.AddAuthorization(); 

var app = builder.Build();

// 根据环境配置 HTTPS 和异常处理
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// 强制使用 HTTPS 重定向
app.UseHttpsRedirection();
// 启用静态文件服务 (CSS, JS, 图片等)
app.UseStaticFiles();
// 启用路由
app.UseRouting();


app.UseCors("AllowFrontendApp"); // <--- CORS 中间件的正确位置，使用您上面定义的策略名称


// 启用认证中间件 (Orchard Core 会处理)
app.UseAuthentication(); 
// 启用授权中间件 (Orchard Core 会处理)
app.UseAuthorization();

// 使用 Orchard Core 中间件，这会处理 CMS 的请求和路由
app.UseOrchardCore(); 

app.Run();