using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using AdUserResetPasswordWebTool.Models;
using System.DirectoryServices.AccountManagement;
using System.Net.Mail;
using NPoco;
using System.Data.SqlClient;

namespace AdUserResetPasswordWebTool.Controllers
{
    public class HomeController : Controller
    {
        [AcceptVerbs("GET", "HEAD")]
        public ActionResult Index(string id)
        {
            return RedirectToAction("ForgetPassword");
        }

        [AcceptVerbs("GET", "HEAD")]
        public ActionResult ForgetPassword(string id)
        {
            var model = new ForgetPasswordModel();

            if (id != null)
            {
                model.UserPrincipalName = id;
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult ForgetPassword(ForgetPasswordModel model)
        {
            try
            {
                if (model.UserPrincipalName == null)
                {
                    throw new Exception("User Principal Name cannnot be empty.");
                }

                string Domain = WebConfigurationManager.AppSettings["ADDomain"];
                string AccountName = WebConfigurationManager.AppSettings["ADAdmin"];
                string Password = WebConfigurationManager.AppSettings["ADAdminPassword"];

                if (AccountName == null || Password == null || Domain == null)
                {
                    throw new Exception("Invalid AD Admin User Setting. Please contact IT support to update Web.config.");
                }

                var pContext = new PrincipalContext(ContextType.Domain, Domain, null, ContextOptions.Negotiate, AccountName, Password);

                var usrPrincipal = UserPrincipal.FindByIdentity(pContext, model.UserPrincipalName);

                if (usrPrincipal == null)
                {
                    throw new Exception("User principal was not found!");
                }

                if (String.IsNullOrEmpty(usrPrincipal.EmailAddress))
                {
                    throw new Exception("Found no Email address. please contact IT support.");
                }

                string ConnectionString = WebConfigurationManager.ConnectionStrings["MainDB"].ConnectionString;
                string SMTPServer = WebConfigurationManager.AppSettings["SMTPServer"];
                string FromAddress = WebConfigurationManager.AppSettings["FromAddress"];
                string AppDomain = WebConfigurationManager.AppSettings["AppDomain"];

                if (SMTPServer == null || FromAddress == null)
                {
                    throw new Exception("Invalid SMTP Setting. Please contact IT support to update Web.config.");
                }

                if (ConnectionString == null || AppDomain == null)
                {
                    throw new Exception("Invalid Server Setting. Please contact IT support to update Web.config.");
                }

                var req = new Request();
                req.RequestID = Guid.NewGuid();
                req.UserID = usrPrincipal.UserPrincipalName;
                req.Date = DateTime.Now;

                using (var db = new Database(new SqlConnection(ConnectionString)))
                {
                    db.Connection.Open();
                    db.Insert("Request", "RequestID", false, req);
                    db.Connection.Close();
                }

                SmtpClient smtp = new SmtpClient(SMTPServer);
                MailMessage message = new MailMessage(FromAddress, usrPrincipal.EmailAddress, "Forget Password", String.Format("Please go to the following link to complete the process: http://{0}/ResetPassword/{1}", AppDomain, req.RequestID.ToString()));
                smtp.Send(message);

                return RedirectToAction("ForgetPassword", new { result = "success" });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("ActiveDirectory", ex.Message);
                if (model == null)
                {
                    model = new ForgetPasswordModel();
                }
                return View(model);
            }
        }

        [AcceptVerbs("GET", "HEAD")]
        public ActionResult ResetPassword(string id)
        {
            try
            {
                var model = new ResetPasswordModel();
                if (Request["result"] == "success")
                {
                    return View(model);
                }

                var RequestID = id;
                if (RequestID == null)
                {
                    return RedirectToAction("ForgetPassword");
                }

                string ConnectionString = WebConfigurationManager.ConnectionStrings["MainDB"].ConnectionString;
                if (ConnectionString == null)
                {
                    throw new Exception("Invalid Server Setting. Please contact IT support to update Web.config.");
                }

                var req = new Request();
                using (var db = new Database(new SqlConnection(ConnectionString)))
                {
                    db.Connection.Open();
                    req = db.FirstOrDefault<Request>("where RequestID = @RequestID", new { RequestID = RequestID });
                    db.Connection.Close();
                }

                model.UserPrincipalName = req.UserID;
                model.RequestID = req.RequestID;
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("ActiveDirectory", ex.Message);
                var model = new ResetPasswordModel();
                return View(model);
            }
        }

        [HttpPost]
        public ActionResult ResetPassword(ResetPasswordModel model)
        {
            try
            {
                if (model.RequestID == Guid.Empty)
                {
                    throw new Exception("Invalid RequestID.");
                }

                string ConnectionString = WebConfigurationManager.ConnectionStrings["MainDB"].ConnectionString;
                if (ConnectionString == null)
                {
                    throw new Exception("Invalid Server Setting. Please contact IT support to update Web.config.");
                }

                var req = new Request();
                using (var db = new Database(new SqlConnection(ConnectionString)))
                {
                    db.Connection.Open();
                    req = db.FirstOrDefault<Request>("where RequestID = @RequestID", new { RequestID = model.RequestID });
                    db.Connection.Close();
                }

                if ((DateTime.Now - req.Date).TotalDays > 1)
                {
                    using (var db = new Database(new SqlConnection(ConnectionString)))
                    {
                        db.Connection.Open();
                        db.Delete<Request>(model.RequestID);
                        db.Connection.Close();
                    }
                    throw new Exception("This password reset link is expired. Please restart the process.");
                }

                string Domain = WebConfigurationManager.AppSettings["ADDomain"];
                string AccountName = WebConfigurationManager.AppSettings["ADAdmin"];
                string Password = WebConfigurationManager.AppSettings["ADAdminPassword"];

                if (AccountName == null || Password == null || Domain == null)
                {
                    throw new Exception("Invalid AD Admin User Setting. Please contact IT support to update Web.config.");
                }

                var pContext = new PrincipalContext(ContextType.Domain, Domain, null, ContextOptions.Negotiate, AccountName, Password);

                var usrPrincipal = UserPrincipal.FindByIdentity(pContext, req.UserID);

                if (usrPrincipal == null)
                {
                    throw new Exception("User principal was not found!");
                }

                usrPrincipal.SetPassword(model.NewPassword);
                if (usrPrincipal.IsAccountLockedOut())
                    usrPrincipal.UnlockAccount();
                usrPrincipal.Save();
                usrPrincipal.Dispose();

                using (var db = new Database(new SqlConnection(ConnectionString)))
                {
                    db.Connection.Open();
                    db.Delete<Request>(model.RequestID);
                    db.Connection.Close();
                }

                return RedirectToAction("ResetPassword", new { result = "success" });
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
