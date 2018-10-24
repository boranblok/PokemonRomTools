using System;
using System.Collections.Generic;

namespace GrabPokemonTextEntries
{
    class ByteBinarySearcher
    {         
        public static List<int> FindMatches(byte[] needle, byte[] haystack)
        {
            var matches = new List<int>();
            for (int i = 0; i + 3 < haystack.Length; i++)
            {
                if(haystack[i] == needle[0] && haystack[i+1] == needle[1] && haystack[i+2] == needle[2] && haystack[i+3] == needle[3])
                {
                    matches.Add(i);
                }
            }
            return matches;
        }
    }
}
