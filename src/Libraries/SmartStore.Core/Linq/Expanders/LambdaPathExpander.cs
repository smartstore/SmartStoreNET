using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using SmartStore.Utilities;

namespace SmartStore.Linq
{
    public class LambdaPathExpander : IPathExpander
    {
        private readonly IList<LambdaExpression> _expands;

        public LambdaPathExpander()
        {
            _expands = new List<LambdaExpression>();
        }

        public IList<LambdaExpression> Expands => _expands;

        public virtual void Expand<T>(Expression<Func<T, object>> path)
        {
            this.Expand<T, T>(path);
        }

        public virtual void Expand<T, TTarget>(Expression<Func<TTarget, object>> path)
        {
            Guard.NotNull(path, "path");
            _expands.Add(path);
        }

        public void Expand<T>(string path)
        {
            this.Expand(typeof(T), path);
        }

        public virtual void Expand(Type type, string path)
        {
            Guard.NotNull(type, "type");
            Guard.NotEmpty(path, "path");

            Type t = type;

            // Wenn der Path durch einen Listen-Typ "unterbrochen" wird,
            // muss erst der ItemType der Liste gezogen und ab dem nächsten
            // Pfad ein neues Expand durchgeführt werden

            var tokenizer = new StringTokenizer(path, ".", false);
            foreach (string member in tokenizer)
            {
                // Property ermitteln
                //MemberInfo prop = t.GetFieldOrProperty(member, true);
                var prop = t.GetProperty(member, BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.Instance);

                if (prop == null)
                    throw new ArgumentException("The property or member '{0}' does not exist in type '{1}'.".FormatInvariant(member, t.FullName));

                Type memberType = prop.PropertyType;

                DoExpand(t, member);

                if (memberType.IsSequenceType())
                    t = TypeHelper.GetElementType(memberType); //TypeHelper.GetListItemType(memberType);
                else
                    t = memberType;
            }

            #region Old
            //var parser = new PathExpressionParser(path);
            //while (parser.Read()) // Token auslesen (also Member)
            //{
            //    string member = parser.CurrentToken.Text;

            //    // Property oder Field des Members ermitteln
            //    MemberInfo prop = t.GetFieldOrProperty(member, true);
            //    //MemberInfo prop = t.GetProperty(member);

            //    if (prop == null)
            //        throw new ArgumentException("The property or member '{0}' does not exist in type '{1}'.".FormatInvariant(member, t.FullName));

            //    Type memberType = (prop.MemberType == MemberTypes.Property) 
            //                      ? ((PropertyInfo)prop).PropertyType
            //                      : ((FieldInfo)prop).FieldType;

            //    DoExpand(t, member);

            //    if (memberType.IsSequenceType())
            //        t = TypeHelper.GetElementType(memberType); //TypeHelper.GetListItemType(memberType);
            //    else
            //        t = memberType;

            //}
            #endregion
        }

        private void DoExpand(Type type, string path)
        {
            var entityParam = Expression.Parameter(type, "x"); // {x}
            path = String.Concat("x.", path.Trim('.'));

            var expression = System.Linq.Dynamic.Core.DynamicExpressionParser.ParseLambda(
                new ParameterExpression[] { entityParam },
                typeof(object),
                path.Trim('.'));

            _expands.Add(expression);
        }

        #region Old
        //public virtual void Expand(Type type, string path)
        //{
        //    Guard.NotNull(type, "type");
        //    Guard.NotEmpty(path, "path");

        //    Type t = type;
        //    IDictionary<Type, string> paths = new Dictionary<Type, string>();

        //    bool splitted = false;
        //    IList<string> members = new List<string>(path.ToArray('.'));
        //    for (int i = 0; i < members.Count; i++)
        //    {
        //        string member = members[i];

        //        PropertyInfo prop = t.GetProperty(member);

        //        if (prop == null)
        //        {
        //            throw new ArgumentException("The property or member '{0}' does not exist in type '{1}'.".FormatInvariant(member, t.FullName));
        //        }

        //        if (!((members.Count - 1) == i) && prop.PropertyType.IsListType())
        //        {
        //            splitted = true;
        //            Type tItem = TypeHelper.GetListItemType(prop.PropertyType); // <== Key
        //        }

        //        t = prop.PropertyType;
        //    }

        //    if (splitted)
        //    {
        //        foreach (var item in paths)
        //        {
        //            DoExpand(item.Key, item.Value);
        //        }
        //        return;
        //    }

        //    DoExpand(type, path);


        //    //for (int i = 0; i < members.Count; i++)
        //    //{
        //    //    string member = members[i];
        //    //    newPath = String.Format("{0}{1}", 
        //    //        (newPath == null ? String.Empty : newPath + "."), 
        //    //        member);

        //    //    PropertyInfo prop = t.GetProperty(member);

        //    //    if (prop == null)
        //    //    {
        //    //        throw new ArgumentException("The property or member '{0}' does not exist in type '{1}'.".FormatInvariant(member, t.FullName));
        //    //    }

        //    //    if (prop.PropertyType.IsListType())
        //    //    {
        //    //        this.Expand(type, newPath); // <== Rekursion

        //    //        // einen zurück
        //    //        Type tItem = TypeHelper.GetListItemType(prop.PropertyType);
        //    //        string newMember = tItem.Name;
        //    //    }

        //    //    t = prop.PropertyType;
        //    //}

        //}

        //private void DoExpand(Type type, IList<string> path)
        //{
        //    if (!path.IsNullOrEmpty())
        //    {
        //        var expression = DynamicExpression.ParseLambda(type, typeof(object), String.Join(".", path.ToArray()));
        //        _expands.Add(expression);
        //    }
        //}
        #endregion
    }

    #region Obsolete
    //public class PathExpressionParser
    //{
    //    public struct Token
    //    {
    //        public string Text;
    //        public int Pos;
    //    }

    //    private char _ch;
    //    private Token _token;

    //    public PathExpressionParser(string path)
    //    {
    //        Guard.NotEmpty(path, "path");

    //        Path = path;
    //        SetPos(0);
    //    }

    //    public string Path { get; private set; }

    //    public int Pos { get; private set; }

    //    public Token CurrentToken
    //    {
    //        get
    //        {
    //            return _token;
    //        }
    //    }

    //    public int Length
    //    {
    //        get
    //        {
    //            return Path.Length;
    //        }
    //    }

    //    public bool IsLastToken
    //    {
    //        get
    //        {
    //            int pos = Pos;
    //            while (pos < Path.Length)
    //            {
    //                if (Path[pos] == '.')
    //                {
    //                    return false;
    //                }
    //                pos++;
    //            }
    //            return true;
    //        }
    //    }

    //    public bool Read()
    //    {
    //        if (_ch == '\0')
    //            return false;

    //        int tokenPos = Pos;

    //        do
    //        {
    //            NextChar();
    //        } while (_ch != '.' && _ch != '\0');

    //        _token.Text = Path.Substring(tokenPos, Pos - tokenPos).Trim('.');
    //        _token.Pos = tokenPos;

    //        return true;
    //        //return (_ch != '\0');
    //    }

    //    private void SetPos(int pos)
    //    {
    //        Pos = pos;
    //        _ch = Pos < Length ? Path[Pos] : '\0';
    //    }

    //    void NextChar()
    //    {
    //        if (Pos < Length) Pos++;
    //        _ch = Pos < Length ? Path[Pos] : '\0';
    //    }

    //}
    #endregion
}
