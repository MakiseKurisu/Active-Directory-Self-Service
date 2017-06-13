using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AdUserResetPasswordWebTool.Models;
using System.DirectoryServices.AccountManagement;

namespace AdUserResetPasswordWebTool.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult Index()
        {
            var model = new ResetPasswordModel();

            return View(model);
        }

        // POST: Home: Index
        [HttpPost]
        public ActionResult Index(ResetPasswordModel model)
        {
            try
            {
                string AccountName = "ami@allegronet.co.il";
                string Password = "Eyhuaev312!";
                var pContext = new PrincipalContext(ContextType.Domain, Environment.UserDomainName, null, ContextOptions.Negotiate, AccountName, Password);
                var usrPrincipal = UserPrincipal.FindByIdentity(pContext, model.UserPrincipalName);

                if (usrPrincipal == null)
                {
                    throw new Exception("User principal was not found!");
                }

                usrPrincipal.ChangePassword(model.CurrentPassword, model.NewPassword);
                usrPrincipal.Save();

                return RedirectToAction("Index", new { msg = 1 });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("ActiveDirectory", ex.Message);
                if (model == null)
                {
                    model = new ResetPasswordModel();
                }
                return View(model);
            }
        }
    }
}
