using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Milka.Models;
using Milka.Settings;

namespace Milka.Controllers
{
    public class HomeController : Controller
    {
        
        private IHostingEnvironment _env;
        private readonly IEmailSender _emailSender;
        private readonly MilkaDbContext _dbContext;

        public HomeController(MilkaDbContext DbContext, IHostingEnvironment env, IEmailSender emailSender)
        {
            _dbContext = DbContext;
            _env = env;
            _emailSender = emailSender;
        }
        // GET:/
        public async Task <ActionResult> Index()
        {
            ViewBag.ServerPath = _env.ContentRootPath;
            
            try{
                var Posts = await _dbContext.Posts.Select(x => x).OrderByDescending(o => o.Created).ToListAsync();
                return View(Posts);
            }
            catch (Exception ex)
            {
                return Ok(ex.Message);
            }
        }
        
        // POST:/Post
        [HttpGet]
        public IActionResult Post(int id)
        {
            ViewBag.ServerPath = _env.ContentRootPath;
            var post = _dbContext.Posts.First(x => x.Id == id);
            
            return View(post);
        }

        // POST:/AddSubscriber
        [HttpPost]
        public async Task<IActionResult> AddSubscriber(string Name, string Email, bool RulesAcceptance)
        {
            if (ModelState.IsValid)
            {
                var sub = new Subscriber
                {
                    Created = DateTime.Now, Email = Email, Name = Name, RulesAcceptance = RulesAcceptance
                };
                if (sub.RulesAcceptance != false)
                {
                    try
                    {
                        await _dbContext.AddAsync(sub);
                        await _dbContext.SaveChangesAsync();
                        await _emailSender.SendEmailsWithTemplate("Twoja subskrypcja newslettera.", sub, "ConfirmNewsletter");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
            
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> DeleteSubscriber(string email)
        {
            Subscriber entity = new Subscriber();

            try
            {
                if (!string.IsNullOrEmpty(email))
                {
                    entity = await _dbContext.Subscribers.FirstAsync(e => e.Email == Regex.Replace(email, "%40", "@"));
                }

            }
            catch (Exception ex)
            {
                 Console.WriteLine(ex.ToString());
            }

            return View(entity);
        }
        
        [HttpGet]
        public async Task<IActionResult> Delete(int Id)
        {
            Subscriber entity;
            
            try
            {
                entity = await _dbContext.Subscribers.FirstAsync(e => e.Id == Id);
                _dbContext.Subscribers.Remove(entity);
                await _dbContext.SaveChangesAsync();
                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return View();
        }
        
    }
}
