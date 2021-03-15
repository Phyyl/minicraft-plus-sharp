using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MinicraftPlusSharp.Java
{
    public abstract class JavaEnum<T> where T : JavaEnum<T>
    {
        private static int ordinal = 0;
        private static readonly Dictionary<string, T> valuesByName = new();
        private static readonly Dictionary<int, T> valuesByOrdinal = new();

        public static T[] All => valuesByOrdinal.Values.ToArray();

        public readonly int Ordinal;
        public readonly string Name;

        protected JavaEnum(string name = default)
            : this(ordinal++, name)
        {
        }

        protected JavaEnum(int ordinal, string name = default)
        {
            Ordinal = ordinal;
            Name = name;

            if (this is T t)
            {
                if (name is not null && !valuesByName.TryAdd(name, t))
                {
                    throw new InvalidOperationException($@"Value with name ""{name}"" was already added to this JavaEnum<{GetType().Name}>.");
                }

                if (!valuesByOrdinal.TryAdd(ordinal, t))
                {
                    throw new InvalidOperationException($@"Value with ordinal ""{ordinal}"" was already added to this JavaEnum<{GetType().Name}>.");
                }
            }
        }

        public static bool IsDefined(T value) => All.Contains(value);
        public static bool IsDefined(int ordinal) => valuesByOrdinal.ContainsKey(ordinal);
        public static bool IsDefined(string name) => valuesByName.ContainsKey(name);
        public static bool TryGetValue(int ordinal, out T value) => valuesByOrdinal.TryGetValue(ordinal, out value);
        public static bool TryGetValue(string name, out T value) => valuesByName.TryGetValue(name, out value);

        public static implicit operator int(JavaEnum<T> value) => value.Ordinal;
        public static implicit operator string(JavaEnum<T> value) => value.Name;
    }
}
