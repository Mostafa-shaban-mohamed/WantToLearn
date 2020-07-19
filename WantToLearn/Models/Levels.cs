using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WantToLearn.Models
{
    public class Levels
    {
        public List<string> Lvls { get; set; }

        public List<string> GetLevels()
        {
            List<string> levels = new List<string>()
            {
                "Pri01",
                "Pri02",
                "Pri03",
                "Pri04",
                "Pri05",
                "Pri06",
                "Prep01",
                "Prep02",
                "Prep03",
                "Sec01",
                "Sec02",
                "Sec03"
            };

            return levels;
        }
    }
}