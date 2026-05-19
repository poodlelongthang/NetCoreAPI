
using PTPMQL_MVC.Data;
using PTPMQL_MVC.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PTPMQL_MVC.Models.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using DocumentFormat.OpenXml.InkML;
namespace PTPMQL_MVC.Controllers
{
    public class StudentController(ApplicationDbContext context) : Controller
    {
        private readonly ApplicationDbContext _context = context;
        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> GetStudents(int page = 1, int pageSize = 10)
        {
            var query = _context.Students
                .Include(s => s.Faculty)
                .AsNoTracking()
                .OrderByDescending(x => x.StudentCode);

            var totalItems = await query.CountAsync();

            var students = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = new PagedResult<Student>
            {
                Items = students,
                CurrentPage = page,
                PageSize = pageSize,
                TotalItems = totalItems
            };

            return PartialView("_StudentTable", result);
        }
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.FacultyId = new SelectList(
                _context.Faculties,"FacultyId","FacultyName"
            );

            return PartialView("Create");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Student student)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.FacultyId = new SelectList(
                _context.Faculties,
                    "FacultyId",
                    "FacultyName",
                student.FacultyId
                );
                return PartialView("Create", student);
            }

            _context.Students.Add(student);

            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true
            });
        }
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var student = await _context.Students.FindAsync(id);

            if (student == null)
            {
                return NotFound();
            }
            ViewBag.FacultyId = new SelectList(
            _context.Faculties,
                "FacultyId",
                "FacultyName",
            student.FacultyId
            );
            return PartialView("Edit", student);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Student student)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("Edit", student);
            }

            var existingStudent = await _context.Students.FindAsync(student.StudentCode);

            if (existingStudent == null)
            {
                return NotFound();
            }

            existingStudent.StudentCode = student.StudentCode;
            existingStudent.FullName = student.FullName;
            existingStudent.FacultyId = student.FacultyId;

            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true
            });
        }
        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            var student = await _context.Students
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.StudentCode == id);

            if (student == null)
            {
                return NotFound();
            }

            return PartialView("Delete", student);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Student student)
        {
            var existingStudent = await _context.Students
                .FindAsync(student.StudentCode);

            if (existingStudent == null)
            {
                return Json(new
                {
                    success = false
                });
            }

            _context.Students.Remove(existingStudent);

            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true
            });
    }
}
}