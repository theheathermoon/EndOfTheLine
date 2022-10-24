using System;
using System.Collections.Generic;

namespace WaterBuoyancy.Collections
{
    public class MinHeap<T>
    {
        private const int INITIAL_CAPACITY = 4;

        private T[] arr;
        private int lastItemIndex;
        private IComparer<T> comparer;

        public MinHeap() : this(INITIAL_CAPACITY, Comparer<T>.Default)
        {
        }

        public MinHeap(int capacity) : this(capacity, Comparer<T>.Default)
        {
        }

        public MinHeap(IComparer<T> comparer): this(INITIAL_CAPACITY, comparer)
        {
        }

        public MinHeap(int capacity, IComparer<T> comparer)
        {
            arr = new T[capacity];
            lastItemIndex = -1;
            this.comparer = comparer;
        }

        public int Count => lastItemIndex + 1;

        public void Add(T item)
        {
            if (lastItemIndex == arr.Length - 1)
            {
                Resize();
            }

            lastItemIndex++;
            arr[lastItemIndex] = item;

            MinHeapifyUp(lastItemIndex);
        }

        public T Remove()
        {
            if (lastItemIndex == -1)
            {
                throw new InvalidOperationException("The heap is empty");
            }

            T removedItem = arr[0];
            arr[0] = arr[lastItemIndex];
            lastItemIndex--;

            MinHeapifyDown(0);

            return removedItem;
        }

        public T Peek()
        {
            if (lastItemIndex == -1)
            {
                throw new InvalidOperationException("The heap is empty");
            }

            return arr[0];
        }

        public void Clear()
        {
            lastItemIndex = -1;
            arr = new T[INITIAL_CAPACITY];
        }

        private void MinHeapifyUp(int index)
        {
            if (index == 0)
            {
                return;
            }

            int childIndex = index;
            int parentIndex = (index - 1) / 2;

            if (comparer.Compare(arr[childIndex], arr[parentIndex]) < 0)
            {
                // swap the parent and the child
                T temp = arr[childIndex];
                arr[childIndex] = arr[parentIndex];
                arr[parentIndex] = temp;

                MinHeapifyUp(parentIndex);
            }
        }

        private void MinHeapifyDown(int index)
        {
            int leftChildIndex = index * 2 + 1;
            int rightChildIndex = index * 2 + 2;
            int smallestItemIndex = index; // The index of the parent

            if (leftChildIndex <= lastItemIndex &&
                comparer.Compare(arr[leftChildIndex], arr[smallestItemIndex]) < 0)
            {
                smallestItemIndex = leftChildIndex;
            }

            if (rightChildIndex <= lastItemIndex &&
                comparer.Compare(arr[rightChildIndex], arr[smallestItemIndex]) < 0)
            {
                smallestItemIndex = rightChildIndex;
            }

            if (smallestItemIndex != index)
            {
                // swap the parent with the smallest of the child items
                T temp = arr[index];
                arr[index] = arr[smallestItemIndex];
                arr[smallestItemIndex] = temp;

                MinHeapifyDown(smallestItemIndex);
            }
        }

        private void Resize()
        {
            T[] newArr = new T[arr.Length * 2];
            for (int i = 0; i < arr.Length; i++)
            {
                newArr[i] = arr[i];
            }

            arr = newArr;
        }
    }
}