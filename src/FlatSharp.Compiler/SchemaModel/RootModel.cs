﻿/*
 * Copyright 2021 James Courtney
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using FlatSharp.CodeGen;
using FlatSharp.Compiler.Schema;

namespace FlatSharp.Compiler.SchemaModel;

public class RootModel
{
    private const AdvancedFeatures SupportedAdvancedFeatures =
        AdvancedFeatures.AdvancedArrayFeatures | // struct vectors
        AdvancedFeatures.AdvancedUnionFeatures | // vectors of union and string members in unions.
        AdvancedFeatures.OptionalScalars;        // nullable scalars in tables.

    private readonly Dictionary<string, BaseSchemaModel> elements = new();

    public RootModel(AdvancedFeatures advancedFeatures)
    {
        if ((advancedFeatures & ~SupportedAdvancedFeatures) != AdvancedFeatures.None)
        {
            // bail immediately. We can't make any progress.
            throw new InvalidFbsFileException($"FBS schema contains advanced features that FlatSharp does not yet support.");
        }
    }

    public void UnionWith(RootModel other)
    {
        foreach (var kvp in other.elements)
        {
            if (this.elements.TryGetValue(kvp.Key, out BaseSchemaModel? model))
            {
                if (model.DeclaringFile != kvp.Value.DeclaringFile)
                {
                    string name = kvp.Value.FriendlyName;
                    ErrorContext.Current.RegisterError($"Duplicate type declared in two different FBS files: {name}. File1: {kvp.Value.DeclaringFile}, File2: {model.DeclaringFile}");
                }
            }
            else
            {
                this.elements[kvp.Key] = kvp.Value;
            }
        }
    }

    public void AddElement(BaseSchemaModel model)
    {
        if (!this.elements.TryAdd(model.FullName, model))
        {
            ErrorContext.Current.RegisterError($"Duplicate type declared in same FBS file: {model.FriendlyName}. File: {model.DeclaringFile}");
        }
    }

    internal void WriteCode(CodeWriter writer, CompileContext context)
    {
        if (!context.Options.MutationTestingMode)
        {
            writer.AppendLine($$"""
                //------------------------------------------------------------------------------
                // <auto-generated>
                //     This code was generated by the FlatSharp FBS to C# compiler (source hash: {{context.InputHash}})
                //
                //     Changes to this file may cause incorrect behavior and will be lost if
                //     the code is regenerated.
                // </auto-generated>
                //------------------------------------------------------------------------------
                """);
        }

        writer.AppendLine("using System;");
        writer.AppendLine("using System.Collections.Generic;");
        writer.AppendLine("using System.Linq;");
        writer.AppendLine("using System.Runtime.CompilerServices;");
        writer.AppendLine("using System.Threading;");
        writer.AppendLine("using System.Threading.Tasks;");
        writer.AppendLine("using FlatSharp;");
        writer.AppendLine("using FlatSharp.Attributes;");
        writer.AppendLine("using FlatSharp.Internal;");


        // disable obsolete warnings. Flatsharp allows marking default constructors
        // as obsolete and we don't want to raise warnings for our own code.
        writer.AppendLine("#pragma warning disable 0618");

#if NET5_0_OR_GREATER
            if (RoslynSerializerGenerator.EnableStrictValidation &&
                context.Options.NullableWarnings == null)
            {
                context = context with
                {
                    Options = context.Options with
                    {
                        NullableWarnings = true
                    }
                };
            }
#endif

        if (context.Options.NullableWarnings == true)
        {
            writer.AppendLine("#nullable enable");
        }
        else
        {
            writer.AppendLine("#nullable enable annotations");
        }

        if (context.CompilePass > CodeWritingPass.PropertyModeling && context.PreviousAssembly is not null)
        {
            context.FullyQualifiedCloneMethodName = CloneMethodsGenerator.GenerateCloneMethodsForAssembly(
                writer,
                context.Options,
                context.PreviousAssembly,
                context.TypeModelContainer);

            HashSet<Type> seenTypes = new();
            foreach (var item in this.elements.Values)
            {
                item.TraverseTypeModel(context, seenTypes);
            }

            foreach (Type t in seenTypes)
            {
                WriteHelperClass(t, context, writer);
            }
        }

        foreach (var item in this.elements.Values)
        {
            item.WriteCode(writer, context);
        }
    }

    private static void WriteHelperClass(Type type, CompileContext context, CodeWriter writer)
    {
        var options = new FlatBufferSerializerOptions();
        var generator = new RoslynSerializerGenerator(options, context.TypeModelContainer);
        string helper = generator.ImplementHelperClass(context.TypeModelContainer.CreateTypeModel(type), new DefaultMethodNameResolver());

        writer.AppendLine(helper);
    }
}
