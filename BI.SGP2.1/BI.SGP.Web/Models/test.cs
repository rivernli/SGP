using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;

namespace BI.SGP.Web.Models
{
    public  class test
    {
        public  int id { get; set; }
        public string textname { get { return "SSS"; } }

        public static string  ssss()
        {
            
            StringBuilder sb = new StringBuilder();
 
            sb.Append(@"<script type=""text/javascript"">
                            $(document).ready(function () {
                                    
                                    $('#btnSave').click(function () {
                                            $.ajax({
                                            type: 'Post',
                                            url:  ");
            
            sb.Append(@"""Pricing/SaveData"",  
                                            data: {ID:""ID"",Name:""YYYYY"",KeY:""kkkll""},
                                            datatype:'json',
                                            success: function (data) {
                                                alert(JSON.stringify(data));
                                                alert(data.b);
                                            }
                                        });                     
                                    });
                                });
                            </script>");

             return sb.ToString();
        }


    }
}