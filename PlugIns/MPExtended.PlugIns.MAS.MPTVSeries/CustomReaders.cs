﻿#region Copyright (C) 2011-2013 MPExtended
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
using System.Linq;
using System.Text;
using System.IO;
using System.Data.SQLite;
using MPExtended.Libraries.SQLitePlugin;
using MPExtended.Libraries.Service.Util;
using MPExtended.Services.Common.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces;

namespace MPExtended.PlugIns.MAS.MPTVSeries
{
    internal struct ExternalSiteReaderParameters
    {
        public Delegates.ReadValue Reader { get; set; }
        public string Site { get; set; }

        public ExternalSiteReaderParameters(Delegates.ReadValue reader, string site)
            : this()
        {
            this.Reader = reader;
            this.Site = site;
        }
    }

    internal struct ArtworkReaderParameters
    {
        public WebFileType FileType { get; set; }
        public string DirectoryName { get; set; }

        public ArtworkReaderParameters(WebFileType type, string dirname)
            : this()
        {
            this.FileType = type;
            this.DirectoryName = dirname;
        }
    }

    internal class CustomReaders
    {
        [MergeListReader]
        public static List<WebExternalId> ExternalIdReader(SQLiteDataReader reader, int idx, object param)
        {
            ExternalSiteReaderParameters args = (ExternalSiteReaderParameters)param;
            List<WebExternalId> list = new List<WebExternalId>();
            if (!reader.IsDBNull(idx))
            {
                string val = (string)args.Reader.Invoke(reader, idx);
                if (!String.IsNullOrEmpty(val) && val != "0")
                {
                    list.Add(new WebExternalId()
                    {
                        Site = args.Site,
                        Id = val
                    });
                }
            }
            return list;
        }

        [MergeListReader]
        public static List<WebArtworkDetailed> ArtworkReader(SQLiteDataReader reader, int index, object param)
        {
            ArtworkReaderParameters args = (ArtworkReaderParameters)param;
            int i = 0;
            return ((IEnumerable<string>)DataReaders.ReadPipeList(reader, index)).Select(x =>
            {
                string path = Path.Combine(args.DirectoryName, x.Replace('/', '\\'));
                return new WebArtworkDetailed()
                {
                    Type = args.FileType,
                    Path = path,
                    Offset = i++,
                    Filetype = Path.GetExtension(path).Substring(1),
                    Rating = 1,
                    Id = path.GetHashCode().ToString()
                };
            }).ToList();
        }

        [MergeListReader]
        public static List<WebArtworkDetailed> FanartArtworkReader(SQLiteDataReader reader, int index, object param)
        {
            ArtworkReaderParameters args = (ArtworkReaderParameters)param;
            var list = ((IEnumerable<string>)DataReaders.ReadPipeList(reader, index)).Select(x =>
            {
                string[] parts = x.Split('?'); // ? is used as separator between filename and rating
                string path = Path.Combine(args.DirectoryName, parts[0].Replace('/', '\\'));
                return new WebArtworkDetailed()
                {
                    Type = args.FileType,
                    Path = path,
                    Offset = 0,
                    Filetype = Path.GetExtension(path).Substring(1),
                    Rating = String.IsNullOrEmpty(parts[1]) ? 1 :
                                (int)Math.Round(Single.Parse(parts[1].Replace(',', '.'), System.Globalization.CultureInfo.InvariantCulture) * 10),
                    Id = parts[2]
                };
            }).ToList();

            list.Sort(AnonymousComparer.FromLambda<WebArtworkDetailed>((x, y) => y.Rating - x.Rating));
            for (int i = 0; i < list.Count; i++)
                list[i].Offset = i;
            return list;
        }

        [MergeListReader]
        public static List<WebArtworkDetailed> PreferedArtworkReader(SQLiteDataReader reader, int index, object param)
        {
            ArtworkReaderParameters args = (ArtworkReaderParameters)param;
            int i = 0;
            var items = (IEnumerable<string>)DataReaders.ReadPipeList(reader, index);

            string preferedItem = (string)DataReaders.ReadString(reader, index + 1);
            if (!String.IsNullOrEmpty(preferedItem))
            {
                if (preferedItem.StartsWith("\\"))
                    preferedItem = preferedItem.Substring(1);
                if (!items.Contains(preferedItem))
                    items = items.Concat(new List<string>() { preferedItem });
            }
            
            return items.Select(filename =>
            {
                if (filename.StartsWith("\\"))
                    filename = filename.Substring(1);
                string path = Path.Combine(args.DirectoryName, filename.Replace('/', '\\'));
                return new WebArtworkDetailed()
                {
                    Type = args.FileType,
                    Path = path,
                    Offset = i++,
                    Filetype = Path.GetExtension(path).Length > 0 ? Path.GetExtension(path).Substring(1) : String.Empty,
                    Rating = filename == preferedItem ? 2 : 1,
                    Id = path.GetHashCode().ToString()
                };
            }).ToList();
        }

        [AllowSQLCompare("(%table.SeriesID || '_s' || %table.SeasonIndex) = %prepared")]
        [AllowSQLSort("%table.SeriesID %order, %table.SeasonIndex %order")]
        public static string ReadSeasonID(SQLiteDataReader reader, int offset)
        {
            return DataReaders.ReadIntAsString(reader, offset - 1) + "_s" + DataReaders.ReadIntAsString(reader, offset);
        }

        public static List<WebActor> ActorReader(SQLiteDataReader reader, int idx)
        {
            return ((IList<string>)DataReaders.ReadPipeList(reader, idx)).Select(x => new WebActor() { Title = x }).ToList();
        }

        public static string FixNameReader(SQLiteDataReader reader, int index)
        {
            // MPTvSeries does some magic with the name: if it's empty in the online series, use the Parsed_Name from the local series. I prefer
            // a complete database, but we can't fix that easily. See DB Classes/DBSeries.cs:359 in MPTvSeries source
            string data = reader.ReadString(index);
            if (data.Length > 0)
                return data;
            return reader.ReadString(index - 1);
        }
    }
}
