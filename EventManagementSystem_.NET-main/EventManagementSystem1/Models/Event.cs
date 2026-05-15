using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventManagementSystem1.Models
{
    public class Event
    {
        [Key]
        public int EventId { get; set; }

        [Required(ErrorMessage = "Event title is required")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string Title { get; set; }

        [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
        public string Description { get; set; }

        [Display(Name = "Category")]
        public int? CategoryId { get; set; }

        [Required(ErrorMessage = "Event date is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Event Date")]
        public DateTime EventDate { get; set; }

        [Required(ErrorMessage = "Start time is required")]
        [DataType(DataType.Time)]
        [Display(Name = "Start Time")]
        public TimeSpan StartTime { get; set; }

        [DataType(DataType.Time)]
        [Display(Name = "End Time")]
        public TimeSpan? EndTime { get; set; }

        [Required(ErrorMessage = "Venue is required")]
        [Display(Name = "Venue")]
        public int VenueId { get; set; }

        [Required(ErrorMessage = "Maximum participants is required")]
        [Range(1, 10000, ErrorMessage = "Maximum participants must be between 1 and 10000")]
        [Display(Name = "Maximum Participants")]
        public int MaxParticipants { get; set; }

        [Display(Name = "Registration Deadline")]
        public DateTime? RegistrationDeadline { get; set; }

        [Range(0, 9999.99, ErrorMessage = "Price must be between 0 and 9999.99")]
        public decimal Price { get; set; } = 0;

        [Display(Name = "Free Event")]
        public bool IsFree { get; set; } = true;

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Public Event")]
        public bool IsPublic { get; set; } = true;

        [Display(Name = "Event Image")]
        public string ImageUrl { get; set; }

        [Required]
        [Display(Name = "Created By")]
        public int CreatedBy { get; set; }

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Updated At")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation properties (virtual for lazy loading)
        public virtual Category Category { get; set; }
        public virtual Venue Venue { get; set; }
        public virtual User Creator { get; set; }

        [NotMapped]
        [Display(Name = "Event Date & Time")]
        public string EventDateTime => $"{EventDate:MMM dd, yyyy} at {StartTime:hh\\:mm tt}";

        [NotMapped]
        public bool IsUpcoming => EventDate >= DateTime.Today;

        [NotMapped]
        public bool IsPast => EventDate < DateTime.Today;
    }
}