// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;

namespace Utils
{
    public static class HashUtils
    {
        // System.String.GetHashCode(): http://referencesource.microsoft.com/#mscorlib/system/string.cs,0a17bbac4851d0d4
        // System.Web.Util.StringUtil.GetStringHashCode(System.String): http://referencesource.microsoft.com/#System.Web/Util/StringUtil.cs,c97063570b4e791a
        public static int CombineHashCodes(IEnumerable<int> hashCodes)
        {
            int hash1 = (5381 << 16) + 5381;
            int hash2 = hash1;

            int i = 0;
            foreach (var hashCode in hashCodes)
            {
                if (i % 2 == 0)
                    hash1 = ((hash1 << 5) + hash1 + (hash1 >> 27)) ^ hashCode;
                else
                    hash2 = ((hash2 << 5) + hash2 + (hash2 >> 27)) ^ hashCode;

                ++i;
            }

            return hash1 + (hash2 * 1566083941);
        }

        public static int CombineHashCodes(int hashCodeA, int hashCodeB)
        {
            return CombineHashCodes(new int[] {hashCodeA, hashCodeB});
        }

        public static int CombineHashCodes(int hashCodeA, int hashCodeB, int hashCodeC)
        {
            return CombineHashCodes(new int[] {hashCodeA, hashCodeB, hashCodeC});
        }

        public static int CombineHashCodes(int hashCodeA, int hashCodeB, int hashCodeC, int hashCodeD)
        {
            return CombineHashCodes(new int[] {hashCodeA, hashCodeB, hashCodeC, hashCodeD});
        }
    }
}
