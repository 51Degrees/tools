using FiftyOne.MetaData.Entities;
using FiftyOne.Pipeline.Engines.FiftyOne.Data;
using PropertyGenerationTool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PropertyGenerator.Builders
{
    /// <summary>
    /// A single value which a property can return, paired with the meaning of
    /// that value.
    /// </summary>
    /// <param name="Name">
    /// The value as it is returned by the property e.g. "True" or "Tablet".
    /// </param>
    /// <param name="Description">
    /// What the value means. May be empty.
    /// </param>
    internal sealed record DocumentedValue(string Name, string Description);

    /// <summary>
    /// Builds the language independent parts of the documentation which is
    /// written into the generated accessors: the fixed width table of the
    /// values a property can return, and the escaping each doc format needs.
    /// The languages differ only in how they wrap these lines, so the layout
    /// is decided once here and reused by every builder.
    /// </summary>
    internal static class PropertyDocumentation
    {
        /// <summary>
        /// Minimum width of the value column. Chosen so that the short
        /// enumeration style values which prompted this documentation
        /// (True, False, Unknown, N/A) line up in a readable column.
        /// </summary>
        private const int MinimumValueColumnWidth = 10;

        /// <summary>
        /// Number of spaces between the longest value and the start of the
        /// description column.
        /// </summary>
        private const int ValueColumnPadding = 2;

        /// <summary>
        /// Appended to a description which had to be shortened.
        /// </summary>
        private const string Ellipsis = "...";

        /// <summary>
        /// Get the values to document for a property from the common
        /// metadata. Nothing is returned unless the metadata says the values
        /// should be exported, as that is the flag which marks a property as
        /// having an enumeration style set of values worth showing.
        /// </summary>
        internal static IReadOnlyList<DocumentedValue> FromMetaData(
            IPropertyMetaData property)
        {
            if (property.ExportValues == false || property.Values == null)
            {
                return Array.Empty<DocumentedValue>();
            }
            return property.Values
                .Select(value => new DocumentedValue(
                    value.Name,
                    value.Description))
                .ToArray();
        }

        /// <summary>
        /// Get the values to document for a property from an engine's meta
        /// data. This is the equivalent of <see cref="FromMetaData"/> for the
        /// builders which read their properties from a data file rather than
        /// from the common metadata.
        /// </summary>
        internal static IReadOnlyList<DocumentedValue> FromEngine(
            IFiftyOneAspectPropertyMetaData property)
        {
            if (property.ShowValues == false)
            {
                return Array.Empty<DocumentedValue>();
            }

            var values = new List<DocumentedValue>();
            foreach (var value in property.GetValues())
            {
                // The engine's values wrap resources owned by the data file,
                // so each one is released once it has been read.
                using (value)
                {
                    values.Add(new DocumentedValue(
                        value.Name,
                        value.Description));
                }
            }
            return values;
        }

        /// <summary>
        /// Lay the values out as a fixed width table of value and description,
        /// one entry per line.
        /// </summary>
        /// <param name="values">
        /// The values a property can return. Entries without a name are
        /// skipped, as the name is the value itself.
        /// </param>
        /// <param name="url">
        /// Page with more information about the property, used to point at the
        /// full list when there are too many values to document. May be null.
        /// </param>
        /// <returns>
        /// The lines of the table, or an empty list if there is nothing to
        /// document. Long value lists are truncated to
        /// <see cref="Constants.MaxDocumentedValues"/> entries so that a
        /// property with thousands of values does not swamp the generated
        /// source or the tooltip which shows it.
        /// </returns>
        internal static IReadOnlyList<string> BuildValueTable(
            IReadOnlyList<DocumentedValue> values,
            string url)
        {
            var named = (values ?? Array.Empty<DocumentedValue>())
                .Where(value => string.IsNullOrWhiteSpace(value.Name) == false)
                .ToArray();
            if (named.Length == 0)
            {
                return Array.Empty<string>();
            }

            var documented = named
                .Take(Constants.MaxDocumentedValues)
                .ToArray();
            var columnWidth = Math.Max(
                documented.Max(value => value.Name.Length) + ValueColumnPadding,
                MinimumValueColumnWidth);

            var lines = documented
                .Select(value => BuildRow(value, columnWidth))
                .ToList();

            var undocumented = named.Length - documented.Length;
            if (undocumented > 0)
            {
                lines.Add($"... and {undocumented} more value" +
                    (undocumented == 1 ? "." : "s.") +
                    (string.IsNullOrWhiteSpace(url) ?
                        string.Empty :
                        $" See {url.Trim()} for the full list."));
            }
            return lines;
        }

        /// <summary>
        /// Build a single row of the table. A value with no description is
        /// written on its own rather than padded into an empty column.
        /// </summary>
        private static string BuildRow(DocumentedValue value, int columnWidth)
        {
            var description = Shorten(value.Description);
            return description.Length == 0 ?
                value.Name :
                value.Name.PadRight(columnWidth) + description;
        }

        /// <summary>
        /// Collapse a description onto a single line and shorten it, so that
        /// one long description cannot break the alignment of the table.
        /// </summary>
        internal static string Shorten(string description)
        {
            if (string.IsNullOrWhiteSpace(description))
            {
                return string.Empty;
            }

            var collapsed = new StringBuilder(description.Length);
            var lastWasSpace = false;
            foreach (var character in description.Trim())
            {
                if (char.IsWhiteSpace(character))
                {
                    if (lastWasSpace == false)
                    {
                        collapsed.Append(' ');
                    }
                    lastWasSpace = true;
                }
                else
                {
                    collapsed.Append(character);
                    lastWasSpace = false;
                }
            }

            var single = collapsed.ToString();
            if (single.Length <= Constants.MaxValueDescriptionLength)
            {
                return single;
            }
            return single
                .Substring(0, Constants.MaxValueDescriptionLength - Ellipsis.Length)
                .TrimEnd() + Ellipsis;
        }

        /// <summary>
        /// Escape the characters which would otherwise be read as markup by
        /// the XML doc comments of C# and the HTML of Javadoc. Descriptions
        /// come from the metadata verbatim, and some of them name HTML
        /// elements such as &lt;header&gt;. The quote is escaped as well so
        /// that the same method can be used on a URL written into an
        /// attribute.
        /// </summary>
        internal static string EscapeMarkup(string text)
        {
            return (text ?? string.Empty)
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;");
        }
    }
}
