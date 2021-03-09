using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AdaptiveExpressions;
using Jint;
using Jint.Native;
using Jint.Native.Json;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Iciclecreek.AdaptiveExpressions
{
    /// <example>
    /// foo.function.js
    /// function userAge(memory) {
    ///     return memory.user.age;
    /// }
    /// 
    /// will be callable as
    /// foo.userAge();
    /// </example>
    public static class JavascriptFunctions
    {
        private static JsonSerializerSettings jsonSettings = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore, Formatting = Formatting.Indented };

        /// <summary>
        /// Find all foo.function.js resources and mount as expression functions
        /// </summary>
        /// <param name="resourceExplorer"></param>
        /// <returns></returns>
        public static void AddJavascriptFunctions(ResourceExplorer resourceExplorer)
        {
            resourceExplorer.AddResourceType("js");

            foreach (var resource in resourceExplorer.GetResources("js").Where(res => res.Id.EndsWith(".function.js")))
            {
                AddFunctions(resource);
            }

            resourceExplorer.Changed -= ResourceExplorer_Changed;
            resourceExplorer.Changed += ResourceExplorer_Changed;
        }

        private static void ResourceExplorer_Changed(Object sender, IEnumerable<Resource> resources)
        {
            foreach (var resource in resources.Where(res => res.Id.EndsWith(".function.js")))
            {
                AddFunctions(resource);
            }
        }

        /// <summary>
        /// Register given resource as function
        /// </summary>
        /// <param name="resource"></param>
        /// <returns></returns>
        internal static Engine AddFunctions(Resource resource)
        {
            if (Path.GetExtension(resource.Id) == ".js")
            {
                // foo.function.js x() => foo.x()
                var ns = Path.GetFileNameWithoutExtension(resource.Id);
                if (ns.EndsWith(".function"))
                {
                    ns = Path.GetFileNameWithoutExtension(ns);
                }

                var script = resource.ReadTextAsync().GetAwaiter().GetResult();

                return AddFunctionsForScript(ns, script);
            }
            throw new Exception($"{resource.Id} is not a js file");
        }

        public static Engine AddFunctionsForScript(String ns, String script)
        {
            if (String.IsNullOrEmpty(ns))
            {
                throw new Exception($"{ns} is not a valid namespace");
            }

            // Engine engine = new Engine((cfg) => cfg.AllowClr(typeof(Expression).Assembly));
            Engine engine = new Engine();
            var parser = new JsonParser(engine);

            // register expression() function so you can evaluate adaptive expressions from within javascript function.
            engine.SetValue("expression", new Func<string, object, object>((exp, state) => Expression.Parse(exp).TryEvaluate(state ?? new object()).value));

            // load script
            engine.Execute(script);

            // do a delta of functions which were added.
            var exports = engine.Global.GetProperty("exports").Value.AsObject();

            // register each added function into Expression.Functions table
            foreach (var function in exports.GetOwnProperties())
            {
                Expression.Functions.Add($"{ns}.{function.Key}", (args) =>
                {
                    // build up function call to be evaluated.
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine($"{function.Key}(");
                    int count = 0;
                    foreach (var arg in args)
                    {
                        if (count++ > 0)
                        {
                            sb.Append(", ");
                        }
                        sb.AppendLine(JsonConvert.SerializeObject(arg, jsonSettings));
                    }
                    sb.AppendLine($")");
                    var jsResult = engine.Eval.Invoke(JsValue.FromObject(engine, sb.ToString()));
                    var result = jsResult.ToObject();
                    return JToken.FromObject(result);
                });
            }
            return engine;
        }
    }
}
