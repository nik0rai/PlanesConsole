﻿using System;
using System.Collections;
using System.Collections.Generic;


namespace TrainCsharp
{
    /// <summary>
    /// The basic data block of a Skip List
    /// </summary>
    class SkipListNode<T> : IDisposable
        where T : IComparable
    {
        private T value;
        private SkipListNode<T> next;
        private SkipListNode<T> previous;
        private SkipListNode<T> above;
        private SkipListNode<T> below;

        public virtual T Value
        {
            get
            {
                return value;
            }
            set
            {
                this.value = value;
            }
        }

        public virtual SkipListNode<T> Next
        {
            get
            {
                return next;
            }
            set
            {
                next = value;
            }
        }

        public virtual SkipListNode<T> Previous
        {
            get
            {
                return previous;
            }
            set
            {
                previous = value;
            }
        }

        public virtual SkipListNode<T> Above
        {
            get
            {
                return above;
            }
            set
            {
                above = value;
            }
        }

        public virtual SkipListNode<T> Below
        {
            get
            {
                return below;
            }
            set
            {
                below = value;
            }
        }

        public SkipListNode(T value)
        {
            this.Value = value;
        }

        public void Dispose()
        {
            value = default(T);
            next = null;
            previous = null;
            above = null;
            previous = null;
        }

        public virtual bool IsHeader()
        {
            return this.GetType() == typeof(SkipListNodeHeader<T>);
        }

        public virtual bool IsFooter()
        {
            return this.GetType() == typeof(SkipListNodeFooter<T>);
        }
    }

    /// <summary>
    /// Represents a Skip List node that is the header of a level
    /// </summary>
    class SkipListNodeHeader<T> : SkipListNode<T>
        where T : IComparable
    {
        public SkipListNodeHeader()
        : base(default(T))
        {
        }
    }

    /// <summary>
    /// Represents a Skip List node that is the footer of a level
    /// </summary>
    class SkipListNodeFooter<T> : SkipListNode<T>
        where T : IComparable
    {
        public SkipListNodeFooter()
        : base(default(T))
        {
        }
    }

