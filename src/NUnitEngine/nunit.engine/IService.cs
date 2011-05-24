// ****************************************************************
// Copyright 2007, Charlie Poole
// This is free software licensed under the NUnit license. You may
// obtain a copy of the license at http://nunit.org
// ****************************************************************
using System;

namespace NUnit.Engine
{
	/// <summary>
	/// The IService interface is implemented by all Services.
	/// </summary>
	public interface IService
	{
        /// <summary>
        /// The ServiceContext
        /// </summary>
        ServiceContext ServiceContext { get; set; }

		/// <summary>
		/// Initialize the Service
		/// </summary>
		void InitializeService();

		/// <summary>
		/// Do any cleanup needed before terminating the service
		/// </summary>
		void UnloadService();
	}
}
