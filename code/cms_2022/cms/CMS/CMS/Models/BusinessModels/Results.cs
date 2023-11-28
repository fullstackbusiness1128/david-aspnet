using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CMS.Models.ViewModels
{
    public class ResultOptions
    {
        public Filter Filter { get; set; }
        public Results Results { get; set; }

        public ResultOptions()
        {
            Filter = new Filter();
            Results = new Results();
        }
    }

    public class Filter
    {
        public string GroupBy { get; set; }
        public Int64 DateFrom { get; set; }
        public Int64 DateTo { get; set; }
        public String FilterA { get; set; }
        public String FilterB { get; set; }
        public String FilterC { get; set; }
        public string userid { get; set; }
    }

    public class Results
    {
        public int pageSize { get; set; }
        public int pageNumber { get; set; }
        public string sortOrder { get; set; }
        public string sortField { get; set; }
        public string searchValue { get; set; }
        public string searchField { get; set; }
        public double totalResults { get; set; }
    }

}