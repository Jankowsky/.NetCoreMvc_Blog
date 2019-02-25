using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Milka.Models;
using Milka.Settings;

namespace Milka.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private IHostingEnvironment _env;
        private readonly MilkaDbContext _dbContext;
        private UserManager<AppUser> _userManager;
        private IUserValidator<AppUser> _userValidator;
        private IPasswordValidator<AppUser> _passwordValidator;
        private IPasswordHasher<AppUser> _passwordHasher;
        private readonly IEmailSender _emailSender;

        public AdminController(MilkaDbContext DbContext,
            IHostingEnvironment env,
            UserManager<AppUser> usrMgr,
            IUserValidator<AppUser> userValid,
            IPasswordValidator<AppUser> passValid,
            IPasswordHasher<AppUser> passHash,
            IEmailSender emailSender)
        {
            _dbContext = DbContext;
            _env = env;
            _userManager = usrMgr;
            _userValidator = userValid;
            _passwordValidator = passValid;
            _passwordHasher = passHash;
            _emailSender = emailSender;
        }

        [HttpGet]
        public IActionResult Index() => View();

        [HttpGet]
        public ViewResult Users() => View(_userManager.Users);

        [HttpGet]
        public ViewResult CreateUser() => View();

        [HttpPost]
        public async Task<IActionResult> CreateUser(CreateUser model)
        {
            if (ModelState.IsValid)
            {
                AppUser user = new AppUser
                {
                    UserName = model.Name,
                    Email = model.Email
                };
                
                IdentityResult result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    return RedirectToAction("Users");
                }
                else
                {
                    foreach (IdentityError error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditUser(string id)
        {
            AppUser user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                return View(user);
            }
            else
            {
                return RedirectToAction("Users");
            }
        }


        [HttpPost]
        public async Task<IActionResult> EditUser(string id , string email, string password)
        {
            AppUser user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                user.Email = email;
                IdentityResult validEmail = await _userValidator.ValidateAsync(_userManager, user);
                if (!validEmail.Succeeded)
                {
                    AddErrorsFromResult(validEmail);
                }

                IdentityResult validPass = null;
                if (!string.IsNullOrEmpty(password))
                {
                    validPass = await _passwordValidator.ValidateAsync(_userManager, user, password);
                    if (validPass.Succeeded)
                    {
                        user.PasswordHash = _passwordHasher.HashPassword(user, password);
                    }
                    else
                    {
                        AddErrorsFromResult(validPass);
                    }        
                }

                if ((validEmail.Succeeded && validPass == null) || (validEmail.Succeeded && password != string.Empty && validPass.Succeeded))
                {
                    IdentityResult result = await _userManager.UpdateAsync(user);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Users");
                    }
                    else
                    {
                        AddErrorsFromResult(result);
                    }
                }
            }
            else
            {
                ModelState.AddModelError("", "User doesn't exist.");
            }

            return View(user);

        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(string id)
        {
            AppUser user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                IdentityResult result = await _userManager.DeleteAsync(user);

                if (result.Succeeded)
                {
                    return RedirectToAction("Users");
                }
                else
                {
                    AddErrorsFromResult(result);
                }
            }
            else
            {
                ModelState.AddModelError("", "User doesn't exist.");
            }

            return View("Users", _userManager.Users);
        }

        private void AddErrorsFromResult(IdentityResult result)
        {
            foreach (IdentityError error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
        }

        
        [HttpGet]
        public async Task <IActionResult> PostManagments()
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


        [HttpGet]
        public IActionResult AddPost() => View();
        


        [HttpPost]
        public async Task<IActionResult> AddPost(PostModel model, IFormFile file)
        {
            try
            {

                if (ModelState.IsValid)
                {
                    //max image size 
                    if (file == null || file.Length == 0 || file.Length > 5500000)
                        return View(model);

                    var path = Path.Combine(
                        _env.ContentRootPath, "src/images",
                        model.Title + Regex.Replace(file.ContentType, "/", "."));

                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    var post = new Post
                    {
                        Author = model.Author, Title = model.Title, PostText = model.PostText, Created = DateTime.Now,
                        Img1 = "/src/images/" + model.Title + Regex.Replace(file.ContentType, "/", ".")
                    };
                    await _dbContext.AddAsync(post);
                    await _dbContext.SaveChangesAsync();
                    
                    ////////  send email with new post and link to it
                    post.Img1 = "src/images/" + Regex.Replace(model.Title, " ", "%20") + Regex.Replace(file.ContentType, "/", ".");

                    List<Subscriber> subs = await _dbContext.Subscribers.Select(x => x).ToListAsync();
                    
                    await _emailSender.SendEmailsWithTemplate(post.Title,subs , post, "Post");
                }
                else
                {
                    return View(model);
                }
                return RedirectToAction("PostManagments");
            }
            catch (Exception ex)
            {
               Console.WriteLine(ex.Message); 
            }
            
            return View(model);
        }

        [HttpGet]
        public async Task <IActionResult> Edit(int id)
        {
            ViewBag.ServerPath = _env.ContentRootPath;
            var post = await _dbContext.Posts.FirstAsync(x => x.Id == id);
           
            return View(post);
        }
        
        [HttpPost]
        public async Task<IActionResult> Edit(Post post)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var entity = _dbContext.Posts.First(e => e.Id == post.Id);
                    entity.Author = post.Author;
                    entity.Title = post.Title;
                    entity.PostText = post.PostText;
                    entity.Edited = DateTime.UtcNow;
                    await _dbContext.SaveChangesAsync();

                    return RedirectToAction("PostManagments");
                }
                else
                {
                    return View(post);
                }
            }
            catch (Exception ex)
            {
                // process exception
               Console.WriteLine(ex.Message);
            }
            return View(post);
        }
        
        
        [HttpGet]
        public async Task <IActionResult> Details(int id)
        {
            ViewBag.ServerPath = _env.ContentRootPath;
            var post = _dbContext.Posts.FirstAsync(x => x.Id == id);
            
            return View(post);
        }

        public async Task<IActionResult> Delete(int Id)
        {
            try
            {
                var entity = await _dbContext.Posts.FirstAsync(e => e.Id == Id);
                _dbContext.Posts.Remove(entity);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());      
            }
            return RedirectToAction("PostManagments");
        }

        public async Task<IActionResult> DownloadImage(int Id)
        {
            try
            {
                var entity = await _dbContext.Posts.FirstAsync(e => e.Id == Id);

                var path = _env.ContentRootPath + entity.Img1;
                Stream stream = System.IO.File.OpenRead(path);

                if (stream == null)
                    return NotFound();

                return File(stream, "image/jpeg", entity.Title+".jpeg");
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }


        public async Task<IActionResult> UploadImage(int Id, IFormFile file)
        {
            if (file == null || file.Length == 0 || file.Length > 5500000)
                return RedirectToAction("PostManagments");
            
            var entity = _dbContext.Posts.First(e => e.Id == Id);
            
            var path = Path.Combine(
                _env.ContentRootPath, "src/images", entity.Title + Regex.Replace(file.ContentType, "/", "."));
            entity.Img1 = "/src/images/" + entity.Title + Regex.Replace(file.ContentType, "/", ".");
            entity.Edited = DateTime.UtcNow;
            using (var stream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            await _dbContext.SaveChangesAsync();
            
            return RedirectToAction("PostManagments");
        }
        
        public async Task<IActionResult> DeleteImage(int Id)
        {
            try
            {
                var entity = await _dbContext.Posts.FirstAsync(e => e.Id == Id);
                System.IO.File.Delete(_env.ContentRootPath + entity.Img1); 
                entity.Img1 = "";
                entity.Edited = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync();
               
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());      
            }
            return RedirectToAction("PostManagments");
        }

        [HttpGet]
        public async Task <IActionResult> Subscribers()
        {
            List<Subscriber> subscribers = new List<Subscriber>();
            try
            {
                subscribers = await _dbContext.Subscribers.Select(x => x).OrderByDescending(o => o.Email).ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return View(subscribers);
        }
        
        
        public async Task<IActionResult> DeleteSubscriber(int Id)
        {
            try
            {
                var entity = await _dbContext.Subscribers.FirstAsync(e => e.Id == Id);
                _dbContext.Subscribers.Remove(entity);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }

            return RedirectToAction("Subscribers");
        }
        
    }
}