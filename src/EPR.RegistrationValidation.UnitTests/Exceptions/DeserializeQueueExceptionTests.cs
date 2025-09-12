namespace EPR.RegistrationValidation.UnitTests.Exceptions;

using System;
using System.Reflection;
using System.Runtime.Serialization;
using EPR.RegistrationValidation.Application.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class DeserializeQueueExceptionTests
{
    [TestMethod]
    public void Exception_DeserializationConstructor_Should_Set_Message()
    {
        // Arrange
#pragma warning disable SYSLIB0050 // Type or member is obsolete
        var info = new SerializationInfo(typeof(DeserializeQueueException), new FormatterConverter());
#pragma warning restore SYSLIB0050 // Type or member is obsolete
        info.AddValue("ClassName", typeof(DeserializeQueueException).FullName);
        info.AddValue("Message", "Deserialization error");
        info.AddValue("InnerException", null, typeof(Exception));
        info.AddValue("HelpURL", null);
        info.AddValue("StackTraceString", null);
        info.AddValue("RemoteStackTraceString", null);
        info.AddValue("RemoteStackIndex", 0);
        info.AddValue("ExceptionMethod", null);
        info.AddValue("HResult", -2146233088); // default HRESULT
        info.AddValue("Source", null);

        var context = default(StreamingContext);

        // Act
        var exception = (DeserializeQueueException)Activator.CreateInstance(
            typeof(DeserializeQueueException),
            BindingFlags.Instance | BindingFlags.NonPublic,
            binder: null,
            args: new object[] { info, context },
            culture: null);

        // Assert
        Assert.IsNotNull(exception);
        Assert.AreEqual("Deserialization error", exception.Message);
    }
}