using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System;
using Complain.Models;  // Adjust based on your project
using Microsoft.AspNetCore.Mvc.Rendering;


public class ComplaintController : Controller
{
    private readonly IConfiguration _configuration;

    public ComplaintController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    // 1. LIST ALL COMPLAINTS
 public IActionResult Index()
{
    var complaints = new List<Complaint>();
    var connString = _configuration.GetConnectionString("DefaultConnection");

    var role = HttpContext.Session.GetString("UserRole");
    var userEmail = HttpContext.Session.GetString("UserEmail"); // or UserId if you store that

    using var conn = new MySqlConnection(connString);
    conn.Open();

    string query;

        // Admin: get all complaints
        if (role == "Admin")
        {
            query = @"SELECT Id, Name, Email, Phone, Title, Description, Category, Status, DateCreated FROM Complaints;";
        }
        else
        {
            // Normal User: get only their own complaints
            query = @"SELECT Id, Name, Email, Phone, Title, Description, Category, Status, DateCreated 
                  FROM Complaints 
                  WHERE Email = @Email;";
                  
    }

    

    using var cmd = new MySqlCommand(query, conn);

    if (role != "Admin")
    {
        cmd.Parameters.AddWithValue("@Email", userEmail);
    }

    using var reader = cmd.ExecuteReader();

    while (reader.Read())
    {
        complaints.Add(new Complaint
        {
            Id = Convert.ToInt32(reader["Id"]),
            Name = reader["Name"].ToString(),
            Email = reader["Email"].ToString(),
            Phone = reader["Phone"].ToString(),
            Title = reader["Title"].ToString(),
            Description = reader["Description"].ToString(),
            Category = reader["Category"].ToString(),
            Status = reader["Status"].ToString(),
            DateCreated = Convert.ToDateTime(reader["DateCreated"])
        });
    }

    return View(complaints);
}


    // 2. CREATE (GET)
  
    [HttpGet]
public IActionResult Create()
{
    ViewBag.Categories = GetCategories();
    return View();
}


    // 3. CREATE (POST)
    [HttpPost]
public IActionResult Create(Complaint complaint)
{
    // Validate model first
    if (!ModelState.IsValid)
    {
        ViewBag.Categories = GetCategories(); // Repopulate dropdown for error return
        return View(complaint);
    }

    // Get connection string
    var connString = _configuration.GetConnectionString("DefaultConnection");

    using var conn = new MySqlConnection(connString);
    conn.Open();

    // SQL insert query
    const string query = @"
        INSERT INTO Complaints (Name, Email, Phone, Title, Description, Category, Status, DateCreated)
        VALUES (@Name, @Email, @Phone, @Title, @Description, @Category, @Status, @DateCreated);
    ";

    // Get user role & email from session
    var role = HttpContext.Session.GetString("UserRole");
    var userEmail = HttpContext.Session.GetString("UserEmail");
    string status = (role == "Admin") ? complaint.Status : "Pending";

    using var cmd = new MySqlCommand(query, conn);

    // Set parameter values
    cmd.Parameters.AddWithValue("@Name", complaint.Name);
    cmd.Parameters.AddWithValue("@Email", userEmail); // Use session-stored email
    cmd.Parameters.AddWithValue("@Phone", complaint.Phone);
    cmd.Parameters.AddWithValue("@Title", complaint.Title);
    cmd.Parameters.AddWithValue("@Description", complaint.Description);
    cmd.Parameters.AddWithValue("@Category", complaint.Category ?? "");
    cmd.Parameters.AddWithValue("@Status", status);
    cmd.Parameters.AddWithValue("@DateCreated", complaint.DateCreated == default ? DateTime.Now : complaint.DateCreated);

    // Execute the insert
    cmd.ExecuteNonQuery();

    TempData["Success"] = "Complaint submitted successfully!";
    return RedirectToAction(nameof(Index));
    
}


