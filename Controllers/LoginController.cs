using Microsoft.AspNetCore.Mvc;
using UserApp.Data;
using System.Linq;
using Microsoft.AspNetCore.Http;

public class LoginController : Controller
{
    private readonly AppDbContext _context;

    public LoginController(AppDbContext context)
    {
        _context = context;
    }

    // ---------- LOGIN ----------
    public IActionResult Login() => View();

    [HttpPost]
    public IActionResult Login(LoginModel login)
    {
        var user = _context.Users
                    .FirstOrDefault(u => u.Email == login.Email && u.Password == login.Password);

        if (user == null)
        {
            ViewBag.Message = "Invalid email or password.";
            return View();
        }

        // Set session variables
        HttpContext.Session.SetString("UserEmail", user.Email);
        HttpContext.Session.SetString("UserRole", user.Role);
          
        HttpContext.Session.SetInt32("UserId", user.Id); // For filtering complaints

        // Redirect based on role
        if (user.Role == "Admin")
            return RedirectToAction("Index", "Complaint");  // Admin Dashboard

        return RedirectToAction("Index", "Complaint");     // User Complaint Form
    }

    // ---------- REGISTER ----------
    public IActionResult Register() => View();

    [HttpPost]
    public IActionResult Register(LoginModel model)
    {
        if (_context.Users.Any(u => u.Email == model.Email))
        {
            ViewBag.Message = "Email already exists.";
            return View();
        }

        _context.Users.Add(model);
        _context.SaveChanges();

        return RedirectToAction("Login");
    }

    // ---------- FORGOT PASSWORD ----------
    public IActionResult ForgotPassword() => View();
    public IActionResult Logout()
    {
        HttpContext.Session.Clear(); // Clear all session data
        return RedirectToAction("Login");
    }

}
