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
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using MPExtended.Libraries.Service;

namespace MPExtended.Applications.WebMediaPortal.Mvc
{
    public class AssetManager
    {
        private enum AssetType
        {
            Stylesheet = 1,
            Script = 2
        }

        private class ReferencedAsset
        {
            public AssetType AssetType { get; set; }
            public int Position { get; set; }
            public string Tag { get; set; }
        }

        private const int DEFAULT_POSITION = 100;

        private HtmlHelper htmlHelper;
        private Dictionary<string, ReferencedAsset> assets = new Dictionary<string, ReferencedAsset>();
        private List<object> scriptBlocks = new List<object>();

        public AssetManager(HtmlHelper htmlHelper)
        {
            this.htmlHelper = htmlHelper;
        }

        public MvcHtmlString AddScript(string path)
        {
            return CreateScript(UrlHelper.GenerateContentUrl(path, htmlHelper.ViewContext.HttpContext), DEFAULT_POSITION);
        }

        public MvcHtmlString AddScript(string path, int position)
        {
            return CreateScript(UrlHelper.GenerateContentUrl(path, htmlHelper.ViewContext.HttpContext), position);
        }

        public MvcHtmlString AddContentScript(string path)
        {
            return CreateScript(UrlHelper.GenerateContentUrl(ContentLocator.Current.LocateContent(path), htmlHelper.ViewContext.HttpContext), DEFAULT_POSITION);
        }

        public MvcHtmlString AddContentScript(string path, int position)
        {
            return CreateScript(UrlHelper.GenerateContentUrl(ContentLocator.Current.LocateContent(path), htmlHelper.ViewContext.HttpContext), position);
        }

        public MvcHtmlString AddViewScript(string path)
        {
            return CreateScript(UrlHelper.GenerateContentUrl(ContentLocator.Current.LocateView(path), htmlHelper.ViewContext.HttpContext), DEFAULT_POSITION);
        }

        public MvcHtmlString AddViewScript(string path, int position)
        {
            return CreateScript(UrlHelper.GenerateContentUrl(ContentLocator.Current.LocateView(path), htmlHelper.ViewContext.HttpContext), position);
        }

        private MvcHtmlString CreateScript(string resolvedPath, int position)
        {
            if (!assets.ContainsKey(resolvedPath))
            {
                var uri = resolvedPath + "?v=" + VersionUtil.GetBuildVersion().GetHashCode().ToString();
                TagBuilder builder = new TagBuilder("script");
                builder.MergeAttribute("type", "text/javascript");
                builder.MergeAttribute("src", uri);

                assets.Add(resolvedPath, new ReferencedAsset()
                {
                    AssetType = AssetType.Script,
                    Tag = builder.ToString(TagRenderMode.Normal),
                    Position = position
                });
            }

            // return an empty string so that we can use @Html.Assets().AddScript()
            return MvcHtmlString.Create(String.Empty);
        }

        public MvcHtmlString AddScriptBlock(object block)
        {
            scriptBlocks.Add(block);
            return MvcHtmlString.Empty;
        }

        public MvcHtmlString AddStylesheet(string path)
        {
            return CreateStylesheet(UrlHelper.GenerateContentUrl(path, htmlHelper.ViewContext.HttpContext), DEFAULT_POSITION);
        }

        public MvcHtmlString AddStylesheet(string path, int position)
        {
            return CreateStylesheet(UrlHelper.GenerateContentUrl(path, htmlHelper.ViewContext.HttpContext), position);
        }

        public MvcHtmlString AddContentStylesheet(string path)
        {
            return CreateStylesheet(UrlHelper.GenerateContentUrl(ContentLocator.Current.LocateContent(path), htmlHelper.ViewContext.HttpContext), DEFAULT_POSITION);
        }

        public MvcHtmlString AddContentStylesheet(string path, int position)
        {
            return CreateStylesheet(UrlHelper.GenerateContentUrl(ContentLocator.Current.LocateContent(path), htmlHelper.ViewContext.HttpContext), position);
        }

        public MvcHtmlString AddViewStylesheet(string path)
        {
            return CreateStylesheet(UrlHelper.GenerateContentUrl(ContentLocator.Current.LocateView(path), htmlHelper.ViewContext.HttpContext), DEFAULT_POSITION);
        }

        public MvcHtmlString AddViewStylesheet(string path, int position)
        {
            return CreateStylesheet(UrlHelper.GenerateContentUrl(ContentLocator.Current.LocateView(path), htmlHelper.ViewContext.HttpContext), position);
        }

        private MvcHtmlString CreateStylesheet(string resolvedPath, int position)
        {
            if (!assets.ContainsKey(resolvedPath))
            {
                var uri = resolvedPath + "?v=" + VersionUtil.GetBuildVersion().GetHashCode().ToString();
                TagBuilder builder = new TagBuilder("link");
                builder.MergeAttribute("rel", "stylesheet");
                builder.MergeAttribute("type", "text/css");
                builder.MergeAttribute("href", uri);

                assets.Add(resolvedPath, new ReferencedAsset()
                {
                    AssetType = AssetType.Stylesheet,
                    Tag = builder.ToString(TagRenderMode.SelfClosing),
                    Position = position
                });
            }

            // return an empty string so that we can just use @Html.Assets().AddStylesheet()
            return MvcHtmlString.Create(String.Empty);
        }

        public MvcHtmlString Render()
        {
            // merge all scripts blocks as a single tag
            if (scriptBlocks.Count > 0)
            {
                TagBuilder builder = new TagBuilder("script");
                builder.MergeAttribute("type", "text/javascript");
                builder.InnerHtml = String.Join("", scriptBlocks);
                assets.Add(String.Empty, new ReferencedAsset()
                {
                    AssetType = AssetType.Script,
                    Tag = builder.ToString(TagRenderMode.Normal),
                    Position = Int32.MaxValue
                });
            }


            // create the HTML
            StringBuilder text = new StringBuilder();
            var tags = assets.Values
                .OrderBy(x => x.AssetType)
                .ThenBy(x => x.Position);
            foreach (var tag in tags)
                text.AppendLine(tag.Tag);

            return MvcHtmlString.Create(text.ToString());
        }
    }
}