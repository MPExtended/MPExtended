using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MPExtended.Services.TVAccessService.Interfaces;

namespace MPExtended.Applications.WebMediaPortal.Models
{
    public class SingleTVProgramModel
    {
        public string Title { get; set; }
        public DateTime StartTime  { get; set; }
        public DateTime EndTime { get; set; }
        public string PercentageWidth { get; set; }
        public string CssClass { get; set; }
        public int IdProgram { get; set; }

        public SingleTVProgramModel(WebProgramDetailed program, DateTime overallStartTime, DateTime overallEndTime)
        {
            Title = program.Title;
            StartTime = program.StartTime;
            EndTime = program.EndTime;
            IdProgram = program.IdProgram;

            PercentageWidth = GetPercentageWidth(program, overallStartTime, overallEndTime);
            CssClass = GetCssClass(program); 
        }

        private string GetCssClass(WebProgramDetailed program)
        {
            if (program.IsScheduled)
                return "TVGuideCellProgramOnScheduled";
            else if (program.StartTime <= DateTime.Now && program.EndTime >= DateTime.Now)
                return "TVGuideCellProgramOnAir";
            else
                return "TVGuideCellProgram";
        }

        private string GetPercentageWidth(WebProgramDetailed program, DateTime overallStartTime, DateTime overallEndTime)
        {
            // This should be handled in the controller
            double totalDuration = (overallEndTime - overallStartTime).TotalMinutes;

            double percentage;

            if (program.StartTime <= overallStartTime)
            {
                percentage = 100.0 / totalDuration * (program.EndTime - overallStartTime).TotalMinutes;
            }
            else if (program.EndTime <= overallEndTime)
            {
                percentage = 100.0 / totalDuration * (program.EndTime - program.StartTime).TotalMinutes;
            }
            else
            {
                double width = (overallEndTime - program.StartTime).TotalMinutes;
                percentage = 100.0 / totalDuration * (width < 0 ? 0 : width);
            }

            if (percentage > 100)
            {
                percentage = 100;
            }

            if (percentage < 0)
            {
                percentage = 0;
            }

            return (percentage * 0.99).ToString(System.Globalization.CultureInfo.InvariantCulture) + "%";
        }
    }
}