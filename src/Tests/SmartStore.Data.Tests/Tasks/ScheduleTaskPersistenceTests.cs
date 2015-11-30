using System;
using SmartStore.Core.Domain.Tasks;
using SmartStore.Tests;
using NUnit.Framework;

namespace SmartStore.Data.Tests.Tasks
{
    [TestFixture]
    public class ScheduleTaskPersistenceTests : PersistenceTest
    {
        [Test]
        public void Can_save_and_load_scheduleTask()
        {
            var scheduleTask = new ScheduleTask
                               {
                                   Name = "Task 1",
                                   CronExpression = "* * * * *",
                                   Type = "some type 1",
                                   Enabled = true,
                                   StopOnError = true,
                                   LastStartUtc = new DateTime(2010, 01, 01),
                                   LastEndUtc = new DateTime(2010, 01, 02),
                                   LastSuccessUtc= new DateTime(2010, 01, 03),
                               };

            var fromDb = SaveAndLoadEntity(scheduleTask);
            fromDb.ShouldNotBeNull();
            fromDb.Name.ShouldEqual("Task 1");
			fromDb.CronExpression.ShouldEqual("* * * * *");
            fromDb.Type.ShouldEqual("some type 1");
            fromDb.Enabled.ShouldEqual(true);
            fromDb.StopOnError.ShouldEqual(true);
            fromDb.LastStartUtc.ShouldEqual(new DateTime(2010, 01, 01));
            fromDb.LastEndUtc.ShouldEqual(new DateTime(2010, 01, 02));
            fromDb.LastSuccessUtc.ShouldEqual(new DateTime(2010, 01, 03));
        }
    }
}