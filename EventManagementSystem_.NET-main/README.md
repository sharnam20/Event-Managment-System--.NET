# 🎉 Event Management System – .NET Web Application

Event Management System is a modern, scalable, and user-friendly web application built using the .NET framework. It allows users to explore events, register, and manage bookings, while admins can efficiently manage events, users, and schedules.

---

## 🚀 Features

👤 User Registration & Authentication  
📅 Browse and view available events  
🎟️ Book and manage event tickets  
🔍 Search and filter events  
👑 Admin Dashboard for event management  
🗂️ CRUD operations for events and users  
📊 Organized backend structure  
📱 Responsive and clean UI  

---

## 🛠️ Tech Stack

**Frontend:** HTML, CSS, JavaScript / Razor Views  
**Backend:** ASP.NET Core (.NET)  
**Database:** SQL Server  
**ORM:** Entity Framework Core  
**Authentication:** Identity / Custom Authentication  
**IDE:** Visual Studio / VS Code  

---

## 📁 Project Structure
```
EventManagementSystem/
├── Controllers/ # Handles application logic
├── Models/ # Data models and entities
├── Views/ # UI pages (Razor / MVC Views)
├── Data/ # Database context and migrations
├── wwwroot/ # Static files (CSS, JS, images)
├── appsettings.json # Configuration file
├── Program.cs # Entry point
├── Startup.cs # Middleware & services config
└── README.md # Project documentation


---

## 🧑‍💻 How to Run Locally

```bash
# Clone the repository
git clone https://github.com/ompatel2109/EventManagementSystem_NET.git

# Open project
cd EventManagementSystem_NET

# Restore dependencies
dotnet restore

# Run the project
dotnet run

⚙️ Configuration

Update your database connection string in:

appsettings.json

Example:

"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER;Database=EventDB;Trusted_Connection=True;"
}


