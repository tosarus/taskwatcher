using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace TaskWatcher.Console
{
    static class CommandBuilder
    {
        public static IEnumerable<Command> ObjectToCommands(object commandsObj)
        {
            if (commandsObj == null)
            {
                throw new ArgumentNullException("commandsObj");
            }

            Type objType = commandsObj.GetType();
            return objType.GetMethods()
                          .Select(methodInfo => BuildCommand(methodInfo, commandsObj))
                          .Where(cmd => cmd != null);
        }

        private static Command BuildCommand(MethodInfo method, object target)
        {
            var cmdAttr = method.GetCustomAttribute<CommandAttribute>();
            if (cmdAttr == null)
            {
                return null;
            }

            return new Command {
                                   Type = cmdAttr.Type,
                                   Name = cmdAttr.Name,
                                   Help = cmdAttr.Help,
                                   UsageArgs = BuildUsage(method),
                                   Proc = args => InvokeMethod(args, method, target)
                               };
        }

        private static string BuildUsage(MethodInfo method)
        {
            var sb = new StringBuilder();
            foreach (ParameterInfo pinfo in method.GetParameters())
            {
                if (pinfo.HasDefaultValue)
                {
                    sb.Append(String.Format("[{0}:{1} = {2}] ",
                                            pinfo.ParameterType.Name, pinfo.Name, pinfo.RawDefaultValue ?? "null"));
                }
                else
                {
                    sb.Append(String.Format("{0}:{1} ", pinfo.ParameterType.Name, pinfo.Name));
                }
            }
            return sb.ToString();
        }

        private static void InvokeMethod(string[] args, MethodInfo method, object target)
        {
            var invokeArgs = new List<object>();
            ParameterInfo[] parameterInfos = method.GetParameters();
            if (args.Length > parameterInfos.Length)
            {
                string message = String.Format("To many arguments provided({0}, expected {1}",
                                               args.Length, parameterInfos.Length);
                throw new ArgumentException(message);
            }

            for (int i = 0; i < parameterInfos.Length; ++i)
            {
                ParameterInfo pinfo = parameterInfos[i];
                if (i < args.Length)
                {
                    object value;
                    try
                    {
                        value = Convert.ChangeType(args[i], pinfo.ParameterType);
                    }
                    catch (Exception e)
                    {
                        string message = String.Format("Argument '{0}': {1}", pinfo.Name, e.Message);
                        throw new ArgumentException(message, e);
                    }
                    invokeArgs.Add(value);
                }
                else if (pinfo.HasDefaultValue)
                {
                    invokeArgs.Add(pinfo.RawDefaultValue);
                }
                else
                {
                    string message = String.Format("Argument '{0}': No value was provided", pinfo.Name);
                    throw new ArgumentException(message);
                }
            }

            try
            {
                method.Invoke(target, invokeArgs.ToArray());
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException;
            }
        }
    }
}
