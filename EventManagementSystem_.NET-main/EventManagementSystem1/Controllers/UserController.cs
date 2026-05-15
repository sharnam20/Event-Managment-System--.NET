using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using EventManagementSystem1.Models;

namespace EventManagementSystem1.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private DatabaseContext db = new DatabaseContext();

        public ActionResult Dashboard()
        {
            try
            {
                int userId = (int)Session["UserId"];
                ViewBag.TotalRegistrations = db.Participants.Count(p => p.UserId == userId);
                ViewBag.UpcomingEvents = db.Participants.Where(p => p.UserId == userId).Count();
                ViewBag.CompletedEvents = db.Participants.Where(p => p.UserId == userId && p.AttendanceStatus == "Attended").Count();
                return View();
            }
            catch
            {
                ViewBag.TotalRegistrations = 0;
                ViewBag.UpcomingEvents = 0;
                ViewBag.CompletedEvents = 0;
                return View();
            }
        }

        // Browse Events
        public ActionResult BrowseEvents()
        {
            try
            {
                // Get all events without filtering first
                var allEvents = db.Events.ToList();
                ViewBag.TotalEventsInDB = allEvents.Count;

                // Show ALL events for now (remove filtering to debug)
                ViewBag.Events = allEvents.OrderBy(e => e.EventDate).ToList();
                ViewBag.FilteredCount = allEvents.Count;

                // Debug info
                ViewBag.CurrentDate = DateTime.Today.ToString("yyyy-MM-dd");
                if (allEvents.Any())
                {
                    ViewBag.FirstEventDate = allEvents.First().EventDate.ToString("yyyy-MM-dd");
                    ViewBag.FirstEventActive = allEvents.First().IsActive;
                    ViewBag.FirstEventPublic = allEvents.First().IsPublic;
                }

                try
                {
                    ViewBag.Categories = db.Categories.ToList();
                }
                catch
                {
                    ViewBag.Categories = new List<Category>();
                }

                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error loading events: " + ex.Message;
                ViewBag.Events = new List<Event>();
                ViewBag.Categories = new List<Category>();
                return View();
            }
        }

        // My Events
        public ActionResult MyEvents()
        {
            try
            {
                int userId = (int)Session["UserId"];
                var myEvents = db.Participants.Where(p => p.UserId == userId)
                    .Select(p => p.Event).ToList();
                ViewBag.MyEvents = myEvents;
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error loading your events: " + ex.Message;
                return View();
            }
        }

        // Upcoming Events
        public ActionResult UpcomingEvents()
        {
            try
            {
                int userId = (int)Session["UserId"];
                var upcomingEvents = db.Participants.Where(p => p.UserId == userId && p.Event.EventDate >= DateTime.Today)
                    .Select(p => p.Event).OrderBy(e => e.EventDate).ToList();
                ViewBag.UpcomingEvents = upcomingEvents;
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error loading upcoming events: " + ex.Message;
                return View();
            }
        }

        // Event History
        public ActionResult EventHistory()
        {
            try
            {
                int userId = (int)Session["UserId"];
                var pastEvents = db.Participants.Where(p => p.UserId == userId && p.Event.EventDate < DateTime.Today)
                    .Select(p => p.Event).OrderByDescending(e => e.EventDate).ToList();
                ViewBag.PastEvents = pastEvents;
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error loading event history: " + ex.Message;
                return View();
            }
        }

        // My Tickets
        public ActionResult MyTickets()
        {
            try
            {
                int userId = (int)Session["UserId"];
                // For now, show registered events as tickets since Tickets table may not exist
                var myParticipations = db.Participants.Where(p => p.UserId == userId).ToList();
                ViewBag.Participations = myParticipations;
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error loading tickets: " + ex.Message;
                return View();
            }
        }

        // Register for Event
        [HttpPost]
        public ActionResult RegisterForEvent(int eventId)
        {
            try
            {
                int userId = (int)Session["UserId"];

                // Check if already registered
                if (db.Participants.Any(p => p.UserId == userId && p.EventId == eventId))
                {
                    TempData["Error"] = "You are already registered for this event!";
                    return RedirectToAction("BrowseEvents");
                }

                // Check event capacity
                var eventObj = db.Events.Find(eventId);
                var currentParticipants = db.Participants.Count(p => p.EventId == eventId);
                if (currentParticipants >= eventObj.MaxParticipants)
                {
                    TempData["Error"] = "Event is full!";
                    return RedirectToAction("BrowseEvents");
                }

                // Register participant
                var participant = new Participant
                {
                    UserId = userId,
                    EventId = eventId,
                    RegistrationDate = DateTime.Now,
                    AttendanceStatus = "Registered"
                };
                db.Participants.Add(participant);
                db.SaveChanges();

                TempData["Success"] = "Successfully registered for the event! Your digital pass is ready.";
                return RedirectToAction("MyEvents");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error registering for event: " + ex.Message;
                return RedirectToAction("BrowseEvents");
            }
        }

        // User Profile
        public ActionResult UserProfile()
        {
            try
            {
                int userId = (int)Session["UserId"];
                var user = db.Users.Find(userId);
                ViewBag.User = user;
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error loading profile: " + ex.Message;
                return View();
            }
        }

        [HttpPost]
        public ActionResult UserProfile(string firstName, string lastName, string email, string phoneNumber)
        {
            try
            {
                int userId = (int)Session["UserId"];
                var user = db.Users.Find(userId);
                if (user != null)
                {
                    user.FirstName = firstName;
                    user.LastName = lastName;
                    user.Email = email;
                    user.PhoneNumber = phoneNumber;
                    db.SaveChanges();
                    Session["FullName"] = firstName + " " + lastName;
                    TempData["Success"] = "Profile updated successfully!";
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error updating profile: " + ex.Message;
            }
            return RedirectToAction("UserProfile");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}