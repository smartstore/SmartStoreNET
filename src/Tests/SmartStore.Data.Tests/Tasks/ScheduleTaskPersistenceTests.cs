using System;
using NUnit.Framework;
using SmartStore.Core.Domain.Tasks;
using SmartStore.Tests;

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
                NextRunUtc = new DateTime(2020, 01, 02),
                RunPerMachine = true
            };

            var fromDb = SaveAndLoadEntity(scheduleTask);
            fromDb.ShouldNotBeNull();
            fromDb.Name.ShouldEqual("Task 1");
            fromDb.CronExpression.ShouldEqual("* * * * *");
            fromDb.Type.ShouldEqual("some type 1");
            fromDb.Enabled.ShouldEqual(true);
            fromDb.StopOnError.ShouldEqual(true);
            fromDb.NextRunUtc.ShouldEqual(new DateTime(2020, 01, 02));
            fromDb.RunPerMachine.ShouldEqual(true);
        }
    }
}