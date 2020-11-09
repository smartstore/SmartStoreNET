// CREDITS to Tawani Anyangwe: http://tawani.blogspot.de/2009/02/topological-sorting-and-cyclic.html
using System;
using System.Collections.Generic;
using System.Linq;

namespace SmartStore.Collections
{

    public interface ITopologicSortable<TKey>
    {
        TKey Key { get; }
        TKey[] DependsOn { get; }
    }

    public static class TopologicalSortExtensions
    {

        public static ITopologicSortable<T>[] SortTopological<T>(this ITopologicSortable<T>[] items)
        {
            return SortTopological(items, null);
        }

        public static ITopologicSortable<T>[] SortTopological<T>(this ITopologicSortable<T>[] items, IEqualityComparer<T> comparer)
        {
            Guard.NotNull(items, nameof(items));

            var sortedIndexes = SortIndexesTopological(items, comparer);
            var sortedList = new List<ITopologicSortable<T>>(sortedIndexes.Length);

            for (var i = 0; i < sortedIndexes.Length; i++)
            {
                //sortedList[i] = items[sortedIndexes[i]];
                sortedList.Add(items[sortedIndexes[i]]);
            }

            return sortedList.ToArray();
        }

        public static int[] SortIndexesTopological<T>(this ITopologicSortable<T>[] items)
        {
            return SortIndexesTopological(items, null);
        }

        public static int[] SortIndexesTopological<T>(this ITopologicSortable<T>[] items, IEqualityComparer<T> comparer)
        {
            Guard.NotNull(items, nameof(items));

            if (items.Length == 0)
            {
                return new int[] { };
            }

            if (comparer == null)
            {
                comparer = EqualityComparer<T>.Default;
            }

            var sorter = new TopologicalSorter(items.Length);
            var indexes = new Dictionary<T, int>(comparer);

            // add vertices
            for (int i = 0; i < items.Length; i++)
            {
                indexes[items[i].Key] = sorter.AddVertex(i);
            }

            // add edges
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i].DependsOn != null)
                {
                    for (int j = 0; j < items[i].DependsOn.Length; j++)
                    {
                        if (indexes.ContainsKey(items[i].DependsOn[j]))
                        {
                            sorter.AddEdge(i, indexes[items[i].DependsOn[j]]);
                        }
                    }
                }
            }

            int[] result = sorter.Sort().Reverse().ToArray();
            return result;
        }
    }

    public class CyclicDependencyException : Exception
    {
        public CyclicDependencyException()
            : base("Cyclic dependency detected")
        {
        }

        public CyclicDependencyException(string message)
            : base(message)
        {
        }
    }

    internal class TopologicalSorter
    {
        #region Private Members

        private readonly int[] _vertices; // list of vertices
        private readonly int[,] _matrix; // adjacency matrix
        private int _numVerts; // current number of vertices
        private readonly int[] _sortedArray;

        #endregion

        #region Ctor

        public TopologicalSorter(int size)
        {
            _vertices = new int[size];
            _matrix = new int[size, size];
            _numVerts = 0;
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    _matrix[i, j] = 0;
                }
            }
            _sortedArray = new int[size]; // sorted vert labels
        }

        #endregion

        #region Public Methods

        public int AddVertex(int vertex)
        {
            _vertices[_numVerts++] = vertex;
            return _numVerts - 1;
        }

        public void AddEdge(int start, int end)
        {
            _matrix[start, end] = 1;
        }

        public int[] Sort() // topological sort
        {
            while (_numVerts > 0) // while vertices remain,
            {
                // get a vertex with no successors, or -1
                int currentVertex = NoSuccessors();
                if (currentVertex == -1)
                {
                    // must be a cycle                
                    throw new CyclicDependencyException();
                }

                // insert vertex label in sorted array (start at end)
                _sortedArray[_numVerts - 1] = _vertices[currentVertex];

                DeleteVertex(currentVertex); // delete vertex
            }

            // vertices all gone; return sortedArray
            return _sortedArray;
        }

        #endregion

        #region Private Helper Methods

        // returns vert with no successors (or -1 if no such verts)
        private int NoSuccessors()
        {
            for (int row = 0; row < _numVerts; row++)
            {
                bool isEdge = false; // edge from row to column in adjMat
                for (int col = 0; col < _numVerts; col++)
                {
                    if (_matrix[row, col] > 0) // if edge to another,
                    {
                        isEdge = true;
                        break; // this vertex has a successor try another
                    }
                }
                if (!isEdge) // if no edges, has no successors
                    return row;
            }
            return -1; // no
        }

        private void DeleteVertex(int delVert)
        {
            // if not last vertex, delete from vertexList
            if (delVert != _numVerts - 1)
            {
                for (int j = delVert; j < _numVerts - 1; j++)
                    _vertices[j] = _vertices[j + 1];

                for (int row = delVert; row < _numVerts - 1; row++)
                    MoveRowUp(row, _numVerts);

                for (int col = delVert; col < _numVerts - 1; col++)
                    MoveColLeft(col, _numVerts - 1);
            }
            _numVerts--; // one less vertex
        }

        private void MoveRowUp(int row, int length)
        {
            for (int col = 0; col < length; col++)
                _matrix[row, col] = _matrix[row + 1, col];
        }

        private void MoveColLeft(int col, int length)
        {
            for (int row = 0; row < length; row++)
                _matrix[row, col] = _matrix[row, col + 1];
        }

        #endregion
    }
}
