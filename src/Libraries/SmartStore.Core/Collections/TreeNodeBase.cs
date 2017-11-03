using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Newtonsoft.Json;
using SmartStore.Core.Infrastructure;

namespace SmartStore.Collections
{
	public abstract class TreeNodeBase<T> where T : TreeNodeBase<T>
	{
		private T _parent;
		private List<T> _children = new List<T>();
		private int? _depth = null;
		private int _index = -1;

		protected object _id;
		private IDictionary<object, TreeNodeBase<T>> _idNodeMap;

		protected IDictionary<string, object> _metadata;
		private readonly static ContextState<Dictionary<string, object>> _contextState = new ContextState<Dictionary<string, object>>("TreeNodeBase.ThreadMetadata");

		public TreeNodeBase()
		{
		}

		#region Id

		public object Id
		{
			get
			{
				return _id;
			}
			set
			{
				if (_parent != null)
				{
					var map = GetIdNodeMap();

					if (_id != value)
					{
						if (_id != null && map.ContainsKey(_id))
						{
							// Remove old id from map
							map.Remove(_id);
						}
					}

					if (value != null)
					{
						map[value] = this;
					}
				}

				_id = value;
			}
		}

		public T SelectNodeById(object id)
		{
			if (id == null || IsLeaf)
				return null;

			var map = GetIdNodeMap();
			var node = (T)map?.Get(id);

			if (node != null && !this.IsAncestorOfOrSelf(node))
			{
				// Found node is NOT a child of this node
				return null;
			}

			return node;
		}

		private IDictionary<object, TreeNodeBase<T>> GetIdNodeMap()
		{
			var map = this.Root._idNodeMap;
			if (map == null)
			{
				map = this.Root._idNodeMap = new Dictionary<object, TreeNodeBase<T>>();
			}

			return map;
		}

		#endregion

		#region Metadata

		public IDictionary<string, object> Metadata
		{
			get
			{
				return _metadata ?? (_metadata = new Dictionary<string, object>());
			}
			set
			{
				_metadata = value;
			}
		}

		public void SetMetadata(string key, object value)
		{
			Guard.NotEmpty(key, nameof(key));

			Metadata[key] = value;
		}

		public void SetThreadMetadata(string key, object value)
		{
			Guard.NotEmpty(key, nameof(key));

			var state = _contextState.GetState();
			if (state == null)
			{
				state = new Dictionary<string, object>();
				_contextState.SetState(state);
			}

			state[GetContextKey(this, key)] = value;
		}

		public TMetadata GetMetadata<TMetadata>(string key, bool recursive = true)
		{
			Guard.NotEmpty(key, nameof(key));

			object metadata;

			if (!recursive)
			{
				return TryGetMetadataForNode(this, key, out metadata) ? (TMetadata)metadata : default(TMetadata);
			}

			// recursively search for the metadata value in current node and ancestors
			var current = this;
			while (!TryGetMetadataForNode(current, key, out metadata))
			{
				current = current.Parent;
				if (current == null)
					break;
			}

			return (TMetadata)metadata;
		}

		private bool TryGetMetadataForNode(TreeNodeBase<T> node, string key, out object metadata)
		{
			metadata = null;

			var state = _contextState.GetState();

			if (state != null)
			{
				var contextKey = GetContextKey(node, key);
				if (state.ContainsKey(contextKey))
				{
					metadata = state[contextKey];
					return true;
				}
			}

			if (node._metadata != null && node._metadata.ContainsKey(key))
			{
				metadata = node._metadata[key];
				return true;
			}

			return false;
		}

		private static string GetContextKey(TreeNodeBase<T> node, string key)
		{
			return node.GetHashCode().ToString() + key;
		}

		#endregion

		private List<T> ChildrenInternal
		{
			get
			{
				if (_children == null)
				{
					_children = new List<T>();
				}
				return _children;
			}
		}

		private void AddChild(T node, bool clone, bool append = true)
		{
			var newNode = node;
			if (clone)
			{
				newNode = node.Clone(true);
			}
			newNode.AttachTo((T)this, append ? (int?)null : 0);
		}

		private void AttachTo(T newParent, int? index)
		{
			Guard.NotNull(newParent, nameof(newParent));

			if (_parent != null)
			{
				// Detach from parent
				_parent.Remove((T)this);
			}
			else
			{
				// Is a root node with a map: get rid of it.
				if (_idNodeMap != null)
				{
					_idNodeMap.Clear();
					_idNodeMap = null;
				}
			}

			if (index == null)
			{
				newParent.ChildrenInternal.Add((T)this);
				_index = newParent.ChildrenInternal.Count - 1;
			}
			else
			{
				newParent.ChildrenInternal.Insert(index.Value, (T)this);
				_index = index.Value;
				FixIndexes(newParent._children, _index + 1, 1);
			}

			_parent = newParent;

			// Set id in new id-node map
			if (_id != null)
			{
				var map = GetIdNodeMap();
				if (map != null)
				{
					map[_id] = (T)this;
				}
			}
		}

