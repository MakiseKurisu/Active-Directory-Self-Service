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
        [AcceptVerbs("GET", "HEAD")]
        public ActionResult Index(string id)
        {
            var model = new ResetPasswordModel();

            if (id != null)
            {
                model.UserPrincipalName = id;
            }

            return View(model);
        }

        // POST: Home: Index
        [HttpPost]
        public ActionResult Index(ResetPasswordModel model)
        {
            try
            {
                string AccountName;
                string Password;
                var pContext = new PrincipalContext(ContextType.Domain, Environment.UserDomainName, null, ContextOptions.Negotiate, AccountName, Password);

                var usrPrincipal = UserPrincipal.FindByIdentity(pContext, model.UserPrincipalName);

                if (usrPrincipal == null)
                {
                    throw new Exception("User principal was not found!");
                }

                usrPrincipal.ChangePassword(model.CurrentPassword, model.NewPassword);
                if (usrPrincipal.IsAccountLockedOut())
                    usrPrincipal.UnlockAccount();
                usrPrincipal.Save();
                usrPrincipal.Dispose();

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
