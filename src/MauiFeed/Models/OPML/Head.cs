// <copyright file="Head.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System.Security;
using System.Text;
using System.Xml;

namespace MauiFeed.Models.OPML
{
    /// <summary>
    /// Head.
    /// </summary>
    public class Head
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Head"/> class.
        /// </summary>
        public Head()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Head"/> class.
        /// Constructor.
        /// </summary>
        /// <param name="element">element of Head.</param>
        public Head(XmlElement element)
        {
            if (element.Name.Equals("head", StringComparison.CurrentCultureIgnoreCase))
            {
                foreach (XmlNode node in element.ChildNodes)
                {
                    this.Title = this.GetStringValue(node, "title", this.Title);
                    this.DateCreated = this.GetDateTimeValue(node, "dateCreated", this.DateCreated);
                    this.DateModified = this.GetDateTimeValue(node, "dateModified", this.DateModified);
                    this.OwnerName = this.GetStringValue(node, "ownerName", this.OwnerName);
                    this.OwnerEmail = this.GetStringValue(node, "ownerEmail", this.OwnerEmail);
                    this.OwnerId = this.GetStringValue(node, "ownerId", this.OwnerId);
                    this.Docs = this.GetStringValue(node, "docs", this.Docs);
                    this.ExpansionState = this.GetExpansionState(node, "expansionState", this.ExpansionState);
                    this.VertScrollState = this.GetStringValue(node, "vertScrollState", this.VertScrollState);
                    this.WindowTop = this.GetStringValue(node, "windowTop", this.WindowTop);
                    this.WindowLeft = this.GetStringValue(node, "windowLeft", this.WindowLeft);
                    this.WindowBottom = this.GetStringValue(node, "windowBottom", this.WindowBottom);
                    this.WindowRight = this.GetStringValue(node, "windowRight", this.WindowRight);
                }
            }
        }

        /// <summary>
        /// Gets or sets title.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets created date.
        /// </summary>
        public DateTime? DateCreated { get; set; } = null;

        /// <summary>
        /// Gets or sets modified date.
        /// </summary>
        public DateTime? DateModified { get; set; } = null;

        /// <summary>
        /// Gets or sets ownerName.
        /// </summary>
        public string OwnerName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets ownerEmail.
        /// </summary>
        public string OwnerEmail { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets ownerId.
        /// </summary>
        public string OwnerId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets docs.
        /// </summary>
        public string Docs { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets expansionState.
        /// </summary>
        public List<string> ExpansionState { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets vertScrollState.
        /// </summary>
        public string VertScrollState { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets windowTop.
        /// </summary>
        public string WindowTop { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets windowLeft.
        /// </summary>
        public string WindowLeft { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets windowBottom.
        /// </summary>
        public string WindowBottom { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets windowRight.
        /// </summary>
        public string WindowRight { get; set; } = string.Empty;

        /// <inheritdoc/>
        public override string ToString()
        {
            StringBuilder buf = new StringBuilder();
            buf.Append("<head>\r\n");
            buf.Append(this.GetNodeString("title", this.Title));
            buf.Append(this.GetNodeString("dateCreated", this.DateCreated));
            buf.Append(this.GetNodeString("dateModified", this.DateModified));
            buf.Append(this.GetNodeString("ownerName", this.OwnerName));
            buf.Append(this.GetNodeString("ownerEmail", this.OwnerEmail));
            buf.Append(this.GetNodeString("ownerId", this.OwnerId));
            buf.Append(this.GetNodeString("docs", this.Docs));
            buf.Append(this.GetNodeString("expansionState", this.ExpansionState));
            buf.Append(this.GetNodeString("vertScrollState", this.VertScrollState));
            buf.Append(this.GetNodeString("windowTop", this.WindowTop));
            buf.Append(this.GetNodeString("windowLeft", this.WindowLeft));
            buf.Append(this.GetNodeString("windowBottom", this.WindowBottom));
            buf.Append(this.GetNodeString("windowRight", this.WindowRight));
            buf.Append("</head>\r\n");
            return buf.ToString();
        }

        private string GetStringValue(XmlNode node, string name, string value)
        {
            if (node.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase))
            {
                return node.InnerText;
            }
            else if (!node.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase))
            {
                return value;
            }
            else
            {
                return string.Empty;
            }
        }

        private DateTime? GetDateTimeValue(XmlNode node, string name, DateTime? value)
        {
            if (node.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase))
            {
                try
                {
                    return DateTime.Parse(node.InnerText);
                }
                catch
                {
                    return null;
                }
            }
            else if (!node.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase))
            {
                return value;
            }
            else
            {
                return null;
            }
        }

        private List<string> GetExpansionState(XmlNode node, string name, List<string> value)
        {
            if (node.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase))
            {
                List<string> list = new List<string>();
                var items = node.InnerText.Split(',');
                foreach (var item in items)
                {
                    list.Add(item.Trim());
                }

                return list;
            }
            else if (!node.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase))
            {
                return value;
            }
            else
            {
                return new List<string>();
            }
        }

        private string GetNodeString(string name, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }
            else
            {
                return $"<{name}>{SecurityElement.Escape(value)}</{name}>\r\n";
            }
        }

        private string GetNodeString(string name, DateTime? value)
        {
            if (value == null)
            {
                return string.Empty;
            }
            else
            {
                return $"<{name}>{value?.ToString("R")}</{name}>\r\n";
            }
        }

        private string GetNodeString(string name, List<string> value)
        {
            if (value.Count == 0)
            {
                return string.Empty;
            }

            StringBuilder buf = new StringBuilder();
            foreach (var item in value)
            {
                buf.Append(item);
                buf.Append(",");
            }

            return $"<{name}>{buf.Remove(buf.Length - 1, 1).ToString()}</{name}>\r\n";
        }
    }
}
