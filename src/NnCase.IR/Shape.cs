﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace NnCase.IR
{
    [DebuggerDisplay("{DebuggerDisplay}")]
    public class Shape : IEquatable<Shape>
    {
        public const int MaxSmallSize = 4;

        private SmallArray _smallValues;
        private readonly int[] _largeValues;

        public int this[int index]
        {
            get
            {
                if (index >= Count)
                    throw new ArgumentOutOfRangeException(nameof(index));

                if (Count <= MaxSmallSize)
                {
                    unsafe
                    {
                        return _smallValues[index];
                    }
                }
                else
                {
                    return _largeValues[index];
                }
            }

            set
            {
                if (index >= Count)
                    throw new ArgumentOutOfRangeException(nameof(index));

                if (Count <= MaxSmallSize)
                {
                    unsafe
                    {
                        _smallValues[index] = value;
                    }
                }
                else
                {
                    _largeValues[index] = value;
                }
            }
        }

        public int Count { get; }

        public Shape(ReadOnlySpan<int> shape)
        {
            Count = shape.Length;
            if (Count <= MaxSmallSize)
            {
                for (int i = 0; i < Count; i++)
                    _smallValues[i] = shape[i];
                _largeValues = null;
            }
            else
            {
                _largeValues = shape.ToArray();
            }
        }

        public Shape(int d0, int d1, int d2, int d3)
        {
            Count = 4;
            _largeValues = null;
            _smallValues[0] = d0;
            _smallValues[1] = d1;
            _smallValues[2] = d2;
            _smallValues[3] = d3;
        }

        public static unsafe implicit operator ReadOnlySpan<int>(Shape shape)
        {
            if (shape.Count <= MaxSmallSize)
            {
                fixed (int* values = shape._smallValues.Values)
                {
                    return new ReadOnlySpan<int>(values, shape.Count);
                }
            }
            else
            {
                return shape._largeValues;
            }
        }

        public static implicit operator Shape(ReadOnlySpan<int> shape)
        {
            return new Shape(shape);
        }

        public static implicit operator Shape(int[] shape)
        {
            return new Shape(shape);
        }

        public Enumerator GetEnumerator()
            => new Enumerator(this);

        public int[] ToArray()
        {
            if (Count <= MaxSmallSize)
            {
                var array = new int[Count];

                for (int i = 0; i < Count; i++)
                    array[i] = _smallValues[i];
                return array;
            }
            else
            {
                return (int[])_largeValues.Clone();
            }
        }

        public override bool Equals(object obj)
        {
            return obj is Shape shape && Equals(shape);
        }

        public unsafe bool Equals(Shape other)
        {
            if (Count == other.Count)
            {
                if (Count <= MaxSmallSize)
                {
                    for (int i = 0; i < Count; i++)
                    {
                        if (_smallValues[i] != other._smallValues[i])
                            return false;
                    }
                }
                else
                {
                    return Array.Equals(_largeValues, other._largeValues);
                }

                return true;
            }

            return false;
        }

        public override int GetHashCode()
        {
            int hashCode = 0;

            if (Count <= MaxSmallSize)
            {
                for (int i = 0; i < Count; i++)
                {
                    hashCode = HashCode.Combine(hashCode, _smallValues[i].GetHashCode());
                }
            }
            else
            {
                for (int i = 0; i < Count; i++)
                {
                    hashCode = HashCode.Combine(hashCode, _largeValues[i].GetHashCode());
                }
            }

            return hashCode;
        }

        public Shape Clone()
        {
            return new Shape(this);
        }

        public static bool operator ==(Shape left, Shape right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Shape left, Shape right)
        {
            return !(left == right);
        }

        public ref struct Enumerator
        {
            private ReadOnlySpan<int> _values;
            private int _index;

            public int Current => _values[_index];

            internal Enumerator(ReadOnlySpan<int> values)
            {
                _values = values;
                _index = -1;
            }

            public bool MoveNext()
            {
                var index = _index + 1;
                if (index < _values.Length)
                {
                    _index = index;
                    return true;
                }

                return false;
            }
        }

        private struct SmallArray
        {
            public unsafe fixed int Values[MaxSmallSize];

            public ref int this[int index]
            {
                get { unsafe { return ref Values[index]; } }
            }
        }

        private string DebuggerDisplay =>
            $"{{{string.Join(",", ToArray())}}}";
    }
}