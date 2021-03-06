﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Schedule_CodeFirstModel.Models;
using Schedule_CodeFirstModel.Domain.Models;

namespace Schedule_CodeFirstModel.Controllers
{
    public class SchedulesController : Controller
    {
        private ScheduleContext db = new ScheduleContext();

        // GET: Schedules
        /// <summary>
        /// Get Schedule for the specific group
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ActionResult> Index(int id)
        {
            var days = new List<string>() { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat" };
            ViewBag.Days = days;
            Schedule schedule = new Schedule();
            var scheduleList = schedule.GetSchedule(id);
            return View(scheduleList);
        }

        public ActionResult GeneratePDF(int id)
        {
            return new Rotativa.ActionAsPdf("Index",id);
        }

        /// <summary>
        /// Get Schedule for the specific teacher
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult ScheduleTeacher(int id)
        {
            var days = new List<string>() { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat" };
            ViewBag.Days = days;
            TeacherSchedule schedule = new TeacherSchedule();
            var scheduleList = schedule.GetSchedule(id);
            return View(scheduleList);
        }

        // GET: Schedules/Create
        public ActionResult Create()
        {
            SelectList groups = new SelectList(db.Groups, "Id", "GroupName");
            ViewBag.Groups = groups;
            SelectList teachers = new SelectList(db.Teachers, "Id", "Name");
            ViewBag.Teachers = teachers;
            SelectList rooms = new SelectList(db.Rooms, "Id", "Number", "PlacesAmount");
            ViewBag.Rooms = rooms;
            SelectList subj = new SelectList(db.Subjects, "Id", "SubjectName");
            ViewBag.Subjects = subj;
            SelectList classes = new SelectList(db.Classes, "Id", "Number");
            ViewBag.Classes = classes;

            return View();
        }

        // POST: Schedules/Create
        /// <summary>
        /// Creates new Schedule
        /// </summary>
        /// <param name="schedule"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,Day,ClassId,WeekNumber,TeacherId,RoomId,SubjectId,GroupId")] Schedule schedule)
        {
            var checkIfScheduleIsAlreadyCreated = db.Schedules.Where(x => x.Group.Id == schedule.GroupId)
                                                              .Where(x => x.Day == schedule.Day)
                                                              .Where(x => x.Class.Id == schedule.ClassId)
                                                              .Where(x => x.WeekNumber == schedule.WeekNumber).FirstOrDefault();

            var checkIfTeacherIsAlreadyTaken = db.Schedules.Where(x => x.GroupId != schedule.GroupId)
                                                           .Where(x => x.RoomId != schedule.RoomId)
                                                           .Where(x => x.Room.PlacesAmount > 20)
                                                           .Where(x => x.SubjectId == schedule.SubjectId)
                                                           .Where(x => x.TeacherId == schedule.TeacherId)
                                                           .Where(x => x.Day == schedule.Day)
                                                           .Where(x => x.Class.Id == schedule.ClassId)
                                                           .Where(x => x.WeekNumber == schedule.WeekNumber).
                                                           FirstOrDefault();

            if (checkIfScheduleIsAlreadyCreated != null)
                ModelState.AddModelError("", "Schedule for this class is already created! You can Edit it on the form");

            if (checkIfTeacherIsAlreadyTaken != null)
                ModelState.AddModelError("", "This teacher already has class at this time");

            if (!ModelState.IsValid)
            {
                SelectList teachers = new SelectList(db.Teachers, "Id", "Name");
                ViewBag.Teachers = teachers;
                SelectList rooms = new SelectList(db.Rooms, "Id", "Number", "PlacesAmount");
                ViewBag.Rooms = rooms;
                SelectList subj = new SelectList(db.Subjects, "Id", "SubjectName");
                ViewBag.Subjects = subj;
                SelectList classes = new SelectList(db.Classes, "Id", "Number");
                ViewBag.Classes = classes;
                SelectList groups = new SelectList(db.Groups, "Id", "GroupName");
                ViewBag.Groups = groups;
            }
            else
            {
                db.Schedules.Add(schedule);
                await db.SaveChangesAsync();
                return RedirectToAction("Index/" + schedule.GroupId);
            }

            return View(schedule);
        }

        // GET: Schedules/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Schedule schedule = await db.Schedules.FindAsync(id);
            if (schedule == null)
            {
                return HttpNotFound();
            }

            SelectList rooms = new SelectList(db.Rooms, "Id", "Number", "PlacesAmount",schedule.RoomId);
            ViewBag.Rooms = rooms;
            SelectList subj = new SelectList(db.Subjects, "Id", "SubjectName",schedule.SubjectId);
            ViewBag.Subjects = subj;
            SelectList groups = new SelectList(db.Groups, "Id", "GroupName");
            ViewBag.Groups = groups;
            SelectList classes = new SelectList(db.Classes, "Id", "Number");
            ViewBag.Classes = classes;
            SelectList teachers = new SelectList(db.Teachers, "Id", "Name", schedule.TeacherId);
            ViewBag.Teachers = teachers;
            return View(schedule);
        }

        // POST: Schedules/Edit/5
        /// <summary>
        /// Update existing Schedule for one class
        /// </summary>
        /// <param name="schedule"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,GroupId,Day,ClassId,WeekNumber,TeacherId,RoomId,SubjectId")] Schedule schedule)
        {
            var checkIfTeacherIsAlreadyTaken = db.Schedules.Where(x => x.GroupId != schedule.GroupId)
                                                           .Where(x => x.RoomId!=schedule.RoomId)
                                                           .Where(x => x.Room.PlacesAmount>20)
                                                           .Where(x => x.SubjectId==schedule.SubjectId)
                                                           .Where(x => x.TeacherId == schedule.TeacherId)
                                                           .Where(x => x.Day == schedule.Day)
                                                           .Where(x => x.Class.Id == schedule.ClassId)
                                                           .Where(x => x.WeekNumber == schedule.WeekNumber).FirstOrDefault();

            if (checkIfTeacherIsAlreadyTaken != null)
                ModelState.AddModelError("SubjectId", "Teacher for this subject already has class at this time in group "+checkIfTeacherIsAlreadyTaken.Group.GroupName);

            if (!ModelState.IsValid)
            {
                SelectList rooms = new SelectList(db.Rooms, "Id", "Number", "PlacesAmount", schedule.RoomId);
                ViewBag.Rooms = rooms;
                SelectList subj = new SelectList(db.Subjects, "Id", "SubjectName", schedule.SubjectId);
                ViewBag.Subjects = subj;
                SelectList groups = new SelectList(db.Groups, "Id", "GroupName");
                ViewBag.Groups = groups;
                SelectList classes = new SelectList(db.Classes, "Id", "Number");
                ViewBag.Classes = classes;
            }
            else
            {
                db.Entry(schedule).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index/" + schedule.GroupId);
            }
            return View(schedule);
        }

        // GET: Schedules/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Schedule schedule = await db.Schedules.FindAsync(id);
            SelectList rooms = new SelectList(db.Rooms, "Id", "Number", "PlacesAmount", schedule.RoomId);
            ViewBag.Rooms = rooms;
            SelectList subj = new SelectList(db.Subjects, "Id", "SubjectName", schedule.SubjectId);
            ViewBag.Subjects = subj;
            SelectList groups = new SelectList(db.Groups, "Id", "GroupName");
            ViewBag.Groups = groups;
            SelectList classes = new SelectList(db.Classes, "Id", "Number");
            ViewBag.Classes = classes;
            if (schedule == null)
            {
                return HttpNotFound();
            }
            return View(schedule);
        }

        // POST: Schedules/Delete/5
        /// <summary>
        /// Deletes specific Schedule for class
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Schedule schedule = await db.Schedules.FindAsync(id);
            db.Schedules.Remove(schedule);
            await db.SaveChangesAsync();
            return RedirectToAction("Index/" + schedule.GroupId);
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
