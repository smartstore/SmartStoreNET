using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace SmartStore.Collections
{
    public class TreeNode<T> : ICloneable<TreeNode<T>>
    {
        private readonly LinkedList<TreeNode<T>> _children = new LinkedList<TreeNode<T>>();
		private int? _depth = null;

        public TreeNode(T value)
        {
            Value = value;
        }

        #region Properties

        public TreeNode<T> Parent { get; private set; }

        public T Value { get; set; }

        public TreeNode<T> this[int i]
        {
            get
            {
                return _children.ElementAt(i);
            }
        }

        public IEnumerable<TreeNode<T>> Children
        {
            get
            {
                return _children;
            }
        }

        public IEnumerable<TreeNode<T>> LeafNodes
        {
            get
            {
                return _children.Where(x => x.IsLeaf);
            }
        }

        public IEnumerable<TreeNode<T>> NonLeafNodes
        {
            get
            {
                return _children.Where(x => !x.IsLeaf);
            }
        }


        public TreeNode<T> FirstChild
        {
            get
            {
                var first = _children.First;
                if (first != null)
                    return first.Value;
                return null;
            }
        }

        public TreeNode<T> LastChild
        {
            get
            {
                var last = _children.Last;
                if (last != null)
                    return last.Value;
                return null;
            }
        }

        public bool IsLeaf
        {
            get
            {
                return _children.Count == 0;
            }
        }

        public bool HasChildren
        {
            get
            {
                return _children.Count > 0;
            }
        }

        public bool IsRoot
        {
            get
            {
                return Parent == null;
            }
        }

		public int Depth
		{
			get
			{
				if (!_depth.HasValue)
				{
					var node = this;
					int depth = -1;
					while (node != null && !node.IsRoot)
					{
						depth++;
						node = node.Parent;
					}
					_depth = depth;
				}

				return _depth.Value;
			}
		}

        public TreeNode<T> Root
        {
            get
            {
                var root = this;
                while (root.Parent != null)
                {
                    root = root.Parent;
                }
                return root;
            }
        }

        public TreeNode<T> Next
        {
            get
            {
                if (this.Parent != null)
                {
                    var self = this.Parent._children.Find(this);
                    var next = self != null ? self.Next : null;
                    if (next != null)
                        return next.Value;
                }
                return null;
            }
        }

        public TreeNode<T> Previous
        {
            get
            {
                if (this.Parent != null)
                {
                    var self = this.Parent._children.Find(this);
                    var prev = self != null ? self.Previous : null;
                    if (prev != null)
                        return prev.Value;
                }
                return null;
            }
        }

        #endregion

        #region Methods/Declarations

        private void AddChild(TreeNode<T> node, bool clone, bool append = true)
        {
            var newNode = node;
            if (clone)
            {
                newNode = node.Clone(true);
            }
            newNode.Parent = this;
			newNode.TraverseTree(x => x._depth = null);
            if (append)
            {
                _children.AddLast(newNode);
            }
            else
            {
                _children.AddFirst(newNode);
            }
        }

        #region Append

        public TreeNode<T> Append(T value)
        {
            var node = new TreeNode<T>(value);
            this.AddChild(node, false);
            return node;
        }

        public void Append(TreeNode<T> node, bool clone = true)
        {
            this.AddChild(node, clone, true);
        }

        public ICollection<TreeNode<T>> AppendMany(IEnumerable<T> values)
        {
            return values.Select(this.Append).AsReadOnly();
        }

        public TreeNode<T>[] AppendMany(params T[] values)
        {
            return values.Select(this.Append).ToArray();
        }

        public void AppendMany(IEnumerable<TreeNode<T>> values)
        {
            values.Each(x => this.AddChild(x, true));
        }

        public void AppendChildrenOf(TreeNode<T> node)
        {
            node._children.Each(x => this.AddChild(x, true));
        }

        #endregion

        #region Prepend

        public TreeNode<T> Prepend(T value)
        {
            var node = new TreeNode<T>(value);
            this.AddChild(node, true, false);
            return node;
        }

        #endregion

        #region Insert[...]

        public void InsertAfter(TreeNode<T> refNode)
        {
            this.Insert(refNode, true);
        }

        public void InsertBefore(TreeNode<T> refNode)
        {
            this.Insert(refNode, false);
        }

        private void Insert(TreeNode<T> refNode, bool after)
        {
            Guard.ArgumentNotNull(() => refNode);

            if (refNode.Parent == null)
            {
                throw Error.Argument("refNode", "The reference node cannot be a root node and must be attached to the tree.");
            }

            var refLinkedList = refNode.Parent._children;
            var refNodeInternal = refLinkedList.Find(refNode);

            if (this.Parent != null)
            {
                var thisLinkedList = this.Parent._children;
                thisLinkedList.Remove(this);
            }

            if (after)
            {
                refLinkedList.AddAfter(refNodeInternal, this);
            }
            else
            {
                refLinkedList.AddBefore(refNodeInternal, this);
            }

            this.Parent = refNode.Parent;
			this.TraverseTree(x => x._depth = null);
        }

        #endregion

        #region Select[...]

        public TreeNode<T> SelectNode(Expression<Func<TreeNode<T>, bool>> predicate)
        {
            Guard.ArgumentNotNull(() => predicate);

            return this.FlattenNodes(predicate, false).FirstOrDefault();
        }

        /// <summary>
        /// Selects all nodes (recursively) with match the given <c>predicate</c>,
        /// but excluding self
        /// </summary>
        /// <param name="predicate">The predicate to match against</param>
        /// <returns>A readonly collection of node matches</returns>
        public ICollection<TreeNode<T>> SelectNodes(Expression<Func<TreeNode<T>, bool>> predicate)
        {
            Guard.ArgumentNotNull(() => predicate);
            
            var result = new List<TreeNode<T>>();

            var flattened = this.FlattenNodes(predicate, false);
            result.AddRange(flattened);

            return result.AsReadOnly();
        }

        #endregion

        public bool RemoveNode(TreeNode<T> node)
        {
			node.TraverseTree(x => x._depth = null);
			return _children.Remove(node);
        }


        public void Clear()
        {
            _children.Clear();
        }

        public void Traverse(Action<T> action)
        {
            action(Value);
            foreach (var child in _children)
                child.Traverse(action);
        }

        public void TraverseTree(Action<TreeNode<T>> action)
        {
            action(this);
            foreach (var child in _children)
                child.TraverseTree(action);
        }

        public IEnumerable<T> Flatten(bool includeSelf = true)
        {
            return this.Flatten(null, includeSelf);
        }

        public IEnumerable<T> Flatten(Expression<Func<T, bool>> expression, bool includeSelf = true)
        {
            IEnumerable<T> list;
            if (includeSelf)
            {
                list = new[] { Value };
            }
            else
            {
                list = Enumerable.Empty<T>();
            }

            var result = list.Union(_children.SelectMany(x => x.Flatten()));
            if (expression != null)
            {
                result = result.Where(expression.Compile());
            }

            return result;
        }

        internal IEnumerable<TreeNode<T>> FlattenNodes(bool includeSelf = true)
        {
            return this.FlattenNodes(null, includeSelf);
        }

        internal IEnumerable<TreeNode<T>> FlattenNodes(Expression<Func<TreeNode<T>, bool>> expression, bool includeSelf = true)
        {
            IEnumerable<TreeNode<T>> list;
            if (includeSelf)
            {
                list = new[] { this };
            }
            else
            {
                list = Enumerable.Empty<TreeNode<T>>();
            }

            var result = list.Union(_children.SelectMany(x => x.FlattenNodes()));
            if (expression != null)
            {
                result = result.Where(expression.Compile());
            }

            return result;
        }

        public TreeNode<T> Find(T value)
        {
            //Guard.ArgumentNotNull(value, "value"); 

            if (this.Value.Equals(value))
            {
                return this;
            }

            TreeNode<T> item = null;

            foreach (var child in _children) {
                item = child.Find(value);
                if (item != null)
                    break;
            }

            return item;
        }

        public TreeNode<T> Clone()
        {
            return Clone(true);
        }

        public TreeNode<T> Clone(bool deep)
        {
            T value = this.Value;

            if (value is ICloneable)
            {
                value = (T)((ICloneable)value).Clone();
            }

            var clone = new TreeNode<T>(value);
            if (deep)
            {
                clone.AppendChildrenOf(this);
            }
            return clone;
        }

        object ICloneable.Clone()
        {
            return Clone(true);
        }

        #endregion
    }
}
