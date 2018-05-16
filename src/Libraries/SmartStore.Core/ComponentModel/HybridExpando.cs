#region License
/*
 **************************************************************
 *  Author: Rick Strahl 
 *          © West Wind Technologies, 2012
 *          http://www.west-wind.com/
 * 
 * Created: Feb 2, 2012
 *
 * Permission is hereby granted, free of charge, to any person
 * obtaining a copy of this software and associated documentation
 * files (the "Software"), to deal in the Software without
 * restriction, including without limitation the rights to use,
 * copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following
 * conditions:
 * 
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
 * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
 * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
 * OTHER DEALINGS IN THE SOFTWARE.
 **************************************************************  
*/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Dynamic;
using System.Reflection;
using System.Collections;
using SmartStore.Utilities;

namespace SmartStore.ComponentModel
{
    public enum MemberOptMethod
	{
		Allow,
		Disallow
	}
	
	/// <summary>
    /// Class that provides extensible properties and methods to an
    /// existing object when cast to dynamic. This
    /// dynamic object stores 'extra' properties in a dictionary or
    /// checks the actual properties of the instance passed via 
    /// constructor.
    /// 
    /// This class can be subclassed to extend an existing type or 
    /// you can pass in an instance to extend. Properties (both
    /// dynamic and strongly typed) can be accessed through an 
    /// indexer.
    /// 
    /// This type allows you three ways to access its properties:
    /// 
    /// Directly: any explicitly declared properties are accessible
    /// Dynamic: dynamic cast allows access to dictionary and native properties/methods
    /// Dictionary: Any of the extended properties are accessible via IDictionary interface
    /// </summary>
    [Serializable]
    public class HybridExpando : DynamicObject, IDictionary<string, object>
    {
        /// <summary>
        /// Instance of object passed in
        /// </summary>
        private object _instance;

        /// <summary>
        /// Cached type of the instance
        /// </summary>
        private Type _instanceType;

		/// <summary>
		/// Adjusted property list for the wrapped instance type after white/black-list members has been applied.
		/// </summary>
		private IDictionary<string, FastProperty> _instanceProps;
		private readonly static IDictionary<string, FastProperty> EmptyProps = new Dictionary<string, FastProperty>();

		/// <summary>
		/// String Dictionary that contains the extra dynamic values
		/// stored on this object/instance
		/// </summary>        
		/// <remarks>Using PropertyBag to support XML Serialization of the dictionary</remarks>
		public PropertyBag Properties = new PropertyBag();

		private readonly HashSet<string> _optMembers;
		private readonly MemberOptMethod _optMethod;

		private readonly bool _returnNullWhenFalsy;

		/// <summary>
		/// This constructor just works off the internal dictionary and any 
		/// public properties of this object.
		/// 
		/// Note you can subclass HybridExpando.
		/// </summary>
		public HybridExpando(bool returnNullWhenFalsy = false)
        {
			_returnNullWhenFalsy = returnNullWhenFalsy;
        }

        /// <summary>
        /// Allows passing in an existing instance variable to 'extend'.        
        /// </summary>
        /// <remarks>
        /// You can pass in null here if you don't want to 
        /// check native properties and only check the Dictionary!
        /// </remarks>
        /// <param name="instance"></param>
        public HybridExpando(object instance, bool returnNullWhenFalsy = false)
        {
			Guard.NotNull(instance, nameof(instance));

			_returnNullWhenFalsy = returnNullWhenFalsy;
			Initialize(instance);
        }

		/// <summary>
		/// Allows passing in an existing instance variable to 'extend'
		/// along with a list of member names to allow or disallow.
		/// </summary>
		/// <param name="instance"></param>
		public HybridExpando(object instance, IEnumerable<string> optMembers, MemberOptMethod optMethod, bool returnNullWhenFalsy = false)
		{
			Guard.NotNull(instance, nameof(instance));

			_returnNullWhenFalsy = returnNullWhenFalsy;
			Initialize(instance);

			_optMethod = optMethod;

			if (optMembers is HashSet<string> h)
			{
				_optMembers = h;
			}
			else
			{
				_optMembers = new HashSet<string>(optMembers);
			}
		}

		protected void Initialize(object instance)
        {
            _instance = instance;
			_instanceType = instance?.GetType();
        }

		protected object WrappedObject
		{
			get { return _instance; }
		}

