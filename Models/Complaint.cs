using System;
using System.ComponentModel.DataAnnotations;

namespace Complain.Models
{
    public class Complaint
    {
        public int Id { get; set; }

        [Required]
        public string? Name { get; set; }

        [EmailAddress]
        [Required]
        public string? Email { get; set; }

        [Required]
        [Phone]
        public string? Phone { get; set; }

        [Required]
        public string? Title { get; set; }

        [Required]
        public string? Description { get; set; }

        public string? Category { get; set; }

        public string? Status { get; set; } = "Pending";

        public DateTime DateCreated { get; set; } = DateTime.Now;
    }
}
