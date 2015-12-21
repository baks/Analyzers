using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using NUnit.Framework;

namespace Analyzers.Test
{
    [TestFixture]
    public class SutSetupBodyCreatorTests
    {
        private static readonly MetadataReference CorlibReference = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
        private static readonly MetadataReference SystemCoreReference = MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location);

        private SutSetupBodyCreator sut;
        private Document document;

        [Test]
        public void CreatesObjectCreationExpressionWhenTypeHasEmptyConstructor()
        {
            var source = @"public class SutClass
{
}

public class TestClass 
{
    private SutClass sut;
}";

            Assert.That(CreatedBody(source), Is.EqualTo("{sut=newSutClass();}"));
        }

        [Test]
        public void CreatesObjectCreationExpressionWhenTypeHasConstructorWithParameters()
        {
            var source = @"public interface ICollaborator {}
public interface IAnotherCollaborator {}

public class SutClass
{
    public SutClass(ICollaborator collaborator, IAnotherCollaborator anotherCollaborator)
    {
    }
}

public class TestClass 
{
    private SutClass sut;
}";

            Assert.That(CreatedBody(source),
                Is.EqualTo(
                    "{collaborator=Substitute.For<ICollaborator>();anotherCollaborator=Substitute.For<IAnotherCollaborator>();sut=newSutClass(collaborator,anotherCollaborator);}"));
        }

        private string CreatedBody(string source)
        {
            SetupSut(source);

            var root = document.GetSyntaxTreeAsync().Result;
            var compilationUnit = root.GetCompilationUnitRoot();
            compilationUnit.Accept(sut);

            return sut.Result.ToFullString();
        }

        private void SetupSut(string source)
        {
            var projectId = ProjectId.CreateNewId(debugName: "ObjectCreationExpressionTests");

            var solution = new AdhocWorkspace()
                .CurrentSolution
                .AddProject(projectId, "ObjectCreationExpressionTests", "ObjectCreationExpressionTests",
                    LanguageNames.CSharp)
                .AddMetadataReference(projectId, CorlibReference)
                .AddMetadataReference(projectId, SystemCoreReference);

            var newFileName = "Test.cs";
            var documentId = DocumentId.CreateNewId(projectId, debugName: newFileName);
            solution = solution.AddDocument(documentId, newFileName, SourceText.From(source));
            document = solution.GetDocument(documentId);
            sut = new SutSetupBodyCreator(document, "sut");
        }
    }
}
