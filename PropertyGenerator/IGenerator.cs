using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropertyGenerator
{
    /// <summary>
    /// Interface used to build the data classes for various strongly typed
    /// languages. This can be implemented for different engines, e.g.
    /// device detection
    /// </summary>
    public interface IGenerator
    {
        /// <summary>
        /// Build the data classes for C# and write the .cs files to the directory
        /// provided.
        /// </summary>
        /// <param name="path"></param>
        void BuildCSharp(string path);
        /// <summary>
        /// Build the data classes for Java and write the .cs files to the directory
        /// provided.
        /// </summary>
        /// <param name="path"></param>
        void BuildJava(string path);
        /// <summary>
        /// Build the data classes for Rust and write the .rs files to the directory
        /// provided.
        /// </summary>
        /// <param name="path"></param>
        void BuildRust(string path);
    }
}
