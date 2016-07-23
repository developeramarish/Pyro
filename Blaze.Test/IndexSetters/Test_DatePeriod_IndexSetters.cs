﻿using System;
using NUnit.Framework;
using Blaze.DataModel.IndexSetter;
using Blaze.DataModel.DatabaseModel.Base;
using Hl7.Fhir.Model;
using NUnit.Framework.Constraints;

namespace Blaze.Test.IndexSetters
{
  [TestFixture]
  class Test_DatePeriod_IndexSetters
  {
    [Test]
    public void Test_Period_DatePeriodIndexSetter_GoodFormat()
    {
      //Arrange
      var Period = new Period();
      var LowDateTimeOffSet = new DateTimeOffset(1974, 12, 25, 10, 35, 45, new TimeSpan(-5, 00, 00));
      var HighDateTimeOffSet = new DateTimeOffset(1974, 12, 25, 11, 35, 45, new TimeSpan(-5, 00, 00));
      Period.Start = "1974-12-25T10:35:45-05:00";
      Period.End = "1974-12-25T11:35:45-05:00";
      DatePeriodIndex Index = new DatePeriodIndex();

      //Act
      Index = IndexSetterFactory.Create(typeof(DatePeriodIndex)).Set(Period, Index) as DatePeriodIndex;

      //Assert
      Assert.AreEqual(Index.DateTimeOffsetLow, LowDateTimeOffSet);
      Assert.AreEqual(Index.DateTimeOffsetHigh, HighDateTimeOffSet);
    }

    [Test]
    public void Test_Period_DatePeriodIndexSetter_LowIsNull()
    {
      //Arrange
      var Period = new Period();      
      var HighDateTimeOffSet = new DateTimeOffset(1974, 12, 25, 11, 35, 45, new TimeSpan(-5, 00, 00));
      Period.StartElement = null;
      Period.End = "1974-12-25T11:35:45-05:00";
      DatePeriodIndex Index = new DatePeriodIndex();

      //Act
      Index = IndexSetterFactory.Create(typeof(DatePeriodIndex)).Set(Period, Index) as DatePeriodIndex;

      //Assert
      Assert.IsNull(Index.DateTimeOffsetLow);
      Assert.AreEqual(Index.DateTimeOffsetHigh, HighDateTimeOffSet);
    }

    [Test]
    public void Test_Period_DatePeriodIndexSetter_HighIsNull()
    {
      //Arrange
      var Period = new Period();
      var LowDateTimeOffSet = new DateTimeOffset(1974, 12, 25, 10, 35, 45, new TimeSpan(-5, 00, 00));

      Period.Start = "1974-12-25T10:35:45-05:00";
      Period.EndElement = null;
      DatePeriodIndex Index = new DatePeriodIndex();

      //Act
      Index = IndexSetterFactory.Create(typeof(DatePeriodIndex)).Set(Period, Index) as DatePeriodIndex;

      //Assert      
      Assert.AreEqual(Index.DateTimeOffsetLow, LowDateTimeOffSet);
      Assert.IsNull(Index.DateTimeOffsetHigh);
    }


    [Test]
    public void Test_Period_DatePeriodIndexSetter_LowAndHighAreNull()
    {
      //Arrange
      var Period = new Period();
      var LowDateTimeOffSet = new DateTimeOffset(1974, 12, 25, 10, 35, 45, new TimeSpan(-5, 00, 00));

      Period.StartElement = null;
      Period.EndElement = null;
      DatePeriodIndex Index = new DatePeriodIndex();

      //Act
      Index = IndexSetterFactory.Create(typeof(DatePeriodIndex)).Set(Period, Index) as DatePeriodIndex;

      //Assert            
      Assert.IsNull(Index);
    }


    [Test]
    public void Test_Period_DatePeriodIndexSetter_NullReferance()
    {
      //Arrange      
      Period Period = null;
      DatePeriodIndex Index = new DatePeriodIndex();

      //Act      
      ActualValueDelegate<DatePeriodIndex> testDelegate = () => IndexSetterFactory.Create(typeof(DatePeriodIndex)).Set(Period, Index) as DatePeriodIndex;

      //Assert
      Assert.That(testDelegate, Throws.TypeOf<ArgumentNullException>());
    }


    [Test]
    public void Test_Period_DatePeriodIndexSetter_BadLowFormat()
    {
      //Arrange
      var Period = new Period();
      var LowDateTimeOffSet = new DateTimeOffset(1974, 12, 25, 10, 35, 45, new TimeSpan(-5,00,00));
      var HighDateTimeOffSet = new DateTimeOffset(1974, 12, 25, 11, 35, 45, new TimeSpan(-5, 00, 00));
      Period.Start = "1974-12-25TXXX10:35:45-05:00";
      Period.End = "1974-12-25T11:35:45-05:00";
      DatePeriodIndex Index = new DatePeriodIndex();

      //Act      
      ActualValueDelegate<DatePeriodIndex> testDelegate = () => IndexSetterFactory.Create(typeof(DatePeriodIndex)).Set(Period, Index) as DatePeriodIndex;

      //Assert
      Assert.That(testDelegate, Throws.TypeOf<FormatException>());      
    }

