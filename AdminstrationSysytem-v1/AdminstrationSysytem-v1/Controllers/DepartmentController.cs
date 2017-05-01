using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Administration.Models;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using Excel = Microsoft.Office.Interop.Excel;


namespace Administration.Controllers
{
    public class DepartmentController : Controller
    {
        ApplicationDbContext db = new ApplicationDbContext();
        // GET: Department
        public ActionResult Index()
        {
            var depList = db.Departments.Include("Instructors");
            return View(depList);
        }
        [HttpGet]
        public ActionResult create()
        {
            ViewBag.mng = new SelectList(db.Instructors, "id", "Name",1);
            return View();
        }
        [HttpPost]
        public ActionResult create(Departments department)
        {
            if(ModelState.IsValid)
            {
                db.Departments.Add(department);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            else
            {
                return View(department);
            }
            
        }
        [HttpGet]
        public ActionResult Edit(int DepartmentId)
        {
            Departments department = db.Departments.Where(e => e.DepartmentId == DepartmentId).Single(); 
            ViewBag.mng = new SelectList(db.Instructors, "id", "Name", 1);
            return PartialView("EditPartialView", department);
        }

        [HttpPost]
        public ActionResult Edit(Departments department)
        {
            Departments dep = db.Departments.FirstOrDefault(a => a.DepartmentId == department.DepartmentId);
            dep.Name = department.Name;
            dep.Capacity = department.Capacity;
            dep.InstructorId = department.InstructorId;
            db.Entry(dep).State = EntityState.Modified;
            db.SaveChanges();
            return View("Index",db.Departments);
        }

        [HttpGet]
        public ActionResult Delete(int DepartmentId)
        {
            Departments department = db.Departments.FirstOrDefault(d => d.DepartmentId == DepartmentId);
            return PartialView("DeletePartialView", department);
        }
        [HttpPost]
        public ActionResult DeleteSection(int DepartmentId)
        {
            Departments dep = db.Departments.FirstOrDefault(a => a.DepartmentId == DepartmentId);
            try
            {
                db.Departments.Remove(dep);
                db.SaveChanges();
            }
            catch (DbUpdateException e)
            {
                return Content(e.InnerException.ToString());
            }
            return RedirectToAction("index");
        }
        [HttpGet]
        public ActionResult Details()
        {
            ViewBag.deps = new SelectList(db.Departments, "DepartmentId", "Name", 1);
            return View(db.Departments.ToList()[0]);
        }
        [HttpPost]
        public ActionResult Details(int DepartmentId)
        {
           Departments  dep = db.Departments.FirstOrDefault(a => a.DepartmentId == DepartmentId);
            ViewBag.students = db.Students.Include(m => m.Departments).Where(d => d.DepartmentId == DepartmentId);
            return PartialView("SingleDepartmentView",dep);
        }
        [HttpGet]
          public ActionResult ChangeManager()   
         {
             ViewBag.deps = new SelectList(db.Departments, "DepartmentId", "Name", 1);
             return View();
         }
       
        [HttpGet]
        public ActionResult Manager(int DepartmentId)
        {
            //var inslist = (from st in db.Instructors where st.Department.DepartmentId == DepartmentId select st).ToList();
            var inslist = db.Instructors
                .Include(e => e.Department)
                .Where(f => f.Department.DepartmentId == DepartmentId).ToList();

            TempData["DeptId"] = DepartmentId; 
                
            return PartialView("InstructorsListPartialView", inslist);
        }
        [HttpPost]
        public ActionResult changeManagerNew()
        {
            var Dept = int.Parse(Request.Form["deptId"]);
            var dept= (from st in db.Departments where st.DepartmentId == Dept select st)
                .First();
            dept.InstructorId = Request.Form["insID"];
            db.SaveChanges();
            return RedirectToAction("index");
        }

        [HttpGet]
        public ActionResult showCourses(int DepartmentId)
        {
           
            var crslist = (from op in db.InstCrsDep
                           join pg in db.Courses on op.DepartmentId equals DepartmentId
                           where pg.CoursId == op.CoursId
                           select new { pg.Name, pg.LabDuration, pg.LectureDuration }).ToList();
            List<Courses> crs = new List<Courses>();

            foreach (var i in crslist)
            {
                crs.Add(new Courses() { Name = i.Name, LabDuration = i.LabDuration, LectureDuration = i.LectureDuration });
            }
            return View(crs);
        }

        public ActionResult showInstructors(int DepartmentId)
        {
           var inslist= db.Instructors.Include(m => m.Department).Where(d => d.Department.DepartmentId == DepartmentId);
            ViewBag.mng = db.Departments.Find(DepartmentId).InstructorId;
            return View(inslist);
        }

        [HttpGet]
        public ActionResult Import()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Import(HttpPostedFileBase excelFile)
        {
            if (excelFile == null || excelFile.ContentLength == 0)
            {
                ViewBag.error = "You must enter excel File";
                return View();
            }
            else
            {
                if (excelFile.FileName.EndsWith("xls") || excelFile.FileName.EndsWith("xlsx"))
                {
                    string path = Server.MapPath("~/Content/" + excelFile.FileName);
                    if (System.IO.File.Exists(path))
                        System.IO.File.Delete(path);
                    excelFile.SaveAs(path);
                    Excel.Application application = new Excel.Application();
                    Excel.Workbook workbook = application.Workbooks.Open(path);
                    Excel.Worksheet worksheet = workbook.ActiveSheet;
                    Excel.Range range = worksheet.UsedRange;

                    for (int i = 2; i < range.Rows.Count + 1; i++)
                    {
                        Departments dept = new Departments();
                        dept.Name = ((Excel.Range)range.Cells[i, 1]).Text;
                        dept.Capacity = int.Parse(((Excel.Range)range.Cells[i, 2]).Text);
                        string x = ((Excel.Range)range.Cells[i, 3]).Text;
                        if (x.Length < 1)
                            dept.InstructorId = null;
                        else
                            dept.InstructorId = x;
                        db.Departments.Add(dept);
                        db.SaveChanges();
                    }
                    var s = db.Departments.ToList();
                    return View("Index", s);
                }
                else
                {
                    ViewBag.error = "You must choose Excel File";
                    return PartialView();
                }

            }

        }
    }
}