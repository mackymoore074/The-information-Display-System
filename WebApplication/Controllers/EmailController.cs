using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClassLibrary.DtoModels.Common;
using ClassLibrary.DtoModels.Email;

namespace WebApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EmailController : ControllerBase
    {
        private readonly IEmailRepository _emailRepository;
        private readonly ILogger<EmailController> _logger;

        public EmailController(IEmailRepository emailRepository, ILogger<EmailController> logger)
        {
            _emailRepository = emailRepository;
            _logger = logger;
        }

        [HttpPost("send-to-staff")]
        public async Task<ActionResult<ApiResponse<bool>>> SendEmailToStaff([FromBody] EmailDto emailDto)
        {
            try
            {
                var result = await _emailRepository.SendEmailToStaffAsync(emailDto);
                return Ok(new ApiResponse<bool> 
                { 
                    Success = result,
                    Data = result,
                    Message = result ? "Email sent successfully" : "Failed to send email"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in SendEmailToStaff: {ex.Message}");
                return BadRequest(new ApiResponse<bool> 
                { 
                    Success = false,
                    Message = "Error sending email",
                    Errors = new List<string> { ex.Message }
                });
            }
        }
    }
} 