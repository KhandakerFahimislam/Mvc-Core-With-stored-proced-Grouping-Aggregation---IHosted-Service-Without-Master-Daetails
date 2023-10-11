﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project_1273081_M09_P1.Models;
using Project_1273081_M09_P1.VeiwModels;
using Project_1273081_M09_P1.VeiwModels.Input;

namespace Project_1273081_M09_P1.Controllers
{
    public class TeachersController : Controller
    {
       
            private readonly TeacherDbContext db;
            private readonly IWebHostEnvironment env;
            public TeachersController(TeacherDbContext db, IWebHostEnvironment env)
            {
                this.db = db;
                this.env = env;
            }
            public async Task<IActionResult> Index()
            {
                return View(await db.Teachers.ToListAsync());
            }
            public async Task<IActionResult> Aggregates()
            {
                var data = await db.Subjects.Include(x => x.Teacher)
                    .ToListAsync();
                return View(data);
            }
            public IActionResult Grouping()
            {
                return View();
            }
            [HttpPost]
            public IActionResult Grouping(string groupby)
            {

                if (groupby == "teachername")
                {
                    var data = db.Subjects.Include(x => x.Teacher)
                   .ToList()
                   .GroupBy(x => x.Teacher?.TeacherName)
                   .Select(g => new GroupedData { Key = g.Key, Data = g })
                   .ToList();

                    return View("GroupingResult", data);
                }
                if (groupby == "year month")
                {
                    var data = db.Subjects.Include(x => x.Teacher)
                        .OrderByDescending(x => x.CourseDuration)
                   .ToList()
                   .GroupBy(x => $"{x.CourseDuration:MMM, yyyy}")
                   .Select(g => new GroupedData { Key = g.Key, Data = g })
                   .ToList();

                    return View("GroupingResult", data);
                }
                if (groupby == "count")
                {
                    var data = db.Subjects.Include(x => x.Teacher)
                        .OrderByDescending(x => x.CourseDuration)
                   .ToList()
                    .GroupBy(x => x.Teacher?.TeacherName)
                   .Select(g => new GroupedDataPrimitive<int> { Key = g.Key, Data = g.Count() })
                   .ToList();

                    return View("GroupingResultPrimitive", data);
                }
                if (groupby == "sum")
                 {
                var data = db.Subjects.Include(x => x.Teacher)

                    .ToList()
                    .GroupBy(x => x.SubjectId)
                    .Select(g => new GroupedDataPrimitive<int?> { Key = g.Key.ToString(), Data = g.Sum(x => x.ExrtaClass) })
                    .ToList();
                return View("GroupingResultPrimitive", data);
                }
                if (groupby == "min")
                {
                var data = db.Subjects.Include(x => x.Teacher)

                    .ToList()
                    .GroupBy(x => x.SubjectId)
                    .Select(g => new GroupedDataPrimitive<int?> { Key = g.Key.ToString(), Data = g.Min(x => x.ExrtaClass) })

                    .ToList();
                return View("GroupingResultPrimitive", data);
                }

               
                return RedirectToAction("Grouping");
            }
            public IActionResult Create()
            {
                return View();
            }
            [HttpPost]
            public async Task<IActionResult> Create(TeacherInputModel model)
            {
                if (ModelState.IsValid)
                {
                    var Teacher = new Teacher
                    {
                        TeacherName = model.TeacherName,
                        ExpectSalary = model.ExpectSalary,
                        Role = model.Role,
                        IsReadyToPrivateCoaching = model.IsReadyToPrivateCoaching
                    };
                    string ext = Path.GetExtension(model.Picture.FileName);
                    string fileName = Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + ext;
                    string savePath = Path.Combine(env.WebRootPath, "Pictures", fileName);
                    FileStream fs = new FileStream(savePath, FileMode.Create);
                    await model.Picture.CopyToAsync(fs);
                    Teacher.Picture = fileName;
                    fs.Close();
                    db.Database.ExecuteSqlInterpolated($"EXEC InsertTeacher {Teacher.TeacherName}, {Teacher.ExpectSalary}, {(int)Teacher.Role}, {Teacher.Picture}, {(model.IsReadyToPrivateCoaching ? 1 : 0)}");
                    return RedirectToAction("Index");
                }
                return View(model);
            }
            public async Task<IActionResult> Edit(int id)
            {
                var data = await db.Teachers.FirstOrDefaultAsync(x => x.TeacherId == id);
                if (data == null) return NotFound();
                return View(new TeacherEditModel
                {
                    TeacherId = data.TeacherId,
                    TeacherName = data.TeacherName,
                    ExpectSalary = data.ExpectSalary,
                    Role = data.Role ?? Role.AssistentTeacher,
                    IsReadyToPrivateCoaching = data.IsReadyToPrivateCoaching ?? false

                });
            }
            [HttpPost]
            public async Task<IActionResult> Edit(TeacherEditModel model)
            {
                if (ModelState.IsValid)
                {
                    var Teacher = await db.Teachers.FirstOrDefaultAsync(x => x.TeacherId == model.TeacherId);
                    if (Teacher == null) return NotFound();
                    Teacher.TeacherId = model.TeacherId;
                    Teacher.TeacherName = model.TeacherName;
                    Teacher.ExpectSalary = model.ExpectSalary;
                    Teacher.Role = model.Role;
                    Teacher.IsReadyToPrivateCoaching = model.IsReadyToPrivateCoaching;

                    if (model.Picture != null)
                    {
                        string ext = Path.GetExtension(model.Picture.FileName);
                        string fileName = Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + ext;
                        string savePath = Path.Combine(env.WebRootPath, "Pictures", fileName);
                        FileStream fs = new FileStream(savePath, FileMode.Create);
                        await model.Picture.CopyToAsync(fs);
                        Teacher.Picture = fileName;
                        fs.Close();
                    }
                    db.Database.ExecuteSqlInterpolated($"EXEC UpdateTeacher {Teacher.TeacherId}, {Teacher.TeacherName}, {Teacher.ExpectSalary}, {(int)Teacher.Role}, {Teacher.Picture}, {(model.IsReadyToPrivateCoaching ? 1 : 0)}");
                    return RedirectToAction("Index");
                }
                return View(model);
            }
            [HttpPost]
            public IActionResult Delete(int id)
            {
                db.Database.ExecuteSqlInterpolated($"EXEC DeleteTeacher {id}");
                return Json(new { success = true, id });
            }
        }
    }

