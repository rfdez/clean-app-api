﻿using CleanApp.Core.Interfaces.Repositories;
using CleanApp.Infrastructure.Repositories.Auth;
using System;
using System.Threading.Tasks;

namespace CleanApp.Core.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IYearRepository YearRepository { get; }

        IMonthRepository MonthRepository { get; }

        IWeekRepository WeekRepository { get; }

        ITenantRepository TenantRepository { get; }

        IRoomRepository RoomRepository { get; }

        IJobRepository JobRepository { get; }

        IAuthenticationRepository AuthenticationRepository { get; }

        IHomeRepository HomeRepository { get; }

        IHomeTenantRepository HomeTenantRepository { get; }

        ICleanlinessRepository CleanlinessRepository { get; }

        void SaveChanges();

        Task SaveChangesAsync();
    }
}