		public override IEnumerable<string> GetDynamicMemberNames()
        {
			foreach (var kvp in this.Properties.Keys)
			{
				yield return kvp;
			}

			foreach (var kvp in GetInstanceProperties())
			{
				if (!this.Properties.ContainsKey(kvp.Key))
				{
					yield return kvp.Key;
				}
			}
		}


		/// <summary>
		/// Try to retrieve a member by name first from instance properties
		/// followed by the collection entries.
		/// </summary>
		/// <param name="binder"></param>
		/// <param name="result"></param>
		/// <returns></returns>
		public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
			return TryGetMemberCore(binder.Name, out result);
        }

		protected virtual bool TryGetMemberCore(string name, out object result)
		{
			result = null;
			bool exists = false;

			// first check the Properties collection for member
			if (Properties.ContainsKey(name))
			{
				result = Properties[name];
				exists = true;
			}

			// Next check for public properties via Reflection
			if (!exists && _instance != null)
			{
				try
				{
					exists = GetProperty(_instance, name, out result);
				}
				catch { }
			}

			// Falsy check
			if (_returnNullWhenFalsy && result != null && !CommonHelper.IsTruthy(result))
			{
				result = null;
			}

			// failed to retrieve a property
			return exists;
		}


		/// <summary>
		/// Property setter implementation tries to retrieve value from instance 
		/// first then into this object
		/// </summary>
		/// <param name="binder"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public override bool TrySetMember(SetMemberBinder binder, object value)
        {
			return TrySetMemberCore(binder.Name, value);
        }

		protected virtual bool TrySetMemberCore(string name, object value)
		{
			// first check to see if there's a dictionary entry to set
			if (Properties.ContainsKey(name))
			{
				Properties[name] = value;
				return true;
			}

			// Check to see if there's a native property to set
			if (_instance != null)
			{
				try
				{
					bool result = SetProperty(_instance, name, value);
					if (result)
						return true;
				}
				catch { }
			}

			// no match - set or add to dictionary
			Properties[name] = value;
			return true;
		}

		/// <summary>
		/// Dynamic invocation method. Currently allows only for Reflection based
		/// operation (no ability to add methods dynamically).
		/// </summary>
		/// <param name="binder"></param>
		/// <param name="args"></param>
		/// <param name="result"></param>
		public void Override(string name, object value = null)
		{
			Guard.NotEmpty(name, nameof(name));
			Properties[name] = value;
		}

		/// <returns></returns>
		/// Dynamic invocation method. Currently allows only for Reflection based
		/// operation (no ability to add methods dynamically).
		/// </summary>
		/// <param name="binder"></param>
		/// <param name="args"></param>
		/// <param name="result"></param>
		/// <returns></returns>
		public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            if (_instance != null)
            {
                try
                {
                    // check instance passed in for methods to invoke
                    if (InvokeMethod(_instance, binder.Name, args, out result))
                        return true;
                }
                catch { }
            }

