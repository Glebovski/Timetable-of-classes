﻿using MvcSchedule.Objects;
using Schedule_CodeFirstModel.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Schedule_CodeFirstModel.Controllers
{
    public class HomeController : Controller
    {
        private ScheduleContext context; 

        public HomeController()
        {
            context = new ScheduleContext();
        }

        /// <summary>
        /// Gets list of groups in the specific university
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ActionResult> Index(int? id)
        {
            if (id != null)
            {
                var groupsByUniver = await context.Groups.Include(s => s.Speciality).Include(c => c.Course).Include(st => st.Students).Where(x => x.UniversityId == id).ToListAsync();
                ViewBag.UniverId = id;
                return View(groupsByUniver);
            }
            else return Redirect("~/Universities/Index");
        }

        /// <summary>
        /// Generates PDF file based on list of groups
        /// </summary>
        /// <returns></returns>
        public ActionResult GeneratePDF()
        {
            return new Rotativa.ActionAsPdf("Index");
        }

        /// <summary>
        /// Filter groups by speciality
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ActionResult> GroupsBySpec(int id)
        {
            var groupsbyspec = await context.Groups.Where(x => x.Speciality.Id == id).Include(spec => spec.Speciality).Include(c => c.Course).ToListAsync();
            return View("Index", groupsbyspec);
        }

        // GET: Teachers/Create
        public ActionResult Create()
        {
            SelectList spec = new SelectList(context.Specialities, "Id", "Name");
            ViewBag.Specs = spec;
            SelectList course = new SelectList(context.Courses, "Id", "Number");
            ViewBag.Course = course;
            return View();
        }

        /// <summary>
        /// Creates new Group
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id, GroupName, CourseId, Students,SpecialityId")] Group group)
        {
            if (ModelState.IsValid)
            {
                context.Groups.Add(group);
                await context.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(group);
        }

        // GET: Teachers/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Group group = await context.Groups.FindAsync(id);

            if (group == null)
            {
                return HttpNotFound();
            }
            SelectList spec = new SelectList(context.Specialities, "Id", "Name",group.SpecialityId);
            ViewBag.Specs = spec;
            SelectList course = new SelectList(context.Courses, "Id", "Number",group.CourseId);
            ViewBag.Course = course;
            return View(group);
        }

        // POST: Teachers/Edit/5
        /// <summary>
        /// Updates existing group
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id, GroupName, CourseId, Students,SpecialityId")] Group group)
        {
            if (ModelState.IsValid)
            {
                context.Entry(group).State = EntityState.Modified;
                await context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(group);
        }

        // GET: Teachers/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Group group = await context.Groups.FindAsync(id);
            if (group == null)
            {
                return HttpNotFound();
            }
            return View(group);
        }

        // POST: Teachers/Delete/5
        /// <summary>
        /// Deletes specific group
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Group group = await context.Groups.FindAsync(id);
            context.Groups.Remove(group);
            await context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                context.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
