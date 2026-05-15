using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventManagementSystem1.Models
{
    public class Participant
    {
        [Key]
        public int ParticipantId { get; set; }

        [Required]
        [Display(Name = "Event")]
        public int EventId { get; set; }

        [Required]
        [Display(Name = "User")]
        public int UserId { get; set; }

        [Display(Name = "Registration Date")]
        public DateTime RegistrationDate { get; set; } = DateTime.Now;

        [StringLength(20)]
        [Display(Name = "Attendance Status")]
        public string AttendanceStatus { get; set; } = "Registered";

        [Display(Name = "Check-in Time")]
        public DateTime? CheckInTime { get; set; }

        [Display(Name = "Check-out Time")]
        public DateTime? CheckOutTime { get; set; }

        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        public string Notes { get; set; }

        // Navigation properties
        public virtual Event Event { get; set; }
        public virtual User User { get; set; }

        [NotMapped]
        public bool IsCheckedIn => CheckInTime.HasValue;

        [NotMapped]
        public bool IsAttended => AttendanceStatus == "Attended";
    }
}