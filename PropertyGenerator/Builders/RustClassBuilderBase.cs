using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using PropertyGenerationTool;

namespace PropertyGenerator.Builders
{
    /// <summary>
    /// Class builder for Rust.
    /// Emits one source file containing a trait of strongly-typed read accessors,
    /// the matching impl block that delegates to the hand-written data base, and a
    /// table pairing each property with its core value type. Methods for getting
    /// info from a property are extracted so the class is not tied to the type of
    /// property, mirroring the C# and Java builders.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    abstract class RustClassBuilderBase<T> : ClassBuilderBase<T>
    {
        /// <summary>
        /// The hand-written data base type the generated impl is implemented for,
        /// and which exposes the by-name store getters delegated to.
        /// </summary>
        protected abstract string DataBaseType { get; }

        /// <summary>
        /// The by-name store getter on the data base to delegate to for a property,
        /// e.g. "string", "integer", "boolean", "float", "weighted_string".
        /// </summary>
        protected abstract string GetStoreGetter(T property);

        /// <summary>
        /// The core PropertyValueType variant name a property is published as in
        /// the metadata table, e.g. "String", "Integer", "KeyValueList".
        /// </summary>
        protected abstract string GetCoreValueType(T property);

        /// <summary>
        /// Convert a 51Degrees PascalCase property name into an idiomatic Rust
        /// snake_case accessor name. An underscore is inserted before an upper-case
        /// letter that follows a lower-case letter or a digit, and before an
        /// upper-case letter that starts a word (an upper-case run followed by a
        /// lower-case letter). Trailing digits stay attached to the preceding word.
        /// </summary>
        internal static string ToSnakeCase(string name)
        {
            var cleaned = name.Replace("/", "").Replace("-", "");
            var sb = new StringBuilder();
            for (int i = 0; i < cleaned.Length; i++)
            {
                char c = cleaned[i];
                if (char.IsUpper(c))
                {
                    bool prevLowerOrDigit = i > 0 &&
                        (char.IsLower(cleaned[i - 1]) || char.IsDigit(cleaned[i - 1]));
                    bool prevUpper = i > 0 && char.IsUpper(cleaned[i - 1]);
                    bool nextLower = i + 1 < cleaned.Length && char.IsLower(cleaned[i + 1]);
                    if (i > 0 && (prevLowerOrDigit || (prevUpper && nextLower)))
                    {
                        sb.Append('_');
                    }
                    sb.Append(char.ToLowerInvariant(c));
                }
                else
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        internal string GetGetterName(T property)
        {
            return ToSnakeCase(GetPropertyName(property));
        }

        /// <summary>
        /// The doc text for a property: its metadata description, or a generated
        /// fallback naming the property when the metadata carries no description,
        /// so the accessor always has a non-empty doc comment.
        /// </summary>
        private string PropertyDoc(T property)
        {
            var description = GetPropertyDescription(property);
            if (string.IsNullOrWhiteSpace(description))
            {
                return $"The `{GetPropertyName(property)}` property.";
            }
            return description;
        }

        internal void Build(
            string copyright,
            string traitName,
            string moduleDescription,
            string traitDescription,
            IEnumerable<string> uses,
            T[] properties,
            string outputPath)
        {
            var filtered = properties
                .Where(p => Constants.excludedProperties.Contains(GetPropertyName(p)) == false)
                .OrderBy(GetPropertyName)
                .ToArray();

            using var outputStream = new FileStream(outputPath, FileMode.Create);
            using var writer = new StreamWriter(outputStream);

            writer.WriteLine(copyright);
            WriteInnerDoc(writer, moduleDescription);
            // Property descriptions are taken verbatim from the metadata, so some
            // contain bare URLs or angle-bracketed HTML-like tokens (the HTML5
            // feature-support properties name elements such as `<header>`).
            // Rustdoc would otherwise warn on these, so the lints are allowed for
            // this generated file rather than mangling the descriptions.
            writer.WriteLine("#![allow(rustdoc::bare_urls, rustdoc::invalid_html_tags)]");
            writer.WriteLine();
            foreach (var use in uses)
            {
                writer.WriteLine($"use {use};");
            }
            writer.WriteLine();

            // Trait.
            WriteOuterDoc(writer, traitDescription, "");
            writer.WriteLine($"pub trait {traitName}: AspectData {{");
            for (int i = 0; i < filtered.Length; i++)
            {
                var property = filtered[i];
                WriteOuterDoc(writer, PropertyDoc(property), "    ");
                writer.WriteLine(
                    $"    fn {GetGetterName(property)}(&self) -> {GetPropertyType(property)};");
                if (i != filtered.Length - 1)
                {
                    writer.WriteLine();
                }
            }
            writer.WriteLine("}");
            writer.WriteLine();

            // Impl that delegates to the by-name store getters.
            writer.WriteLine($"impl {traitName} for {DataBaseType} {{");
            for (int i = 0; i < filtered.Length; i++)
            {
                var property = filtered[i];
                writer.WriteLine(
                    $"    fn {GetGetterName(property)}(&self) -> {GetPropertyType(property)} {{");
                writer.WriteLine(
                    $"        self.{GetStoreGetter(property)}(\"{GetPropertyName(property)}\")");
                writer.WriteLine("    }");
                if (i != filtered.Length - 1)
                {
                    writer.WriteLine();
                }
            }
            writer.WriteLine("}");
            writer.WriteLine();

            // Property value-type table, used to publish per-property metadata.
            writer.WriteLine(
                "/// Every generated property paired with its core value type, in name");
            writer.WriteLine(
                "/// order. The data base uses this to publish per-property metadata.");
            writer.WriteLine(
                "pub const GENERATED_PROPERTY_TYPES: &[(&str, PropertyValueType)] = &[");
            foreach (var property in filtered)
            {
                writer.WriteLine(
                    $"    (\"{GetPropertyName(property)}\", " +
                    $"PropertyValueType::{GetCoreValueType(property)}),");
            }
            writer.WriteLine("];");
        }

        /// <summary>
        /// Write a description as one or more `//!` inner doc lines.
        /// </summary>
        private static void WriteInnerDoc(StreamWriter writer, string description)
        {
            foreach (var line in SplitLines(description))
            {
                writer.WriteLine(line.Length == 0 ? "//!" : "//! " + line);
            }
        }

        /// <summary>
        /// Write a description as one or more `///` outer doc lines at the given
        /// indent.
        /// </summary>
        private static void WriteOuterDoc(StreamWriter writer, string description, string indent)
        {
            foreach (var line in SplitLines(description))
            {
                writer.WriteLine(line.Length == 0 ? $"{indent}///" : $"{indent}/// " + line);
            }
        }

        private static IEnumerable<string> SplitLines(string description)
        {
            return (description ?? string.Empty)
                .Replace("\r\n", "\n")
                .Split('\n');
        }
    }
}
