﻿namespace VisualMutator.Model.Mutations.Types
{
    #region

    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.Windows.Forms;
    using Exceptions;
    using Extensibility;
    using Infrastructure;
    using log4net;
    using Microsoft.Cci;
    using MutantsTree;
    using StoringMutants;
    using UsefulTools.CheckboxedTree;
    using UsefulTools.ExtensionMethods;

    #endregion

    public interface ITypesManager
    {

        bool IsAssemblyLoadError { get; set; }

        IList<AssemblyNode> CreateNodesFromAssemblies(IModuleSource modules,
            ICodePartsMatcher constraints);
        MutationFilter CreateFilterBasedOnSelection(ICollection<AssemblyNode> assemblies);
    }
    public static class Helpers
    {
        public static string GetTypeFullName(this INamespaceTypeReference t)
        {
            var nsPart = TypeHelper.GetNamespaceName(t.ContainingUnitNamespace, NameFormattingOptions.None);
            var typePart = t.Name.Value + (t.MangleName ? "`"+t.GenericParameterCount : "");
            return nsPart + "." + typePart;
        }
    }
    public class SolutionTypesManager : ITypesManager
    {
        private readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public bool IsAssemblyLoadError { get; set; }
   
        public SolutionTypesManager()
        {
        }

        public MutationFilter CreateFilterBasedOnSelection(ICollection<AssemblyNode> assemblies)
        {
            var methods = assemblies
                .SelectManyRecursive<CheckedNode>(node => node.Children, node => node.IsIncluded ?? true, leafsOnly: true)
                .OfType<MethodNode>().Select(type => type.MethodDefinition).ToList();
            return new MutationFilter(new List<TypeIdentifier>(), methods.Select(m => new MethodIdentifier(m)).ToList());
        }

        public IList<AssemblyNode> CreateNodesFromAssemblies(IModuleSource modules,
            ICodePartsMatcher constraints)
        {
            var matcher = constraints.Join(new ProperlyNamedMatcher());

            List<AssemblyNode> assemblyNodes = modules.Modules.Select(m => CreateAssemblyNode(m, matcher)).ToList();
            var root = new RootNode();
            root.Children.AddRange(assemblyNodes);
            root.IsIncluded = true;

            return assemblyNodes;
        }

     

        public AssemblyNode CreateAssemblyNode(IModuleInfo module, 
            ICodePartsMatcher matcher)
        {
            var assemblyNode = new AssemblyNode(module.Name);

            System.Action<CheckedNode, ICollection<INamedTypeDefinition>> typeNodeCreator = (parent, leafTypes) =>
            {
                foreach (INamedTypeDefinition typeDefinition in leafTypes)
                {
                   // _log.Debug("For types: matching: ");
                    if (matcher.Matches(typeDefinition))
                    {
                        var type = new TypeNode(parent, typeDefinition.Name.Value);
                        foreach (var method in typeDefinition.Methods)
                        {
                            if (matcher.Matches(method))
                            {
                                type.Children.Add(new MethodNode(type, method.Name.Value, method, false));
                            }
                        }
                        parent.Children.Add(type);
                    }
                }
            };
            Func<INamedTypeDefinition, string> namespaceExtractor = typeDef =>
                TypeHelper.GetDefiningNamespace(typeDef).Name.Value;


            NamespaceGrouper<INamespaceTypeDefinition, CheckedNode>.
                GroupTypes(assemblyNode,
                    namespaceExtractor,
                    (parent, name) => new TypeNamespaceNode(parent, name),
                    typeNodeCreator,
                        module.Module.GetAllTypes().ToList());


            //remove empty amespaces. 
            //TODO to refactor...
            List<TypeNamespaceNode> checkedNodes = assemblyNode.Children.OfType<TypeNamespaceNode>().ToList();
            foreach (TypeNamespaceNode node in checkedNodes)
            {
                RemoveFromParentIfEmpty(node);
            }
            return assemblyNode;
        }
        public void RemoveFromParentIfEmpty(MutationNode node)
        {
            var children = node.Children.ToList();
            while (children.OfType<TypeNamespaceNode>().Any())
            {
                TypeNamespaceNode typeNamespaceNode = node.Children.OfType<TypeNamespaceNode>().First();
                RemoveFromParentIfEmpty(typeNamespaceNode);
                children.Remove(typeNamespaceNode);
            }
            while (children.OfType<TypeNode>().Any())
            {
                TypeNode typeNamespaceNode = node.Children.OfType<TypeNode>().First();
                RemoveFromParentIfEmpty(typeNamespaceNode);
                children.Remove(typeNamespaceNode);
            }
            if (!node.Children.Any())
            {
                node.Parent.Children.Remove(node);
                node.Parent = null;
            }
        }

        public class ProperlyNamedMatcher : CodePartsMatcher
        {
            public override bool Matches(IMethodReference method)
            {
                return true;
            }

            public override bool Matches(ITypeReference typeReference)
            {
                INamedTypeReference named = typeReference as INamedTypeReference;
                return named != null
                       && !named.Name.Value.StartsWith("<")
                       && !named.Name.Value.Contains("=");
            }
        }

    }
}