// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Shwarm.Vdb
{
    public class Singleton<T> where T : class, new()
    {
        protected Singleton()
        {
        }

        public static T Instance => Nested.instance;

        private class Nested
        {
            // Explicit static constructor to tell C# compiler
            // not to mark type as beforefieldinit
            static Nested()
            {
            }

            internal static readonly T instance = new T();
        }
    }
}
