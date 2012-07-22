#region Copyright (C) 2011-2012 MPExtended
// Copyright (C) 2011-2012 MPExtended Developers, http://mpextended.github.com/
// 
// MPExtended is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MPExtended is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MPExtended. If not, see <http://www.gnu.org/licenses/>.
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MPExtended.Applications.WebMediaPortal.Code;
using MPExtended.Applications.WebMediaPortal.Models;
using MPExtended.Libraries.Client;
using MPExtended.Services.TVAccessService.Interfaces;
using MPExtended.Services.Common.Interfaces;

namespace MPExtended.Applications.WebMediaPortal.Controllers
{
    [ServiceAuthorize]
    public class ScheduleController : BaseController
    {
        //
        // GET: /Schedule/
        public ActionResult Index()
        {
            var list = MPEServices.TAS.GetSchedules(WebSortField.Name, WebSortOrder.Asc).Select(x => new ScheduleViewModel(x));
            return View(list);
        }

        public ActionResult DeleteSchedule(int programId)
        {
            var program = MPEServices.TAS.GetProgramDetailedById(programId);
            int id = MPEServices.TAS.GetSchedules().Where(p => p.IdChannel == program.IdChannel && p.StartTime == program.StartTime && p.EndTime == program.EndTime).First().Id;
            MPEServices.TAS.DeleteSchedule(id);
            return RedirectToAction("ProgramDetails", "Television", new { programId = programId });
        }

        public ActionResult DeleteScheduleById(int id)
        {
            MPEServices.TAS.DeleteSchedule(id);
            return RedirectToAction("Index");
        }

        public ActionResult AddSchedule(int? programId = null)
        {
            if (programId != null && programId != 0)
            {
                var program = MPEServices.TAS.GetProgramDetailedById(programId.Value);
                if (program == null)
                {
                    return HttpNotFound();
                }
                return View("AddScheduleByProgram", new ScheduleViewModel(program));
            }
            else
            {
                return View("AddScheduleForm", new ScheduleViewModel());
            }
        }

        public ActionResult EditSchedule(int id)
        {
            var schedule = MPEServices.TAS.GetScheduleById(id);
            return View(new ScheduleViewModel(schedule));
        }

        [HttpPost]
        public ActionResult Save(ScheduleViewModel model)
        {
            // show view again if user failed to fill in correctly
            if (!ModelState.IsValid)
            {
                return AddSchedule(model.ProgramId);
            }

            // delete old schedule if this is an edit
            if (model.Id != 0)
            {
                MPEServices.TAS.DeleteSchedule(model.Id);
            }

            // add schedule
            MPEServices.TAS.AddSchedule(model.Channel, model.Title, model.StartTime.Value, model.EndTime.Value, model.ScheduleType);

            // redirect
            if (model.ProgramId != 0)
            {
                return RedirectToAction("ProgramDetails", "Television", new { programId = model.ProgramId });
            }
            else
            {
                return RedirectToAction("Index");
            }
        }
    }
}
