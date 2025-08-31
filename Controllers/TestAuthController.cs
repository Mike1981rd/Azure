using Microsoft.AspNetCore.Mvc;
using BCrypt.Net;

namespace WebsiteBuilderAPI.Controllers
{
    [ApiController]
    [Route("api/test")]
    public class TestAuthController : ControllerBase
    {
        [HttpPost("hash")]
        public IActionResult GenerateHash([FromBody] TestHashDto dto)
        {
            var hash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            return Ok(new { password = dto.Password, hash = hash });
        }

        [HttpPost("verify")]
        public IActionResult VerifyHash([FromBody] TestVerifyDto dto)
        {
            var result = BCrypt.Net.BCrypt.Verify(dto.Password, dto.Hash);
            return Ok(new { password = dto.Password, hash = dto.Hash, verified = result });
        }
    }

    public class TestHashDto
    {
        public string Password { get; set; }
    }

    public class TestVerifyDto
    {
        public string Password { get; set; }
        public string Hash { get; set; }
    }
}