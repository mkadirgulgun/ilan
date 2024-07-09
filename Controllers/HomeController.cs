using Dapper;
using IlanSitesi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Diagnostics;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace IlanSitesi.Controllers
{
    public class HomeController : Controller
    {
        string connectionString = "TrustServerCertificate=True";
        public IActionResult Index()
        {
            using var connection = new SqlConnection(connectionString);
            var posts = connection.Query<Ilan>("SELECT * FROM ilanlar  WHERE IsApproved = 1 ORDER BY UpdatedDate DESC").ToList();

            return View(posts);
        }
    }
}
