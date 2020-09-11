using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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

        private void PropagateNodeId(object value /* Id */, IDictionary<object, TreeNodeBase<T>> idNodeMap)
        {
            if (value != null)
            {
                // We support multi-keys for single nodes
                var ids = value as IEnumerable<object> ?? new object[] { value };
                foreach (var id in ids)
                {
                    idNodeMap[id] = this;
                }
            }
        }

        private void RemoveNodeId(object value /* Id */, IDictionary<object, TreeNodeBase<T>> idNodeMap)
        {
            if (value != null)
            {
                // We support multi-keys for single nodes
                var ids = value as IEnumerable<object> ?? new object[] { value };
                foreach (var id in ids)
                {
                    if (idNodeMap.ContainsKey(id))
                    {
                        idNodeMap.Remove(id);
                    }
                }
            }
        }

        /// <summary>
        /// Responsible for propagating node ids when detaching/attaching nodes
        /// </summary>
        private void FixIdNodeMap(T prevParent, T newParent)
        {
            ICollection<TreeNodeBase<T>> keyedNodes = null;

            if (prevParent != null)
            {
                // A node is moved. We need to detach first.
                keyedNodes = new List<TreeNodeBase<T>>();

                // Detach ids from prev map
                var prevMap = prevParent.GetIdNodeMap();

                Traverse(x =>
                {
                    // Collect all child node's ids
                    if (x._id != null)
                    {
                        keyedNodes.Add(x);
                        // Remove from map
                        RemoveNodeId(x._id, prevMap);
                    }
                }, true);
            }

            if (keyedNodes == null && _idNodeMap != null)
            {
                // An orphan/root node is attached
                keyedNodes = _idNodeMap.Values;
            }

            if (newParent != null)
            {
                // Get new *root map
                var map = newParent.GetIdNodeMap();

                // Merge *this map with *root map
                if (keyedNodes != null)
                {
                    foreach (var node in keyedNodes)
                    {
                        node.PropagateNodeId(node._id, map);
                    }

                    // Get rid of *this map after memorizing keyed nodes
                    if (_idNodeMap != null)
                    {
                        _idNodeMap.Clear();
                        _idNodeMap = null;
                    }
                }

                if (prevParent == null && _id != null)
                {
                    // When *this was a root, but is keyed, then *this id
                    // was most likely missing in the prev id-node-map.
                    PropagateNodeId(_id, map);
                }
            }
        }

        public object Id
        {
            get => _id;
            set
            {
                var prevId = _id;
                _id = value;

                if (_parent != null)
                {
                    var map = GetIdNodeMap();

                    // Remove old id(s) from map
                    RemoveNodeId(prevId, map);

                    // Set id
                    PropagateNodeId(value, map);
                }
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
            get => _metadata ?? (_metadata = new Dictionary<string, object>());
            set => _metadata = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

            if (metadata != null)
            {
                return (TMetadata)metadata;
            }

            return default(TMetadata);
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

            var prevParent = _parent;

            if (_parent != null)
            {
                // Detach from parent
                _parent.Remove((T)this);
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

            FixIdNodeMap(prevParent, newParent);
        }

        [JsonIgnore]
        public T Parent => _parent;

        public T this[int i] => _children?[i];

        public IReadOnlyList<T> Children => ChildrenInternal;

        [JsonIgnore]
        public IEnumerable<T> LeafNodes => _children != null
                    ? _children.Where(x => x.IsLeaf)
                    : Enumerable.Empty<T>();

        [JsonIgnore]
        public IEnumerable<T> NonLeafNodes => _children != null
                    ? _children.Where(x => !x.IsLeaf)
                    : Enumerable.Empty<T>();


        [JsonIgnore]
        public T FirstChild => _children?.FirstOrDefault();

        [JsonIgnore]
        public T LastChild => _children?.LastOrDefault();

        [JsonIgnore]
        public bool IsLeaf => _children == null || _children.Count == 0;

        [JsonIgnore]
        public bool HasChildren => _children == null || _children.Count > 0;

        [JsonIgnore]
        public bool IsRoot => _parent == null;

        [JsonIgnore]
        public int Index => _index;

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
        public T First => _parent?._children?.FirstOrDefault();

        [JsonIgnore]
        public T Last => _parent?._children?.LastOrDefault();

        [JsonIgnore]
        public T Next => _parent?._children?.ElementAtOrDefault(_index + 1);

        [JsonIgnore]
        public T Previous => _parent?._children?.ElementAtOrDefault(_index - 1);

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
        public T Closest(Func<T, bool> predicate)
        {
            Guard.NotNull(predicate, nameof(predicate));

            var test = predicate;

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Append(T value)
        {
            this.AddChild(value, false, true);
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T SelectNode(Func<T, bool> predicate, bool includeSelf = false)
        {
            Guard.NotNull(predicate, nameof(predicate));

            return this.FlattenNodes(predicate, includeSelf).FirstOrDefault();
        }

        /// <summary>
        /// Selects all nodes (recursively) witch match the given <c>predicate</c>
        /// </summary>
        /// <param name="predicate">The predicate to match against</param>
        /// <returns>A readonly collection of node matches</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<T> SelectNodes(Func<T, bool> predicate, bool includeSelf = false)
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
                    node.FixIdNodeMap(node._parent, null);

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

            FixIdNodeMap(_parent, null);
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<T> FlattenNodes(bool includeSelf = true)
        {
            return this.FlattenNodes(null, includeSelf);
        }

        protected IEnumerable<T> FlattenNodes(Func<T, bool> predicate, bool includeSelf = true)
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
                result = result.Where(predicate);
            }

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
