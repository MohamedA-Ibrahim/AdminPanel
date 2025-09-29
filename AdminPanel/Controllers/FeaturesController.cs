using AdminPanel.Models.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AdminPanel.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FeaturesController : ControllerBase
    {
        private readonly FeatureConfig _features;

        public FeaturesController(IOptions<FeatureConfig> options)
        {
            _features = options.Value;
        }

        [HttpGet]
        public IActionResult GetFeatures()
        {
            var response = new
            {
                EnableGrpc = _features.EnableGrpc
            };

            return Ok(response);
        }
    }
}
