using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BI.SGP.Web.Models
{
    public class PagingGrid
    {
        public int TotalRows { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int TotalPages {
            get
            {
                double pages = (double)TotalRows / (double)PageSize;
                pages = Math.Ceiling(pages);
                return (int)pages;
            } 
        }

        public int StartIndex {
            get
            {
                int i = PageIndex * PageSize - PageSize + 1;
                return i;
            }
        
        }

        public int EndIndex { 
        
            get
            {
                int i = PageIndex * PageSize;
                return i;
            }
        
        }




        public string OrderField { get;set;}
        public string SortType { get; set; }

        public PagingGrid()
        {
            PageIndex = 1;
            PageSize = 15;
            TotalRows = 150;
        }
        public void Next()
        {
            if(PageIndex<PageSize)
            {

                PageIndex = PageIndex + 1;

            }


        }

    }
}