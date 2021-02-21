using System;
using System.Text;

namespace BenchmarkLogGenerator.Utilities
{
    #region class StringBuilderCache
    /// <summary>
    /// A helper for creating a per-thread ([ThreadStatic]) StringBuilder cache.
    /// 
    /// This code is stolen from the .NET Framework source code. The code requires
    /// the caller to declare a [ThreadStatic] field member of type StringBuilder,
    /// and provide a reference to that field with each operation. See
    /// <see cref="UtilsStringBuilderCache"/> for an example.
    /// 
    /// Note that it's not advisable to share such objects if their lifetime
    /// overlaps (which is why <see cref="UtilsStringBuilderCache"/> is made
    /// internal -- to prevent people from making mistakes).
    /// </summary>
    public static class StringBuilderCache
    {
        private const int MAX_BUILDER_SIZE = 24 * 1024;
        private const int DEFAULT_CAPACITY = 16;

        /// <summary>
        /// Given a [ThreadStatic] field, returns a "clean" instance of <see cref="StringBuilder"/>.
        /// </summary>
        /// <param name="threadStaticStringBuilder">[ThreadStatic] static System.Text.StringBuilder s_myStringBuilderCache</param>
        /// <param name="capacity">Capacity to ensure the returned object holds.</param>
        /// <param name="maxBuilderSize">The maximum size to allow the string builder to grow to.</param>
        /// <returns></returns>
        public static StringBuilder Acquire(ref StringBuilder threadStaticStringBuilder, int capacity = DEFAULT_CAPACITY, int maxBuilderSize = MAX_BUILDER_SIZE)
        {
            if (capacity <= maxBuilderSize)
            {
                StringBuilder sb = threadStaticStringBuilder;
                if (sb != null)
                {
                    // Avoid stringbuilder block fragmentation by getting a new StringBuilder
                    // when the requested size is larger than the current capacity
                    if (capacity <= sb.Capacity)
                    {
                        threadStaticStringBuilder = null;
                        sb.Clear();
                        return sb;
                    }
                }
            }
            return new StringBuilder(capacity);
        }

        /// <summary>
        /// Given a [ThreadStatic] field, returns an instance of <see cref="StringBuilder"/> with the given initial value.
        /// </summary>
        /// <param name="threadStaticStringBuilder">[ThreadStatic] static System.Text.StringBuilder s_myStringBuilderCache</param>
        /// <param name="value">Initial value to assign the <see cref="StringBuilder"/> being returned.</param>
        /// <returns></returns>
        public static StringBuilder Acquire(ref StringBuilder threadStaticStringBuilder, string value)
        {
            StringBuilder sb = Acquire(ref threadStaticStringBuilder, System.Math.Max(value.Length, DEFAULT_CAPACITY));
            sb.Append(value);
            return sb;
        }

        /// <summary>
        /// Given a [ThreadStatic] field and an existing <see cref="StringBuilder"/> that was acquired from it,
        /// release the acquired instance to make it available in the future to other functions.
        /// </summary>
        /// <param name="threadStaticStringBuilder">[ThreadStatic] static System.Text.StringBuilder s_myStringBuilderCache</param>
        /// <param name="sb"></param>
        public static void Release(ref StringBuilder threadStaticStringBuilder, StringBuilder sb, int maxBuilderSize = MAX_BUILDER_SIZE)
        {
            if (sb.Capacity <= maxBuilderSize)
            {
                threadStaticStringBuilder = sb;
            }
        }

        /// <summary>
        /// Given a [ThreadStatic] field and an existing <see cref="StringBuilder"/> that was acquired from it,
        /// release the acquired instance to make it available in the future to other functions.
        /// Returns the string held in the returned <paramref name="sb"/>.
        /// </summary>
        /// <param name="threadStaticStringBuilder">[ThreadStatic] static System.Text.StringBuilder s_myStringBuilderCache</param>
        /// <param name="sb"></param>
        public static string GetStringAndRelease(ref StringBuilder threadStaticStringBuilder, StringBuilder sb, int maxBuilderSize = MAX_BUILDER_SIZE)
        {
            string result = sb.ToString();
            Release(ref threadStaticStringBuilder, sb, maxBuilderSize);
            return result;
        }

        public static string GetStringAndClear(ref StringBuilder threadStaticStringBuilder, StringBuilder sb)
        {
            string result = sb.ToString();
            sb.Clear();
            return result;
        }
    }
    #endregion

    #region class UtilsStringBuilderCache
    /// <summary>
    /// A per-thread cache of up to one <see cref="StringBuilder"/> object.
    /// This code is stolen from the .NET Framework source code. It is explicitly
    /// </summary>
    internal static class UtilsStringBuilderCache
    {
        private const int MAX_BUILDER_SIZE = 24*1024; // Originally 260
        private const int DEFAULT_CAPACITY = 16;

        [ThreadStatic]
        private static StringBuilder t_cachedInstance;

        public static StringBuilder Acquire(int capacity = DEFAULT_CAPACITY)
        {
            return StringBuilderCache.Acquire(ref t_cachedInstance, capacity);
        }

        public static StringBuilder Acquire(string value)
        {
            return StringBuilderCache.Acquire(ref t_cachedInstance, value);
        }

        public static void Release(StringBuilder sb)
        {
            StringBuilderCache.Release(ref t_cachedInstance, sb);
        }

        public static string GetStringAndRelease(StringBuilder sb)
        {
            return StringBuilderCache.GetStringAndRelease(ref t_cachedInstance, sb);
        }

        public static string GetStringAndClear(StringBuilder sb)
        {
            return StringBuilderCache.GetStringAndClear(ref t_cachedInstance, sb);
        }
    }
    #endregion
}