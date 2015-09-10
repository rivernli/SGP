using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using BI.SGP.BLL.Utils;

namespace BI.SGP.BLL.CostEngine
{
    public class Expression
    {
        private object instance;
        private MethodInfo method;
        public string ExpressionBody;
        public List<string> ExpressionKey = new List<string>();
        private static Dictionary<string, Expression> _expressionObjects = new Dictionary<string, Expression>();

        public static Expression GetExpression(string expression)
        {
            
            if (!_expressionObjects.ContainsKey(expression))
            {
                _expressionObjects.Add(expression, new Expression(expression));
            }
            return _expressionObjects[expression];
        }

        public Expression(string expression)
        {
            CompilerParameters p = new CompilerParameters();
            p.GenerateInMemory = true;
            System.Text.RegularExpressions.MatchCollection mc = System.Text.RegularExpressions.Regex.Matches(expression, @"\{([^\}]*)\}");
            string paramsStr = "";
            for (int i = 0; i < mc.Count; i++)
            {
                string mcValue = mc[i].Value;
                string mcGroupValue = mc[i].Groups[1].Value;
                if (!ExpressionKey.Contains(mcGroupValue))
                {
                    ExpressionKey.Add(mcGroupValue);
                    paramsStr += "double d" + i + ",";
                    expression = expression.Replace(mcValue, "d" + i);
                }
            }
            this.ExpressionBody = expression;
            paramsStr = paramsStr.TrimEnd(',');
            string clsStr = String.Format("using System;sealed class Expression{{public double Compute({0}){{return {1};}}}}", paramsStr, expression);
            CompilerResults cr = new CSharpCodeProvider().CompileAssemblyFromSource(p, clsStr);
            if (cr.Errors.Count > 0)
            {
                string msg = "Expression(\"" + expression + "\"): \n";
                foreach (CompilerError err in cr.Errors) msg += err.ToString() + "\n";
                throw new Exception(msg);
            }
            instance = cr.CompiledAssembly.CreateInstance("Expression");
            method = instance.GetType().GetMethod("Compute");
        }

        public double Compute(Dictionary<string, object> data)
        {
            List<object> paramsValue = new List<object>();
            foreach (string key in ExpressionKey)
            {
                if (data.ContainsKey(key))
                {
                    paramsValue.Add(ParseHelper.Parse<double>(data[key]));
                }
                else
                {
                    throw new Exception(String.Format("[{0}] is required.", key));
                }
            }
            
            return (double)method.Invoke(instance, paramsValue.ToArray());
        }
    }
}
