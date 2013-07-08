using System;
using SmartStore.Core.Domain.Localization;

namespace SmartStore.Services.Installation
{
    public partial interface IInstallationService
    {
		void InstallEarlyRequiredData();
        void InstallData(InstallDataContext context /* codehint: sm-edit */);
    }
}
