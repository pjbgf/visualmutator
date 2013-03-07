﻿namespace VisualMutator.Model
{
    #region Usings

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;

    using CommonUtilityInfrastructure;
    using Decompilation;
    using Decompilation.CodeDifference;
    using Mono.Cecil;
    using Mutations.MutantsTree;
    using StoringMutants;
    using VisualMutator.Model.Mutations;
    using VisualMutator.Model.Tests.TestsTree;

    #endregion

    public class XmlResultsGenerator
    {
        private readonly ICodeDifferenceCreator _codeDifferenceCreator;

        private readonly IAssembliesManager _assembliesManager;

        public XmlResultsGenerator(
            ICodeDifferenceCreator codeDifferenceCreator,
            IAssembliesManager assembliesManager)
        {
            _codeDifferenceCreator = codeDifferenceCreator;
            _assembliesManager = assembliesManager;
        }

        public XDocument GenerateResults(MutationTestingSession session, 
            bool includeDetailedTestResults,
            bool includeCodeDifferenceListings)
        {
            List<Mutant> mutants = session.MutantsGroupedByOperators.SelectMany(op => op.Mutants).ToList();
            List<Mutant> mutantsWithErrors = mutants.Where(m => m.State == MutantResultState.Error).ToList();
            List<Mutant> testedMutants = mutants.Where(m => m.MutantTestSession.IsComplete).ToList();
            List<Mutant> live = testedMutants.Where(m => m.State == MutantResultState.Live).ToList();

            var mutantsNode = new XElement("Mutants",
                new XAttribute("Total", mutants.Count),
                new XAttribute("Live", live.Count),
                new XAttribute("Killed", testedMutants.Count - live.Count),
                new XAttribute("Untested", mutants.Count - testedMutants.Count),
                new XAttribute("WithError", mutantsWithErrors.Count),
                new XAttribute("TotalSizeKilobytes", -1),//mutants.Sum(mut => mut.StoredAssemblies.SizeInKilobytes())),
                new XAttribute("AverageSizeKilobytes", -1),//mutants.AverageOrZero(mut => mut.StoredAssemblies.SizeInKilobytes())),
                new XAttribute("FindingMutationTargetsTotalTimeMiliseconds", session.MutantsGroupedByOperators
                    .Sum(oper => oper.FindTargetsTimeMiliseconds)),
                new XAttribute("TotalMutationTimeMiliseconds", session.MutantsGroupedByOperators
                    .Sum(oper => oper.MutationTimeMiliseconds)),
                new XAttribute("AverageMutationTimePerOperatorMiliseconds", session.MutantsGroupedByOperators
                    .AverageOrZero(oper => oper.MutationTimeMiliseconds)),
                new XAttribute("TotalTestingTimeMiliseconds", testedMutants
                    .Sum(mut => mut.MutantTestSession.TestingTimeMiliseconds)),
                new XAttribute("AverageTestingTimeMiliseconds", testedMutants
                    .AverageOrZero(mut => mut.MutantTestSession.TestingTimeMiliseconds)),
                from oper in session.MutantsGroupedByOperators
                select new XElement("Operator",
                    new XAttribute("Identificator", oper.Identificator),
                    new XAttribute("Name", oper.Name),
                    new XAttribute("NumberOfMutants", oper.Children.Count),
                    new XAttribute("NumberOfKilledMutants", oper.Mutants.Count(m=>m.State == MutantResultState.Killed)),
                    new XAttribute("NumberOfLiveMutants", oper.Mutants.Count(m => m.State == MutantResultState.Live)),
                    new XAttribute("FindingMutationTargetsTimeMiliseconds", oper.FindTargetsTimeMiliseconds),
                    new XAttribute("TotalMutantsCreationTimeMiliseconds", oper.MutationTimeMiliseconds),
                    new XAttribute("AverageMutantCreationTimeMiliseconds", oper.Children.Count == 0 ? 0 :
                        oper.MutationTimeMiliseconds / oper.Children.Count),
                    from mutant in oper.Mutants
                    select new XElement("Mutant",
                        new XAttribute("Id", mutant.Id),
                        //    new XAttribute("SizeInKilobytes", mutant.StoredAssemblies.SizeInKilobytes()),
                        new XAttribute("State", Switch.Into<string>().From(mutant.State)
                        .Case(MutantResultState.Killed, "Killed")
                        .Case(MutantResultState.Live, "Live")
                        .Case(MutantResultState.Untested, "Untested")
                        .Case(MutantResultState.Error, "WithError")
                        .GetResult()
                        ),
                        new XElement("ErrorInfo",
                             new XElement("Description", mutant.MutantTestSession.ErrorDescription),
                             new XElement("ExceptionMessage", mutant.MutantTestSession.ErrorMessage)
                             ).InArrayIf(mutant.State == MutantResultState.Error)
                          


                        )
                    )
                );


            var optionalElements = new List<XElement>();

            if (includeCodeDifferenceListings)
            {

                optionalElements.Add(CreateCodeDifferenceListings(mutants, 
                    _assembliesManager.Load(session.StoredSourceAssemblies)));
            }

            if (includeDetailedTestResults)
            {
                optionalElements.Add(CreateDetailedTestingResults(mutants));
            }

            long totalTimeMs = session.MutantsGroupedByOperators.Sum(oper => oper.MutationTimeMiliseconds)
                            + mutants.Sum(m => m.MutantTestSession.TestingTimeMiliseconds);
            return
                new XDocument(
                    new XElement("MutationTestingSession",
                        new XAttribute("MutationScore", session.MutationScore),
                        new XAttribute("TotalTimeSeconds", totalTimeMs/1000),
                        mutantsNode,
                        optionalElements));
            
        }
        public XElement CreateCodeDifferenceListings(List<Mutant> mutants, AssembliesProvider originalAssemblies)
        {

            return new XElement("CodeDifferenceListings",
                from mutant in mutants
                select new XElement("MutantCodeListing",
                    new XAttribute("MutantId", mutant.Id),
                    new XElement("Code", 
                        Environment.NewLine+_codeDifferenceCreator.CreateDifferenceListing(CodeLanguage.CSharp, 
                        mutant, originalAssemblies).Code)
                        )
                    );

        }

        public XElement CreateDetailedTestingResults(List<Mutant> mutants)
        {
            return new XElement("DetailedTestingResults",   
                from mutant in mutants
                where mutant.MutantTestSession.IsComplete
                let groupedTests = mutant.MutantTestSession.TestMap.Values.GroupBy(m => m.State).ToList()
                select new XElement("TestedMutant",
                    new XAttribute("MutantId", mutant.Id),
                    new XAttribute("TestingTimeMiliseconds", mutant.MutantTestSession.TestingTimeMiliseconds),
                    new XAttribute("LoadTestsTimeRawMiliseconds", mutant.MutantTestSession.LoadTestsTimeRawMiliseconds),
                    new XAttribute("RunTestsTimeRawMiliseconds", mutant.MutantTestSession.RunTestsTimeRawMiliseconds),
                    new XElement("Tests",
                    new XAttribute("NumberOfFailedTests", groupedTests.SingleOrDefault(g => g.Key == TestNodeState.Failure).ToEmptyIfNull().Count()),
                    new XAttribute("NumberOfPassedTests", groupedTests.SingleOrDefault(g => g.Key == TestNodeState.Success).ToEmptyIfNull().Count()),
                    new XAttribute("NumberOfInconlusiveTests", groupedTests.SingleOrDefault(g => g.Key == TestNodeState.Inconclusive).ToEmptyIfNull().Count()),
                    from testClass in mutant.MutantTestSession.TestNamespaces.SelectMany(n => n.TestClasses)
                    select new XElement("TestClass",
                        new XAttribute("Name", testClass.Name),
                        new XAttribute("FullName", testClass.FullName),
                        from testMethod in testClass.Children.Cast<TestNodeMethod>()
                        where testMethod.State == TestNodeState.Failure
                        select new XElement("TestMethod",
                            new XAttribute("Name", testMethod.Name),
                            new XAttribute("Outcome", "Failed"),
                            new XElement("Message", testMethod.Message)),
                        from testMethod in testClass.Children.Cast<TestNodeMethod>()
                        where testMethod.State.IsIn(TestNodeState.Success, TestNodeState.Inconclusive)
                        select new XElement("TestMethod",
                            new XAttribute("Name", testMethod.Name),
                            new XAttribute("Outcome", Functional.ValuedSwitch<TestNodeState, string>(testMethod.State)
                            .Case(TestNodeState.Success, "Passed")
                            .Case(TestNodeState.Inconclusive, "Inconclusive")
                            .GetResult())
                            )
                        )
                    )
                ));
        }

    }
}