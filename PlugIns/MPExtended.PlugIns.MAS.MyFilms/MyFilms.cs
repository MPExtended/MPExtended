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
using System.ComponentModel.Composition;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using MPExtended.Libraries.Service;
using MPExtended.Libraries.Service.Util;
using MPExtended.Libraries.Service.Extensions;
using MPExtended.Services.Common.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces.Movie;

namespace MPExtended.PlugIns.MAS.MyFilms
{
    [Export(typeof(IMovieLibrary))]
    [ExportMetadata("Name", "My Films (Ant Movie Catalog only)")]
    [ExportMetadata("Id", 13)]
    public partial class MyFilms : IMovieLibrary
    {
        public bool Supported { get; set; }
        private string DatabasePath { get; set; }
        private string PicturePath { get; set; }

        private string sourcePrefix;
        private Regex stripActorName;
        private Regex imdbId;

        [ImportingConstructor]
        public MyFilms(IPluginData data)
        {
            Supported = false;

            try
            {
                // load MyFilms.xml path
                if (!Mediaportal.HasValidConfigFile())
                    return;

                string configPath = Path.Combine(Mediaportal.GetLocation(MediaportalDirectory.Config), "MyFilms.xml");
                if (!File.Exists(configPath))
                    return;

                // load current config section
                XElement configFile = XElement.Load(configPath);
                var entries = configFile.Elements("section").First(x => x.Attribute("name").Value == "MyFilms").Elements("entry");
                if (!entries.Any(x => x.Attribute("name").Value == "Default_Config") && !entries.Any(x => x.Attribute("name").Value == "Current_Config"))
                {
                    Log.Info("MyFilms: couldn't find Current_Config or Default_Config value in MyFilms.xml");
                    return;
                }
                var configSectionName = entries.Any(x => x.Attribute("name").Value == "Current_Config") ? 
                                        entries.First(x => x.Attribute("name").Value == "Current_Config").Value :
                                        entries.First(x => x.Attribute("name").Value == "Default_Config").Value;

                // look for database path
                var thisSection = configFile.Elements("section").First(x => x.Attribute("name").Value == configSectionName);
                if(!thisSection.Elements("entry").Any(x => x.Attribute("name").Value == "AntCatalog"))
                {
                    Log.Info("MyFilms: couldn't find AntCatalog entry in current section ({0})", configSectionName);
                    return;
                }

                // load database
                DatabasePath = thisSection.Elements("entry").FirstOrDefault(x => x.Attribute("name").Value == "AntCatalog").Value;
                if (!File.Exists(DatabasePath))
                {
                    Log.Info("MyFilms: cannot find database {0}", DatabasePath);
                    return;
                }

                PicturePath = thisSection.Elements("entry").Any(x => x.Attribute("name").Value == "AntPicture") ?
                    thisSection.Elements("entry").FirstOrDefault(x => x.Attribute("name").Value == "AntPicture").Value : String.Empty;
                sourcePrefix = thisSection.Elements("entry").Any(x => x.Attribute("name").Value == "PathStorage") ?
                    thisSection.Elements("entry").FirstOrDefault(x => x.Attribute("name").Value == "PathStorage").Value : String.Empty;
                Supported = true;
            }
            catch (Exception ex)
            {
                Log.Warn("MyFilms: failed to load database path", ex);
                return;
            }

            // initialize some regular expressions
            stripActorName = new Regex(@"^([^(]*)(\(.*\))*", RegexOptions.Compiled);
            imdbId = new Regex(@"^.*imdb.[a-z]+/title/(tt[0-9]+)/*$", RegexOptions.Compiled);
        }

        public IEnumerable<WebMovieBasic> GetAllMovies()
        {
            return XElement.Load(DatabasePath)
                .Element("Catalog").Element("Contents").Elements("Movie")
                .Select(x => NodeToMovieBasic(x))
                .Where(x => x != null);
        }

        public IEnumerable<WebMovieDetailed> GetAllMoviesDetailed()
        {
            return XElement.Load(DatabasePath)
                .Element("Catalog").Element("Contents").Elements("Movie")
                .Select(x => NodeToMovieDetailed(x))
                .Where(x => x != null);
        }

