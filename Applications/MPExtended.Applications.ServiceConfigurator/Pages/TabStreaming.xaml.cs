#region Copyright (C) 2011-2013 MPExtended
// Copyright (C) 2011-2013 MPExtended Developers, http://www.mpextended.com/
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
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Threading;
using MPExtended.Applications.ServiceConfigurator.Code;
using MPExtended.Libraries.Client;
using MPExtended.Libraries.Service;
using MPExtended.Libraries.Service.Strings;
using MPExtended.Libraries.Service.Util;
using MPExtended.Services.StreamingService.Interfaces;

namespace MPExtended.Applications.ServiceConfigurator.Pages
{
    /// <summary>
    /// Interaction logic for TabStreaming.xaml
    /// </summary>
    public partial class TabStreaming : Page, ITabCloseCallback
    {
        public TabStreaming()
        {
            InitializeComponent();

            // load language list
            var languages = CultureDatabase.GetLanguages()
                .OrderBy(x => x.TwoLetterISOLanguageName)
                .ToDictionary(x => x.TwoLetterISOLanguageName, x => String.Format("{0} ({1})", x.TwoLetterISOLanguageName, x.DisplayName));

            // set valid items
            cbAudio.DataContext = new Dictionary<string, string>() { 
                { "first", UI.SubtitlesFirstStream } 
            }.Concat(languages);

            cbSubtitle.DataContext = new Dictionary<string, string>()
            {
                { "none", UI.SubtitlesDisabled },
                { "first", UI.SubtitlesFirstStream },
                { "external", UI.SubtitlesExternal }
            }.Concat(languages);

            // set default item
            cbAudio.SelectedValue = Configuration.Streaming.DefaultAudioStream;
            cbSubtitle.SelectedValue = Configuration.Streaming.DefaultSubtitleStream;
        }


        public void TabClosed()
        {
            Configuration.Streaming.DefaultAudioStream = (string)cbAudio.SelectedValue;
            Configuration.Streaming.DefaultSubtitleStream = (string)cbSubtitle.SelectedValue;
            Configuration.Save();
        }
    }
}
