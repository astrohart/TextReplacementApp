using System.Linq;

namespace TextReplacementApp
{
    /// <summary>
    /// Exposes static extension methods for checking the contents of a list.
    /// </summary>
    public static class ListExtensions
    {
        /// <summary>
        /// Compares the <paramref name="value" /> object with the
        /// <paramref name="testObjects" /> provided, to see if any of the
        /// <paramref name="testObjects" /> is a match.
        /// </summary>
        /// <typeparam name="T"> Type of the object to be tested. </typeparam>
        /// <param name="value"> Source object to check. </param>
        /// <param name="testObjects">
        /// Object or objects that should be compared to value
        /// with the <see cref="M:System.Object.Equals" /> method.
        /// </param>
        /// <returns>
        /// True if any of the <paramref name="testObjects" /> equals the value;
        /// false otherwise.
        /// </returns>
        public static bool IsAnyOf<T>(this T value, params T[] testObjects)
            => testObjects.Contains(value);
    }
}