  private List<SelectListItem> GetCategories()
{
    return new List<SelectListItem>
    {
        new SelectListItem { Value = "Infrastructure", Text = "Infrastructure" },
        new SelectListItem { Value = "IT Services", Text = "IT Services" },
        new SelectListItem { Value = "Cleanliness", Text = "Cleanliness" },
        new SelectListItem { Value = "Hostel", Text = "Hostel" },
        new SelectListItem { Value = "Security", Text = "Security" },
        new SelectListItem { Value = "Electricity", Text = "Electricity" },
        new SelectListItem { Value = "Food & Canteen", Text = "Food & Canteen" }
    };
}


    // 4. DETAILS
    public IActionResult Details(int id)
    {
        var complaint = GetComplaintById(id);
        if (complaint == null) return NotFound();
        return View(complaint);
    }

    // 5. EDIT (GET)
    [HttpGet]
    public IActionResult Edit(int id)
    {
        var complaint = GetComplaintById(id);
        if (complaint == null) return NotFound();
        return View(complaint);
    }

    // 6. EDIT (POST)
    [HttpPost]
    public IActionResult Edit(Complaint complaint)
    {
        if (!ModelState.IsValid)
            return View(complaint);

        var connString = _configuration.GetConnectionString("DefaultConnection");

        using var conn = new MySqlConnection(connString);
        conn.Open();

        const string query = @"
            UPDATE Complaints
            SET Name = @Name,
                Email = @Email,
                Phone = @Phone,
                Title = @Title,
                Description = @Description,
                Category = @Category,
                Status = @Status,
                DateCreated = @DateCreated
            WHERE Id = @Id;
        ";

        using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@Id", complaint.Id);
        cmd.Parameters.AddWithValue("@Name", complaint.Name);
        cmd.Parameters.AddWithValue("@Email", complaint.Email);
        cmd.Parameters.AddWithValue("@Phone", complaint.Phone);
        cmd.Parameters.AddWithValue("@Title", complaint.Title);
        cmd.Parameters.AddWithValue("@Description", complaint.Description);
        cmd.Parameters.AddWithValue("@Category", complaint.Category ?? "");
        cmd.Parameters.AddWithValue("@Status", complaint.Status ?? "Pending");
        cmd.Parameters.AddWithValue("@DateCreated", complaint.DateCreated == default ? DateTime.Now : complaint.DateCreated);

        cmd.ExecuteNonQuery();

        TempData["Success"] = "Complaint updated successfully!";
        return RedirectToAction(nameof(Index));
    }

    // 7. DELETE (GET)
    [HttpGet]
    public IActionResult Delete(int id)
    {
        var complaint = GetComplaintById(id);
        if (complaint == null) return NotFound();
        return View(complaint);
    }

    // 8. DELETE (POST)
    [HttpPost, ActionName("Delete")]
    public IActionResult DeleteConfirmed(int id)
    {
        var connString = _configuration.GetConnectionString("DefaultConnection");

        using var conn = new MySqlConnection(connString);
        conn.Open();

        const string query = @"DELETE FROM Complaints WHERE Id = @Id;";
        using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@Id", id);
        cmd.ExecuteNonQuery();

        TempData["Success"] = "Complaint deleted successfully!";
        return RedirectToAction(nameof(Index));
    }

    // 🔍 Helper method
    private Complaint? GetComplaintById(int id)
    {
        var connString = _configuration.GetConnectionString("DefaultConnection");

        using var conn = new MySqlConnection(connString);
        conn.Open();

        const string query = @"SELECT Id, Name, Email, Phone, Title, Description, Category, Status, DateCreated
                               FROM Complaints
                               WHERE Id = @Id;";

        using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@Id", id);

        using var reader = cmd.ExecuteReader();
        if (!reader.Read()) return null;

        return new Complaint
        {
            Id = Convert.ToInt32(reader["Id"]),
            Name = reader["Name"].ToString(),
            Email = reader["Email"].ToString(),
            Phone = reader["Phone"].ToString(),
            Title = reader["Title"].ToString(),
            Description = reader["Description"].ToString(),
            Category = reader["Category"].ToString(),
            Status = reader["Status"].ToString(),
            DateCreated = Convert.ToDateTime(reader["DateCreated"])
        };
    }
}