		[JsonIgnore]
		public T Parent
		{
			get
			{
				return _parent;
			}
		}

		public T this[int i]
		{
			get
			{
				return _children?[i];
			}
		}

		public IReadOnlyList<T> Children
		{
			get
			{
				return ChildrenInternal;
			}
		}

		[JsonIgnore]
		public IEnumerable<T> LeafNodes
		{
			get
			{
				return _children != null
					? _children.Where(x => x.IsLeaf)
					: Enumerable.Empty<T>();
			}
		}

		[JsonIgnore]
		public IEnumerable<T> NonLeafNodes
		{
			get
			{
				return _children != null
					? _children.Where(x => !x.IsLeaf)
					: Enumerable.Empty<T>();
			}
		}


		[JsonIgnore]
		public T FirstChild
		{
			get
			{
				return _children?.FirstOrDefault();
			}
		}

		[JsonIgnore]
		public T LastChild
		{
			get
			{
				return _children?.LastOrDefault();
			}
		}

		[JsonIgnore]
		public bool IsLeaf
		{
			get
			{
				return _children == null || _children.Count == 0;
			}
		}

		[JsonIgnore]
		public bool HasChildren
		{
			get
			{
				return _children == null || _children.Count > 0;
			}
		}

		[JsonIgnore]
		public bool IsRoot
		{
			get
			{
				return _parent == null;
			}
		}

		[JsonIgnore]
		public int Index
		{
			get
			{
				return _index;
			}
		}

