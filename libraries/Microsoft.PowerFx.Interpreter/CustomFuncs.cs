﻿//------------------------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.PowerFx.Core.Functions;
using Microsoft.PowerFx.Core.Localization;
using Microsoft.PowerFx.Core.Public.Types;
using Microsoft.PowerFx.Core.Public.Values;
using Microsoft.PowerFx.Core.Types;
using Microsoft.PowerFx.Core.Utils;
using static Microsoft.PowerFx.Core.Localization.TexlStrings;

namespace Microsoft.PowerFx
{
    /// <summary>
    /// Internal adapter for adding custom functions. 
    /// </summary>
    class CustomTexlFunction : TexlFunction
    {
        public Func<FormulaValue[], FormulaValue> _impl;
        public override bool SupportsParamCoercion => true;

        public CustomTexlFunction(string name, FormulaType returnType, params FormulaType[] paramTypes)
            : this(name, returnType._type, Array.ConvertAll(paramTypes, x => x._type))
        {
        }

        public CustomTexlFunction(string name, DType returnType, params DType[] paramTypes)
            : base(DPath.Root, name, name, SG("Custom func " + name), FunctionCategories.MathAndStat, returnType, 0,
                  paramTypes.Length, paramTypes.Length, paramTypes)
        {

        }

        public override bool IsSelfContained => true;

        public static StringGetter SG(string text)
        {
            return (string locale) => text;
        }

        public override IEnumerable<TexlStrings.StringGetter[]> GetSignatures()
        {
            yield return new[] { SG("Arg 1") };
        }


        public virtual FormulaValue Invoke(FormulaValue[] args)
        {
            return _impl(args);
        }

        /*
                // build one over reflection 
                public static CustomTexlFunction Reflect(MethodInfo method)
                {
                    var retType = method.ReturnType;
                    var parameters = method.GetParameters();
                }*/
    }

    // Pass *instances* for custom funcs. 



    // Reflect and find the "Execute()" method.
    // Allow extensible functions 
    public abstract class ReflectionFunction
    {
        private FunctionDescr _info;

        // Assume by defaults. Will reflect to get primitive types
        protected ReflectionFunction()
        {
            _info = null;
        }

        // Explicitly provide types.
        // Necessary for Tables/Records
        protected ReflectionFunction(string name, FormulaType returnType, params FormulaType[] paramTypes)
        {
            var t = this.GetType();
            MethodInfo m = t.GetMethod("Execute", BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
            if (m == null)
            {
                throw new InvalidOperationException($"Missing Execute method");
            }

            _info = new FunctionDescr
            {
                name = name,
                retType = returnType,
                paramTypes = paramTypes,
                _method = m
            };

        }

        class FunctionDescr
        {
            public FormulaType retType;
            public FormulaType[] paramTypes;
            public string name;

            public MethodInfo _method;
        }

        private FunctionDescr Scan()
        {
            if (_info == null)
            {
                var info = new FunctionDescr();

                var t = this.GetType();

                var suffix = "Function";
                info.name = t.Name.Substring(0, t.Name.Length - suffix.Length);

                MethodInfo m = t.GetMethod("Execute", BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                if (m == null)
                {
                    throw new InvalidOperationException($"Missing Execute method");
                }

                info.retType = GetType(m.ReturnType);
                info.paramTypes = Array.ConvertAll(m.GetParameters(), p => GetType(p.ParameterType));
                info._method = m;

                _info = info;
            }
            return _info;
        }

        static FormulaType GetType(Type t)
        {
            if (t == typeof(NumberValue)) return FormulaType.Number;
            throw new NotImplementedException($"Marshal type {t.Name}");
        }


        internal TexlFunction GetTexlFunction()
        {
            var info = this.Scan();
            return new CustomTexlFunction(info.name, info.retType, info.paramTypes)
            {
                _impl = (args) => this.Invoke(args)
            };
        }

        public FormulaValue Invoke(FormulaValue[] args)
        {
            this.Scan();
            var result = _info._method.Invoke(this, args);

            return (FormulaValue)result;

        }
    }

    // Function implemented by PowerFx
    public class PowerFxFunction : ReflectionFunction
    {
        public string Name { get; set; }
        public NamedFormulaType[] InputParameters { get; set; }
        public FormulaType ReturnValue { get; set; }

        public string FormulaText { get; set; }
    }

#if false
// Examples

    // Sample arithmetic function
    public class MyWorkFunction : ReflectionFunction
    {
        public NumberValue Execute(NumberValue arg1)
        {
            return new NumberValue(arg1.Value * 2);
        }
    }

    // Sample Table function. 
    // Tables can't be reflected over. 
    class MyTableFunction : ReflectionFunction
    {
        public static TableType _retType = new TableType()
                .Add(new NamedFormulaType("X", FormulaType.Number))
                .Add(new NamedFormulaType("Y", FormulaType.Number));

        public MyTableFunction() : base("MyTable", _retType)
        {
        }

        public TableValue Execute()
        {
            var values = new[]
            {
                new { X = 10, Y = 15},
                new { X = 20, Y = 25}
            };

            TableValue v = TableValue.FromRecords(values, _retType);
            return v;
        }
    }
#endif
}