        public WebMovieBasic GetMovieBasicById(string movieId)
        {
            return XElement.Load(DatabasePath)
                .Element("Catalog").Element("Contents").Elements("Movie")
                .Where(x => x.Attribute("Number").Value == movieId)
                .Select(x => NodeToMovieBasic(x))
                .Where(x => x != null)
                .First();
        }

        public WebMovieDetailed GetMovieDetailedById(string movieId)
        {
            return XElement.Load(DatabasePath)
                .Element("Catalog").Element("Contents").Elements("Movie")
                .Where(x => x.Attribute("Number").Value == movieId)
                .Select(x => NodeToMovieDetailed(x))
                .Where(x => x != null)
                .First();
        }

        private WebMovieBasic NodeToMovieBasic(XElement item)
        {
            return NodeToMovie<WebMovieBasic>(item);
        }

        private WebMovieDetailed NodeToMovieDetailed(XElement item)
        {
            WebMovieDetailed movie = NodeToMovie<WebMovieDetailed>(item);
            if (movie == null)
                return null;

            if (item.Attribute("Description") != null)
                movie.Summary = item.Attribute("Description").Value;

            // director
            if (item.Attribute("Director") != null)
                movie.Directors.Add(item.Attribute("Director").Value);

            // tagline
            if (item.Element("CustomFields") != null && item.Element("CustomFields").Attribute("TagLine") != null)
                movie.Tagline = item.Element("CustomFields").Attribute("TagLine").Value.Trim();

            // writers
            if (item.Element("CustomFields") != null && item.Element("CustomFields").Attribute("Writer") != null)
            {
                // I've no clue what the things in parentheses mean here, but let's remove them
                string writer = item.Element("CustomFields").Attribute("Writer").Value;
                movie.Writers = item.Element("CustomFields")
                    .Attribute("Writer").Value
                    .Split(',', '|')
                    .Select(x => stripActorName.Replace(x, "$1").Trim())
                    .ToList();
            }

            return movie;
        }

        private T NodeToMovie<T>(XElement item) where T : WebMovieBasic, new()
        {
            if (item.Attribute("Source") == null)
                // we ignore movies without a source
                return null;

            var movie = new T()
            {
                Id = item.Attribute("Number").Value,
                Path = new List<string>() { sourcePrefix == String.Empty ? item.Attribute("Source").Value : Path.Combine(sourcePrefix, item.Attribute("Source").Value) },

                Title = item.Attribute("OriginalTitle").Value,
                Year = item.Attribute("Year") != null ? Int32.Parse(item.Attribute("Year").Value) : 0,
                Runtime = item.Attribute("Length") != null ? Int32.Parse(item.Attribute("Length").Value) : 0,
                Rating = item.Attribute("Rating") != null ? Single.Parse(item.Attribute("Rating").Value, CultureInfo.InvariantCulture) : 0,
            };

            /* I've seen two ways in which the date is saved:
             * - A DateAdded childnode in a standard YYYY-MM-DDTHH:MM:SS+HH:MM format, which we can just give to DateTime.Parse
             * - A Date attribute in YYYY-MM-DD format or dd/MM/yyyy format, which we've to try with DateTime.TryParseExact()
             * If neither of these two works, just don't set a date
             */
            DateTime tmp;
            if (item.Element("DateAdded") != null)
            {
                movie.DateAdded = DateTime.Parse(item.Element("DateAdded").Value);
            }
            else if (item.Attribute("Date") != null && (
                      DateTime.TryParseExact(item.Attribute("Date").Value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out tmp) ||
                      DateTime.TryParseExact(item.Attribute("Date").Value, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out tmp)))
            {
                movie.DateAdded = tmp;
            }

            if (item.Attribute("Category") != null)
                movie.Genres = item.Attribute("Category").Value
                    .Split(',', '|')
                    .Select(x => x.Trim())
                    .Distinct()
                    .ToList();

            if (item.Attribute("Actors") != null)
                movie.Actors = item.Attribute("Actors").Value
                    .Split(',', '|')
                    .Select(x => new WebActor() { Title = stripActorName.Replace(x, "$1").Trim() })
                    .ToList();

            /* I've seen two (there are probably more...) ways the IMDB ID is saved:
             * - An IMDB_Id childnode, with just the id
             * - An URL attribute where we should extract the id from
             */
            Match match;
            if (item.Element("IMDB_Id") != null)
            {
                movie.ExternalId.Add(new WebExternalId() { Site = "IMDB", Id = item.Element("IMDB_Id").Value });
            }
            else if (item.Attribute("URL") != null && (match = imdbId.Match(item.Attribute("URL").Value)).Success)
            {
                movie.ExternalId.Add(new WebExternalId() { Site = "IMDB", Id = match.Groups[1].Value });
            }

            // Picture path
            if (item.Attribute("Picture") != null)
            {
                string name = item.Attribute("Picture").Value;
                if (!Path.IsPathRooted(name))
                {
                    name = Path.Combine(PicturePath, name);
                }
                movie.Artwork.Add(new WebArtworkDetailed()
                {
                    Path = name,
                    Type = WebFileType.Cover,
                    Filetype = Path.GetExtension(name).Substring(1),
                    Id = name.GetHashCode().ToString("X8"),
                    Offset = 0,
                    Rating = 1,
                });
            }

            // Some movies point to an URL for fanart
            if (item.Element("CustomFields") != null && item.Element("CustomFields").Attribute("Fanart") != null)
            {
                string path = item.Element("CustomFields").Attribute("Fanart").Value;
                Uri uri = new Uri(path);
                movie.Artwork.Add(new WebArtworkDetailed()
                {
                    Path = path,
                    Type = WebFileType.Backdrop,
                    Filetype = Path.GetExtension(uri.LocalPath).Substring(1),
                    Id = path.GetHashCode().ToString("X8"),
                    Offset = 0,
                    Rating = 1
                });
            }

            return movie;
        }

