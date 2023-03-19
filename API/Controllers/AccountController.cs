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
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    { 
        private readonly DataContext _dataContext;
        private readonly IToken _tokenService;
        private readonly IMapper _mapper;
        public AccountController(DataContext dataContext, IToken tokenService, IMapper mapper)
        {
            _mapper = mapper;
            _dataContext = dataContext;
            _tokenService = tokenService;
        }

        [HttpPost("register")]

        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {
            if(await ExistedUsername(registerDto.UserName)) return BadRequest("Username already taken!");

            var user= _mapper.Map<AppUser>(registerDto);
    
            using var hmac = new HMACSHA512();

            
                user.UserName = registerDto.UserName.ToLower();
                user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password));
                user.PasswordSalt = hmac.Key;
           
            await _dataContext.Users.AddAsync(user);
            await _dataContext.SaveChangesAsync();

            return new UserDto {
                UserName = registerDto.UserName.ToLower(),
                KnownAs =  user.KnownAs,
                Token = _tokenService.CreateToken(user),
                Gender = user.Gender

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
                PhotoUrl = loginUser.Photos.FirstOrDefault(x => x.IsMain)?.Url,
                KnownAs =  loginUser.KnownAs,
                Gender = loginUser.Gender


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