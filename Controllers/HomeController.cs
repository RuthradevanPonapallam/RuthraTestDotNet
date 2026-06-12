using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using RuthraTestDotnet.Data;
using RuthraTestDotnet.Models;

namespace RuthraTestDotnet.Controllers;

public class HomeController : Controller
{
    private readonly EmployeeRepository _employeeRepository;
    private readonly ILogger<HomeController> _logger;

    public HomeController(EmployeeRepository employeeRepository, ILogger<HomeController> logger)
    {
        _employeeRepository = employeeRepository;
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View(_employeeRepository.GetFilteredEmployees());
    }

    public IActionResult Controls()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
