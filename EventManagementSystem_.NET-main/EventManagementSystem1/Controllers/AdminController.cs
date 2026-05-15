using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using EventManagementSystem1.Models;

namespace EventManagementSystem1.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private DatabaseContext db = new DatabaseContext();

        public AdminController()
        {
            // Initialize database on first access
            try
            {
                db.Database.CreateIfNotExists();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Database initialization failed: {ex.Message}");
            }
        }

        public ActionResult Dashboard()
        {
            try { ViewBag.TotalEvents = db.Events.Count(); } catch { ViewBag.TotalEvents = 0; }
            try { ViewBag.TotalUsers = db.Users.Count(); } catch { ViewBag.TotalUsers = 0; }
            try { ViewBag.TotalParticipants = db.Participants.Count(); } catch { ViewBag.TotalParticipants = 0; }
            try { ViewBag.TotalVenues = db.Venues.Count(); } catch { ViewBag.TotalVenues = 0; }
            return View();
        }

        // Event Management
        public ActionResult CreateEvent()
        {
            try
            {
                ViewBag.Categories = db.Categories.ToList();
            }
            catch
            {
                ViewBag.Categories = new List<Category>();
            }

            try
            {
                ViewBag.Venues = db.Venues.ToList();
            }
            catch
            {
                ViewBag.Venues = new List<Venue>();
            }

            return View();
        }

        [HttpPost]
        public ActionResult CreateEvent(string title, string description, int? categoryId, int? venueId, DateTime eventDate, string startTime, int maxParticipants, decimal price)
        {
            try
            {
                // Check if user is logged in
                if (Session["UserId"] == null)
                {
                    ViewBag.Error = "You must be logged in to create events.";
                    try { ViewBag.Categories = db.Categories.ToList(); } catch { ViewBag.Categories = new List<Category>(); }
                    try { ViewBag.Venues = db.Venues.ToList(); } catch { ViewBag.Venues = new List<Venue>(); }
                    return View();
                }

                // Determine venue to use
                int selectedVenueId;
                if (venueId.HasValue && venueId.Value > 0)
                {
                    selectedVenueId = venueId.Value;
                }
                else
                {
                    // Ensure at least one venue exists
                    var defaultVenue = db.Venues.FirstOrDefault();
                    if (defaultVenue == null)
                    {
                        // Create a default venue if none exists
                        defaultVenue = new Venue
                        {
                            Name = "Default Venue",
                            Address = "123 Main Street",
                            City = "Default City",
                            State = "Default State",
                            ZipCode = "12345",
                            Capacity = 100,
                            ContactPhone = "000-000-0000",
                            ContactEmail = "admin@venue.com",
                            IsActive = true,
                            CreatedAt = DateTime.Now
                        };
                        db.Venues.Add(defaultVenue);
                        db.SaveChanges();
                    }
                    selectedVenueId = defaultVenue.VenueId;
                }

                var eventObj = new Event
                {
                    Title = title,
                    Description = description,
                    CategoryId = categoryId,
                    EventDate = eventDate,
                    StartTime = TimeSpan.Parse(startTime),
                    VenueId = selectedVenueId,
                    MaxParticipants = maxParticipants,
                    Price = price,
                    IsFree = price == 0,
                    CreatedBy = (int)Session["UserId"],
                    CreatedAt = DateTime.Now,
                    IsActive = true,
                    IsPublic = true
                };

                db.Events.Add(eventObj);
                db.SaveChanges();
                TempData["Success"] = "Event created successfully!";
                return RedirectToAction("Dashboard");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error creating event: " + ex.Message + (ex.InnerException != null ? " Inner: " + ex.InnerException.Message : "");
                try { ViewBag.Categories = db.Categories.ToList(); } catch { ViewBag.Categories = new List<Category>(); }
                try { ViewBag.Venues = db.Venues.ToList(); } catch { ViewBag.Venues = new List<Venue>(); }
                return View();
            }
        }

        // Venue Management
        public ActionResult AddVenue()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AddVenue(string name, string address, string city, string state, string zipCode, int capacity, string contactPhone, string contactEmail)
        {
            try
            {
                var venue = new Venue
                {
                    Name = name,
                    Address = address,
                    City = city,
                    State = state,
                    ZipCode = zipCode,
                    Capacity = capacity,
                    ContactPhone = contactPhone,
                    ContactEmail = contactEmail,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };

                db.Venues.Add(venue);
                db.SaveChanges();
                TempData["Success"] = "Venue added successfully!";
                return RedirectToAction("Dashboard");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error adding venue: " + ex.Message;
                return View();
            }
        }

        // Category Management
        public ActionResult AddCategory()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AddCategory(string name, string description)
        {
            try
            {
                var category = new Category
                {
                    Name = name,
                    Description = description,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };

                db.Categories.Add(category);
                db.SaveChanges();
                TempData["Success"] = "Category added successfully!";
                return RedirectToAction("Dashboard");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error adding category: " + ex.Message;
                return View();
            }
        }

        // Reports
        public ActionResult Reports()
        {
            try { ViewBag.TotalEvents = db.Events.Count(); } catch { ViewBag.TotalEvents = 0; }
            try { ViewBag.ActiveEvents = db.Events.Count(e => e.IsActive); } catch { ViewBag.ActiveEvents = 0; }
            try { ViewBag.TotalUsers = db.Users.Count(); } catch { ViewBag.TotalUsers = 0; }
            try { ViewBag.TotalRegistrations = db.Participants.Count(); } catch { ViewBag.TotalRegistrations = 0; }
            try { ViewBag.RecentEvents = db.Events.OrderByDescending(e => e.CreatedAt).Take(5).ToList(); } catch { ViewBag.RecentEvents = new List<Event>(); }
            return View();
        }

        // Manage Events
        public ActionResult ManageEvents()
        {
            try
            {
                ViewBag.Events = db.Events.OrderByDescending(e => e.CreatedAt).ToList();
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error loading events: " + ex.Message;
                ViewBag.Events = new List<Event>();
            }
            return View();
        }

        public ActionResult EditEvent(int id)
        {
            try
            {
                var eventObj = db.Events.Find(id);
                ViewBag.Event = eventObj;
                ViewBag.Categories = db.Categories.ToList();
                ViewBag.Venues = db.Venues.ToList();
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error loading event: " + ex.Message;
                return View();
            }
        }

        [HttpPost]
        public ActionResult EditEvent(int id, string title, string description, int categoryId, DateTime eventDate, string startTime, int venueId, int maxParticipants, decimal price, bool isActive)
        {
            try
            {
                var eventObj = db.Events.Find(id);
                if (eventObj != null)
                {
                    eventObj.Title = title;
                    eventObj.Description = description;
                    eventObj.CategoryId = categoryId;
                    eventObj.EventDate = eventDate;
                    eventObj.StartTime = TimeSpan.Parse(startTime);
                    eventObj.VenueId = venueId;
                    eventObj.MaxParticipants = maxParticipants;
                    eventObj.Price = price;
                    eventObj.IsFree = price == 0;
                    eventObj.IsActive = isActive;
                    eventObj.UpdatedAt = DateTime.Now;
                    db.SaveChanges();
                    TempData["Success"] = "Event updated successfully!";
                }
                return RedirectToAction("ManageEvents");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error updating event: " + ex.Message;
                ViewBag.Categories = db.Categories.ToList();
                ViewBag.Venues = db.Venues.ToList();
                return View();
            }
        }

        [HttpPost]
        public ActionResult DeleteEvent(int id)
        {
            try
            {
                var eventToDelete = db.Events.Find(id);
                if (eventToDelete != null)
                {
                    db.Events.Remove(eventToDelete);
                    db.SaveChanges();
                    TempData["Success"] = "Event deleted successfully!";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error deleting event: " + ex.Message;
            }
            return RedirectToAction("ManageEvents");
        }

        // Manage Participants
        public ActionResult ManageParticipants()
        {
            try
            {
                ViewBag.Participants = db.Participants.OrderByDescending(p => p.RegistrationDate).ToList();
                ViewBag.Users = db.Users.ToList();
                ViewBag.Events = db.Events.ToList();
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error loading participants: " + ex.Message;
                ViewBag.Participants = new List<Participant>();
                ViewBag.Users = new List<User>();
                ViewBag.Events = new List<Event>();
            }
            return View();
        }

        [HttpPost]
        public ActionResult DeleteParticipant(int id)
        {
            try
            {
                var participant = db.Participants.Find(id);
                if (participant != null)
                {
                    db.Participants.Remove(participant);
                    db.SaveChanges();
                    TempData["Success"] = "Participant removed successfully!";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error removing participant: " + ex.Message;
            }
            return RedirectToAction("ManageParticipants");
        }

        // Admin Profile
        public ActionResult AdminProfile()
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
        public ActionResult AdminProfile(string firstName, string lastName, string email, string phoneNumber)
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
                    TempData["Success"] = "Profile updated successfully!";
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error updating profile: " + ex.Message;
            }
            return RedirectToAction("AdminProfile");
        }

        // Manage Users
        public ActionResult ManageUsers()
        {
            try
            {
                ViewBag.Users = db.Users.OrderByDescending(u => u.CreatedAt).ToList();
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error loading users: " + ex.Message;
                return View();
            }
        }

        public ActionResult AddUser()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AddUser(string username, string password, string email, string firstName, string lastName, string phoneNumber, string role)
        {
            try
            {
                var user = new User
                {
                    Username = username,
                    Password = password,
                    Email = email,
                    FirstName = firstName,
                    LastName = lastName,
                    PhoneNumber = phoneNumber,
                    Role = role,
                    CreatedAt = DateTime.Now,
                    IsActive = true
                };

                db.Users.Add(user);
                db.SaveChanges();
                TempData["Success"] = "User added successfully!";
                return RedirectToAction("ManageUsers");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error adding user: " + ex.Message;
                return View();
            }
        }

        public ActionResult EditUser(int id)
        {
            try
            {
                var user = db.Users.Find(id);
                ViewBag.User = user;
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error loading user: " + ex.Message;
                return View();
            }
        }

        [HttpPost]
        public ActionResult EditUser(int id, string username, string email, string firstName, string lastName, string phoneNumber, string role, bool isActive)
        {
            try
            {
                var user = db.Users.Find(id);
                if (user != null)
                {
                    // Validate required fields manually
                    if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email) ||
                        string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
                    {
                        ViewBag.Error = "Username, Email, First Name, and Last Name are required.";
                        ViewBag.User = user;
                        return View();
                    }

                    user.Username = username;
                    user.Email = email;
                    user.FirstName = firstName;
                    user.LastName = lastName;
                    user.PhoneNumber = phoneNumber;
                    user.Role = role;
                    user.IsActive = isActive;

                    // Disable validation and save
                    db.Configuration.ValidateOnSaveEnabled = false;
                    db.SaveChanges();
                    db.Configuration.ValidateOnSaveEnabled = true;

                    TempData["Success"] = "User updated successfully!";
                }
                return RedirectToAction("ManageUsers");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error updating user: " + ex.Message;
                var user = db.Users.Find(id);
                ViewBag.User = user;
                return View();
            }
        }

        [HttpPost]
        public ActionResult DeleteUser(int id)
        {
            try
            {
                var user = db.Users.Find(id);
                if (user != null && user.UserId != (int)Session["UserId"])
                {
                    // Remove related participants first
                    var userParticipants = db.Participants.Where(p => p.UserId == id).ToList();
                    foreach (var participant in userParticipants)
                    {
                        db.Participants.Remove(participant);
                    }

                    // Remove events created by this user
                    var userEvents = db.Events.Where(e => e.CreatedBy == id).ToList();
                    foreach (var eventItem in userEvents)
                    {
                        // Remove participants for these events first
                        var eventParticipants = db.Participants.Where(p => p.EventId == eventItem.EventId).ToList();
                        foreach (var participant in eventParticipants)
                        {
                            db.Participants.Remove(participant);
                        }
                        db.Events.Remove(eventItem);
                    }

                    // Now remove the user
                    db.Users.Remove(user);
                    db.SaveChanges();
                    TempData["Success"] = "User deleted successfully!";
                }
                else
                {
                    TempData["Error"] = "Cannot delete your own account!";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error deleting user: " + ex.Message;
            }
            return RedirectToAction("ManageUsers");
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