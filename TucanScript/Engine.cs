using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace TucanScript
{
    public class Engine
    {
        private readonly Dictionary<string, ParamFunctionTypeDef> internalFunctions;
        private readonly Dictionary<string, ParamExecutableTypeDef> internalExecutables;
        private readonly List<string> instancesNames;
        
        private Dictionary<string, IInvokable> invokables;

        public Engine()
        {
            internalFunctions = new Dictionary<string, ParamFunctionTypeDef>();
            internalExecutables = new Dictionary<string, ParamExecutableTypeDef>();
            instancesNames = new List<string>();
            invokables = new Dictionary<string, IInvokable>();
            Evaluator = new Evaluator();
            Terminal = new Output();
            Variables = new Dictionary<string, object>();
        }
        
        public Evaluator Evaluator { get; }

        public Dictionary<string, object> Variables { get; private set; }

        public object GetVariable(string name)
        {
            return Variables[name];
        }
        
        public void SetVariable(string variableName, object variable)
        {
            if (Variables.ContainsKey(variableName))
            {
                Variables[variableName] = variable;
                return;
            }
            
            Variables.Add(variableName, variable);
        }
        
        public void SetExecutable(string functionName, ExecutableTypeDef function)
        {
            var operation = new Executable(Variables, function);
            
            if (invokables.ContainsKey(functionName))
            {
                invokables[functionName] = operation;
                return;
            }
            
            invokables.Add(functionName, operation);
        }
        
        public void Invoke(string functionName)
        {
            invokables[functionName].Invoke();
        }
        
        public object InvokeInternalFunction(string functionName, object[] parameters)
        {
            return internalFunctions[functionName].Invoke(Variables, parameters);
        }
        
        public void InvokeInternalExecutable(string functionName, object[] parameters)
        {
            internalExecutables[functionName].Invoke(Variables, parameters);
        }

        public bool ContainsInternalExecutable(string name)
        {
            return internalExecutables.ContainsKey(name);
        }
        
        public bool ContainsInternalFunction(string name)
        {
            return internalFunctions.ContainsKey(name);
        }

        public bool ContainsInstance(string name)
        {
            return instancesNames.Contains(name);
        }
        
        public bool ContainsInstanceType(string name)
        {
            return structsInits.ContainsKey(name);
        }

        public void CreateInstanceOfStruct(string name, string type)
        {
            instancesNames.Add(name);
        }

        public void LoadFromFile(string fileName)
        {
            var functionName = new FileInfo(fileName).Name;
            var functionSource = File.ReadAllText(fileName);
            LoadFromSource(functionName, functionSource);
        }
        
        public void LoadFromSource(string functionName, string functionSource)
        {
            var function = new Invokable(Invokable.None, this, functionSource);
            invokables.Add(functionName, function);
        }

        public void Dispose()
        {
            for (var index = 0; index < invokables.Count; index++)
            {
                invokables.ElementAt(index).Value.Dispose();
            }

            invokables = null;
            Variables = null;
        }
    }
}