    class SkipList<T> : ICollection<T>
        where T : IComparable
    {
        internal SkipListNode<T> topLeft;
        internal SkipListNode<T> bottomLeft;
        internal Random random;
        private int levels;
        private int size;
        private int maxLevels = int.MaxValue;

        public virtual int Levels
        {
            get
            {
                return levels;
            }
        }

        public virtual int MaxLevels
        {
            get
            {
                return maxLevels;
            }
            set
            {
                maxLevels = value;
            }
        }

        public virtual int Count
        {
            get
            {
                return size;
            }
        }

        public virtual bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public virtual SkipListNode<T> Head
        {
            get
            {
                return bottomLeft;
            }
        }

        public SkipList()
        {
            topLeft = getEmptyLevel(); //create an empty level
            bottomLeft = topLeft;
            levels = 1; //update the level count
            size = 0; //no elements added
            random = new Random(); //used for adding new values
        }

        /// <summary>
        /// Creates an empty level with a header and footer node
        /// </summary>
        protected SkipListNode<T> getEmptyLevel()
        {
            SkipListNode<T> negativeInfinity = new SkipListNodeHeader<T>();
            SkipListNode<T> positiveInfinity = new SkipListNodeFooter<T>();

            negativeInfinity.Next = positiveInfinity;
            positiveInfinity.Previous = negativeInfinity;

            return negativeInfinity;
        }

        /// <summary>
        /// Randomly determines how many levels to add
        /// </summary>
        protected int getRandomLevels()
        {
            int newLevels = 0;
            while (random.Next(0, 2) == 1 && newLevels < maxLevels) //1 is heads, 0 is tails
            {
                newLevels++;
            }
            return newLevels;
        }

        /// <summary>
        /// Removes all the empty levels leftover in the Skip List
        /// </summary>
        protected void clearEmptyLevels()
        {
            if (this.levels > 1) //more than one level, don't want to remove bottom level
            {
                SkipListNode<T> currentNode = this.topLeft;

                while (currentNode != this.bottomLeft) //do not remove the bottom level
                {
                    if (currentNode.IsHeader() && currentNode.Next.IsFooter())
                    {
                        SkipListNode<T> belowNode = currentNode.Below;

                        //Remove the empty level

                        //Update pointers
                        topLeft = currentNode.Below;

                        //Remove links
                        currentNode.Next.Dispose();
                        currentNode.Dispose();

                        //Update counters
                        this.levels--;

                        currentNode = belowNode; //scan down
                    }
                    else
                    {
                        break;    //a single non-emtpy level means the rest of the levels are not empty
                    }
                }
            }
        }

        /// <summary>
        /// Add a value to the Skip List
        /// </summary>
        public virtual void Add(T value)
        {
            int valueLevels = getRandomLevels(); //determine height of value's tower

            //Add levels to entire list if necessary
            int newLevelCount = valueLevels - this.levels; //number of levels missing
            while (newLevelCount > 0)
            {
                //Create new level
                SkipListNode<T> newLevel = getEmptyLevel();

                //Link down
                newLevel.Below = this.topLeft;
                this.topLeft.Above = newLevel;
                this.topLeft = newLevel; //update reference to most top-left node

                //Update counters
                newLevelCount--;
                this.levels++;
            }

            //Insert the value in the proper position, creating as many levels as was randomly determined
            SkipListNode<T> currentNode = this.topLeft;
            SkipListNode<T> lastNodeAbove = null; //keeps track of the upper-level nodes in a tower
            int currentLevel = this.levels - 1;

            while (currentLevel >= 0 && currentNode != null)
            {
                if (currentLevel > valueLevels) //too high on the list, nothing would be added to this level
                {
                    currentNode = currentNode.Below; //scan down
                    currentLevel--; //going one level lower
                    continue; //skip adding to this level
                }

                //Add the value to the current level

                //Find the biggest value on the current level that is less than the value to be added
                while (currentNode.Next != null)
                {
                    if (!currentNode.Next.IsFooter() && currentNode.Next.Value.CompareTo(value) < 0) //smaller
                    {
                        currentNode = currentNode.Next;    //keep scanning across
                    }
                    else
                    {
                        break;    //the next node would be bigger than the value
                    }

                }

                //Insert the value right after the node found
                SkipListNode<T> newNode = new SkipListNode<T>(value);
                newNode.Next = currentNode.Next;
                newNode.Previous = currentNode;
                newNode.Next.Previous = newNode;
                currentNode.Next = newNode;

                //Link down/up the tower
                if (lastNodeAbove != null) //is this node part of a tower?
                {
                    lastNodeAbove.Below = newNode;
                    newNode.Above = lastNodeAbove;
                }
                lastNodeAbove = newNode; //start/continue tower

                //Scan down
                currentNode = currentNode.Below;
                currentLevel--;
            }

            this.size++; //update count
        }

        /// <summary>
        /// Returns the first node whose value matches the input value
        /// </summary>
        public virtual SkipListNode<T> Find(T value)
        {
            SkipListNode<T> foundNode = this.topLeft;

            //Look for the highest-level node with an element value matching the parameter value
            while (foundNode != null && foundNode.Next != null)
            {
                if (!foundNode.Next.IsFooter() && foundNode.Next.Value.CompareTo(value) < 0) //next node's value is still smaller
                {
                    foundNode = foundNode.Next;    //keep scanning across
                }
                else
                {
                    if (!foundNode.Next.IsFooter() && foundNode.Next.Value.Equals(value)) //value found
                    {
                        foundNode = foundNode.Next;
                        break;
                    }
                    else
                    {
                        foundNode = foundNode.Below;    //element not in this level, scan down
                    }
                }
            }

            return foundNode;
        }

        /// <summary>
        /// Returns the lowest node on the first tower to match the input value
        /// </summary>
        public virtual SkipListNode<T> FindLowest(T value)
        {
            SkipListNode<T> valueNode = this.Find(value);
            return this.FindLowest(valueNode);
        }

        /// <summary>
        /// Returns the lowest node on the first tower to match the input value
        /// </summary>
        public virtual SkipListNode<T> FindLowest(SkipListNode<T> valueNode)
        {
            if (valueNode == null)
            {
                return null;
            }
            else
            {
                //Scan down to the lowest level
                while (valueNode.Below != null)
                {
                    valueNode = valueNode.Below;
                }
                return valueNode;
            }
        }

        /// <summary>
        /// Returns the highest node on the first tower to match the input value
        /// </summary>
        public virtual SkipListNode<T> FindHighest(T value)
        {
            SkipListNode<T> valueNode = this.Find(value);
            return this.FindHighest(valueNode);
        }

        /// <summary>
        /// Returns the highest node on the first tower to match the input value
        /// </summary>
        public virtual SkipListNode<T> FindHighest(SkipListNode<T> valueNode)
        {
            if (valueNode == null)
            {
                return null;
            }
            else
            {
                //Scan up to the highest level
                while (valueNode.Above != null)
                {
                    valueNode = valueNode.Above;
                }
                return valueNode;
            }
        }

        /// <summary>
        /// Returns whether a value exists in the Skip List
        /// </summary>
        public virtual bool Contains(T value)
        {
            return (this.Find(value) != null);
        }

        /// <summary>
        /// Removes a value or node from the Skip List
        /// </summary>
        public virtual bool Remove(T value)
        {
            SkipListNode<T> valueNode = this.FindHighest(value);
            return this.Remove(valueNode);
        }

        /// <summary>
        /// Removes a value or node from the Skip List
        /// </summary>
        public virtual bool Remove(SkipListNode<T> valueNode)
        {
            if (valueNode == null)
            {
                return false;
            }
            else
            {
                //Make sure node is top-level node in it's tower
                if (valueNode.Above != null)
                {
                    valueNode = this.FindHighest(valueNode);
                }

                //---Delete nodes going down the tower
                SkipListNode<T> currentNodeDown = valueNode;
                while (currentNodeDown != null)
                {
                    //Remove right-left links
                    SkipListNode<T> previousNode = currentNodeDown.Previous;
                    SkipListNode<T> nextNode = currentNodeDown.Next;

                    //Link the previous and next nodes to each other
                    previousNode.Next = nextNode;
                    nextNode.Previous = previousNode;

                    SkipListNode<T> belowNode = currentNodeDown.Below; //scan down
                    currentNodeDown.Dispose(); //unlink previous

                    currentNodeDown = belowNode;
                }

                //update counter
                this.size--;

                //Clean up the Skip List by removing levels that are now empty
                this.clearEmptyLevels();

                return true;
            }
        }

        /// <summary>
        /// Removes all values in the Skip List
        /// </summary>
        public virtual void Clear()
        {
            SkipListNode<T> currentNode = this.Head;

            while (currentNode != null)
            {
                SkipListNode<T> nextNode = currentNode.Next; //save reference to next node

                if (!currentNode.IsHeader() && !currentNode.IsFooter())
                {
                    this.Remove(currentNode);
                }

                currentNode = nextNode;
            }
        }

        /// <summary>
        /// Copies the values of the Skip List to an array
        /// </summary>
        public virtual void CopyTo(T[] array)
        {
            CopyTo(array, 0);
        }

        /// <summary>
        /// Copies the values of the Skip List to an array
        /// </summary>
        public virtual void CopyTo(T[] array, int startIndex)
        {
            IEnumerator<T> enumerator = this.GetEnumerator();

            for (int i = startIndex; i < array.Length; i++)
            {
                if (enumerator.MoveNext())
                {
                    array[i] = enumerator.Current;
                }
                else
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Gets the number of levels of a value in the Skip List
        /// </summary>
        public virtual int GetHeight(T value)
        {
            SkipListNode<T> valueNode = this.FindLowest(value);
            return this.GetHeight(valueNode);
        }

        /// <summary>
        /// Gets the number of levels of a value in the Skip List
        /// </summary>
        public virtual int GetHeight(SkipListNode<T> valueNode)
        {
            int height = 0;
            SkipListNode<T> currentNode = valueNode;

            //Move all the way down to the bottom first
            while (currentNode.Below != null)
            {
                currentNode = currentNode.Below;
            }

            //Count going back up to the top
            while (currentNode != null)
            {
                height++;
                currentNode = currentNode.Above;
            }

            return height;
        }

        /// <summary>
        /// Gets the enumerator for the Skip List
        /// </summary>
        public IEnumerator<T> GetEnumerator()
        {
            return new SkipListEnumerator(this);
        }

        /// <summary>
        /// Gets the enumerator for the Skip List
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <summary>
        /// Enumerator for a Skip List. Scans across the lowest level of a Skip List.
        /// </summary>
        internal class SkipListEnumerator : IEnumerator<T>
        {
            private SkipListNode<T> current;
            private SkipList<T> skipList;

            public SkipListEnumerator(SkipList<T> skipList)
            {
                this.skipList = skipList;
            }

            public T Current
            {
                get
                {
                    return current.Value;
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    return this.Current;
                }
            }

            public void Dispose()
            {
                current = null;
            }

            public void Reset()
            {
                current = null;
            }

            public bool MoveNext()
            {
                if (current == null)
                {
                    current = this.skipList.Head.Next;    //Head is header node, start after
                }
                else
                {
                    current = current.Next;
                }

                if (current != null && current.IsFooter())
                {
                    current = null;    //end of list
                }

                return (current != null);
            }
        }
    }
}
