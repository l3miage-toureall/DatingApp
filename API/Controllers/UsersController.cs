using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using API.Data;
using API.Dtos;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class UsersController : BaseApiController
    {
       private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        public IPhotoService _photoService { get; }
        public UsersController(IUserRepository userRepository, IMapper mapper, IPhotoService photoService )
        {
            _photoService = photoService;
            _userRepository =  userRepository;
            _mapper = mapper;
        }

        [HttpGet()]
        [Authorize]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers()
        {
            var users = await _userRepository.GetUserMember();
            
            return Ok(users);
        }

        [HttpGet("id/{id}")]
        public async Task<ActionResult<MemberDto>> GetUserById(int id)
        {
            
            var user = await _userRepository.GetUserByIdAsync(id);

            var userToReturn = _mapper.Map<MemberDto>(user);

            return userToReturn;
        }

        [HttpGet("{username}")]
        public async Task<ActionResult<MemberDto>> GetUser(string username)
        {
          return await _userRepository.GetUserMemberByUsernameAsync(username);

        }

        [HttpPut]
        public async Task<ActionResult> UpdatedUser(MemberUpdateDto memberUpdateDto){
            

            var user = await _userRepository.GetUserByUsernameAsync(User.GetUsername());

            if(user == null) return Unauthorized();


            _mapper.Map(memberUpdateDto, user);

            if(await _userRepository.SaveAllAsync()) return NoContent();

            return BadRequest("Failed to update user");
        }

        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
        {
            
            var user = await _userRepository.GetUserByUsernameAsync(User.GetUsername());

            if(user == null) return NotFound();

            var result = await _photoService.AddPhotoAsync(file);

            if(result.Error != null) return BadRequest(result.Error.Message);

            var photo = new Photo 
            {
                Url = result.SecureUrl.AbsoluteUri,
                PublicId = result.PublicId
            };

            if(user.Photos.Count == 0 ) photo.IsMain = true;

            user.Photos.Add(photo);

            if(await _userRepository.SaveAllAsync()) 
            {
                return CreatedAtAction(nameof(GetUser), new { username = user.UserName}, _mapper.Map<PhotoDto>(photo));
            }
            
            //return _mapper.Map<PhotoDto>(photo);

            return BadRequest("Problem adding photo");

        }

        [HttpPut("set-main-photo/{photoId}")]
    public async Task<ActionResult> SetMainPhoto(int photoId)
    {
        var user = await _userRepository.GetUserByUsernameAsync(User.GetUsername());

        if(user == null) return NotFound();

        var photo =  user.Photos.FirstOrDefault(x => x.Id == photoId);

        if( photo == null) return NotFound();

        if(photo.IsMain) return BadRequest("This photo is already the main photo");

        var currentMainPhoto = user.Photos.FirstOrDefault( x => x.IsMain);

        if( currentMainPhoto != null) currentMainPhoto.IsMain = false;

        photo.IsMain = true;

        if( await _userRepository.SaveAllAsync()) return NoContent();

        return BadRequest("Problem setting the main photo");
    }

    }

    
}