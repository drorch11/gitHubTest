using journeyService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace journeyService.Controllers
{
    [Route("api/Example")]
    [ApiController]
    public class ExampleController : ControllerBase
    {
        private readonly IOptions<DataBaseSetting> _databasesetting;
        private readonly IConfiguration _config;
        public ExampleController(IConfiguration configuration,
            IOptions<DataBaseSetting> databasesetting)
        {
            _databasesetting = databasesetting;
            _config = configuration;
        }
        // GET: api/<ExampleController>
        [HttpGet]
        public IEnumerable<string> Get()
        {

            //from site,
            //http://localhost/journeyApp/api/Example
            //from program 
            //http://localhost:5279/api/Example
            //
            //http://umi-appsites/journeyApp/api/Example



            //return new string[] { _databasesetting.Value.ConnectionString ,
            //_config.GetValue<string>("Database:ConnectionString") ?? ""};

            //return new string[] { 
            //_config.GetValue<string>("ApiTafnit:TafnitEndPoint") ?? ""
            //};
            //Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Production");
            return new string[] {
            Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "",
            _config.GetValue<string>("Database:ConnectionString") ?? "",
            _config.GetValue<string>("Params:phoneTest") ?? "",
            _config.GetValue<string>("ApiTafnit:TafnitEndPoint") ?? ""
            };

            






        }


    }
}
