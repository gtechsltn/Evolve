﻿using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace Evolve.Tests.Infrastructure
{
    public abstract class DbContainerFixture<T> where T : IDbContainer, new()
    {
        protected readonly T _container = new T();

        public string CnxStr => _container.CnxStr;

        [SuppressMessage("Design", "CA1031: Do not catch general exception types")]
        public virtual void Run(bool fromScratch = false)
        {
            int retries = 1;
            bool isDbStarted = false;

            _container.Start(fromScratch);

            while (!isDbStarted)
            {
                if (retries > _container.TimeOutInSec)
                {
                    throw new Exception($"{typeof(T).Name} timed-out after {_container.TimeOutInSec} sec.");
                }

                Thread.Sleep(TimeSpan.FromSeconds(1));
                try
                {
                    using var cnn = CreateDbConnection();
                    cnn.Open();
                    isDbStarted = cnn.State == ConnectionState.Open;
                }
                catch { }
                retries++;
            }
        }

        public DbConnection CreateDbConnection() => _container.CreateDbConnection();

        public void Dispose() => _container.Dispose();
    }
}
