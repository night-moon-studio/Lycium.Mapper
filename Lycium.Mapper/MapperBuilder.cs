using Natasha.CSharp;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Lycium.Mapper
{
    public class InitMapperBuilder
    {
        static InitMapperBuilder()
        {
            AssemblyDomain.Init();
        }
    }
    public class MapperBuilder<TSrc, TDst> : InitMapperBuilder
    {
        public static MapperBuilder<TSrc, TDst> Create()
        {
            return new MapperBuilder<TSrc, TDst>();
        }

        public MapperBuilder()
        {
            _ignore_fields = new HashSet<string>();
        }


        private string _add_prefix;
        private string _remove_prefix;
        private HashSet<string> _ignore_fields;
        private bool _ignore_case;


        /// <summary>
        /// 映射时给目标字段添加前缀
        /// </summary>
        /// <param name="prefix"></param>
        /// <returns></returns>
        public MapperBuilder<TSrc, TDst> SetPrefix(string prefix)
        {

            _add_prefix = prefix;
            return this;

        }


        /// <summary>
        /// 映射时移除源字段的前缀
        /// </summary>
        /// <param name="prefix"></param>
        /// <returns></returns>
        public MapperBuilder<TSrc, TDst> RemovePrefix(string prefix)
        {

            _remove_prefix = prefix;
            return this;

        }


        /// <summary>
        /// 映射时忽略需要映射的属性名
        /// </summary>
        /// <param name="ignoreMemberNames"></param>
        /// <returns></returns>
        public MapperBuilder<TSrc, TDst> Ignore(params string[] ignoreMemberNames)
        {

            for (int i = 0; i < ignoreMemberNames.Length; i++)
            {
                _ignore_fields.Add(ignoreMemberNames[i]);
            }
            return this;

        }
        public MapperBuilder<TSrc, TDst> Ignore<TTemp>(Expression<Func<TSrc, TTemp>> expression)
        {

            _ignore_fields.Add(expression.Body.ToString().Split('.')[1]);
            return this;

        }


        /// <summary>
        /// 映射时忽略大小写
        /// </summary>
        /// <returns></returns>
        public MapperBuilder<TSrc, TDst> IgnoreCase(bool shut = true)
        {
            _ignore_case = shut;
            return this;
        }



        public Func<TSrc, TDst> Compile()
        {

            StringBuilder script = new StringBuilder();
            var srcMembers = new LinkedList<MemberInfo>(typeof(TSrc).GetMembers(BindingFlags.Public | BindingFlags.Instance));
            var tempSrcNode = srcMembers.First;
            while (tempSrcNode!=null)
            {

                if (tempSrcNode.Value.MemberType == MemberTypes.Method || tempSrcNode.Value.MemberType == MemberTypes.Constructor)
                {

                    tempSrcNode = tempSrcNode.Next;
                    srcMembers.Remove(tempSrcNode.Previous);

                }
                else
                {
                    if (tempSrcNode.Value.MemberType == MemberTypes.Property)
                    {

                        var info = (PropertyInfo)tempSrcNode.Value;
                        if (!info.CanRead)
                        {
                            tempSrcNode = tempSrcNode.Next;
                            srcMembers.Remove(tempSrcNode.Previous);
                        }
                        else
                        {
                            tempSrcNode = tempSrcNode.Next;
                        }

                    }
                    else
                    {
                        tempSrcNode = tempSrcNode.Next;
                    }
                    
                }
                
            }
            var dstMembers = new LinkedList<MemberInfo>(typeof(TDst).GetMembers(BindingFlags.Public | BindingFlags.Instance));
            var tempDstNode = dstMembers.First;
            while (tempDstNode != null)
            {

                if (tempDstNode.Value.MemberType == MemberTypes.Method || tempDstNode.Value.MemberType == MemberTypes.Constructor)
                {

                    tempDstNode = tempDstNode.Next;
                    dstMembers.Remove(tempDstNode.Previous);

                }
                else
                {

                    if (tempDstNode.Value.MemberType == MemberTypes.Property)
                    {

                        var info = (PropertyInfo)tempDstNode.Value;
                        if (!info.CanWrite)
                        {
                            tempDstNode = tempDstNode.Next;
                            dstMembers.Remove(tempDstNode.Previous);
                        }
                        else
                        {
                            tempDstNode = tempDstNode.Next;
                        }

                    }
                    else if (tempDstNode.Value.MemberType == MemberTypes.Field)
                    {

                        var info = (FieldInfo)tempDstNode.Value;
                        if (info.IsLiteral || info.IsInitOnly)
                        {
                            tempDstNode = tempDstNode.Next;
                            dstMembers.Remove(tempDstNode.Previous);
                        }
                        else
                        {
                            tempDstNode = tempDstNode.Next;
                        }
                    }
                    else
                    {
                        tempDstNode = tempDstNode.Next;
                    }

                }

            }
            script.Append($"var dst = new {typeof(TDst).GetDevelopName()}();");


            foreach (var item in dstMembers)
            {

                if (_ignore_fields.Contains(item.Name))
                {
                    continue;
                }
                var node = srcMembers.First;
                while (node != null)
                {

                    var dstName = item.Name;
                    var srcName = node.Value.Name;
                    if (_ignore_case)
                    {

                        if (
                            dstName.ToLower() == srcName.ToLower()
                            ||
                             (_remove_prefix != default && dstName.ToLower() == srcName.ToLower().Replace(_remove_prefix, ""))
                            ||
                            dstName.ToLower() == _add_prefix + srcName.ToLower()
                            )
                        {
                            var dstType = GetRealType(item);
                            var srcType = GetRealType(node.Value);
                            var srcCaller = IsStatic(node.Value) ? typeof(TSrc).GetDevelopName() : "arg";
                            var dstCaller = IsStatic(item) ? typeof(TDst).GetDevelopName() : "dst";

                            if (dstType == srcType)
                            {
                                script.Append($"{dstCaller}.{dstName} = {srcCaller}.{srcName};");
                                srcMembers.Remove(node);
                                break;
                            }
                            else if (
                                (dstType.IsPrimitive || dstType == typeof(string) || dstType == typeof(DateTime))
                                &&
                                (srcType.IsPrimitive || srcType == typeof(string) || srcType == typeof(DateTime))
                                )
                            {
                                script.Append($"{dstCaller}.{dstName} = Convert.To{dstType.Name}({srcCaller}.{srcName});");
                                srcMembers.Remove(node);
                                break;
                            }

                        }

                    }
                    else
                    {
                        if (
                            dstName == srcName
                            ||
                             (_remove_prefix != default && dstName == srcName.Replace(_remove_prefix, ""))
                            ||
                            dstName == _add_prefix + srcName
                            )
                        {
                            var dstType = GetRealType(item);
                            var srcType = GetRealType(node.Value);
                            var srcCaller = IsStatic(node.Value) ? typeof(TSrc).GetDevelopName() : "arg";
                            var dstCaller = IsStatic(item) ? typeof(TDst).GetDevelopName() : "dst";

                            if (dstType == srcType)
                            {
                                script.Append($"{dstCaller}.{dstName} = {srcCaller}.{srcName};");
                                srcMembers.Remove(node);
                                break;
                            }
                            else if (
                                (dstType.IsPrimitive || dstType == typeof(string) || dstType == typeof(DateTime))
                                &&
                                (srcType.IsPrimitive || srcType == typeof(string) || srcType == typeof(DateTime))
                                )
                            {
                                script.Append($"{dstCaller}.{dstName} = Convert.To{dstType.Name}({srcCaller}.{srcName});");
                                srcMembers.Remove(node);
                                break;
                            }

                        }

                    }
                    node = node.Next;
                }

            }
            script.Append("return dst;");
            //if (!(typeof(TDst).IsAbstract && typeof(TDst).IsSealed))
            //{
                
            //}
            return LyciumMapper<TSrc,TDst>.Mapper = NDelegate.RandomDomain().Func<TSrc, TDst>(script.ToString());

        }

        public static Type GetRealType(MemberInfo info)
        {
            switch (info.MemberType)
            {
              
                case MemberTypes.Field:
                    return ((FieldInfo)info).FieldType;
                case MemberTypes.Property:
                    return ((PropertyInfo)info).PropertyType;
            }
            return null;
        }

        public static bool IsStatic(MemberInfo info)
        {

            switch (info.MemberType)
            {

                case MemberTypes.Field:
                    return ((FieldInfo)info).IsStatic;
                case MemberTypes.Property:
                    var method = ((PropertyInfo)info).GetGetMethod(true);
                    if (method == null)
                    {
                        method = ((PropertyInfo)info).GetSetMethod(true);
                    }
                    return method.IsStatic;
            }
            return false;

        }

    }


}
