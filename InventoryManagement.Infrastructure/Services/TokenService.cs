using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using InventoryManagement.Core.Entities;
using InventoryManagement.Core.Interfaces;
using InventoryManagement.Core.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace InventoryManagement.Infrastructure.Services;

public class TokenService : ITokenService
{
    private readonly IConfiguration _config;
    private readonly UserManager<User> _userManager;
    private readonly IPermissionRepository _permissionRepository;

    
    public TokenService(IConfiguration config, UserManager<User> userManager,IPermissionRepository permissionRepository)
    {
        _config = config;
        _userManager = userManager;
        _permissionRepository = permissionRepository;
    }

    public async Task<string> CreateToken(User user)
    {
        var permission = await _permissionRepository.GetByUserIdAsync(user.Id);
        
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email)
        };

        if (permission != null)
        {
            if (permission.CanView)
                claims.Add(new Claim("Permission", "CanView"));
            if (permission.CanCreate)
                claims.Add(new Claim("Permission", "CanCreate"));
            if (permission.CanEdit)
                claims.Add(new Claim("Permission", "CanEdit"));
            if (permission.CanDelete)
                claims.Add(new Claim("Permission", "CanDelete"));
        }

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