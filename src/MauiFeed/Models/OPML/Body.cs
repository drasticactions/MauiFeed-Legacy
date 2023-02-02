// <copyright file="Body.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System.Text;
using System.Xml;

namespace MauiFeed.Models.OPML
{
    /// <summary>
    /// Body.
    /// </summary>
    public class Body
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Body"/> class.
        /// Constructor.
        /// </summary>
        public Body()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Body"/> class.
        /// Constructor.
        /// </summary>
        /// <param name="element">element of Body.</param>
        public Body(XmlElement element)
        {
            if (element.Name.Equals("body", StringComparison.CurrentCultureIgnoreCase))
            {
                foreach (XmlNode node in element.ChildNodes)
                {
                    if (node.Name.Equals("outline", StringComparison.CurrentCultureIgnoreCase))
                    {
                        this.Outlines.Add(new Outline((XmlElement)node));
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets outline list.
        /// </summary>
        public List<Outline> Outlines { get; set; } = new List<Outline>();

        /// <inheritdoc/>
        public override string ToString()
        {
            StringBuilder buf = new StringBuilder();
            buf.Append("<body>\r\n");
            foreach (Outline outline in this.Outlines)
            {
                buf.Append(outline.ToString());
            }

            buf.Append("</body>\r\n");

            return buf.ToString();
        }
    }
}
