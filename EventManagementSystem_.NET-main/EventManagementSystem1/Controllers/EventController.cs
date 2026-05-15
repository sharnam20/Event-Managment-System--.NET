using EventManagementSystem1.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace EventManagementSystem1.Controllers
{
    public class EventController : Controller
    {
        private DatabaseContext db = new DatabaseContext();

        // GET: Event/PublicEvents
        [AllowAnonymous]
        public ActionResult PublicEvents()
        {
            try
            {
                var events = db.Events.Where(e => e.IsActive).ToList();
                return View(events);
            }
            catch
            {
                return View(new List<Event>());
            }
        }

        // GET: Event/Details/5
        [AllowAnonymous]
        public ActionResult Details(int id)
        {
            try
            {
                var eventModel = db.Events.Find(id);
                if (eventModel == null)
                {
                    return HttpNotFound();
                }
                return View(eventModel);
            }
            catch
            {
                return RedirectToAction("PublicEvents");
            }
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