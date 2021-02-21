using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BenchmarkLogGenerator.Utilities
{
    public static class ExtendedEnumerable
    {
        public static bool SafeFastAny<T>(this IEnumerable<T> collection, Func<T, bool> predicate)
        {
            if (!collection.SafeFastAny())
            {
                return false;
            }

            return collection.Any(predicate);
        }
        public static bool SafeFastAny(this IEnumerable collection)
        {
            if (collection == null)
            {
                return false;
            }

            { // Scope to ensure no cross-talk with next block

                if (collection is System.Collections.ICollection asICollection)
                {
                    return asICollection.Count != 0;
                }
            }

            var e = collection.GetEnumerator();
            using (e as IDisposable)
            {
                if (e.MoveNext()) return true;
            }
            return false;
        }
    }
    public sealed class ExtendedStringBuilder
    {
        #region fields and properties
        private const int c_tabSize = 4;

        private int m_tabSize;
        private int m_indentation;
        private char[] m_indentors;
        private bool m_firstColumn;

        public StringBuilder StringBuilder { get; private set; }
        #endregion

        #region constructor
        /// <summary>
        /// Creates the StringBuilder with no inedtation
        /// </summary>
        public ExtendedStringBuilder()
            : this(c_tabSize, 0, null)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="tabSize">How many spaces to indent by</param>
        /// <param name="initialIndentation">Initial indentation to start with</param>
        /// <param name="indentors">Two characters -- one to indent, another to unindent (null means don't add indent/unindent chars)</param>
        public ExtendedStringBuilder(int tabSize, int initialIndentation, char[] indentors)
        {
            m_tabSize = tabSize;
            m_indentation = initialIndentation;
            m_indentors = indentors;
            m_firstColumn = true;

            StringBuilder = new StringBuilder();
        }
        #endregion

        #region public methods
        /// <summary>
        /// Appends multiple lines to a string builder
        /// </summary>
        public void AppendLines(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                StringBuilder.AppendLine();
                m_firstColumn = true;
                return;
            }

            int current = 0;
            while (true)
            {
                AppendIndent();
                var nl = str.IndexOf('\r', current);
                if (nl < 0)
                {
                    // Everything remaining is a single line
                    if (current == 0)
                    {
                        StringBuilder.AppendLine(str);
                    }
                    else
                    {
                        for (; current < str.Length; current++)
                        {
                            StringBuilder.Append(str[current]);
                        }
                        StringBuilder.AppendLine();
                    }
                    m_firstColumn = true;
                    return;
                }

                for (; current < nl; current++)
                {
                    StringBuilder.Append(str[current]);
                }
                StringBuilder.AppendLine();
                m_firstColumn = true;
                current++; // Move beyond the \r

                if (current < str.Length)
                {
                    if (str[current] == '\n')
                    {
                        current++;
                    }
                }
                if (current == str.Length)
                {
                    return;
                }
            }
        }

        private void AppendIndent()
        {
            if (m_firstColumn)
            {
                for (int i = 0; i < m_indentation * m_tabSize; i++)
                {
                    StringBuilder.Append(' ');
                }
                m_firstColumn = false;
            }
        }

        private void UpdateFirstColumn(string writtenLast)
        {
            if (!string.IsNullOrEmpty(writtenLast))
            {
                m_firstColumn = writtenLast.EndsWith(System.Environment.NewLine);
            }
        }

        /// <summary>
        /// Appends a string to the string builder
        /// </summary>
        /// <param name="str">The string to append</param>
        public void Append(string str)
        {
            AppendIndent();
            StringBuilder.Append(str);
            UpdateFirstColumn(str);
        }

        /// <summary>
        /// Appends a string to the string builder (no indent).
        /// </summary>
        /// <param name="str">The string to append</param>
        public void AppendNoIndent(string str)
        {
            StringBuilder.Append(str);
            UpdateFirstColumn(str);
        }

        /// <summary>
        /// Appends a line to the string builder
        /// </summary>
        /// <param name="str">The line to append</param>
        public void AppendLine(string str)
        {
            AppendIndent();
            StringBuilder.AppendLine(str);
            m_firstColumn = true;
        }

        /// <summary>
        /// Appends a character to the string builder
        /// </summary>
        /// <param name="c">The character to append</param>
        public void AppendLine(char c)
        {
            for (int i = 0; i < m_indentation * m_tabSize; i++)
            {
                StringBuilder.Append(' ');
            }
            StringBuilder.Append(c);
            StringBuilder.AppendLine();
            m_firstColumn = true;
        }

        /// <summary>
        /// Appends an empty line to the string builder
        /// </summary>
        public void AppendLine()
        {
            StringBuilder.AppendLine();
            m_firstColumn = true;
        }

        /// <summary>
        /// returns the built string
        /// </summary>
        override public string ToString()
        {
            return StringBuilder.ToString();
        }

        /// <summary>
        /// Increase the indent level, potentially adding an indentor line.
        /// </summary>
        public void Indent()
        {
            if (m_indentors != null)
            {
                AppendLine(m_indentors[0]);
            }
            m_indentation++;
        }

        /// <summary>
        /// Decrease the indent level, potentially adding an unindentor line
        /// </summary>
        public void Unindent()
        {
            m_indentation--;
            if (m_indentors != null)
            {
                AppendLine(m_indentors[1]);
            }
        }
        #endregion
    }
}
