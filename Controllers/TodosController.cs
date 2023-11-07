﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using 業務報告システム.Data;
using 業務報告システム.Models;

namespace 業務報告システム.Controllers
{
    public class TodosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TodosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Todos
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.todo.Include(t => t.User);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Todos/Create
        [Authorize]
        public IActionResult Create()
        {
            ViewData["UserId"] = new SelectList(_context.applicationuser, "Id", "Id");
            return View();
        }

        // POST: Todos/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TodoId,TaskName,Progress,StartDate,EndDate,Comment,UserId")] Todo todo)
        {
            ModelState.Remove("User");
            if (ModelState.IsValid)
            {
                _context.Add(todo);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(todo);
        }

        // GET: Todos/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.todo == null)
            {
                return NotFound();
            }

            var todo = await _context.todo.FindAsync(id);
            if (todo == null)
            {
                return NotFound();
            }
            //Todo Edit画面でタスクIDとログインユーザーIDが一致していない場合は「アクセス権がありません」と表示する。
            //追記=================================
            var loginUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!todo.UserId.Equals(loginUserId))
            {
                return NotFound("アクセス権がありません");
            }
            //追記=================================
            ViewData["UserId"] = new SelectList(_context.applicationuser, "Id", "Id", todo.UserId);
            return View(todo);
        }

        // POST: Todos/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TodoId,TaskName,Progress,StartDate,EndDate,Comment,UserId")] Todo todo)
        {
            if (id != todo.TodoId)
            {
                return NotFound();
            }

            ModelState.Remove("User");
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(todo);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TodoExists(todo.TodoId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserId"] = new SelectList(_context.applicationuser, "Id", "Id", todo.UserId);
            return View(todo);
        }

        // GET: Todos/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.todo == null)
            {
                return NotFound();
            }

            var todo = await _context.todo
                .Include(t => t.User)
                .FirstOrDefaultAsync(m => m.TodoId == id);
            if (todo == null)
            {
                return NotFound();
            }

            return View(todo);
        }

        // POST: Todos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.todo == null)
            {
                return Problem("Entity set 'ApplicationDbContext.todo'  is null.");
            }
            var todo = await _context.todo.FindAsync(id);
            //if (todo != null)
            //{
                _context.todo.Remove(todo);
           // }

            TempData["AlertTodo"] = "Todoを削除しました。";
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
           
            //return Redirect("/Todos/Index");
        }

        //Manager用************************************
        //*********************************************

        // GET: Todos/MgrCreate
        [Authorize]
        public IActionResult MgrCreate()
        {
            if (User.IsInRole("Manager"))
            {

                var users = _context.applicationuser.ToList();
                var members = users.Select(user => new SelectListItem
                {
                    Value = user.Id,
                    Text = $"{user.LastName} {user.FirstName}",
                });

                ViewBag.Members = new SelectList(members, "Value", "Text");

                return View();
            }
            return NotFound("アクセス権が有りません");
        }


        // POST: Todos/MgrCreate
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MgrCreate([Bind("TodoId,TaskName,Progress,StartDate,EndDate,Comment,UserId")] Todo todo)
        {
            //ModelState.Remove("User");
            //if (ModelState.IsValid)
            //{
            //    _context.Add(todo);
            //    await _context.SaveChangesAsync();
            //    return RedirectToAction(nameof(Index));
            //}
            return View(todo);
        }



        //************************************************

        private bool TodoExists(int id)
        {
          return (_context.todo?.Any(e => e.TodoId == id)).GetValueOrDefault();
        }
    }
}
