using System;
using System.Collections.Generic;

namespace BenchmarkLogGenerator
{
    public sealed class PriorityQueue<T> where T : IComparable<T>
    {
        private long m_count = long.MinValue;
        private IndexedItem[] m_items;
        private int m_size;

        public PriorityQueue()
            : this(16)
        {
        }

        public PriorityQueue(int capacity)
        {
            m_items = new IndexedItem[capacity];
            m_size = 0;
        }

        private bool IsHigherPriority(int left, int right)
        {
            return m_items[left].CompareTo(m_items[right]) < 0;
        }

        private int Percolate(int index)
        {
            if (index >= m_size || index < 0)
            {
                return index;
            }

            var parent = (index - 1) / 2;
            while (parent >= 0 && parent != index && IsHigherPriority(index, parent))
            {
                // swap index and parent
                var temp = m_items[index];
                m_items[index] = m_items[parent];
                m_items[parent] = temp;

                index = parent;
                parent = (index - 1) / 2;
            }

            return index;
        }

        private void Heapify(int index)
        {
            if (index >= m_size || index < 0)
            {
                return;
            }

            while (true)
            {
                var left = 2 * index + 1;
                var right = 2 * index + 2;
                var first = index;

                if (left < m_size && IsHigherPriority(left, first))
                {
                    first = left;
                }

                if (right < m_size && IsHigherPriority(right, first))
                {
                    first = right;
                }

                if (first == index)
                {
                    break;
                }

                // swap index and first
                var temp = m_items[index];
                m_items[index] = m_items[first];
                m_items[first] = temp;

                index = first;
            }
        }

        public int Count => m_size;

        public T Peek()
        {
            if (m_size == 0)
            {
                throw new InvalidOperationException("Heap empty");
            }

            return m_items[0].Value;
        }

        private void RemoveAt(int index)
        {
            m_items[index] = m_items[--m_size];
            m_items[m_size] = default;

            if (Percolate(index) == index)
            {
                Heapify(index);
            }

            if (m_size < m_items.Length / 4)
            {
                var temp = m_items;
                m_items = new IndexedItem[m_items.Length / 2];
                Array.Copy(temp, 0, m_items, 0, m_size);
            }
        }

        public T Dequeue()
        {
            var result = Peek();
            RemoveAt(0);
            return result;
        }

        public void Enqueue(T item)
        {
            if (m_size >= m_items.Length)
            {
                var temp = m_items;
                m_items = new IndexedItem[m_items.Length * 2];
                Array.Copy(temp, m_items, temp.Length);
            }

            var index = m_size++;
            m_items[index] = new IndexedItem { Value = item, Id = ++m_count };
            Percolate(index);
        }

        public bool Remove(T item)
        {
            for (var i = 0; i < m_size; ++i)
            {
                if (EqualityComparer<T>.Default.Equals(m_items[i].Value, item))
                {
                    RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        private struct IndexedItem : IComparable<IndexedItem>
        {
            public T Value;
            public long Id;

            public int CompareTo(IndexedItem other)
            {
                var c = Value.CompareTo(other.Value);
                if (c == 0)
                {
                    c = Id.CompareTo(other.Id);
                }

                return c;
            }
        }
    }
}