using OddJobsApp.Models.Data;
using OddJobsApp.Models.ViewModels.Account;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure.MappingViews;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace OddJobsApp.Controllers
{
    public class AccountController : Controller
    {
        // GET: /
        public ActionResult Index()
        {
            //confirm use is not logged in

            string username = User.Identity.Name;

            if (!string.IsNullOrEmpty(username))
                return Redirect("~/" + username);


            return View();
        }

        // POST: Account/CreateAccount
        [HttpPost]
        public ActionResult CreateAccount(UserVM model, HttpPostedFileBase file)
        {
            // init db
            DB db = new DB();

            //check model state
            if(!ModelState.IsValid)
            {
                return View("Index", model);
            }

            //make sure username is unique
            if(db.Users.Any(x => x.Username.Equals(model.Username))) {
                ModelState.AddModelError("", "Username " + model.Username + " is taken.");
                model.Username = "";
                return View("Index", model);
            }

            // create userdto
            UserDTO userDTO = new UserDTO()
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                EmailAddress = model.EmailAddress,
                Username = model.Username,
                Password = model.Password
            };

            //add to DTO
            db.Users.Add(userDTO);

            // save
            db.SaveChanges();

            // get inserted id
            int userId = userDTO.Id;

            //login user
            FormsAuthentication.SetAuthCookie(model.Username, false);

            //set uploads dir
            var uploadsDir = new DirectoryInfo(string.Format("{0}Uploads", Server.MapPath(@"\")));

            // check if a file was uploaded
            if(file != null && file.ContentLength > 0)
            {
                string ext = file.ContentType.ToLower();

                if (ext != "image/jpg" &&
                    ext != "image/jpeg" &&
                    ext != "image/pjpeg" &&
                    ext != "image/gif" &&
                    ext != "image/x-png" &&
                    ext != "image/png")
                {
                    ModelState.AddModelError("", "The image was not uploaded - wrong image extension.");
                    return View("Index", model);
                }

                //set image name
                string imageName = userId + ".jpg";

                //set image path
                var path = string.Format("{0}\\{1}", uploadsDir, imageName);

                //save image
                file.SaveAs(path);
            }

            //redirect
            return Redirect("~/" + model.Username);
        }

        // GET: /{username}
        public string Username(string username = "")
        {
            return username;
        }

        // GET: account/Logout
        [Authorize]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();


            return Redirect("~/");
        }
    }
}