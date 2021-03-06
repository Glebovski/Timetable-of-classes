﻿using Schedule_CodeFirstModel.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Xml;

namespace Schedule_CodeFirstModel.Controllers
{
    public class TeachersBookkeepingController : Controller
    {
        private ScheduleContext context;

        public TeachersBookkeepingController()
        {
            context = new ScheduleContext();
        }
        // GET: TeachersBookkeeping
        /// <summary>
        /// Gets info about specific bookkeeping teacher
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Index(int? id)
        {
            if (id == null)
                return Redirect("~/Universities/Index");
            else
            {
                CalculateActualSalary();
                var teachers = context.teachersBookkeepings.Include(x => x.Teachers).Where(x => x.Teachers.UniversityId == id).ToList();
                ViewBag.UniverId = id;
                return View(teachers);
            }
        }

        /// <summary>
        /// Calculates salary of the teacher based on parameters
        /// </summary>
        public void CalculateActualSalary()
        {
            var teachers = context.teachersBookkeepings.ToList();
            foreach (var item in teachers)
            {
                double tax = item.Salary * item.Taxes;
                item.ActualSalary = item.Salary - tax;
                double bonus = item.Salary * item.Bonus;
                item.ActualSalary = item.Salary + bonus;
                context.SaveChanges();
            }
        }

        /// <summary>
        /// Gets map with markers where teachers live
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult TeachersMap(int id)
        {
            XmlDocument doc = new XmlDocument();
            var xml = context.Database.SqlQuery<string>("GetTeachers @univerId="+id).ToList();
            doc.LoadXml(xml[0].ToString());
            doc.Save(@"D:\VSProjects\GIS_KURSACH\Schedule_CodeFirstModel\teachers.xml");
            return View();
        }

        // GET: TeachersBookkeeping/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: TeachersBookkeeping/Create
        public ActionResult Create()
        {
            List<Teacher> teachers = new List<Teacher>();
            var tId = context.teachersBookkeepings.Select(x=>x.Teachers).ToList();
            var tList = context.Teachers.ToList();
            foreach (var item in tList)
            {
                if (tId.All(x => x.Id != item.Id) && tId.Any(x => x.UniversityId == item.UniversityId))
                    teachers.Add(item);
            }
            SelectList list = new SelectList(teachers, "Id", "Name",context.Teachers.Select(x=>x.Id));
            ViewBag.Teachers = list;
            return View();
        }

        // POST: TeachersBookkeeping/Create
        /// <summary>
        /// Creates bookkeeping for a new Teacher
        /// </summary>
        /// <param name="teacher"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Create(TeachersBookkeeping teacher)
        {
            teacher.TeacherId = context.Teachers.Where(x => x.Id == teacher.Teachers.Id).Select(x=>x.Id).First();
            teacher.Teachers = context.Teachers.Find(teacher.TeacherId);
            try
            {
                context.teachersBookkeepings.Add(teacher);
                context.SaveChanges();
                return RedirectToAction("Index/" + teacher.Teachers.UniversityId);
            }
            catch
            {
                return View();
            }
        }

        // GET: TeachersBookkeeping/Edit/5
        public ActionResult Edit(int id)
        {
            return View(context.teachersBookkeepings.Include(x => x.Teachers).Where(x => x.Id == id).First());
        }

        // POST: TeachersBookkeeping/Edit/5
        /// <summary>
        /// Updates information about teacher's bookkeeping
        /// </summary>
        /// <param name="id"></param>
        /// <param name="teacher"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Edit(int id, [Bind(Include = "Id,TeacherId,Teachers,Hours,Experience_months,Salary,Taxes,Bonus")]TeachersBookkeeping teacher)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    context.Entry(teacher).State = EntityState.Modified;
                    context.SaveChanges();
                    return RedirectToAction("Index/"+teacher.Teachers.UniversityId);
                }
                return View(teacher);
            }
            catch(Exception ex)
            {
                return View(teacher);
            }
        }

        // GET: TeachersBookkeeping/Delete/5
        public ActionResult Delete(int id)
        {
            return View(context.teachersBookkeepings.Find(id));
        }

        // POST: TeachersBookkeeping/Delete/5
        /// <summary>
        /// Deletes teacher's bookkeeping
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost,ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                context.teachersBookkeepings.Remove(context.teachersBookkeepings.Find(id));
                context.SaveChanges();
                return RedirectToAction("Index");
            }
            catch
            {
                return View(context.teachersBookkeepings.Find(id));
            }
        }
    }
}
