using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace BenchmarkLogGenerator.Utilities
{

    #region class CommandLineArgAttribute
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class CommandLineArgAttribute : Attribute
    {
        /// <summary>
        /// The short name of the command-line switch.
        /// </summary>
        public string ShortName;

        /// <summary>
        /// Additional alises of the command-line switch.
        /// </summary>
        public string[] Aliases;

        /// <summary>
        /// The long name of the command-line switch.
        /// </summary>
        public string FullName;

        /// <summary>
        /// A human-readable description of the comand-line switch.
        /// </summary>
        public string Description;

        /// <summary>
        /// Default value to be used if the user doesn't specify the command-line switch.
        /// </summary>
        public object DefaultValue;

        /// <summary>
        /// Is this a mandatory switch?
        /// </summary>
        public bool Mandatory;

        /// <summary>
        /// Is this argument a secret? (If so, we won't print it.)
        /// </summary>
        public bool IsSecret;

        /// <summary>
        /// Does this switch contain "known" secrets? (If so, we don't print them.)
        /// Known secrets are secrets whose pattern is recognized by <see cref="Obfuscator.RemoveKnownSecrets(string)"/>.
        /// </summary>
        public bool ContainsKnownSecrets;

        /// <summary>
        /// Does this switch support encryption? (If so, it can be attempted at being decrypted after parsing, using the 'Decrypt()' method)
        /// </summary>
        public bool SupportsEncryption;

        /// <summary>
        /// Trigger -- the name of a parameterless instance method that will get invoked
        /// following processing of the field.
        /// </summary>
        public string WhenSet;

        /// <summary>
        /// Can the user include the switch without providing a value?
        /// Applies only to string switches.
        /// </summary>
        public bool AllowNull;

        public CommandLineArgAttribute(string fullName, string description)
        {
            ShortName = fullName;
            FullName = fullName;
            Description = description;
        }

        public CommandLineArgAttribute(string fullName, string description, params string[] aliases)
        {
            ShortName = fullName;
            FullName = fullName;
            Description = description;
            Aliases = aliases;
        }

        public IEnumerable<string> GetShortNameAndAliases()
        {
            // TODO: We might want to cache this...
            HashSet<string> ret = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            ret.Add(FullName);

            if (!string.IsNullOrWhiteSpace(ShortName))
            {
                ret.Add(ShortName);
            }

            if (Aliases.SafeFastAny())
            {
                foreach (var alias in Aliases)
                {
                    if (!string.IsNullOrWhiteSpace(alias))
                    {
                        ret.Add(alias);
                    }
                }

            }

            ret.Remove(FullName);
            return ret;
        }

    }
    #endregion

    #region class CommandLineArgsParser
    public class CommandLineArgsParser
    {
        #region Private constants
        private static char[] c_quote = new char[] { '"' };
        private static char c_multiValueSeparator = '\x03';
        private static string c_multiValueSeparatorStr = "\x03";
        private static char[] c_multiValueSeparatorArray = new[] { c_multiValueSeparator };
        private const BindingFlags WhenXxxLookup = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        #endregion

        public static T Parse<T>(IEnumerable<string> args, T target, Action<string[], T, string> faultAction = null, bool autoHelp = false, string envVar = null)
        {
            if (autoHelp && faultAction == null)
            {
                faultAction = WriteHelpStringToConsoleAndQuit;
            }

            LinkedList<string> freeArgs = null; // All arguments that appear before the first arg with '/' or '-'
            Dictionary<string, string> candidateArgs = null;
            GetFreeAndCandidateArgs(args, envVar, ref freeArgs, ref candidateArgs);

            if (autoHelp)
            {
                if (candidateArgs.ContainsKey("h") || candidateArgs.ContainsKey("?") || candidateArgs.ContainsKey("help"))
                {
                    WriteHelpStringToConsoleAndQuit(args, target, null, markdown: false);
                }
            }

            AssignArgsToTargetGetValue(args, target, faultAction, freeArgs, candidateArgs);

            return target;
        }
        public static void WriteHelpStringToConsoleAndQuit<T>(string[] args, T target, string fault)
        {
            WriteHelpStringToConsoleAndQuit(args, target, fault, markdown: false);
        }

        public static void WriteHelpStringToConsoleAndQuit<T>(string[] args, T target, string fault, bool markdown)
        {
            WriteHelpStringToConsoleAndQuit<T>(args as IEnumerable<string>, target, fault, markdown);
        }
        private static void WriteHelpStringToConsoleAndQuit<T>(IEnumerable<string> args, T target, string fault, bool markdown)
        {
            var esb = new ExtendedStringBuilder();
            if (!string.IsNullOrWhiteSpace(fault))
            {
                esb.Indent();
                esb.AppendLine("Bad input:");
                esb.Indent();
                esb.AppendLine(fault);
                esb.AppendLine();
                esb.Unindent();
                esb.Unindent();
            }
            CommandLineArgsParser.WriteHelpString(esb, target);
            Console.WriteLine(esb.ToString());
            Environment.Exit(0);
        }

        /// <summary>
        /// Given one or more objects of a type whose public fields are attributed
        /// by <see cref="CommandLineArgAttribute"/> (and the first of said objects
        /// potentially attributed by <see cref="CommandLineArgsAttribute"/>),
        /// writes a corresponding help string to the string builder.
        /// </summary>
        public static void WriteHelpString(ExtendedStringBuilder esb, params object[] targets)
        {
            // Gather all the args
            var names = new HashSet<string>();
            var args = new List<Tuple<CommandLineArgAttribute, FieldInfo>>();
            var descriptions = new List<string>();
            GatherArgsForHelpString(targets, names, args, descriptions);

            // Synopsis line
            esb.Indent();
            esb.AppendLine("Synopsis:");
            esb.Indent();
            var attributes = string.Join(" ", args.Select(arg => FormatArg(arg, false)));
            esb.AppendLine(attributes);
            esb.Unindent();
            esb.AppendLine();

            // Description
            if (descriptions.Count > 0)
            {
                esb.AppendLine("Description:");
                esb.Indent();
                // TODO: Smart algorithm to break lines might be added here...
                foreach (var description in descriptions)
                {
                    foreach (var line in description.SplitLines())
                    {
                        esb.AppendLine(line);
                    }
                }
                esb.Unindent();
                esb.AppendLine();
            }

            // Arguments
            esb.AppendLine("Arguments:");
            bool firstArgument = true;
            esb.Indent();
            foreach (var arg in args)
            {
                if (firstArgument)
                {
                    firstArgument = false;
                }
                else
                {
                    esb.AppendLine();
                }

                if (arg.Item1.Mandatory)
                {
                    esb.AppendLine("[Mandatory: True]");
                }
                var fullName = arg.Item1.FullName;
                var aliases = arg.Item1.GetShortNameAndAliases();
                esb.AppendLine(FormatArg(arg, true));
                foreach (var alias in aliases)
                {
                    esb.AppendLine("[-" + alias + ":...]");
                }

                esb.Indent();
                foreach (var line in arg.Item1.Description.SplitLines())
                {
                    esb.AppendLine(line); // TODO: break long lines
                }
                esb.Unindent();
            }

            if (!firstArgument)
            {
                esb.AppendLine();
            }

            esb.Unindent();
            esb.Unindent();

            PrintUsage(esb);
        }

        public static void PrintUsage(ExtendedStringBuilder esb)
        {
            esb.AppendLine("Usage examples:");
            esb.AppendLine();
            esb.Indent();
            esb.AppendLine(@"[Write to local disk]");
            esb.AppendLine(@"BenchmarkLogGenerator -output:LocalDisk -localPath:""c:\users\foo\documents""");
            esb.AppendLine();
            esb.AppendLine(@"[Write to Azure Storage container]");
            esb.AppendLine(@"BenchmarkLogGenerator -output:AzureStorage -azureStorageAccountConnection:""DefaultEndpointsProtocol=https;AccountName=NAME;AccountKey=KEY;EndpointSuffix=core.windows.net""");
            esb.AppendLine();
            esb.AppendLine(@"[Write to EventHub]");
            esb.AppendLine(@"BenchmarkLogGenerator -output:EventHub -eventHubConnection:""Endpoint=sb://EVENTHUB_NAMESPACE.windows.net/;SharedAccessKeyName=readwrite;SharedAccessKey=KEY""");
            esb.AppendLine();
            Console.WriteLine(esb.ToString());
            Environment.Exit(1);
        }

        private static void GatherArgsForHelpString(object[] targets, HashSet<string> names, List<Tuple<CommandLineArgAttribute, FieldInfo>> args, List<string> descriptions)
        {
            foreach (var target in targets)
            {
                GatherArgsForHelpString(target, names, args, descriptions);
            }
        }

        private static void GatherArgsForHelpString(object target, HashSet<string> names, List<Tuple<CommandLineArgAttribute, FieldInfo>> args, List<string> descriptions)
        {
            // Desciption at the target level
            var targetAttribute = target.GetType().GetCustomAttribute<CommandLineArgAttribute>();
            if (targetAttribute != null && !string.IsNullOrWhiteSpace(targetAttribute.Description))
            {
                descriptions.Add(targetAttribute.Description);
            }

            int insertIndex = 0;
            foreach (var field in target.GetType().GetFieldsOrdered())
            {
                var attribute = (CommandLineArgAttribute)field.GetCustomAttribute(typeof(CommandLineArgAttribute));
                if (attribute == null)
                {
                    continue;
                }

                if (field.FieldType.GetCustomAttribute(typeof(CommandLineArgAttribute)) != null)
                {
                    // Nested
                    GatherArgsForHelpString(field.GetValue(target), names, args, descriptions);
                    continue;
                }

                var item = new Tuple<CommandLineArgAttribute, FieldInfo>(attribute, field);
                if (string.IsNullOrWhiteSpace(attribute.FullName))
                {
                    args.Insert(insertIndex, item);
                    insertIndex++;
                }
                else
                {
                    if (!names.Contains(attribute.FullName))
                    {
                        names.Add(attribute.FullName);
                        args.Add(item);
                    }
                    else
                    {
                        // TODO: We should add validation logic here, or add a disambiguation, or...
                    }
                }
            }
        }

        private static void GetFreeAndCandidateArgs(IEnumerable<string> args, string envVar, ref LinkedList<string> freeArgs, ref Dictionary<string, string> candidateArgs)
        {
            freeArgs = freeArgs ?? new LinkedList<string>(); // All arguments that appear before the first arg with '/' or '-'
            candidateArgs = candidateArgs ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            bool acceptingFreeArgs = true;

            

            // Make a list of all arg candidates
            foreach (var arg in args)
            {
                string a = arg.Trim();
                if (!StartsWithSwitchCharacter(a))
                {
                    if (acceptingFreeArgs)
                    {
                        freeArgs.AddLast(a);
                    }
                    continue;
                }

                acceptingFreeArgs = false;
                a = a.Substring(1);
                string value;
                a = a.SplitFirst(':', out value);
                if (string.IsNullOrEmpty(a))
                {
                    continue;
                }

                if (candidateArgs.ContainsKey(a))
                {
                    // This is a hack to support multi-valued args:
                    // When we encounter more than one value for a switch, we concat the old value with
                    // the new value using a control character nobody would provide normally.
                    candidateArgs[a] = candidateArgs[a] + c_multiValueSeparator + value;
                    continue;
                }

                candidateArgs.Add(a, value);
            }
        }

        private static bool StartsWithSwitchCharacter(string what)
        {
            if (string.IsNullOrEmpty(what))
            {
                return false;
            }

            char c = what[0];
            // The last one is a Unicode dash, which is often generated by auto-correct tools
            // such as Microsoft Word
            return c == '-' || c == '/' || c == '\u2013';
        }

        private static void AssignArgsToTargetGetValue<T>(IEnumerable<string> args, T target, Action<string[], T, string> faultAction, LinkedList<string> freeArgs, Dictionary<string, string> candidateArgs)
        {
            var targetType = target.GetType();
            foreach (var field in targetType.GetFieldsOrdered())
            {
                var attribute = (CommandLineArgAttribute)field.GetCustomAttribute(typeof(CommandLineArgAttribute));
                if (attribute == null)
                {
                    continue;
                }
                var aliases = attribute.GetShortNameAndAliases();

                bool needsSpecifying = attribute.Mandatory;
                string value;

                if (field.FieldType.GetCustomAttribute(typeof(CommandLineArgAttribute)) != null)
                {
                    var fieldValue = field.GetValue(target);
                    if (fieldValue != null)
                    {
                        // Nested parse
                        // TODO: This is typeless (T==object), ignoring faultAction and/or autoHelp
                        // TODO: This is done without any prefix on nested arg names
                        var trampolineT = typeof(CommandLineArgsParser).GetMethod("ParseTrampoline", BindingFlags.NonPublic | BindingFlags.Static);
                        if (trampolineT != null)
                        {
                            // private static void ParseTrampoline<T>(IEnumerable<string> args, T target)
                            // TODO: Add support for autoHelp, envVar to ParseTrampoline<T> and add them here
                            var trampoline = trampolineT.MakeGenericMethod(new Type[] { fieldValue.GetType() });
                            trampoline.Invoke(null, new object[] { args, fieldValue });
                        }
                        else
                        {
                            // Fallback to typeless (T is System.Object) with no faultAction and default autoHelp.
                            // TODO: This is done without any prefix on nested arg names
                            Parse(args, fieldValue);
                        }

                    }
                }
                else if (string.IsNullOrWhiteSpace(attribute.FullName) && freeArgs.Count > 0)
                {
                    // A switch with no name is assumed to refer to the "free args",
                    // and we have some unconsumed free args, so the next one is used
                    // to assign a valur to the switch
                    value = freeArgs.First();
                    freeArgs.RemoveFirst();

                    needsSpecifying = false;
                    SetField(target, field, "[free arg]", value, allowNull: false);
                }
                else if (!string.IsNullOrWhiteSpace(attribute.FullName) && TryGetValue(candidateArgs, attribute.FullName, aliases, out value))
                {
                    needsSpecifying = false;
                    SetField(target, field, attribute.FullName, value, allowNull: attribute.AllowNull);
                }
                else if (attribute.DefaultValue != null)
                {
                    needsSpecifying = false;
                    field.SetValue(target, attribute.DefaultValue);
                }

                if (needsSpecifying)
                {
                    var fault = string.Format("Argument '" + attribute.FullName + "' must be specified as it is marked as mandatory");
                    if (faultAction != null)
                    {
                        var argsArray = args.ToArray();
                        faultAction(argsArray, target, fault);
                    }
                }

                if (attribute.WhenSet != null)
                {
                    var method = targetType.GetMethod(attribute.WhenSet, WhenXxxLookup);
                    //Ensure.ArgIsNotNull(method, "method(" + attribute.WhenSet + ")"); // TODO: Could use a better exception here...
                    method.Invoke(target, null);
                }
            }
        }

        private static void SetField(object target, FieldInfo field, string switchName, string value, bool allowNull)
        {
            // Support for nullable types - get the real field type
            var fieldType = field.FieldType.GetTypeWithNullableSupport();

            // TODO: Each call to Parse below may throw an exception.
            //       Since this is user-input, we should catch the exception and
            //       provide some means to indicate a user input error instead
            //       of crashing the caller.
            try
            {
                if (fieldType == typeof(string))
                {
                    value = GetLastValueIfMultivalue(value);
                    if (allowNull && value == null)
                    {
                        field.SetValue(target, null);
                    }
                    else
                    {
                        VerifyNoNullValue(switchName, value);
                        VerifyNoMultiValue(switchName, value);
                        field.SetValue(target, value.Trim(c_quote));
                    }
                }
                else if(fieldType == typeof(WriterType))
                {
                    value = GetLastValueIfMultivalue(value);
                    VerifyNoNullValue(switchName, value);
                    VerifyNoMultiValue(switchName, value);
                    field.SetValue(target, Enum.Parse(typeof(WriterType), value, true));
                }
                else if (fieldType == typeof(BenchmarkDataSize))
                {
                    value = GetLastValueIfMultivalue(value);
                    VerifyNoNullValue(switchName, value);
                    VerifyNoMultiValue(switchName, value);
                    field.SetValue(target, Enum.Parse(typeof(BenchmarkDataSize), value, true));
                }
                else if (fieldType == typeof(int))
                {
                    value = GetLastValueIfMultivalue(value);
                    VerifyNoNullValue(switchName, value);
                    field.SetValue(target, int.Parse(value));
                }
            }
            catch (Exception)
            {
                WriteHelpStringToConsoleAndQuit(new string[] { }, target, string.Format("CommandLineArgsParser failed to parse argument '{0}' of type '{1}' with value '{2}'",
                    switchName, field.FieldType.ToString(), value));
            }
        }

        private static void VerifyNoNullValue(string switchName, string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(string.Format("No value provided for switch -{0}. Please use the format: -{0}:VALUE", switchName));
            }
        }

        private static void VerifyNoMultiValue(string switchName, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return;
            }
            if (value.Contains(c_multiValueSeparator))
            {
                int count = value.Split(c_multiValueSeparator).Length;
                throw new ArgumentOutOfRangeException(switchName,
                    count.ToString() + " appearences of the switch -" + switchName + ": expecting at most one.");
            }
        }

        private static string GetLastValueIfMultivalue(string value)
        {
            if (string.IsNullOrEmpty(value) || value.IndexOf(c_multiValueSeparator) < 0)
            {
                return value;
            }

            var values = value.Split(c_multiValueSeparatorArray);
            return values[values.Length - 1];
        }

        private static string FormatArg(Tuple<CommandLineArgAttribute, FieldInfo> pair, bool includeDefaultValue)
        {
            var attribute = pair.Item1;
            var fieldInfo = pair.Item2;

            var ret = new System.Text.StringBuilder();
            if (!attribute.Mandatory)
            {
                ret.Append('[');
            }

            bool hasName = false;
            if (!string.IsNullOrWhiteSpace(attribute.FullName))
            {
                ret.Append('-');
                ret.Append(attribute.FullName);
                hasName = true;
            }

            // Support for nullable types - get the real field type
            var fieldType = fieldInfo.FieldType.GetTypeWithNullableSupport();

            if (typeof(IEnumerable<string>).IsAssignableFrom(fieldType))
            {
                ret.Append(hasName ? ":string*" : "string*");
            }
            else if (fieldType == typeof(bool))
            {
                ret.Append(hasName ? "[:true-or-false]" : "[true-or-false]");
            }
            else
            {
                ret.Append((hasName ? ":" : "") + fieldType.Name);
            }

            if (includeDefaultValue && attribute.DefaultValue != null)
            {
                var defaultValueAsString = attribute.DefaultValue.ToString();
                if (!string.IsNullOrWhiteSpace(defaultValueAsString))
                {
                    ret.Append(" (default is: " + attribute.DefaultValue.ToString() + ")");
                }
            }

            if (!attribute.Mandatory)
            {
                ret.Append(']');
            }

            return ret.ToString();
        }

        private static bool TryGetValue(Dictionary<string, string> candidateArgs, string fullName, IEnumerable<string> aliases, out string value)
        {
            string v;
            value = null;
            var ret = false;

            if (candidateArgs.TryGetValue(fullName, out v))
            {
                value = v; // No need for string.Join here
                ret = true;
            }

            foreach (var alias in aliases)
            {
                if (candidateArgs.TryGetValue(alias, out v))
                {
                    if (ret == false)
                    {
                        value = v;
                    }
                    else
                    {
                        value = string.Join(c_multiValueSeparatorStr, v, value);
                    }
                    ret = true;
                }
            }

            return ret;
        }
    }
    #endregion
}