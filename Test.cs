using NUnit.Framework;
using GoogleTranslateAPI;
using GoogleTranslateAPI.Controllers;
using GoogleTranslateAPI.Security;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography.X509Certificates;

namespace UnitTests
{
    [TestFixture]
    public class Test
    {
        private TranslateController controller;
        private CertificateValidation certificateValidation;

        private string rootCertificateName = "ca.iq-x.co.crt";

        public Test()
        {
            ILoggerFactory loggerFactory = new LoggerFactory().AddConsole().AddDebug();
            ILogger<TranslateController> logger = loggerFactory.CreateLogger<TranslateController>();

            controller = new TranslateController(logger);

            certificateValidation = new CertificateValidation(rootCertificateName);
        }

        [TestCase("Hello World", "en", "es")] // Valid entry
        [TestCase("Hello World", "en", "en")] // Same language error
        [TestCase("Hello World", "", "es")] // No source language error
        [TestCase("Hello World", "en", "")] // No target language error
        [TestCase("Hello World", "", "")] // No source or target language error
        [TestCase("", "en", "es")] // No query error
        [TestCase("", "en", "en")] // No query, same language error
        [TestCase("", "", "es")] // No query or source language error
        [TestCase("", "en", "")] // No query or target language error
        [TestCase("", "", "")] // No query, source languaage, or target langauge error
        [TestCase("Hello World", "test", "es")] // Invalid source language error
        [TestCase("Hello World", "en", "test")] // Invalid target language error
        [TestCase("Hello World", "test", "test")] // Invalid source and target language error
        [TestCase("Hello World", "test", "")] // Invalid source language and no target language error
        [TestCase("Hello World", "", "test")] // No source language and invalid target language error
        [TestCase("", "test", "es")] // No query, invalid source language error
        [TestCase("", "en", "test")] // No query, invalid target langauge error
        [TestCase("", "test", "")] // No query, invalid source language, no target language error
        [TestCase("", "", "test")] // No query, no source language, invalid target language error
        [TestCase("", "test", "test")] // No query, invalid source language, invalid target language error        
        public void TestAPI(string query, string source, string target) // Should never error out. Should always return SOMETHING to the client
        {
            string result = controller.TranslateQuery(query, source, target).Result;

            var boolResult = false;
            if (result != null)
                boolResult = true;

            System.Console.WriteLine("RESULT IS HERE: " + result);
            Assert.IsTrue(boolResult, result);
        }

        [TestCase(false, "ExpiredCertificateTest.pfx")]
        [TestCase(true, "translate.iq-x.co.pfx")]
        public void CertificateValidationTest(bool expectedResult, string certificateName)
        {
            
            X509Certificate2 clientCertificate = new X509Certificate2(certificateName, "P4$$w0RD");
            
            bool result = (certificateValidation.ClientCertificateValidation(clientCertificate, null, 0) == expectedResult);

            if (expectedResult)
                Assert.IsTrue(result, "Certificate validation failed!");
            else
                Assert.IsTrue(result, "Certificate validation passed on an invalid certificate!");
        }
    }
}
