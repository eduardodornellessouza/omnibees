using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OB.BL.Operations.Helper.Masking.Guard;
using OB.BL.Operations.Helper.Masking.Interfaces;

namespace OB.BL.Operations.Test.Helper.Masking
{
    [TestClass]
    public class UnmaskGuardTest
    {
        [TestMethod]
        [TestCategory("Masking")]
        public void AsOriginal_Called()
        {
            var mockMaskedObject = new Mock<object>().As<IMasked<object>>();
            using (new UnmaskGuard<object>(mockMaskedObject.Object))
            {
                mockMaskedObject.Verify(m => m.AsOriginal(), Times.Once);
                mockMaskedObject.VerifyNoOtherCalls();
            }
        }

        [TestMethod]
        [TestCategory("Masking")]
        public void AsMasked_Called()
        {
            var mockMaskedObject = new Mock<object>().As<IMasked<object>>();
            using (new UnmaskGuard<object>(mockMaskedObject.Object))
            {
                mockMaskedObject.Reset();
            }

            mockMaskedObject.Verify(m => m.AsMasked(), Times.Once);
            mockMaskedObject.VerifyNoOtherCalls();
        }

        [TestMethod]
        [TestCategory("Masking")]
        public void Lifecycle_NoExceptionWhenObjectIsNotIMasked()
        {
            using (new UnmaskGuard<int>(1))
            {

            }
        }
    }
}
