namespace Axion.Core {
    internal static class Utilities {
        /// <summary>
        ///     Returns count of bits set to 1 on specified <see cref="number" />.
        /// </summary>
        public static int GetSetBitCount(long number) {
            var count = 0;

            // Loop the value while there are still bits
            while (number != 0) {
                // Remove the end bit
                number = number & (number - 1);

                // Increment the count
                count++;
            }

            // Return the count
            return count;
        }
    }
}