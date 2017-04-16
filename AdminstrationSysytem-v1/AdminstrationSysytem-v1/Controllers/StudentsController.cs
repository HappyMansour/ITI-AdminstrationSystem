﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AdminstrationSysytem_v1.Models;
using System.Data.Entity;

namespace AdminstrationSysytem_v1.Controllers
{
    [Authorize(Roles = "Admin")]
    public class StudentsController : Controller
    {

        ApplicationDbContext db = new ApplicationDbContext();

        // GET: Students
        public ActionResult StudentsList()
        {
            var Students = db.Students.ToList();
            return View(Students);
        }

        [HttpPost]
        public ActionResult ActivateAccounts()
        {
            var Students = db.Students.ToList();
            foreach (var item in Students)
            {
                if (item.Name == Request.Form[item.Name])
                {
                    item.IsActivated = true;
                    db.Entry(item).State = EntityState.Modified;
                }
            }

            db.SaveChanges();

            return RedirectToAction("StudentsList"); 
        }

        public ActionResult UserProfile()
        {

            return View();
        }
    }
}