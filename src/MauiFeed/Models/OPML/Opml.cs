// <copyright file="Opml.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System.Text;
using System.Xml;

namespace MauiFeed.Models.OPML
{
    /// <summary>
    /// Opml.
    /// </summary>
    public class Opml
    {
        private const string NAMESPACEURI = "http://opml.org/spec2";

        /// <summary>
        /// Initializes a new instance of the <see cref="Opml"/> class.
        /// Constructor.
        /// </summary>
        public Opml()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Opml"/> class.
        /// Constructor.
        /// </summary>
        /// <param name="location">Location of the OPML file.</param>
        public Opml(string location)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(location);
            this.ReadOpmlNodes(doc);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Opml"/> class.
        /// Constructor.
        /// </summary>
        /// <param name="doc">XMLDocument of the OPML.</param>
        public Opml(XmlDocument doc)
        {
            this.ReadOpmlNodes(doc);
        }

        /// <summary>
        /// Gets or sets version of OPML.
        /// </summary>
        public string Version { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets encoding of OPML.
        /// </summary>
        public string Encoding { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether include namespace in XML.
        /// </summary>
        public bool UseNamespace { get; set; }

        /// <summary>
        /// Gets or sets head of OPML.
        /// </summary>
        public Head? Head { get; set; }

        /// <summary>
        /// Gets or sets body of OPML.
        /// </summary>
        public Body Body { get; set; } = new Body();

        /// <inheritdoc/>
        public override string ToString()
        {
            StringBuilder buf = new StringBuilder();
            string ecoding = string.IsNullOrEmpty(this.Encoding) ? "UTF-8" : this.Encoding;
            buf.Append($"<?xml version=\"1.0\" encoding=\"{ecoding}\" ?>\r\n");
            string version = string.IsNullOrEmpty(this.Version) ? "2.0" : this.Version;

            if (this.UseNamespace)
            {
                buf.Append($"<opml version=\"{version}\" xmlns=\"{NAMESPACEURI}\">\r\n");
            }
            else
            {
                buf.Append($"<opml version=\"{version}\">\r\n");
            }

            buf.Append(this.Head?.ToString());
            buf.Append(this.Body.ToString());
            buf.Append("</opml>");

            return buf.ToString();
        }

        private void ReadOpmlNodes(XmlDocument doc)
        {
            foreach (XmlNode nodes in doc)
            {
                if (nodes.Name.Equals("opml", StringComparison.CurrentCultureIgnoreCase))
                {
                    foreach (XmlNode childNode in nodes)
                    {
                        if (childNode.Name.Equals("head", StringComparison.CurrentCultureIgnoreCase))
                        {
                            this.Head = new Head((XmlElement)childNode);
                        }

                        if (childNode.Name.Equals("body", StringComparison.CurrentCultureIgnoreCase))
                        {
                            this.Body = new Body((XmlElement)childNode);
                        }
                    }
                }
            }
        }
    }
}
