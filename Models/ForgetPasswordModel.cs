using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AdUserResetPasswordWebTool.Models
{
    public class ForgetPasswordModel
    {
        [Required, Display(Name = "User Principal Name")]
        public string UserPrincipalName { get; set; }
    }
}