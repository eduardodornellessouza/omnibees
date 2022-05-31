using Microsoft.VisualStudio.TestTools.UnitTesting;
using OB.BL.Operations.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OB.BL.Operations.Test.POCOS
{
    [TestClass]
    public class DatePeriodHelperTest
    {
        [TestMethod, TestCategory("DatePeriodHelper")]
        public void TestCompressDatesIntoIntervalsEmpty()
        {
            var dates = new List<DateTime>();
            var result = DatePeriodHelper.CompressDatesIntoIntervals(dates);

            Assert.IsTrue(result.Count == 0);
        }

        [TestMethod, TestCategory("DatePeriodHelper")]
        public void TestCompressDatesIntoIntervalsOneElement()
        {
            var dates = new List<DateTime>();
            dates.Add(DateTime.Now.Date);
            var result = DatePeriodHelper.CompressDatesIntoIntervals(dates);

            
            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result[0].Item1 == result[0].Item2);
        }

        [TestMethod, TestCategory("DatePeriodHelper")]
        public void TestCompressDatesIntoIntervalsTwoElementsContinuous()
        {
            var dates = new List<DateTime>();
            dates.Add(DateTime.Now.Date);
            dates.Add(DateTime.Now.Date.Add(TimeSpan.FromDays(1)));
            var result = DatePeriodHelper.CompressDatesIntoIntervals(dates);


            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result[0].Item1.Add(TimeSpan.FromDays(1)) == result[0].Item2);
        }

        [TestMethod, TestCategory("DatePeriodHelper")]
        public void TestCompressDatesIntoIntervalsTwoElementsWithBreaks()
        {
            var dates = new List<DateTime>();
            dates.Add(DateTime.Now.Date);
            dates.Add(DateTime.Now.Date.Add(TimeSpan.FromDays(2)));
            var result = DatePeriodHelper.CompressDatesIntoIntervals(dates);


            Assert.IsTrue(result.Count == 2);
            Assert.IsTrue(result[0].Item1 == result[0].Item2);
            Assert.IsTrue(result[1].Item1 == result[1].Item2 && result[0].Item1 != result[1].Item1);
        }


        [TestMethod, TestCategory("DatePeriodHelper")]
        public void TestCompressDatesIntoIntervalsMultipleElementsSameDate()
        {
            var dates = new List<DateTime>();
            dates.Add(DateTime.Now.Date);
            dates.Add(DateTime.Now.Date);
            var result = DatePeriodHelper.CompressDatesIntoIntervals(dates);


            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result[0].Item1 == result[0].Item2);
        }

        [TestMethod, TestCategory("DatePeriodHelper")]
        public void TestCompressDatesIntoIntervalsMultipleElementsOdd()
        {
            var dates = new List<DateTime>();
            dates.Add(DateTime.Now.Date);
            dates.Add(DateTime.Now.Date.Add(TimeSpan.FromDays(1)));
            dates.Add(DateTime.Now.Date.Add(TimeSpan.FromDays(2)));
            var result = DatePeriodHelper.CompressDatesIntoIntervals(dates);


            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result[0].Item1.Add(TimeSpan.FromDays(2)) == result[0].Item2);
        }

        [TestMethod, TestCategory("DatePeriodHelper")]
        public void TestCompressDatesIntoIntervalsMultipleElementsEven()
        {
            var dates = new List<DateTime>();
            dates.Add(DateTime.Now.Date);
            dates.Add(DateTime.Now.Date.Add(TimeSpan.FromDays(1)));
            dates.Add(DateTime.Now.Date.Add(TimeSpan.FromDays(2)));
            dates.Add(DateTime.Now.Date.Add(TimeSpan.FromDays(3)));
            var result = DatePeriodHelper.CompressDatesIntoIntervals(dates);


            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result[0].Item1.Add(TimeSpan.FromDays(3)) == result[0].Item2);
        }

        [TestMethod, TestCategory("DatePeriodHelper")]
        public void TestCompressDatesIntoIntervalsMultipleElementsWithBreaks()
        {
            var dates = new List<DateTime>();
            dates.Add(DateTime.Now.Date);
            dates.Add(DateTime.Now.Date.Add(TimeSpan.FromDays(1)));
            dates.Add(DateTime.Now.Date.Add(TimeSpan.FromDays(5)));
            dates.Add(DateTime.Now.Date.Add(TimeSpan.FromDays(6)));
            var result = DatePeriodHelper.CompressDatesIntoIntervals(dates);


            Assert.IsTrue(result.Count == 2);
            Assert.IsTrue(result[0].Item1.Add(TimeSpan.FromDays(1)) == result[0].Item2);
            Assert.IsTrue(result[1].Item1.Add(TimeSpan.FromDays(1)) == result[1].Item2);
        }

    }
}