		/// <summary>
		/// Root starts with 0
		/// </summary>
		[JsonIgnore]
		public int Depth
		{
			get
			{
				if (!_depth.HasValue)
				{
					var node = this;
					int depth = 0;
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

		[JsonIgnore]
		public T Root
		{
			get
			{
				var root = this;
				while (root._parent != null)
				{
					root = root._parent;
				}
				return (T)root;
			}
		}

		[JsonIgnore]
		public T First
		{
			get
			{
				return _parent?._children?.FirstOrDefault();
			}
		}

		[JsonIgnore]
		public T Last
		{
			get
			{
				return _parent?._children?.LastOrDefault();
			}
		}

		[JsonIgnore]
		public T Next
		{
			get
			{
				return _parent?._children?.ElementAtOrDefault(_index + 1);
			}
		}

		[JsonIgnore]
		public T Previous
		{
			get
			{
				return _parent?._children?.ElementAtOrDefault(_index - 1);
			}
		}

		public bool IsDescendantOf(T node)
		{
			var parent = _parent;
			while (parent != null)
			{
				if (parent == node)
				{
					return true;
				}
				parent = parent._parent;
			}

			return false;
		}

		public bool IsDescendantOfOrSelf(T node)
		{
			if (node == (T)this)
				return true;

			return IsDescendantOf(node);
		}

		public bool IsAncestorOf(T node)
		{
			return node.IsDescendantOf((T)this);
		}

		public bool IsAncestorOfOrSelf(T node)
		{
			if (node == (T)this)
				return true;

			return node.IsDescendantOf((T)this);
		}

		[JsonIgnore]
		public IEnumerable<T> Trail
		{
			get
			{
				var trail = new List<T>();

				var node = (T)this;
				do
				{
					trail.Insert(0, node);
					node = node._parent;
				} while (node != null);
				
				return trail;
			}
		}

		/// <summary>
		/// Gets the first element that matches the predicate by testing the node itself 
		/// and traversing up through its ancestors in the tree.
		/// </summary>
		/// <param name="predicate">predicate</param>
		/// <returns>The closest node</returns>
		public T Closest(Expression<Func<T, bool>> predicate)
		{
			Guard.NotNull(predicate, nameof(predicate));

			var test = predicate.Compile();

			if (test((T)this))
			{
				return (T)this;
			}

			var parent = _parent;

			while (parent != null)
			{
				if (test(parent))
				{
					return parent;
				}
				parent = parent._parent;
			}

			return null;
		}

		public T Append(T value)
		{
			this.AddChild(value, false, true);
			return value;
		}

		public void AppendRange(IEnumerable<T> values)
		{
			values.Each(x => Append(x));
		}

		public void AppendChildrenOf(T node)
		{
			if (node?._children != null)
			{
				node._children.Each(x => this.AddChild(x, true, true));
			}
		}

		public T Prepend(T value)
		{
			this.AddChild(value, false, false);
			return value;
		}

		public void InsertAfter(int index)
		{
			var refNode = _children?.ElementAtOrDefault(index);
			if (refNode != null)
			{
				InsertAfter(refNode);
			}

			throw new ArgumentOutOfRangeException(nameof(index));
		}

		public void InsertAfter(T refNode)
		{
			this.Insert(refNode, true);
		}

		public void InsertBefore(int index)
		{
			var refNode = _children?.ElementAtOrDefault(index);
			if (refNode != null)
			{
				InsertBefore(refNode);
			}

			throw new ArgumentOutOfRangeException(nameof(index));
		}

		public void InsertBefore(T refNode)
		{
			this.Insert(refNode, false);
		}

		private void Insert(T refNode, bool after)
		{
			Guard.NotNull(refNode, nameof(refNode));

			var refParent = refNode._parent;
			if (refParent == null)
			{
				throw Error.Argument("refNode", "The reference node cannot be a root node and must be attached to the tree.");
			}

			AttachTo(refParent, refNode._index + (after ? 1 : 0));
		}

		public T SelectNode(Expression<Func<T, bool>> predicate, bool includeSelf = false)
		{
			Guard.NotNull(predicate, nameof(predicate));

			return this.FlattenNodes(predicate, includeSelf).FirstOrDefault();
		}

		/// <summary>
		/// Selects all nodes (recursively) with match the given <c>predicate</c>
		/// </summary>
		/// <param name="predicate">The predicate to match against</param>
		/// <returns>A readonly collection of node matches</returns>
		public IEnumerable<T> SelectNodes(Expression<Func<T, bool>> predicate, bool includeSelf = false)
		{
			Guard.NotNull(predicate, nameof(predicate));

			return this.FlattenNodes(predicate, includeSelf);
		}

		private void FixIndexes(IList<T> list, int startIndex, int summand = 1)
		{
			if (startIndex < 0 || startIndex >= list.Count)
				return;

			for (var i = startIndex; i < list.Count; i++)
			{
				list[i]._index += summand;
			}
		}

		public void Remove(T node)
		{
			Guard.NotNull(node, nameof(node));

			if (!node.IsRoot)
			{
				var list = node._parent?._children;
				if (list.Remove(node))
				{
					// Remove id from id node map
					if (node._id != null)
					{
						var map = node.GetIdNodeMap();
						if (map != null && map.ContainsKey(node._id))
						{
							map.Remove(node._id);
						}
					}

					FixIndexes(list, node._index, -1);

					node._index = -1;
					node._parent = null;
					node.Traverse(x => x._depth = null, true);
				}
			}
		}

		public void Clear()
		{
			Traverse(x => x._depth = null, false);
			if (_children != null)
			{
				_children.Clear();
			}

			var map = GetIdNodeMap();
			if (map != null)
			{
				map.Clear();
			}
		}

		public void Traverse(Action<T> action, bool includeSelf = false)
		{
			Guard.NotNull(action, nameof(action));

			if (includeSelf)
				action((T)this);

			if (_children != null)
			{
				foreach (var child in _children)
					child.Traverse(action, true);
			}
		}

		public void TraverseParents(Action<T> action, bool includeSelf = false)
		{
			Guard.NotNull(action, nameof(action));

			if (includeSelf)
				action((T)this);

			var parent = _parent;

			while (parent != null)
			{
				action(parent);
				parent = parent._parent;
			}
		}

		public IEnumerable<T> FlattenNodes(bool includeSelf = true)
		{
			return this.FlattenNodes(null, includeSelf);
		}

		protected IEnumerable<T> FlattenNodes(Expression<Func<T, bool>> predicate, bool includeSelf = true)
		{
			IEnumerable<T> list;
			if (includeSelf)
			{
				list = new[] { (T)this };
			}
			else
			{
				list = Enumerable.Empty<T>();
			}

			if (_children == null)
				return list;

			var result = list.Union(_children.SelectMany(x => x.FlattenNodes()));
			if (predicate != null)
			{
				result = result.Where(predicate.Compile());
			}

			return result;
		}

		public T Clone()
		{
			return Clone(true);
		}

		public virtual T Clone(bool deep)
		{
			var clone = CreateInstance();
			if (deep)
			{
				clone.AppendChildrenOf((T)this);
			}
			return clone;
		}

		protected abstract T CreateInstance();
	}
}
