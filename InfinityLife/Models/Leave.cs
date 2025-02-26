using System.ComponentModel.DataAnnotations;

namespace InfinityLife.Models
{
    public class Leave
    {
        public int LeaveId { get; set; }

        [Required(ErrorMessage = "Employee ID is required")]
        public string EmployeeId { get; set; }

        [Required(ErrorMessage = "From date is required")]
        [DataType(DataType.Date)]
        [Display(Name = "From Date")]
        public DateTime FromDate { get; set; }

        [Required(ErrorMessage = "To date is required")]
        [DataType(DataType.Date)]
        [Display(Name = "To Date")]
        public DateTime ToDate { get; set; }

        [Required(ErrorMessage = "Reason is required")]
        [MinLength(10, ErrorMessage = "Reason must be at least 10 characters long")]
        public string Reason { get; set; }
        public string Status { get; set; } // Pending, Approved, Rejected
        public DateTime RequestDate { get; set; }
        public string? ResponseComment { get; set; }
    }
}
