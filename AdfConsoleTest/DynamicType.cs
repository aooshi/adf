using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Reflection;

namespace AdfConsoleTest
{
    public interface IDynamicType
    {
        void a();
        int b();
        void c(int i);
        void d(ref byte i);
        void f(out int i, ref string a);
        Adf.Arguments g();
    }

    public class DynamicType
    {
        public static void Test()
        {
            var iifType = typeof(IDynamicType);
            var voidType = typeof(void);

            var methods = iifType.GetMethods();
            var typebuilder = new StringBuilder();

            typebuilder.AppendLine("public class DynamicTypeDemo : AdfConsoleTest.IDynamicType{ ");
            foreach (var m in methods)
            {
                var isVoid = m.ReturnType.Equals(voidType);
                //build method
                if (isVoid)
                    typebuilder.AppendFormat("public void {0}(", m.Name);
                else
                    typebuilder.AppendFormat("public {0} {1}(", m.ReturnType.FullName, m.Name);

                //build parameters
                var parameterForOuts = new Dictionary<int, string>();
                var parameterInfos = m.GetParameters();
                if (parameterInfos.Length > 0)
                {
                    var parameters = new string[parameterInfos.Length];
                    for (int i = 0, l = parameterInfos.Length; i < l; i++)
                    {
                        var typeName = parameterInfos[i].ParameterType.FullName.TrimEnd('&');

                        if (parameterInfos[i].ParameterType.IsByRef)
                        {
                            if (parameterInfos[i].IsOut)
                            {
                                parameters[i] = string.Format("out {0} p{1}", typeName, i);
                                parameterForOuts.Add(i, typeName);
                            }
                            else
                                parameters[i] = string.Format("ref {0} p{1}", typeName, i);
                        }
                        else
                            parameters[i] = string.Format("{0} p{1}", typeName, i);
                    }
                    typebuilder.Append(string.Join(",", parameters));
                }
                typebuilder.AppendLine("){");
                //init parames
                if (parameterInfos.Length > 0)
                {
                    //build out paramenters
                    foreach (var item in parameterForOuts)
                        typebuilder.AppendLine("p" + item.Key + " = default(" + item.Value + ");");

                    typebuilder.AppendLine("object[] parames = new object[" + parameterInfos.Length + "];");
                    for (int i = 0, l = parameterInfos.Length; i < l; i++)
                        typebuilder.AppendLine("parames[" + i + "] = p" + i + ";");
                }

                //build method body
                if (!isVoid)
                {
                    typebuilder.AppendLine("throw new System.NotImplementedException();");
                }

                //method end
                typebuilder.AppendLine("}");

            }

            typebuilder.AppendLine("}");


            var assembly = encoder(typebuilder);


            Console.WriteLine(typebuilder.ToString());

            var dynamicTypeDemo = (IDynamicType)assembly.CreateInstance("DynamicTypeDemo");
            Console.WriteLine(dynamicTypeDemo != null);

            Console.Read();
        }

        private static Assembly encoder(StringBuilder typebuilder)
        {
            //创建编译器实例。 

            var provider = new CSharpCodeProvider();

            //设置编译参数。 

            var compiler = new CompilerParameters();

            compiler.GenerateExecutable = false;
            compiler.GenerateInMemory = true;
            // Set compiler argument to optimize output.
            compiler.CompilerOptions = "/optimize";

            //var refernceAssmblies = System.Reflection.Assembly.GetEntryAssembly().GetReferencedAssemblies();
            var refernceAssmblies = System.AppDomain.CurrentDomain.GetAssemblies();
            foreach (var refernceAssmblie in refernceAssmblies)
            {
                compiler.ReferencedAssemblies.Add(refernceAssmblie.Location);
            }

            //compiler.ReferencedAssemblies.Add("System.dll");
            //compiler.ReferencedAssemblies.Add("System.Data.dll");
            //compiler.ReferencedAssemblies.Add("System.Drawing.dll");

            //编译代码。 
            CompilerResults result = provider.CompileAssemblyFromSource(compiler, typebuilder.ToString());
            if (result.Errors.Count > 0)
            {
                for (int i = 0; i < result.Errors.Count; i++)
                    throw new InvalidProgramException(result.Errors[i].ToString());
                //for (int i = 0; i < result.Errors.Count; i++)
                //    Console.WriteLine(result.Errors[i]);
                //Console.WriteLine("error");
                //return null;
            }

            //获取编译后的程序集。 
            Assembly assembly = result.CompiledAssembly;
            return assembly;
        }
    }
}
