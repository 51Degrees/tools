using FiftyOne.MetaData.Entities;
using FiftyOne.Pipeline.Engines.FiftyOne.Data;
using PropertyGenerationTool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

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
    /// The documentation shared by every emitter for one property: the page
    /// with more information, and the laid out value table. The languages
    /// differ only in how they wrap these.
    /// </summary>
    /// <param name="Url">
    /// Page with more information about the property. May be null or empty.
    /// </param>
    /// <param name="ValueTable">
    /// The lines of the fixed width value table. Empty when there is nothing
    /// to document.
    /// </param>
    internal sealed record PropertyDocs(
        string Url,
        IReadOnlyList<string> ValueTable);

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
        /// Widest the value column is allowed to become. Without a ceiling a
        /// single long value pushes every description out to its width, which
        /// is neither a readable table nor a line an IDE tooltip can show.
        /// </summary>
        private const int MaximumValueColumnWidth = 40;

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
        /// An ampersand which does not open a recognised character entity.
        /// Some metadata descriptions arrive already HTML escaped (e.g. the
        /// Html-Media-Capture description carries &amp;lt;input ...&amp;gt;),
        /// and escaping their ampersands again would display the entity text
        /// literally. Only the five predefined entities and numeric forms are
        /// recognised, so an accidental "&amp;D;" is still escaped rather
        /// than left as an unknown entity.
        /// </summary>
        private static readonly Regex BareAmpersand = new Regex(
            "&(?!(amp|lt|gt|quot|apos|#[0-9]+|#x[0-9a-fA-F]+);)",
            RegexOptions.Compiled);

        /// <summary>
        /// Get the values to document for a property from the common
        /// metadata. Nothing is returned unless the metadata says the values
        /// should be exported, as that is the flag which marks a property as
        /// having an enumeration style set of values worth showing.
        /// </summary>
        internal static IReadOnlyList<DocumentedValue> FromMetaData(
            IPropertyMetaData property)
        {
            // ValuesOmitted is deliberately not consulted. It is a signal
            // within a metadata generation run and is never serialised, so on
            // the deserialised metadata this tool reads it is always false -
            // its own documentation says not to reason about deserialised
            // metadata with it. And where it can be true, the extract omits
            // the values entirely rather than truncating, so a partial list
            // with the flag set does not occur.
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
        /// <remarks>
        /// Every value is read, not just the few the table will show, because
        /// the overflow line needs the exact total and each engine value must
        /// be disposed regardless. For a data file property with many values
        /// the objects beyond the table cap are transient. Acceptable while
        /// these builders are only used ad hoc; revisit if an engine backed
        /// generator is ever wired into <c>Program</c>.
        /// </remarks>
        internal static IReadOnlyList<DocumentedValue> FromEngine(
            IFiftyOneAspectPropertyMetaData property)
        {
            if (property.ShowValues == false)
            {
                return Array.Empty<DocumentedValue>();
            }

            var values = new List<DocumentedValue>();
            foreach (var value in property.GetValues() ??
                Enumerable.Empty<IValueMetaData>())
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
        /// skipped, as the name is the value itself. Names are collapsed onto
        /// a single line the same way descriptions are: every line of the
        /// table becomes one line of a comment in the generated source, so a
        /// name carrying a line break would otherwise end the comment early
        /// and leave the rest of itself as code.
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
                .Select(value => new DocumentedValue(
                    Shorten(value.Name),
                    value.Description))
                .Where(value => value.Name.Length > 0)
                .ToArray();
            if (named.Length == 0)
            {
                return Array.Empty<string>();
            }

            var documented = named
                .Take(Constants.MaxDocumentedValues)
                .ToArray();
            var columnWidth = Math.Min(
                Math.Max(
                    documented.Max(value => value.Name.Length) +
                        ValueColumnPadding,
                    MinimumValueColumnWidth),
                MaximumValueColumnWidth);

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
        /// written on its own rather than padded into an empty column, and a
        /// value wider than the capped column gets its own separator, as
        /// padding is a no-op for it and the description would otherwise butt
        /// straight up against it.
        /// </summary>
        private static string BuildRow(DocumentedValue value, int columnWidth)
        {
            var description = Shorten(value.Description);
            if (description.Length == 0)
            {
                return value.Name;
            }
            // The name's own length is the discriminator, not the padded
            // length: padding brings every shorter name out at exactly the
            // column width, so comparing the padded length would either miss
            // the name that fills the column exactly (>) or double pad every
            // row (>=). A name at or over the column width fills it with no
            // room left, so it carries its own separator.
            return value.Name.Length >= columnWidth ?
                value.Name + new string(' ', ValueColumnPadding) + description :
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
            return BareAmpersand.Replace(text ?? string.Empty, "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;");
        }

        /// <summary>
        /// Escape a line for a Javadoc comment. Javadoc is a block comment, so
        /// a value carrying "*/" would end it early and leave the rest of the
        /// table as code that does not compile. A line starting with "@", or
        /// carrying "{@", is read as a tag even inside a &lt;pre&gt; block,
        /// which is an error under -Xdoclint. C# and Rust write line comments,
        /// so only Java needs this on top of <see cref="EscapeMarkup"/>.
        /// </summary>
        internal static string EscapeJavaDoc(string text)
        {
            var escaped = EscapeMarkup(text)
                .Replace("*/", "*&#47;")
                .Replace("{@", "{&#64;");
            return escaped.StartsWith("@", StringComparison.Ordinal) ?
                "&#64;" + escaped.Substring(1) :
                escaped;
        }
    }
}
