using AutoMapper;
using Contracts;
using Entities.Exceptions;
using Entities.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Service.Contracts;
using Shared.DataTransferObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public sealed class AuthenticationService : IAuthenticationService
    {
        private readonly ILoggerManager _logger;
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AuthenticationService(ILoggerManager logger, IMapper mapper,UserManager<User> userManager, IConfiguration configuration, RoleManager<IdentityRole> roleManager)
        {
            _logger = logger;
            _mapper = mapper;
            _userManager = userManager;
            _configuration = configuration;
            _roleManager = roleManager;
        }

        public async Task<IdentityResult> RegisterUser(UserForRegistrationDto userForRegistration)
        {
            var user = _mapper.Map<User>(userForRegistration);
            var result = await _userManager.CreateAsync(user,
            userForRegistration.Password);
            if (result.Succeeded)
            {
                foreach (var item in userForRegistration.Roles)
                {
                    if (await _roleManager.RoleExistsAsync(item)) 
                    {
                        await _userManager.AddToRolesAsync(user, userForRegistration.Roles);

                    } 
                    throw  new RoleNotFoundException(item);



                }


                
            }
               
            return result;
        }


    }
}
