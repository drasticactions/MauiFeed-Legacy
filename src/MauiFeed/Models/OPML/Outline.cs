// <copyright file="Outline.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System.Security;
using System.Text;
using System.Xml;

namespace MauiFeed.Models.OPML
{
    /// <summary>
    /// Outline.
    /// </summary>
    public class Outline
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Outline"/> class.
        /// Constructor.
        /// </summary>
        /// <param name="element">element of Head.</param>
        public Outline(XmlElement element)
        {
            this.Text = element.GetAttribute("text");
            this.IsComment = element.GetAttribute("isComment");
            this.IsBreakpoint = element.GetAttribute("isBreakpoint");
            this.Created = this.GetDateTimeAttribute(element, "created");
            this.Category = this.GetCategoriesAtrribute(element, "category");
            this.Description = element.GetAttribute("description");
            this.HTMLUrl = element.GetAttribute("htmlUrl");
            this.Language = element.GetAttribute("language");
            this.Title = element.GetAttribute("title");
            this.Type = element.GetAttribute("type");
            this.Version = element.GetAttribute("version");
            this.XMLUrl = element.GetAttribute("xmlUrl");

            if (element.HasChildNodes)
            {
                foreach (XmlNode child in element.ChildNodes)
                {
                    if (child.Name.Equals("outline", StringComparison.CurrentCultureIgnoreCase))
                    {
                        this.Outlines.Add(new Outline((XmlElement)child));
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets text of the XML file (required).
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets true / false.
        /// </summary>
        public string IsComment { get; set; }

        /// <summary>
        /// Gets or sets true / false.
        /// </summary>
        public string IsBreakpoint { get; set; }

        /// <summary>
        /// Gets or sets outline node was created.
        /// </summary>
        public DateTime? Created { get; set; } = null;

        /// <summary>
        /// Gets or sets categories.
        /// </summary>
        public List<string> Category { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets hTML URL.
        /// </summary>
        public string HTMLUrl { get; set; }

        /// <summary>
        /// Gets or sets language.
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// Gets or sets title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets type (rss/atom).
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets version of RSS.
        /// RSS1 for RSS1.0. RSS for 0.91, 0.92 or 2.0.
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Gets or sets uRL of the XML file.
        /// </summary>
        public string XMLUrl { get; set; }

        /// <summary>
        /// Gets or sets outline list.
        /// </summary>
        public List<Outline> Outlines { get; set; } = new List<Outline>();

        /// <inheritdoc/>
        public override string ToString()
        {
            StringBuilder buf = new StringBuilder();
            buf.Append("<outline");
            buf.Append(this.GetAttributeString("text", this.Text));
            buf.Append(this.GetAttributeString("isComment", this.IsComment));
            buf.Append(this.GetAttributeString("isBreakpoint", this.IsBreakpoint));
            buf.Append(this.GetAttributeString("created", this.Created));
            buf.Append(this.GetAttributeString("category", this.Category));
            buf.Append(this.GetAttributeString("description", this.Description));
            buf.Append(this.GetAttributeString("htmlUrl", this.HTMLUrl));
            buf.Append(this.GetAttributeString("language", this.Language));
            buf.Append(this.GetAttributeString("title", this.Title));
            buf.Append(this.GetAttributeString("type", this.Type));
            buf.Append(this.GetAttributeString("version", this.Version));
            buf.Append(this.GetAttributeString("xmlUrl", this.XMLUrl));

            if (this.Outlines.Count > 0)
            {
                buf.Append(">\r\n");
                foreach (Outline outline in this.Outlines)
                {
                    buf.Append(outline.ToString());
                }

                buf.Append("</outline>\r\n");
            }
            else
            {
                buf.Append(" />\r\n");
            }

            return buf.ToString();
        }

        private DateTime? GetDateTimeAttribute(XmlElement element, string name)
        {
            string dt = element.GetAttribute(name);

            try
            {
                return DateTime.Parse(dt);
            }
            catch
            {
                return null;
            }
        }

        private List<string> GetCategoriesAtrribute(XmlElement element, string name)
        {
            List<string> list = new List<string>();
            var items = element.GetAttribute(name).Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in items)
            {
                list.Add(item.Trim());
            }

            return list;
        }

        private string GetAttributeString(string name, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }
            else
            {
                return $" {name}=\"{SecurityElement.Escape(value)}\"";
            }
        }

        private string GetAttributeString(string name, DateTime? value)
        {
            if (value == null)
            {
                return string.Empty;
            }
            else
            {
                return $" {name}=\"{value?.ToString("R")}\"";
            }
        }

        private string GetAttributeString(string name, List<string> value)
        {
            if (value.Count == 0)
            {
                return string.Empty;
            }

            StringBuilder buf = new StringBuilder();
            foreach (var item in value)
            {
                buf.Append(SecurityElement.Escape(item));
                buf.Append(",");
            }

            return $" {name}=\"{buf.Remove(buf.Length - 1, 1).ToString()}\"";
        }
    }
}
