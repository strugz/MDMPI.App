using MDMPI.App.Core.Logistic.Interfaces;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MDMPI.App.Api.Controllers.Logistic
{
    [Route("api/[controller]")]
    [ApiController]
    public class RequestController : ControllerBase
    {
        private readonly IRequestRepository _requestRepository;
        private readonly IMobileRepository _mobileRepository;

        public RequestController(IRequestRepository requestRepository, IMobileRepository mobileRepository)
        {
            _requestRepository = requestRepository;
            _mobileRepository = mobileRepository;
        }

        
        [HttpGet("all")]
        public async Task<ActionResult> GetRequestAll()
        {
            var result = await _requestRepository.GetAllRequestsAsync();

            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }

        [HttpGet("cancel/{requestid}")]
        public async Task<ActionResult> GetCancelledRemarks(string requestid)
        {
           var result = await _requestRepository.GetAllRemarks(requestid);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }

        [HttpGet("images/{requestid}")]
        public async Task<ActionResult> GetRequestImages(string requestid)
        {
            var result = await _requestRepository.GetRequestProofImage(requestid);
            if (result == null)
            {
                return NotFound();
            }
            return File(result, "image/png");
        }

        [HttpGet("signature/{requestid}")]
        public async Task<ActionResult> GetRequestSignature(string requestid)
        {
            var result = await _requestRepository.GetRequestSignatureImage(requestid);
            if (result == null)
            {
                return NotFound();
            }
            return File(result, "image/png");
        }

        [HttpGet("mobile")]
        public async Task<ActionResult> GetMobile()
        {
            var result = await _mobileRepository.GetAllMobilesAsync();
            if(result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }


        // POST api/<RequestController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<RequestController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<RequestController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
