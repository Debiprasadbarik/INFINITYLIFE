using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InfinityLife.Models
{
    public class PaySlipViewModel
    {
        [Key]
        public int PaySlipId { get; set; }

        [Required(ErrorMessage = "Employee number is required")]
        [Display(Name = "Employee ID")]
        public string? EmployeeId { get; set; }
        public string? EmployeeName { get; set; } = null;   
        public string? EmployeeMail { get; set; }

        [Required(ErrorMessage = "Pay Period is required")]
        [Display(Name = "Pay Period")]
        public DateTime PayPeriod { get; set; }
        
        [Display(Name = "Basic Salary")]
        public int BasicSalary { get; set; }

        [Display(Name = "HRA")]
        public int HRA { get; set; }

        [Display(Name = "Conveyance Allowance")]
        public int Conveyance { get; set; }

        [Display(Name = "Other Allowance")]
        public int OtherAllowances { get; set; }

        [Display(Name = "Bonus")]
        public int Bonus { get; set; }

        [Display(Name = "Provident Fund")]
        public int PF { get; set; }

        [Display(Name = "ESIC")]
        public int ESIC { get; set; }

        [Display(Name = "Professional Tax")]
        public int ProfessionalTax { get; set; }

        [Display(Name = "Income Tax")]
        public int IncomeTax { get; set; }

        [Display(Name = "Created Date")]
        public DateTime GeneratedDate { get; set; }

        [NotMapped]
        public int GrossSalary {  get; set; }

        [NotMapped]
        public int TotalDeductions {  get; set; }

        [NotMapped]
        public int NetSalary { get; set; }

        public virtual Employee? Employee { get; set; }
    }
}
