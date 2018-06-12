using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AdUserResetPasswordWebTool.Models
{
    public class ResetPasswordModel
    {
        [Required, Display(Name = "User Principal Name")]
        public string UserPrincipalName { get; set; }

        public Guid RequestID { get; set; }

        [Required, Display(Name = "New Password")]
        public string NewPassword { get; set; }

        [System.ComponentModel.DataAnnotations.Compare("NewPassword"), Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; }
    }
}