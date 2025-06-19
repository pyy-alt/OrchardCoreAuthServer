// File: OrchardCoreAuthServer/Controllers/RegistrationController.cs
using Microsoft.AspNetCore.Identity; // 用于 UserManager
using Microsoft.AspNetCore.Mvc;
using OrchardCore.Users.Models; // 引入 OrchardCore.Users.Models.User
using System.Threading.Tasks;
using System; // For Console.WriteLine
using System.ComponentModel.DataAnnotations; // For Required, EmailAddress, StringLength, Compare

[ApiController]
[Route("api/[controller]")] // 路由将是 /api/Registration
public class RegistrationController : ControllerBase
{
    // 将 UserManager 的泛型参数从 ApplicationUser 修改为 OrchardCore.Users.Models.User
    private readonly UserManager<User> _userManager;

    // 构造函数也修改为 User
    public RegistrationController(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    [HttpPost("register")] // 端点将是 /api/Registration/register
    // 这个接口将允许匿名访问，因为它是用于新用户注册的
    [Microsoft.AspNetCore.Authorization.AllowAnonymous] 
    [IgnoreAntiforgeryToken] // <--- 添加这一行
    public async Task<IActionResult> Register([FromBody] RegisterRequestModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // 1. 检查用户是否已存在
        var existingUser = await _userManager.FindByNameAsync(model.Username);
        if (existingUser != null)
        {
            return BadRequest(new { message = "Username already exists." });
        }

        // 2. 创建新用户实例
        // 将 ApplicationUser 替换为 OrchardCore.Users.Models.User
        var newUser = new User
        {
            UserName = model.Username,
            Email = model.Email, // 可选：如果您的用户模型需要Email
            // IsEmailConfirmed = false // OrchardCore.Users.Models.User 使用 IsEmailConfirmed
            // 如果您有其他自定义属性，Orchard Core 用户模型通常通过内容项扩展
            // 对于简单注册，这里只设置基本属性即可
        };

        // 3. 创建用户并设置密码
        // IdentityResult 包含操作是否成功的信息以及可能发生的错误
        var result = await _userManager.CreateAsync(newUser, model.Password);

        if (result.Succeeded)
        {
            // 可选：在这里添加用户到特定角色 (如果需要)
            // await _userManager.AddToRoleAsync(newUser, "SomeRoleName");

            Console.WriteLine($"New user registered: {newUser.UserName}");
            return Ok(new { message = "User registered successfully." });
        }
        else
        {
            // 如果创建失败，返回 IdentityResult 的错误信息
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
                Console.WriteLine($"User registration error for {model.Username}: {error.Description}");
            }
            return BadRequest(ModelState);
        }
    }
}

// 注册请求的数据模型
public class RegisterRequestModel
{
    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    [EmailAddress] // 可选：如果需要邮箱格式验证
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}