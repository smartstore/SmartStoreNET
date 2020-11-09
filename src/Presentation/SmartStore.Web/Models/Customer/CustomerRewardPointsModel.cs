using System;
using System.Collections.Generic;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Web.Models.Customer
{
    public partial class CustomerRewardPointsModel : ModelBase
    {
        public CustomerRewardPointsModel()
        {
            RewardPoints = new List<RewardPointsHistoryModel>();
        }

        public IList<RewardPointsHistoryModel> RewardPoints { get; set; }
        public string RewardPointsBalance { get; set; }

        #region Nested classes
        public partial class RewardPointsHistoryModel : EntityModelBase
        {
            [SmartResourceDisplayName("RewardPoints.Fields.Points")]
            public int Points { get; set; }

            [SmartResourceDisplayName("RewardPoints.Fields.PointsBalance")]
            public int PointsBalance { get; set; }

            [SmartResourceDisplayName("RewardPoints.Fields.Message")]
            public string Message { get; set; }

            [SmartResourceDisplayName("Common.CreatedOn")]
            public DateTime CreatedOn { get; set; }
        }

        #endregion
    }
}