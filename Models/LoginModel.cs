using System.ComponentModel.DataAnnotations;

public class LoginModel
{
    [Key]
    public int Id { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    // Optional: to differentiate between user and admin
    public string Role { get; set; } = "User"; // or "Admin"
}
