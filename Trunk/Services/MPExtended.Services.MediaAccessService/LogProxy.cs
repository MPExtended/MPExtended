#region Copyright (C) 2011 MPExtended
// Copyright (C) 2011 MPExtended Developers, http://mpextended.codeplex.com/
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
using System.Text;
using MPExtended.Libraries.General;
using MPExtended.Services.MediaAccessService.Interfaces;

namespace MPExtended.Services.MediaAccessService
{
    internal class LogProxy : ILogger
    {
        public void Trace(string _msg)
        {
            Log.Trace(_msg);
        }

        public void Trace(string _msg, Exception ex)
        {
            Log.Trace(_msg, ex);
        }

        public void Trace(string _msg, params object[] args)
        {
            Log.Trace(_msg, args);
        }

        public void Debug(string _msg)
        {
            Log.Debug(_msg);
        }

        public void Debug(string _msg, Exception ex)
        {
            Log.Debug(_msg, ex);
        }

        public void Debug(string _msg, params object[] args)
        {
            Log.Debug(_msg, args);
        }

        public void Info(string _msg)
        {
            Log.Info(_msg);
        }

        public void Info(string _msg, Exception ex)
        {
            Log.Info(_msg, ex);
        }

        public void Info(string _msg, params object[] args)
        {
            Log.Info(_msg, args);
        }

        public void Warn(string _msg)
        {
            Log.Warn(_msg);
        }

        public void Warn(string _msg, Exception ex)
        {
            Log.Warn(_msg, ex);
        }

        public void Warn(string _msg, params object[] args)
        {
            Log.Warn(_msg, args);
        }

        public void Error(string _msg)
        {
            Log.Error(_msg);
        }

        public void Error(string _msg, Exception ex)
        {
            Log.Error(_msg, ex);
        }

        public void Error(string _msg, params object[] arg)
        {
            Log.Error(_msg, arg);
        }

        public void Fatal(string _msg)
        {
            Log.Fatal(_msg);
        }

        public void Fatal(string _msg, Exception ex)
        {
            Log.Fatal(_msg, ex);
        }

        public void Fatal(string _msg, params object[] args)
        {
            Log.Fatal(_msg, args);
        }
    }
}
