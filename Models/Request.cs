using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NPoco;

namespace AdUserResetPasswordWebTool.Models
{
    [TableName("Request"), PrimaryKey("RequestID")]
    public class Request
    {
        public Guid RequestID { set; get; }
        public string UserID { set; get; }
        public DateTime Date { set; get; }
    }
}