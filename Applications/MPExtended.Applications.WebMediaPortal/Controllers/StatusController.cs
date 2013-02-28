#region Copyright (C) 2012 MPExtended
// Copyright (C) 2012 MPExtended Developers, http://mpextended.github.com/
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
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Web;
using System.Web.Mvc;
using MPExtended.Applications.WebMediaPortal.Code;
using MPExtended.Applications.WebMediaPortal.Models;
using MPExtended.Libraries.Service;
using MPExtended.Services.Common.Interfaces;

namespace MPExtended.Applications.WebMediaPortal.Controllers
{
    [ServiceAuthorize]
    public class StatusController : BaseController
    {
        private static PerformanceCounter cpuCounter = new PerformanceCounter();
        private static PerformanceCounter memoryCounter = new PerformanceCounter();
        private static int totalMemory;

        static StatusController()
        {
            totalMemory = (int)(GetTotalMemoryBytes() / 1024 / 1024);
            cpuCounter.CategoryName = "Processor";
            cpuCounter.CounterName = "% Processor Time";
            cpuCounter.InstanceName = "_Total";
            memoryCounter.CategoryName = "Memory";
            memoryCounter.CounterName = "Available MBytes";
        }

        private static long GetTotalMemoryBytes()
        {
            ObjectQuery objectQuery = new ObjectQuery("SELECT TotalPhysicalMemory from Win32_ComputerSystem");
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(objectQuery);
            var enumerator = searcher.Get().GetEnumerator();
            if (!enumerator.MoveNext()) return 0;
            return Convert.ToInt64(enumerator.Current["TotalPhysicalMemory"]);
        }

        //
        // GET: /Status/
        public ActionResult Index()
        {
            var model = new StatusViewModel();

            if (Connections.Current.HasTASConnection)
            {
                model.AddDiskInfo(Connections.Current.TAS.GetAllRecordingDiskInformation());
                model.AddDiskInfo(Connections.Current.TAS.GetLocalDiskInformation());
                model.SetCardList(Connections.Current.TAS.GetCards(), Connections.Current.TAS.GetActiveCards());
            }

            if (Connections.Current.HasMASConnection)
            {
                model.AddDiskInfo(Connections.Current.MAS.GetLocalDiskInformation());
            }

            try
            {
                model.CpuUsage = (int)Math.Round(cpuCounter.NextValue());
                model.TotalMemoryMegaBytes = totalMemory;
                model.UsedMemoryMegaBytes = (int)Math.Round(totalMemory - memoryCounter.NextValue());
                model.HasSystemInformation = true;
            }
            catch (UnauthorizedAccessException)
            {
                Log.Info("Failed to read performance counters, got UnauthorizedAccessException");
                model.HasSystemInformation = false;
            }
            catch (Exception ex)
            {
                Log.Info("Failed to read performance counters", ex);
                model.HasSystemInformation = false;
            }

            return View(model);
        }

        public ActionResult Stop(string user)
        {
            Connections.Current.TAS.CancelCurrentTimeShifting(user);
            return RedirectToAction("Index");
        }

        public ActionResult Details(string user)
        {
            var vcard = Connections.Current.TAS.GetActiveCards().Where(vc => vc.User.Name == user).FirstOrDefault();
            if (vcard == null)
                return HttpNotFound();

            var card = Connections.Current.TAS.GetCards().Where(c => c.Id == vcard.Id).FirstOrDefault();
            if (card == null)
                return HttpNotFound();

            return View(new TVCardViewModel(card, vcard));
        }

        public JsonResult GetPerformanceCounters()
        {
            // No exception handling needed anymore, because we already check whether performance counters work in Index()
            var returnObject = new
            {
                CPU = (int)Math.Round(cpuCounter.NextValue()),
                Memory = (int)Math.Round(totalMemory - memoryCounter.NextValue())
            };
            return Json(returnObject, JsonRequestBehavior.AllowGet);
        } 
    }
}
