/*
****************************************************************************
*  Copyright (c) 2023,  Skyline Communications NV  All Rights Reserved.    *
****************************************************************************

By using this script, you expressly agree with the usage terms and
conditions set out below.
This script and all related materials are protected by copyrights and
other intellectual property rights that exclusively belong
to Skyline Communications.

A user license granted for this script is strictly for personal use only.
This script may not be used in any way by anyone without the prior
written consent of Skyline Communications. Any sublicensing of this
script is forbidden.

Any modifications to this script by the user are only allowed for
personal use and within the intended purpose of the script,
and will remain the sole responsibility of the user.
Skyline Communications will not be responsible for any damages or
malfunctions whatsoever of the script resulting from a modification
or adaptation by the user.

The content of this script is confidential information.
The user hereby agrees to keep this confidential information strictly
secret and confidential and not to disclose or reveal it, in whole
or in part, directly or indirectly to any person, entity, organization
or administration without the prior written consent of
Skyline Communications.

Any inquiries can be addressed to:

	Skyline Communications NV
	Ambachtenstraat 33
	B-8870 Izegem
	Belgium
	Tel.	: +32 51 31 35 69
	Fax.	: +32 51 31 01 29
	E-mail	: info@skyline.be
	Web		: www.skyline.be
	Contact	: Ben Vandenberghe

****************************************************************************
Revision History:

DATE		VERSION		AUTHOR			COMMENTS

06/06/2023	1.0.0.1		ACA, Skyline	Initial version
****************************************************************************
*/

using Newtonsoft.Json;
using Skyline.DataMiner.Automation;
using Skyline.DataMiner.Net.Apps.UserDefinableApis.Actions;
using System.Linq;
using Skyline.DataMiner.CommunityLibrary.Utility.Automation;
using System.Collections.Generic;
using Skyline.DataMiner.CommunityLibrary.Utility;
using Skyline.DataMiner.Net.Apps.UserDefinableApis;

namespace UserDefinableApiScripts.Examples.ExistingWithEntryPoint
{
	public class Script
	{

		public enum ECSParameters
		{
			WIPTable = 1600,
		}

		[AutomationEntryPoint(AutomationEntryPointType.Types.OnApiTrigger)]
		public ApiTriggerOutput OnApiTrigger(IEngine iengine, ApiTriggerInput requestData)
		{
			Engine engine = (Engine)iengine;

			// Find Elements
			Element[] ecsElements = engine.FindElementsByProtocol("Skyline ECS Agile Metrics");
			ecsElements = ecsElements.Where(x => x.ProtocolVersion == "Production").ToArray();

			List<WIPTask> wipTasks = new List<WIPTask>();
			uint[] tableIDXs = new uint[] { 0, 1, 2, 3, 7, 8, 9, 10, 28, 29 };

			foreach (var ecsElement in ecsElements)
			{
				string squadName = ecsElement.ElementName.Split(' ').LastOrDefault();

				List<WIPTask> squadTasks = engine.GetColumns(
					ecsElement.DmaId,
					ecsElement.ElementId,
					1600,
					tableIDXs,
					(string id, string title, double cycleTime, double leadTime, double inprogress, double investigation, double qualityassurance, double codereview, string currentStatus, string assignee) =>
					{
						return new WIPTask
						{
							ID = id,
							Title = title,
							CycleTime = cycleTime,
							LeadTime = leadTime,
							InProgress = inprogress,
							Investigation = investigation,
							QualityAssurance = qualityassurance,
							CodeReview = codereview,
							CurrentStatus = currentStatus,
							Assignee = assignee,
						};
					}).ToList();

				foreach (var squadTask in squadTasks)
				{
					squadTask.SquadName = squadName;
					squadTask.CollaborationURL = string.Format("https://collaboration.skyline.be/task/{0}", squadTask.ID);
				}

				wipTasks.AddRange(squadTasks);
			}

			string results = JsonConvert.SerializeObject(wipTasks);

			return new ApiTriggerOutput()
			{
				ResponseBody = results,
				ResponseCode = (int)StatusCode.Ok,
			};
		}


		public void Run(Engine engine)
		{ }

		
	}

	public class WIPTask
	{
		public string ID { get; set; }
		public string Title { get; set; }
		public double CycleTime { get; set; }
		public double LeadTime { get; set; }
		public double InProgress { get; set; }
		public double QualityAssurance { get; set; }
		public double CodeReview { get; set; }
		public double Investigation { get; set; }
		public double WaitingTime { get; set; }
		public string CurrentStatus { get; set; }
		public string Assignee { get; set; }
		public string CollaborationURL { get; set; }
		public string SquadName { get; set; }
	}
}