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
using System.Web;
using System.Text;
using System.Web.Mvc;

namespace MPExtended.Applications.WebMediaPortal.Mvc
{
    public class AssetManager
    {
        private HtmlHelper htmlHelper;
        private Dictionary<string, string> tags = new Dictionary<string, string>();
        private List<object> scriptBlocks = new List<object>();

        public AssetManager(HtmlHelper htmlHelper)
        {
            this.htmlHelper = htmlHelper;
        }

        public MvcHtmlString AddScript(string path)
        {
            return CreateScript(UrlHelper.GenerateContentUrl(path, htmlHelper.ViewContext.HttpContext));
        }

        public MvcHtmlString AddSkinScript(string path)
        {
            return CreateScript(UrlHelperExtensionMethods.GenerateSkinContentUrl(path, htmlHelper.ViewContext.HttpContext));
        }

        public MvcHtmlString AddViewScript(string path)
        {
            return CreateScript(UrlHelperExtensionMethods.GenerateViewContentUrl(path, htmlHelper.ViewContext.HttpContext));
        }

        private MvcHtmlString CreateScript(string resolvedPath)
        {
            if (!tags.ContainsKey(resolvedPath))
            {
                TagBuilder builder = new TagBuilder("script");
                builder.MergeAttribute("type", "text/javascript");
                builder.MergeAttribute("src", resolvedPath);
                tags.Add(resolvedPath, builder.ToString(TagRenderMode.Normal));
            }

            // return an empty string so that we can just use @Html.Assets().AddScript()
            return MvcHtmlString.Create(String.Empty);
        }

        public MvcHtmlString AddScriptBlock(object block)
        {
            scriptBlocks.Add(block);
            return MvcHtmlString.Empty;
        }

        public MvcHtmlString AddStylesheet(string path)
        {
            return CreateStylesheet(UrlHelper.GenerateContentUrl(path, htmlHelper.ViewContext.HttpContext));
        }

        public MvcHtmlString AddSkinStylesheet(string path)
        {
            return CreateStylesheet(UrlHelperExtensionMethods.GenerateSkinContentUrl(path, htmlHelper.ViewContext.HttpContext));
        }

        public MvcHtmlString AddViewStylesheet(string path)
        {
            return CreateStylesheet(UrlHelperExtensionMethods.GenerateViewContentUrl(path, htmlHelper.ViewContext.HttpContext));
        }

        private MvcHtmlString CreateStylesheet(string resolvedPath)
        {
            if (!tags.ContainsKey(resolvedPath))
            {
                TagBuilder builder = new TagBuilder("link");
                builder.MergeAttribute("rel", "stylesheet");
                builder.MergeAttribute("type", "text/css");
                builder.MergeAttribute("href", resolvedPath);
                tags.Add(resolvedPath, builder.ToString(TagRenderMode.SelfClosing));
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
                tags.Add(String.Empty, builder.ToString(TagRenderMode.Normal));
            }

            // first add all standard tags
            StringBuilder text = new StringBuilder();
            foreach (var tag in tags.Values)
            {
                text.AppendLine(tag);
            }

            return MvcHtmlString.Create(text.ToString());
        }
    }
}