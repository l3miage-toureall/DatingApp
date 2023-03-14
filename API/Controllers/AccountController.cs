using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using API.Data;
using API.Dtos;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    { 
        private readonly DataContext _dataContext;
        private readonly IToken _tokenService;

        public AccountController(DataContext dataContext, IToken tokenService)
        {
            _dataContext = dataContext;
            _tokenService = tokenService;
        }

        [HttpPost("register")]

        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {
            if(await ExistedUsername(registerDto.UserName)) return BadRequest("Username already taken!");
    
            using var hmac = new HMACSHA512();

            var user = new  AppUser
            {
                UserName = registerDto.UserName.ToLower(),
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
                PasswordSalt = hmac.Key
            };

            await _dataContext.Users.AddAsync(user);
            _dataContext.SaveChanges();

            return new UserDto {
                UserName = registerDto.UserName.ToLower(),
                Token = _tokenService.CreateToken(user)

            };   
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            var loginUser = await UserToLogin(loginDto.UserName);
            if(loginUser == null) return Unauthorized("Invalid username!");

            using var hmac = new HMACSHA512(loginUser.PasswordSalt);
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

            for( int i = 0; i< computedHash.Length; i++)
            {
                if(loginUser.PasswordHash[i] != computedHash[i]) return Unauthorized("Invalid password!");
            }

            return new UserDto {
                UserName = loginDto.UserName.ToLower(),
                Token = _tokenService.CreateToken(loginUser),
                PhotoUrl = loginUser.Photos.FirstOrDefault(x => x.IsMain)?.Url


            };
        }

        private async Task<AppUser> UserToLogin(string userName)
        {
            return await _dataContext.Users
            .Include( p => p.Photos)
            .SingleOrDefaultAsync(x => x.UserName == userName.ToLower());
        }

        private async Task<bool> ExistedUsername(string userName)
        {
            return await _dataContext.Users.AnyAsync( user=> user.UserName == userName.ToLower());
        }
    }
}