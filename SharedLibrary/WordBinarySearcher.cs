﻿using System;
using System.Collections.Generic;

namespace PkmnAdvanceTranslation
{
    public class WordBinarySearcher
    {         
        public static List<int> FindMatches(byte[] needle, byte[] haystack)
        {
            if(haystack.Length %2 != 0 || needle.Length != 4)
            {
                throw new Exception("Both needle and haystack need to be comprised of Words(16 bit segments)");
            }

            var matches = new List<int>();
            for (int i = 0; i + 3 < haystack.Length; i += 2)
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
