using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductApi.Models
{
    public class ReportEvaluation
    {
        [Key]
        public Guid Id { get; set; }
        
        [Required]
        public Guid ReportId { get; set; }
        
        [ForeignKey("ReportId")]
        public virtual NoonReport Report { get; set; } = null!;
        
        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Normal";
        
        [Required]
        public string ContentVi { get; set; } = string.Empty;
        
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
