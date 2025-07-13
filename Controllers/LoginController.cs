using Microsoft.AspNetCore.Mvc;
using UserApp.Data;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Complain.Models;


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




    [HttpGet]
    public IActionResult ForgotPassword()
    {
        return View();
    }

    [HttpPost]
    public IActionResult ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Simulate: Generate a dummy token (you can make it secure later)
        string token = Guid.NewGuid().ToString();
        string resetLink = Url.Action("ResetPassword", "Login",
            new { email = model.Email, token = token }, Request.Scheme);

        // Just display the link (in real use, send via email)
        TempData["Success"] = resetLink;

        return View();
    }

    [HttpGet]
    public IActionResult ResetPassword(string email, string token)
    {
        var model = new ResetPasswordViewModel
        {
            Email = email,
            Token = token
        };
        return View(model);
    }

    [HttpPost]
    public IActionResult ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Update password in your user table
        var user = _context.Users.FirstOrDefault(u => u.Email == model.Email);
        if (user == null)
        {
            TempData["Error"] = "User not found";
            return View();
        }

        user.Password = model.NewPassword; // You can hash this for real apps
        _context.SaveChanges();

        TempData["Success"] = "Password has been reset successfully!";
        return RedirectToAction("Login");
    }

      public IActionResult Logout()
    {
        // Clear session
        HttpContext.Session.Clear();

        // Optionally, redirect to landing page
        return RedirectToAction("Index", "Home"); // Or "Landing", if you use Landing as your homepage
    }


}
