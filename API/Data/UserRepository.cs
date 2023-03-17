
using API.Dtos;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class UserRepository : IUserRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public UserRepository(DataContext context , IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        

        public async Task<AppUser> GetUserByIdAsync(int id)
        {
            return await _context.Users
                    .FindAsync(id);
        }

        public async Task<AppUser> GetUserByUsernameAsync(string username)
        {
            return await _context.Users
                        .Include(p => p.Photos)
                        .SingleOrDefaultAsync( x => x.UserName == username);
        }

        public async Task< PagedList<MemberDto>> GetUserMember(UserParam userParam)
        {
          var query =  _context.Users.AsQueryable();

         query = query.Where(u => u.UserName !=userParam.CurrentUsername);
         query = query.Where(u => u.Gender == userParam.Gender);

         var minDob = DateOnly.FromDateTime(DateTime.Today.AddYears(-userParam.MaxAge -1));
         var maxDob = DateOnly.FromDateTime(DateTime.Today.AddYears(-userParam.MinAge -1));

         query = query.Where(u => u.DateOfBirth >= minDob && u.DateOfBirth <= maxDob);

        
        return await PagedList<MemberDto>.CreateAsync(
                    query.ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
                    .AsNoTracking(), userParam.PageNumber, userParam.PageSize);
                    
        }

        public async Task<MemberDto> GetUserMemberByUsernameAsync(string username)
        {
            return await _context.Users
                    .Where(x => x.UserName == username)
                    .ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
                    .SingleOrDefaultAsync();
        }

        public async Task<IEnumerable<AppUser>> GetUsersAsync()
        {
            return await _context.Users
                    .Include(p => p.Photos)
                    .ToListAsync();
        }

        public async Task<bool> SaveAllAsync()
        {
           return await _context.SaveChangesAsync() > 0;
        }

        public void Update(AppUser user)
        {
          _context.Entry(user).State = EntityState.Modified;
        }
    }
}