        public IEnumerable<WebGenre> GetAllGenres()
        {
            return XElement.Load(DatabasePath)
                .Element("Catalog").Element("Contents").Elements("Movie")
                .Select(x => x.Attribute("Category"))
                .Where(x => x != null)
                .SelectMany(x => x.Value.Split(',', '|', '/'))
                .Select(x => x.Trim())
                .Distinct()
                .Select(x => new WebGenre() { Title = x });
        }

        public IEnumerable<WebSearchResult> Search(string text)
        {
            return XElement.Load(DatabasePath)
                .Element("Catalog").Element("Contents").Elements("Movie")
                .Where(x => x.Attribute("OriginalTitle").Value.Contains(text, false))
                .Select(x => new WebSearchResult()
                {
                    Type = WebMediaType.Movie,
                    Id = x.Attribute("Number").Value,
                    Title = x.Attribute("OriginalTitle").Value,
                    Score = (int)Math.Round(40 + (decimal)text.Length / x.Attribute("OriginalTitle").Value.Length * 40),
                    Details = new WebDictionary<string>()
                    {
                        { "Year", x.Attribute("Year").Value },
                        { "Genres", String.Join(", ", x.Attribute("Category").Value.Split(',', '|').Select(g => g.Trim()).Distinct()) }
                    }
                });
        }

        public IEnumerable<WebCategory> GetAllCategories()
        {
            return new List<WebCategory>();
        }

        public WebFileInfo GetFileInfo(string path)
        {
            if (path.StartsWith("http://"))
            {
                return ArtworkRetriever.GetFileInfo(path);
            }

            return new WebFileInfo(new FileInfo(PathUtil.StripFileProtocolPrefix(path)));
        }

        public Stream GetFile(string path)
        {
            if (path.StartsWith("http://"))
            {
                return ArtworkRetriever.GetStream(path);
            }

            return new FileStream(PathUtil.StripFileProtocolPrefix(path), FileMode.Open, FileAccess.Read);
        }

        public WebDictionary<string> GetExternalMediaInfo(WebMediaType type, string id)
        {
            return new WebDictionary<string>()
            {
                { "Type", "my films" },
                { "Id", id }
            };
        }
    }
}