            result = null;
            return false;
        }


        /// <summary>
        /// Reflection Helper method to retrieve a property
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="name"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        protected bool GetProperty(object instance, string name, out object result)
        {
			if (GetInstanceProperties().TryGetValue(name, out var fastProp))
			{
				result = fastProp.GetValue(instance ?? this);
				return true;
			}

			result = null;
            return false;
		}

        /// <summary>
        /// Reflection helper method to set a property value
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        protected bool SetProperty(object instance, string name, object value)
        {
			if (GetInstanceProperties().TryGetValue(name, out var fastProp))
			{
				fastProp.SetValue(instance ?? this, value);
				return true;
			}

			return false;
        }

        /// <summary>
        /// Reflection helper method to invoke a method
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="name"></param>
        /// <param name="args"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        protected bool InvokeMethod(object instance, string name, object[] args, out object result)
        {
            // Look at the instanceType
            var mi = _instanceType?.GetMethod(name, BindingFlags.Instance | BindingFlags.Public);
            if (mi != null)
            {
                result = mi.Invoke(instance ?? this, args);
				return true;
            }

            result = null;
            return false;
        }


		/// <summary>
		/// Convenience method that provides a string Indexer 
		/// to the Properties collection AND the strongly typed
		/// properties of the object by name.
		/// 
		/// // dynamic
		/// exp["Address"] = "112 nowhere lane"; 
		/// // strong
		/// var name = exp["StronglyTypedProperty"] as string; 
		/// </summary>
		/// <remarks>
		/// The getter checks the Properties dictionary first
		/// then looks in PropertyInfo for properties.
		/// The setter checks the instance properties before
		/// checking the Properties dictionary.
		/// </remarks>
		/// <param name="key"></param>
		/// 
		/// <returns></returns>
		public object this[string key]
		{
			get
			{
				if (!TryGetMemberCore(key, out var result))
				{
					throw new KeyNotFoundException();
				}

				return result;
			}
			set
			{
				TrySetMemberCore(key, value);
			}
		}


		/// <summary>
		/// Returns all properties 
		/// </summary>
		/// <param name="includeInstanceProperties"></param>
		/// <returns></returns>
		public IEnumerable<KeyValuePair<string, object>> GetProperties(bool includeInstanceProperties = false)
        {
			foreach (var kvp in this.Properties)
			{
				yield return kvp;
			}
				
			if (includeInstanceProperties)
            {
                foreach (var prop in GetInstanceProperties().Values)
				{
					if (!this.Properties.ContainsKey(prop.Name))
					{
						yield return new KeyValuePair<string, object>(prop.Name, prop.GetValue(_instance));
					}
				}
            }
        }

		private IDictionary<string, FastProperty> GetInstanceProperties()
		{
			if (_instance == null)
			{
				return EmptyProps;
			}

			if (_instanceProps == null)
			{
				var props = FastProperty.GetProperties(_instance) as IDictionary<string, FastProperty>;

				if (_optMembers != null)
				{
					props = props
						.Where(x => _optMethod == MemberOptMethod.Allow ? _optMembers.Contains(x.Key) : !_optMembers.Contains(x.Key))
						.ToDictionary(x => x.Key, x => x.Value);
				}

				_instanceProps = props;
			}

			return _instanceProps;
		}

        /// <summary>
        /// Checks whether a property exists in the Property collection
        /// or as a property on the instance
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Contains(KeyValuePair<string, object> item, bool includeInstanceProperties = false)
        {
            return this.Contains(item.Key, includeInstanceProperties);
        }

        /// <summary>
        /// Checks whether a property exists in the Property collection
        /// or as a property on the instance
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="includeInstanceProperties"></param>
        /// <returns></returns>
        public bool Contains(string propertyName, bool includeInstanceProperties = false)
        {
            if (Properties.ContainsKey(propertyName))
			{
				return true;
			}

            if (includeInstanceProperties)
            {
				return GetInstanceProperties().ContainsKey(propertyName);
			}

            return false;
        }

		#region IDictionary<string, object>

		ICollection<string> IDictionary<string, object>.Keys
		{
			get
			{
				return GetProperties(true).Select(x => x.Key).AsReadOnly();
			}
		}

		ICollection<object> IDictionary<string, object>.Values
		{
			get
			{
				return GetProperties(true).Select(x => x.Value).AsReadOnly();
			}
		}

		int ICollection<KeyValuePair<string, object>>.Count
		{
			get
			{
				return GetDynamicMemberNames().Count();
				}
			}

		bool ICollection<KeyValuePair<string, object>>.IsReadOnly
		{
			get
			{
				return false;
			}
		}

		object IDictionary<string, object>.this[string key]
		{
			get
			{
				return this[key];
			}

			set
			{
				this[key] = value;
			}
		}

		bool IDictionary<string, object>.ContainsKey(string key)
		{
			return Contains(key, true);
		}

		void IDictionary<string, object>.Add(string key, object value)
		{
			throw new NotImplementedException();
		}

		bool IDictionary<string, object>.Remove(string key)
		{
			throw new NotImplementedException();
		}

		public bool TryGetValue(string key, out object value)
		{
			value = null;

			if (this.Contains(key, true))
			{
				value = this[key];
				return true;
			}

			return false;
		}

		void ICollection<KeyValuePair<string, object>>.Add(KeyValuePair<string, object> item)
		{
			throw new NotImplementedException();
		}

		void ICollection<KeyValuePair<string, object>>.Clear()
		{
			throw new NotImplementedException();
		}

		bool ICollection<KeyValuePair<string, object>>.Contains(KeyValuePair<string, object> item)
		{
			return Contains(item.Key, true);
		}

		void ICollection<KeyValuePair<string, object>>.CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
		{
			throw new NotImplementedException();
		}

		bool ICollection<KeyValuePair<string, object>>.Remove(KeyValuePair<string, object> item)
		{
			throw new NotImplementedException();
		}

		IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator()
		{
			return GetProperties(true).GetEnumerator();
        }

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetProperties(true).GetEnumerator();
		}

		#endregion
	}
}