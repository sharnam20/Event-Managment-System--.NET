using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventManagementSystem1.Models
{
    public class Ticket
    {
        [Key]
        public int TicketId { get; set; }

        [Required]
        [Display(Name = "Event")]
        public int EventId { get; set; }

        [Required]
        [Display(Name = "Participant")]
        public int ParticipantId { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Ticket Number")]
        public string TicketNumber { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Pass Code")]
        public string PassCode { get; set; }

        [StringLength(500)]
        [Display(Name = "QR Code Data")]
        public string QRCodeData { get; set; }

        [StringLength(20)]
        public string Status { get; set; } = "Active";

        [Display(Name = "Issued At")]
        public DateTime IssuedAt { get; set; } = DateTime.Now;

        [Display(Name = "Used At")]
        public DateTime? UsedAt { get; set; }

        // Navigation properties
        public virtual Event Event { get; set; }
        public virtual Participant Participant { get; set; }

        [NotMapped]
        public bool IsUsed => Status == "Used";

        [NotMapped]
        public bool IsActive => Status == "Active";
    }
}