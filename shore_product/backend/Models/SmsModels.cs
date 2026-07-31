using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ProductApi.Models
{
    /// <summary>
    /// Represents an ISM Code Clause / Element (Chương ISM)
    /// </summary>
    public class IsmElement
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; } // 1 to 16

        [Required]
        [MaxLength(200)]
        public string ChapterName { get; set; } = string.Empty;

        // Sync Metadata
        public bool IsSynced { get; set; } = false;
        
        [MaxLength(50)]
        public string OriginNode { get; set; } = "SHORE";
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Relations
        public virtual ICollection<SmsProcedure> Procedures { get; set; } = new List<SmsProcedure>();
    }

    /// <summary>
    /// Represents an SMS Standard Operating Procedure (SOP)
    /// </summary>
    public class SmsProcedure
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public int IsmElementId { get; set; }

        [ForeignKey("IsmElementId")]
        [JsonIgnore]
        public virtual IsmElement? IsmElement { get; set; }

        [Required]
        [MaxLength(50)]
        public string ProcedureCode { get; set; } = string.Empty; // e.g. SOP-07-05

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Column(TypeName = "text")]
        public string Content { get; set; } = string.Empty; // SOP Content (PDF or HTML instruction text)

        [MaxLength(500)]
        public string? FilePath { get; set; }

        [Required]
        [MaxLength(20)]
        public string Version { get; set; } = "Rev 1.0"; // e.g. Rev 1.0

        [Required]
        public DateTime PublishDate { get; set; } = DateTime.UtcNow;

        [Required]
        [MaxLength(30)]
        public string Status { get; set; } = "Active"; // Draft, Active, Obsolete

        public DateTime? ObsoleteDate { get; set; }

        [MaxLength(1000)]
        public string? ChangeNote { get; set; }

        [MaxLength(100)]
        public string WatermarkText { get; set; } = "TÀI LIỆU ĐƯỢC KIỂM SOÁT";

        // Sync Metadata
        public bool IsSynced { get; set; } = false;

        [MaxLength(50)]
        public string OriginNode { get; set; } = "SHORE";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Relations
        public virtual ICollection<SmsFormTemplate> FormTemplates { get; set; } = new List<SmsFormTemplate>();
        public virtual ICollection<SmsProcedureAcknowledge> Acknowledgements { get; set; } = new List<SmsProcedureAcknowledge>();
    }

    /// <summary>
    /// Tracks crew reading and acknowledging a procedure
    /// </summary>
    public class SmsProcedureAcknowledge
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid SmsProcedureId { get; set; }

        [ForeignKey("SmsProcedureId")]
        [JsonIgnore]
        public virtual SmsProcedure? Procedure { get; set; }

        [Required]
        [MaxLength(100)]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Rank { get; set; } = string.Empty;

        [Required]
        public DateTime AcknowledgedAt { get; set; } = DateTime.UtcNow;

        // Sync Metadata
        public bool IsSynced { get; set; } = false;

        [MaxLength(50)]
        public string OriginNode { get; set; } = "SHORE";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Represents a dynamic Form/Checklist Template linked to a Procedure
    /// </summary>
    public class SmsFormTemplate
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid SmsProcedureId { get; set; }

        [ForeignKey("SmsProcedureId")]
        [JsonIgnore]
        public virtual SmsProcedure? Procedure { get; set; }

        [Required]
        [MaxLength(50)]
        public string FormCode { get; set; } = string.Empty; // e.g. BM-07-03

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "text")]
        public string ContentSchema { get; set; } = "[]"; // JSON schema specifying fields (labels, types, choices)

        // Sync Metadata
        public bool IsSynced { get; set; } = false;

        [MaxLength(50)]
        public string OriginNode { get; set; } = "SHORE";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Relations
        public virtual ICollection<SmsFilledRecord> FilledRecords { get; set; } = new List<SmsFilledRecord>();
    }

    /// <summary>
    /// Represents a filled out Form Template (Hồ sơ đã điền)
    /// </summary>
    public class SmsFilledRecord
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid SmsFormTemplateId { get; set; }

        [ForeignKey("SmsFormTemplateId")]
        [JsonIgnore]
        public virtual SmsFormTemplate? FormTemplate { get; set; }

        [MaxLength(50)]
        public string FormCode { get; set; } = string.Empty;

        /// <summary>Denormalized from SmsFormTemplate.Title for query convenience.</summary>
        [MaxLength(300)]
        public string FormTitle { get; set; } = string.Empty;

        /// <summary>Denormalized from SmsProcedure.ProcedureCode for query convenience.</summary>
        [MaxLength(50)]
        public string ProcedureCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string VesselName { get; set; } = "M/V Green Star";

        [Required]
        [MaxLength(100)]
        public string FilledBy { get; set; } = string.Empty;

        [Required]
        public DateTime FilledDate { get; set; } = DateTime.UtcNow;

        [Required]
        [Column(TypeName = "text")]
        public string FilledData { get; set; } = "{}"; // JSON containing the field values

        [Column(TypeName = "text")]
        public string DigitalSignatures { get; set; } = "[]"; // JSON array of stamps containing Name, Rank, Timestamp, PinHash, SigCode

        [Required]
        [MaxLength(30)]
        public string Status { get; set; } = "Draft"; // Draft, Submitted, Approved

        // Sync Metadata
        public bool IsSynced { get; set; } = false;

        [MaxLength(50)]
        public string OriginNode { get; set; } = "SHORE";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
