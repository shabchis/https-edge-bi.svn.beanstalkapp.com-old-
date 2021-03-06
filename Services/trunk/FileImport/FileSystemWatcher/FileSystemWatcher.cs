﻿using System;
using System.Collections.Generic;
using System.IO;
using Easynet.Edge.Core.Configuration;
using Easynet.Edge.Core.Services;
using Easynet.Edge.Core.Utilities;
using Easynet.Edge.Core.Scheduling;
using Easynet.Edge.Core;
using Easynet.Edge.Core.Data;
using Easynet.Edge.Services.FileImport.Configuration;


namespace Easynet.Edge.Services.FileImport
{
	
    public class FileSystemWatcherService: Service
	{
		#region Members
		/*=========================*/

		string _scheduleManagerUrl;
		string _scheduleManagerConfiguration;

		bool _loaded = false;
		Dictionary<string, FileSystemWatcher> _watchers = new Dictionary<string, FileSystemWatcher>();
		Dictionary<FileSystemWatcher, string> _handlers = new Dictionary<FileSystemWatcher, string>();
		
		/*=========================*/
		#endregion

		#region Service implementation
		/*=========================*/
		
		protected override ServiceOutcome DoWork()
		{
			if (!_loaded)
			{
				if (!Instance.Configuration.Options.TryGetValue("ScheduleManagerUrl", out _scheduleManagerUrl))
					_scheduleManagerUrl = null;
				if (!Instance.Configuration.Options.TryGetValue("ScheduleManagerConfiguration", out _scheduleManagerConfiguration))
					_scheduleManagerConfiguration = null;

				if (Instance.Configuration.ExtendedElements.ContainsKey("Directories"))
				{
					foreach (DirectoryElement dir in (DirectoryElementCollection) Instance.Configuration.ExtendedElements["Directories"])
					{
						try
						{
							this.Add(dir.Path, dir.Filter, dir.IncludeSubdirs, dir.HandlerService);
						}
						catch (Exception ex)
						{
							Log.Write(String.Format("Failed to add watcher for {0}.", dir.Path), ex);
						}
					}
				}
				
				if (_watchers.Count < 1)
				{
					throw new Exception("No directories are watched by the file system watcher. Check the log for more information.");
				}

				_loaded = true;
			}

			// Don't return
			return ServiceOutcome.Unspecified;
		}
	
        private void Add(string path, string filter, bool includeSubdirs, string handlerService)
        {
			// EXCEPTION:
            if (String.IsNullOrEmpty(path) || !Directory.Exists(path))
                throw new Exception("Invalid path, path cannot be empty, or path does not exist.");

			// EXCEPTION:
			if (_watchers.ContainsKey(path))
                throw new Exception("Already monitoring path: " + path);

            FileSystemWatcher watcher = new FileSystemWatcher(path);
			watcher.NotifyFilter = NotifyFilters.FileName;
            watcher.EnableRaisingEvents = true;
			watcher.IncludeSubdirectories = includeSubdirs;

			if (!String.IsNullOrEmpty(filter))
                watcher.Filter = filter;

			watcher.Changed += new FileSystemEventHandler(watcher_Changed);
			watcher.Created += new FileSystemEventHandler(watcher_Changed);
			//watcher.Deleted += new FileSystemEventHandler(watcher_Changed);

			_watchers.Add(path, watcher);

			if (handlerService != null)
				_handlers.Add(watcher, handlerService);
        }
	
        public void Remove(string path)
        {
			if (_watchers.Count < 1)
                return;

			// EXCEPTION:
			if (String.IsNullOrEmpty(path))
                throw new Exception("Invalid path. Path cannot be empty");

			// EXCEPTION:
			if (!_watchers.ContainsKey(path))
                throw new Exception("Directory " + path + " is not being watched.");

			_watchers.Remove(path);
		}

		/*=========================*/
		#endregion

		#region Events
		/*=========================*/
		void watcher_Changed(object sender, FileSystemEventArgs e)
        {
            Log.Write(String.Format("{0} has been {1} ", e.Name, e.ChangeType), LogMessageType.Information);

			FileSystemWatcher watcher = (FileSystemWatcher) sender;
			string handlerService;
			if (!_handlers.TryGetValue(watcher, out handlerService))
			{
				Log.Write(String.Format("Invalid handler specified for {0}.", e.Name), LogMessageType.Warning);
				return;
			}

			// Make the request to the schedule manager
            using (ServiceClient<IScheduleManager> scheduleManager = _scheduleManagerConfiguration != null && _scheduleManagerUrl != null ?
                new ServiceClient<IScheduleManager>(_scheduleManagerConfiguration, _scheduleManagerUrl ):  new ServiceClient<IScheduleManager>())
			{
				SettingsCollection options = new SettingsCollection();
				options.Add("SourceFilePath", e.FullPath);

                //If we have the HandlerServiceParameters attribute in the configuration, append it
                //to the options we are sending to the service.
                foreach (DirectoryElement de in (DirectoryElementCollection)Instance.Configuration.ExtendedElements["Directories"])
                {
                    if (de.Path == e.FullPath && 
                        de.HandlerServiceParameters != String.Empty)
                    {
                        //This is the path. Get the additional information.
                        SettingsCollection sc = new SettingsCollection(de.HandlerServiceParameters);
                        options.Merge(sc);
                        break;
                    }
                }

                scheduleManager.Service.AddToSchedule(handlerService, -1, DateTime.Now, options);
			}
		}
		/*=========================*/
		#endregion
	}
}
