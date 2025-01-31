using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using InventoryManagement.Core.Entities;
using InventoryManagement.Core.Interfaces;
using InventoryManagement.Core.Services;
using InventoryManagement.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace InventoryManagement.Infrastructure.Services;

public class TokenService : ITokenService
{
    private readonly IConfiguration _config;
    private readonly UserManager<User> _userManager;
    private readonly IPermissionRepository _permissionRepository;
    private readonly ApplicationDbContext _context;
    
    public TokenService(IConfiguration config, UserManager<User> userManager,IPermissionRepository permissionRepository,ApplicationDbContext context)
    {
        _config = config;
        _userManager = userManager;
        _permissionRepository = permissionRepository;
        _context = context;
    }

    public async Task<string> CreateToken(User user)
    {
        var userPermission = await _permissionRepository.GetByUserIdAsync(user.Id);

        // departman iznini çek
        DepartmentPermission? deptPermission = null;
        if (user.DepartmentId.HasValue)
        {
            deptPermission = await _context.DepartmentPermissions
                .FirstOrDefaultAsync(dp => dp.DepartmentId == user.DepartmentId.Value);
        }

        // 3) OR'lama
        bool finalCanView = (userPermission != null && userPermission.CanView) 
                            || (deptPermission != null && deptPermission.CanView);

        bool finalCanCreate = (userPermission != null && userPermission.CanCreate) 
                              || (deptPermission != null && deptPermission.CanCreate);

        bool finalCanEdit = (userPermission != null && userPermission.CanEdit) 
                            || (deptPermission != null && deptPermission.CanEdit);

        bool finalCanDelete = (userPermission != null && userPermission.CanDelete) 
                              || (deptPermission != null && deptPermission.CanDelete);
        
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email)
        };

        if (finalCanView)
            claims.Add(new Claim("Permission", "CanView"));
        if (finalCanCreate)
            claims.Add(new Claim("Permission", "CanCreate"));
        if (finalCanEdit)
            claims.Add(new Claim("Permission", "CanEdit"));
        if (finalCanDelete)
            claims.Add(new Claim("Permission", "CanDelete"));

        var roles = await _userManager.GetRolesAsync(user);
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["TokenKey"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.Now.AddDays(7),
            SigningCredentials = creds
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}