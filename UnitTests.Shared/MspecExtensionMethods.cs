﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using KellermanSoftware.CompareNetObjects;
using Machine.Specifications;
using Machine.Specifications.Annotations;
using Rhino.Mocks.Interfaces;

namespace UnitTests.Shared
{
    public static class MspecExtensionMethods
    {
        public static IMethodOptions<Task<HttpContent>> ReturnContentOf(this IMethodOptions<Task<HttpContent>> subject, string filePath)
        {
            HttpContent content = new ByteArrayContent(ReadContentasByteArray(filePath));
            return subject.Return(Task.FromResult(content));
        }
        public static IMethodOptions<Task<string>> ReturnContentOf(this IMethodOptions<Task<string>> subject, string filePath)
        {
            return subject.Return(Task.FromResult(ReadContent(filePath)));// ReturnContentOf(subject, filePath);
        }
        public static IMethodOptions<Task<byte[]>> ReturnContentAsByteArrayOf(this IMethodOptions<Task<byte[]>> subject, string filePath)
        {
            return subject.Return(Task.FromResult(ReadContentasByteArray(filePath)));// ReturnContentOf(subject, filePath);
        }

        public static byte[] ReadContentasByteArray(string filePath)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var dottedFilePath = filePath.Replace('\\', '.');
            var resourceName = "GeocachingToolbox.UnitTests." + dottedFilePath;
            byte[] content;

            try
            {
                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                //using (StreamReader reader = new StreamReader(stream))
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        stream.CopyTo(memoryStream);
                        content = memoryStream.ToArray();
                    }
                }

            }
            catch (ArgumentNullException)
            {
                throw new FileNotFoundException("Make sure that the file '" + filePath
                                                + "' exists and has 'Build Action' set to 'Embedded Resource'.");
            }
            return content;
        }

        public static string ReadContent(string filePath)
        {
            var assembly = Assembly.GetAssembly() GetExecutingAssembly();
            var dottedFilePath = filePath.Replace('\\', '.');
            var resourceName = "GeocachingToolbox.UnitTests." + dottedFilePath;
            string content = "";

            try
            {
                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                using (StreamReader reader = new StreamReader(stream))
                {
                    content = reader.ReadToEnd();
                }
            }
            catch (ArgumentNullException)
            {
                throw new FileNotFoundException("Make sure that the file '" + filePath
                                                + "' exists and has 'Build Action' set to 'Embedded Resource'.");
            }
            return content;
        }

        [AssertionMethod]
        public static void ShouldBeTrue([AssertionCondition(AssertionConditionType.IS_TRUE)] this bool condition, string message)
        {
            if (!condition)
                throw new SpecificationException(message);
        }

        [AssertionMethod]
        public static void ShouldEqualRecursively([AssertionCondition(AssertionConditionType.IS_TRUE)] this object subject, object compareWith)
        {
            var result = (new CompareLogic()).Compare(subject, compareWith);
            result.AreEqual.ShouldBeTrue(result.DifferencesString);
        }
    }
}
