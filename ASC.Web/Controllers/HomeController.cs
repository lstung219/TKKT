using ASC.Utilities;
using ASC.Web.Configuration;
using ASC.Web.Models;
using ASC.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace ASC.Web.Controllers
{
    public class HomeController : AnonymousController
    {
        private readonly ILogger<HomeController> _logger;

        private IOptions<ApplicationSettings> _settings;
        public HomeController(IOptions<ApplicationSettings> settings)
        {
            _settings = settings;
        }
        public IActionResult Index()
        {
            // Set Session
            HttpContext.Session.SetSession("Test", _settings.Value);
            // Get Session
            var settings = HttpContext.Session.GetSession<ApplicationSettings>("Test");
            // Usage of IOptions
            ViewBag.Title = _settings.Value.ApplicationTitle;
            return View();
        }
        //public IActionResult Index()
        //{
        //    //Test fail test case
        //    ViewData.Model = "Test";
        //    throw new Exception("Login Fail!!!");
        //    return View();
        //}

        //public HomeController(ILogger<HomeController> logger, IOptions<ApplicationSettings> settings)
        //{
        //    _logger = logger;
        //    _settings = settings;
        //}

        //public IActionResult Index([FromServices] IEmailSender emailSender)
        //{
        //    var emailService = this.HttpContext.RequestServices.GetService(typeof(IEmailSender)) as IEmailSender;
        //    ViewBag.Title = _settings.Value.ApplicationTitle;
        //    return View();
        //}
        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";
            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";
            return View();
        }

        //public IActionResult Dashboard()
        //{
        //    return View();
        //}

        //public IActionResult Privacy()
        //{
        //    return View();
        //}

        //[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            //return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            return View();
        }
    }
}
