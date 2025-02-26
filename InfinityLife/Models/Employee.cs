using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace InfinityLife.Models
{
    [Table("Employee")]
    public class Employee
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int EmpNo { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public string? EmpId { get; set; }

        [Required(ErrorMessage = "Role is required")]
        [ForeignKey("EmployeeRole")]
        [Display(Name = "Role")]
        public int EmpRoleId { get; set; }

        [Required]
        [StringLength(50)]
        [Column("EmpFirstName")]
        public string? EmpFirstName { get; set; }

        [Required]
        [StringLength(50)]
        [Column("EmpLastName")]
        public string? EmpLastName { get; set; }

        [Required]
        [StringLength(15)]
        public string? EmpPhone { get; set; }

        [Required]
        [StringLength(100)]
        [EmailAddress]
        [Column("EmpEmail")]
        public string? EmpEmail { get; set; }

        [Required]
        [StringLength(255)]
        public string? EmpAddress { get; set; }

        [StringLength(100)]
        public string? EmergencyContact { get; set; }

        [StringLength(10)]
        public string? Bloodgroup { get; set; }

        [StringLength(12)]
        public string? AadharNo { get; set; }

        [StringLength(10)]
        public string? PanNo { get; set; }

        [Display(Name = "Date of Birth")]
        [Column(TypeName = "datetime")]
        public DateTime? DateOfBirth { get; set; }

        [Display(Name = "Date of Joining")]
        [Column(TypeName = "datetime")]
        public DateTime? DateOfJoining { get; set; }

        [Required]
        public string? Password { get; set; }

        public byte[]? ProfilePicture { get; set; }
        [Display(Name = "Basic Salary")]
        [Column(TypeName = "int")]
        public int BasicSalary { get; set; }

        [Display(Name = "HRA")]
        [Column(TypeName = "int")]
        public int HRA { get; set; }

        [Display(Name = "Conveyance")]
        [Column(TypeName = "int")]
        public int Conveyance { get; set; }

        [Display(Name = "Other Allowances")]
        [Column(TypeName = "int")]
        public int OtherAllowances { get; set; }

        // Deductions
        [Display(Name = "PF")]
        [Column(TypeName = "int")]
        public int PF { get; set; }

        [Display(Name = "ESIC")]
        [Column(TypeName = "int")]
        public int ESIC { get; set; }

        [Display(Name = "Professional Tax")]
        [Column(TypeName = "int")]
        public int ProfessionalTax { get; set; }

        [Display(Name = "Income Tax")]
        [Column(TypeName = "int")]
        public int IncomeTax { get; set; }

        // Calculated properties
        [NotMapped]
        public int GrossSalary => BasicSalary + HRA + Conveyance + OtherAllowances;

        [NotMapped]
        public int TotalDeductions => PF + ESIC + ProfessionalTax + IncomeTax;

        [NotMapped]
        public int NetSalary => GrossSalary - TotalDeductions;
        // Navigation property
        [ValidateNever] // Add this attribute to ignore validation for this navigation property
        public virtual EmployeeRole? EmployeeRole { get; set; }
    }
}
