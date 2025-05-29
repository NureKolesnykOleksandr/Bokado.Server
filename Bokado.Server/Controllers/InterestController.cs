using Bokado.Server.Dtos;
using Bokado.Server.Interfaces;
using Bokado.Server.Models;
using Bokado.Server.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;

namespace Bokado.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InterestController : ControllerBase
    {
        private readonly IInterestRepository _interestRepository;

        public InterestController(IInterestRepository interestRepository)
        {
            _interestRepository = interestRepository;
        }

        [HttpGet]
        public async Task<ActionResult> GetChallenges()
        {
            var interests = await _interestRepository.GetInterests();
            return Ok(interests);
        }

    }
}