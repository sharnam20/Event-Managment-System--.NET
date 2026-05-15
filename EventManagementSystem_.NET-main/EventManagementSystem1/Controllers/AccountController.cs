using EventManagementSystem1.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace EventManagementSystem1.Controllers
{
    public class AccountController : Controller
    {
        private DatabaseContext db = new DatabaseContext();

        // GET: Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [AllowAnonymous]
        public ActionResult Login(string username, string password, string returnUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    ViewBag.Error = "Username and password are required";
                    return View();
                }

                // Check if database connection works
                var totalUsers = db.Users.Count();

                // Try to find user by username first
                var userByUsername = db.Users.FirstOrDefault(u => u.Username == username);

                if (userByUsername == null)
                {
                    ViewBag.Error = $"Username '{username}' not found. Total users in database: {totalUsers}";
                    return View();
                }

                // Check password
                if (userByUsername.Password != password)
                {
                    ViewBag.Error = "Incorrect password";
                    return View();
                }

                // Login successful
                FormsAuthentication.SetAuthCookie(userByUsername.Username, false);
                Session["UserId"] = userByUsername.UserId;
                Session["Username"] = userByUsername.Username;
                Session["FullName"] = userByUsername.FirstName + " " + userByUsername.LastName;
                Session["Role"] = userByUsername.Role;

                // Redirect based on role
                if (userByUsername.Role == "Admin")
                    return RedirectToAction("Dashboard", "Admin");
                else
                    return RedirectToAction("Dashboard", "User");
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Database error: {ex.Message}";
                return View();
            }
        }

        // GET: Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [AllowAnonymous]
        public ActionResult Register(string Username, string Password, string Email, string FirstName, string LastName, string PhoneNumber)
        {
            ViewBag.Error = null; // Clear any previous errors

            try
            {
                // Debug: Show what we received
                ViewBag.Debug = $"Received: Username={Username}, Email={Email}, FirstName={FirstName}, LastName={LastName}";

                // Basic validation
                if (string.IsNullOrWhiteSpace(Username))
                {
                    ViewBag.Error = "Username is required";
                    return View();
                }
                if (string.IsNullOrWhiteSpace(Password))
                {
                    ViewBag.Error = "Password is required";
                    return View();
                }
                if (string.IsNullOrWhiteSpace(Email))
                {
                    ViewBag.Error = "Email is required";
                    return View();
                }
                if (string.IsNullOrWhiteSpace(FirstName))
                {
                    ViewBag.Error = "First Name is required";
                    return View();
                }
                if (string.IsNullOrWhiteSpace(LastName))
                {
                    ViewBag.Error = "Last Name is required";
                    return View();
                }

                // Check database connection
                var userCount = db.Users.Count();

                // Check if username exists
                var existingUser = db.Users.FirstOrDefault(u => u.Username == Username);
                if (existingUser != null)
                {
                    ViewBag.Error = "Username already exists. Please choose a different username.";
                    return View();
                }

                // Check if email exists
                var existingEmail = db.Users.FirstOrDefault(u => u.Email == Email);
                if (existingEmail != null)
                {
                    ViewBag.Error = "Email already exists. Please use a different email.";
                    return View();
                }

                // Validate password length (model requires minimum 6 characters)
                if (Password.Length < 6)
                {
                    ViewBag.Error = "Password must be at least 6 characters long.";
                    return View();
                }

                // Create new user with all required properties
                var newUser = new User
                {
                    Username = Username.Trim(),
                    Password = Password,
                    ConfirmPassword = Password, // Set to same as Password to satisfy validation
                    Email = Email.Trim(),
                    FirstName = FirstName.Trim(),
                    LastName = LastName.Trim(),
                    PhoneNumber = string.IsNullOrWhiteSpace(PhoneNumber) ? null : PhoneNumber.Trim(),
                    Role = "User",
                    IsActive = true,
                    CreatedAt = DateTime.Now,
                    LastLoginAt = null,
                    ProfileImage = null
                };

                // Add to database
                db.Users.Add(newUser);
                db.SaveChanges();

                TempData["Success"] = "Registration successful! You can now login with your credentials.";
                return RedirectToAction("Login");
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                var errorMessages = new List<string>();
                foreach (var validationErrors in ex.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        errorMessages.Add($"{validationError.PropertyName}: {validationError.ErrorMessage}");
                    }
                }
                ViewBag.Error = "Validation failed: " + string.Join("; ", errorMessages);
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Registration failed: {ex.Message}";
                if (ex.InnerException != null)
                {
                    ViewBag.Error += $" Inner: {ex.InnerException.Message}";
                }
                return View();
            }
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Clear();
            Session.Abandon();

            // Clear any cached authentication
            Response.Cache.SetExpires(DateTime.UtcNow.AddMinutes(-1));
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();

            return RedirectToAction("Login");
        }

        // Clear session to fix anti-forgery token issues
        [AllowAnonymous]
        public ActionResult ClearSession()
        {
            Session.Clear();
            Session.Abandon();
            FormsAuthentication.SignOut();
            return RedirectToAction("Login");
        }

        // Helper action to create test users if they don't exist
        [AllowAnonymous]
        public ActionResult CreateTestUsers()
        {
            try
            {
                // Check if admin user exists
                if (!db.Users.Any(u => u.Username == "admin"))
                {
                    var admin = new User
                    {
                        Username = "admin",
                        Password = "admin123",
                        Email = "admin@eventhub.com",
                        FirstName = "Admin",
                        LastName = "User",
                        PhoneNumber = "1234567890",
                        Role = "Admin",
                        CreatedAt = DateTime.Now,
                        IsActive = true
                    };
                    db.Users.Add(admin);
                }

                // Check if test user exists
                if (!db.Users.Any(u => u.Username == "testuser"))
                {
                    var testUser = new User
                    {
                        Username = "testuser",
                        Password = "user123",
                        Email = "test@eventhub.com",
                        FirstName = "Test",
                        LastName = "User",
                        PhoneNumber = "0987654321",
                        Role = "User",
                        CreatedAt = DateTime.Now,
                        IsActive = true
                    };
                    db.Users.Add(testUser);
                }

                db.SaveChanges();
                TempData["Success"] = "Test users created successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error creating test users: {ex.Message}";
            }

            return RedirectToAction("Login");
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