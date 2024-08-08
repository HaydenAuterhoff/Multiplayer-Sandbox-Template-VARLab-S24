using System;
using System.Collections.Generic;

namespace VARLab.Sandbox.SAR
{
    /// <summary>
    ///     Static utility that generates random strings 
    ///     comprised of the NATO phonetic alphabet code words.
    /// </summary>
    /// <remarks>
    ///     This is a quick word generator example that can be expanded to
    ///     support multiple class instances of a random name generator where
    ///     the constructor takes a set of words and optionally a randomizer seed
    ///     and allows the caller to use GetRandomName() as a member function with
    ///     that custom dictionary
    /// </remarks>
    public class RandomNameGenerator
    {
        public static string[] Alphabet = {
            "Alfa", "Bravo", "Charlie", "Delta", "Echo", "Foxtrot", "Golf", "Hotel",
            "India", "Juliett", "Kilo", "Lima", "Mike", "November", "Oscar", "Papa",
            "Quebec", "Romeo", "Sierra", "Tango", "Uniform", "Victor", "Whiskey",
            "Xray", "Yankee", "Zulu"
        };

        private readonly static Random Randomizer = new();


        /// <summary>
        ///     Generates a random string comprised of words from the <see cref="Alphabet"/> static array.
        /// </summary>
        /// <param name="length">Length of the random string in number of tokens (words)</param>
        /// <param name="spaces">If true, a space will be placed between each token</param>
        /// <returns></returns>
        public static string GetRandomName(int length, bool spaces = false)
        {
            List<string> names = new();

            for (int index = 0; index < length; index++)
            {
                names.Add(Alphabet[Randomizer.Next(Alphabet.Length)]);
            }

            // Tokens are separated by spaces only if the 'spaces' bool is true
            return string.Join(spaces ? " " : string.Empty, names);
        }
    }
}