using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using WebAPI.Models;
using WebAPI.ViewModels;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApplicationUserController : ControllerBase
    {
          private readonly UserManager<ApplicationUser> _userManager;
          private readonly SignInManager<ApplicationUser> _signInManager;
          private readonly ApplicationSetting _applicationSetting;

          public ApplicationUserController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IOptions<ApplicationSetting>  applicationSetting)
          {
              _userManager = userManager;
              _signInManager = signInManager;
              _applicationSetting = applicationSetting.Value;
          }

          [HttpPost]
          [Route("Register")]
          //POST :/api/ApplicationUser/Register
          public async Task<Object> PostApplicationUser(ApplicationUserModel model)
          {
              var applicationUser = new ApplicationUser()
              {
                  UserName = model.UserName,
                  Email = model.Email,
                  FullName = model.FullName
              };
              try
              {
                  //password pass in here becuse to do encription process
                  var result = await _userManager.CreateAsync(applicationUser, model.Password);
                  return Ok(result);
              }
              catch (Exception ex)
              {

                  throw ex;
              }
          }

          [HttpPost]
          [Route("Login")]
          //POST :/api/ApplicationUser/Login
          public async Task<IActionResult> Login(LoginModel model)
          {
              var user = await _userManager.FindByNameAsync(model.UserName);

              if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
              {
                  var tokenDescriptor = new SecurityTokenDescriptor
                  {
                      //pass claims associated with the user
                      Subject = new ClaimsIdentity(new Claim[]
                      {
                          new Claim("UserID",user.Id.ToString()) 
                      }),
                      Expires = DateTime.UtcNow.AddMinutes(5),
                      SigningCredentials =  new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_applicationSetting.JWT_Secret)),SecurityAlgorithms.HmacSha256Signature)


                  };
                  var tokenHandler = new JwtSecurityTokenHandler();
                  var securityToken = tokenHandler.CreateToken(tokenDescriptor);
                  var token = tokenHandler.WriteToken(securityToken);

                  return Ok(new {token});
              }
              else
              {
                  return BadRequest(new {message = "Username or Password is incorrect."});
              }
          }
    }
}
