using InfinityLife.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
public class PaySlip
{
    [Key]
    public int PaySlipId { get; set; }

    [Required]
    public string? EmployeeId { get; set; }

    [Required]
    public DateTime PayPeriod { get; set; }

    [Column(TypeName = "int")]
    public int BasicSalary { get; set; }

    [Column(TypeName = "int")]
    public int HRA { get; set; }

    [Column(TypeName = "int")]
    public int Conveyance { get; set; }

    [Column(TypeName = "int")]
    public int OtherAllowances { get; set; }

    [Column(TypeName ="int")]
    public int Bonus { get; set; }

    [Column(TypeName = "int")]
    public int PF { get; set; }

    [Column(TypeName = "int")]
    public int ESIC { get; set; }

    [Column(TypeName = "int")]
    public int ProfessionalTax { get; set; }

    [Column(TypeName = "int")]
    public int IncomeTax { get; set; }

    public DateTime GeneratedDate { get; set; }

    [NotMapped]
    public int GrossSalary => BasicSalary + HRA + Conveyance + OtherAllowances;

    [NotMapped]
    public int TotalDeductions => PF + ESIC + ProfessionalTax + IncomeTax;

    [NotMapped]
    public int NetSalary => GrossSalary - TotalDeductions;

    public virtual Employee Employee { get; set; }
}