using nanoFramework.HttpMultipartParser.Parts;
using nanoFramework.Json;
using nanoFramework.TestFramework;
using System;
using System.IO;
using System.Text;

namespace nanoFramework.HttpMultipartParser.Test
{
    [TestClass]
    public class FormDataTests
    {
        [TestMethod]
        public void FormWithParametersTest()
        {
            var stream = FormDataProvider.CreateFormWithParameters();

            var parser = MultipartFormDataParser.Parse(stream);

            Assert.IsNotNull(parser);

            var parameters = parser.Parameters;
            Assert.IsTrue(parameters.Length == 2);
            Assert.IsTrue(parameters[0].Name == "param1" && parameters[0].Data == "value1");
            Assert.IsTrue(parameters[1].Name == "param2" && parameters[1].Data == "value2");
        }

        [TestMethod]
        public void FormWithFileTest()
        {
            var stream = FormDataProvider.CreateFormWithFile();

            var parser = MultipartFormDataParser.Parse(stream);

            Assert.IsNotNull(parser);

            var parameters = parser.Parameters;
            Assert.IsTrue(parameters.Length == 0);

            var files = parser.Files;
            Assert.IsTrue(files.Length == 1);
            ValidateFile(files[0], "somefile.json", "Chuck Norris", 999);
        }

        [TestMethod]
        public void FormWithMultipleFilesTest()
        {
            var stream = FormDataProvider.CreateFormWithFiles();

            var parser = MultipartFormDataParser.Parse(stream);

            Assert.IsNotNull(parser);

            var parameters = parser.Parameters;
            Assert.IsTrue(parameters.Length == 0);

            var files = parser.Files;
            Assert.IsTrue(files.Length == 2);
            ValidateFile(files[0], "first.json", "Chuck Norris", 999);
            ValidateFile(files[1], "second.json", "Darth Vader", 9999);
        }

        [TestMethod]
        public void FormWithEverythingTest()
        {
            var stream = FormDataProvider.CreateFormWithEverything();

            var parser = MultipartFormDataParser.Parse(stream);

            Assert.IsNotNull(parser);

            var parameters = parser.Parameters;
            Assert.IsTrue(parameters.Length == 2);
            Assert.IsTrue(parameters[0].Name == "param1" && parameters[0].Data == "value1");
            Assert.IsTrue(parameters[1].Name == "param2" && parameters[1].Data == "value2");

            var files = parser.Files;
            Assert.IsTrue(files.Length == 2);
            ValidateFile(files[0], "first.json", "Chuck Norris", 999);
            ValidateFile(files[1], "second.json", "Darth Vader", 9999);
        }

        [TestMethod]
        public void FormWithLargeFileTest()
        {
            var fileIn = FormDataProvider.CreateContent(4096);
            var stream = FormDataProvider.CreateFormWithFile(fileIn);

            var parser = MultipartFormDataParser.Parse(stream);

            var parameters = parser.Parameters;
            Assert.IsTrue(parameters.Length == 0);

            var files = parser.Files;
            Assert.IsTrue(files.Length == 1);

            using var sr = new StreamReader(files[0].Data);
            var fileOut = sr.ReadToEnd();

            Assert.AreEqual(fileIn, fileOut);
        }

        [TestMethod]
        public void EmptyFormTest()
        {
            var stream = FormDataProvider.CreateEmptyForm();

            var parser = MultipartFormDataParser.Parse(stream, ignoreInvalidParts: true);
            Assert.IsNotNull(parser);

            Assert.ThrowsException(typeof(Exception), () => MultipartFormDataParser.Parse(stream));
        }

        [TestMethod]
        public void InvalidFormTest()
        {
            var stream = FormDataProvider.CreateInvalidForm();

            var parser = MultipartFormDataParser.Parse(stream, ignoreInvalidParts: true);
            Assert.IsNotNull(parser);

            Assert.ThrowsException(typeof(Exception), () => MultipartFormDataParser.Parse(stream));
        }

        private void ValidateFile(FilePart file, string filename, string personName, int personAge)
        {
            Assert.IsTrue(file.FileName == filename);
            var sr = new StreamReader(file.Data);
            var content = sr.ReadToEnd();

            var person = JsonConvert.DeserializeObject(content, typeof(Person)) as Person;
            Assert.IsNotNull(person);
            Assert.IsTrue(person.Name == personName && person.Age == personAge);
        }

        internal class Person
        {
            public string Name { get; set; }
            public int Age { get; set; }
        }
    }
}
