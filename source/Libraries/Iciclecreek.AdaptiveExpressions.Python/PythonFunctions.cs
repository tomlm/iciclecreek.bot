using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AdaptiveExpressions;
using IronPython.Hosting;
using IronPython.Runtime;
using IronPython.Runtime.Types;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Hosting;
using Newtonsoft.Json.Linq;

namespace Iciclecreek.AdaptiveExpressions
{
    /// <summary>
    /// Example foo.functions.py
    /// </summery>
    /// <example>
    /// # Define a function `plus()`
    /// def plus(a, b):
    ///     return a + b
    /// </example>
    public static class PythonFunctions
    {
        /// <summary>
        /// Find all foo.function.py resources and mount as expression functions
        /// </summary>
        /// <param name="resourceExplorer"></param>
        /// <returns></returns>
        public static void AddPythonFunctions(ResourceExplorer resourceExplorer)
        {
            resourceExplorer.AddResourceType("py");

            foreach (var resource in resourceExplorer.GetResources("py").Where(res => res.Id.EndsWith(".function.py")))
            {
                AddFunctions(resource);
            }

            resourceExplorer.Changed -= ResourceExplorer_Changed;
            resourceExplorer.Changed += ResourceExplorer_Changed;
        }

        private static void ResourceExplorer_Changed(Object sender, IEnumerable<Resource> resources)
        {
            foreach (var resource in resources.Where(res => res.Id.EndsWith(".function.py")))
            {
                AddFunctions(resource);
            }
        }

        /// <summary>
        /// Register given resource as function
        /// </summary>
        /// <param name="resource"></param>
        /// <returns></returns>
        internal static ScriptEngine AddFunctions(Resource resource)
        {
            if (Path.GetExtension(resource.Id) == ".py")
            {
                // foo.function.py x() => foo.x()
                var ns = Path.GetFileNameWithoutExtension(resource.Id);
                if (ns.EndsWith(".function"))
                {
                    ns = Path.GetFileNameWithoutExtension(ns);
                }

                var script = resource.ReadTextAsync().GetAwaiter().GetResult();

                return AddFunctionsForScript(ns, script);
            }
            throw new Exception($"{resource.Id} is not a py file");
        }

        public static ScriptEngine AddFunctionsForScript(String ns, String script)
        {
            if (String.IsNullOrEmpty(ns))
            {
                throw new Exception($"{ns} is not a valid namespace");
            }

            // Engine engine = new Engine((cfg) => cfg.AllowClr(typeof(Expression).Assembly));
            var engine = Python.CreateEngine();

            var source = engine.CreateScriptSourceFromString(script);
            var scope = engine.CreateScope();

            // register expression() function so you can evaluate adaptive expressions from within javascript function.
            scope.SetVariable("expression", new Func<string, object, object>((exp, state) => Expression.Parse(exp).TryEvaluate(PythonToJToken(state ?? new object())).value));

            // executing script in scope
            source.Execute(scope);

            // register each added function into Expression.Functions table
            foreach (var function in scope.GetVariableNames())
            {
                if (!function.StartsWith("__"))
                {
                    var func = scope.GetVariable(function);

                    Expression.Functions.Add($"{ns}.{function}", (args) =>
                    {
                        var objArgs = args.Cast<object>().Select(a => JToken.FromObject(a)).ToArray();
                        var raw = scope.Engine.Operations.Invoke(func, objArgs);
                        return PythonToJToken(raw);
                    });
                }
            }
            return engine;
        }

        private static JToken PythonToJToken(object raw)
        {
            if (raw is PythonDictionary dict)
            {
                var result = new JObject();
                foreach(var key in dict.Keys)
                {
                    result[key] = PythonToJToken(dict[key]);
                }
                return result;
            }

            if (raw is List list)
            {
                var result = new JArray();
                foreach(var item in list)
                {
                    result.Add(PythonToJToken(item));
                }
                return result;
            }

            if (raw is JValue val)
            {
                return val;
            }

            return JToken.FromObject(raw);
        }
    }
}