    [Test]
    public void Test_Period_DatePeriodIndexSetter_BadHighStringFormat()
    {
      //Arrange
      var Period = new Period();
      var LowDateTimeOffSet = new DateTimeOffset(1974, 12, 25, 10, 35, 45, new TimeSpan(-5, 00, 00));
      var HighDateTimeOffSet = new DateTimeOffset(1974, 12, 25, 11, 35, 45, new TimeSpan(-5, 00, 00));
      Period.Start = "1974-12-25T10:35:45-05:00";
      Period.End = "1974-12-25T11XXX:35:45-05:00";
      DatePeriodIndex Index = new DatePeriodIndex();

      //Act      
      ActualValueDelegate<DatePeriodIndex> testDelegate = () => IndexSetterFactory.Create(typeof(DatePeriodIndex)).Set(Period, Index) as DatePeriodIndex;
     
      //Assert
      Assert.That(testDelegate, Throws.TypeOf<FormatException>());
    }

    [Test]
    public void Test_Period_DatePeriodIndexSetter_BadStringFormat()
    {
      //Arrange
      var Period = new Period();
      Period.Start = "1974-12-25XXXT10:35:45-05:00";
      Period.End = "1974-12-25T11XXX:35:45-05:00";
      DatePeriodIndex Index = new DatePeriodIndex();

      //Act
      ActualValueDelegate<DatePeriodIndex> testDelegate = () => IndexSetterFactory.Create(typeof(DatePeriodIndex)).Set(Period, Index) as DatePeriodIndex; ;

      //Assert
      Assert.That(testDelegate, Throws.TypeOf<FormatException>());      
    }

    [Test]
    public void Test_Timing_DatePeriodIndexSetter_GoodFormat()
    {
      //Arrange
      var Timing = new Timing();
      DatePeriodIndex Index = new DatePeriodIndex();

      var LowDate = new DateTimeOffset(1974, 12, 25, 10, 00, 00, new TimeSpan(-5, 00, 00));
      var HighDate = new DateTimeOffset(1974, 12, 26, 11, 10, 00, new TimeSpan(-5, 00, 00));

      Timing.EventElement = new System.Collections.Generic.List<FhirDateTime>();
      var EventStart1 = new FhirDateTime(new DateTimeOffset(1974, 12, 26, 11, 00, 00, new TimeSpan(-5, 00, 00)));
      var EventStart2 = new FhirDateTime(LowDate);
      
      Timing.EventElement.Add(EventStart1);
      Timing.EventElement.Add(EventStart2);


      Timing.Repeat = new Timing.RepeatComponent();      
      Timing.Repeat.Duration = new decimal(10.0);
      Timing.Repeat.DurationUnit = Timing.UnitsOfTime.Min;

      //Calculation (ToDo: This still needs review)
      //2min Duration + last EventStartdate (11:00am) = 11:20am 26/12/1974

      
      //Act
      Index = IndexSetterFactory.Create(typeof(DatePeriodIndex)).Set(Timing, Index) as DatePeriodIndex;

      //Assert      
      Assert.AreEqual(Index.DateTimeOffsetLow, LowDate);
      Assert.AreEqual(Index.DateTimeOffsetHigh, HighDate);
    }

    [Test]
    public void Test_Timing_DatePeriodIndexSetter_HighIsNull()
    {
      //Arrange
      var Timing = new Timing();
      DatePeriodIndex Index = new DatePeriodIndex();

      var LowDate = new DateTimeOffset(1974, 12, 25, 10, 00, 00, new TimeSpan(-5, 00, 00));
      var HighDate = new DateTimeOffset(1974, 12, 26, 11, 10, 00, new TimeSpan(-5, 00, 00));

      Timing.EventElement = new System.Collections.Generic.List<FhirDateTime>();
      var EventStart1 = new FhirDateTime(new DateTimeOffset(1974, 12, 26, 11, 00, 00, new TimeSpan(-5, 00, 00)));
      var EventStart2 = new FhirDateTime(LowDate);

      Timing.EventElement.Add(EventStart1);
      Timing.EventElement.Add(EventStart2);

      //Act
      Index = IndexSetterFactory.Create(typeof(DatePeriodIndex)).Set(Timing, Index) as DatePeriodIndex;

      //Assert      
      Assert.AreEqual(Index.DateTimeOffsetLow, LowDate);
      Assert.IsNull(Index.DateTimeOffsetHigh);
    }

    [Test]
    public void Test_Timing_DatePeriodIndexSetter_NoLowDate()
    {
      //Arrange
      var Timing = new Timing();
      DatePeriodIndex Index = new DatePeriodIndex();

      Timing.EventElement = new System.Collections.Generic.List<FhirDateTime>();
  
      Timing.Repeat = new Timing.RepeatComponent();
      Timing.Repeat.Duration = new decimal(10.0);
      Timing.Repeat.DurationUnit = Timing.UnitsOfTime.Min;

      //Act
      Index = IndexSetterFactory.Create(typeof(DatePeriodIndex)).Set(Timing, Index) as DatePeriodIndex;

      //Assert            
      Assert.IsNull(Index);
    }

    [Test]
    public void Test_Timing_DatePeriodIndexSetter_TimingIsNull()
    {
      //Arrange
      Timing Timing = null;
      DatePeriodIndex Index = new DatePeriodIndex();

      //Act      
      ActualValueDelegate<DatePeriodIndex> testDelegate = () => IndexSetterFactory.Create(typeof(DatePeriodIndex)).Set(Timing, Index) as DatePeriodIndex;

      //Assert
      Assert.That(testDelegate, Throws.TypeOf<ArgumentNullException>());
    }



  }
}
