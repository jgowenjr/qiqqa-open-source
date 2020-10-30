﻿using System;
using System.Collections.Generic;
using System.Windows;
using Alphaleonis.Win32.Filesystem;
using Qiqqa.DocumentLibrary.WebLibraryStuff;
using Qiqqa.Synchronisation;
using Qiqqa.Synchronisation.BusinessLogic;
using Utilities;
using Utilities.GUI;

namespace Qiqqa.DocumentLibrary.AutoSyncStuff
{
    internal class AutoSyncManager
    {
        public static AutoSyncManager Instance = new AutoSyncManager();

        private AutoSyncManager()
        {
        }

        internal void DoMaintenance()
        {
            WPFDoEvents.AssertThisCodeIs_NOT_RunningInTheUIThread();

            List<string> library_identifiers = new List<string>();
            List<WebLibraryDetail> web_library_details = WebLibraryManager.Instance.WebLibraryDetails_WorkingWebLibraries;

            List<Library> libraries_to_sync = new List<Library>();
            foreach (WebLibraryDetail web_library_detail in web_library_details)
            {
                // This needs to be written for intranet libraries
                //
                // The best way to do it might be, at the end of a sync, to write a timestamp
                // to the shared folder at the end of a sync, and write that same timestamp to
                // library.WebLibraryDetail.LastSynced
                //
                // That way, if the timestamp in the shared folder has been changed
                // to what our client last saw, we know we need to sync.
                //
                if (web_library_detail.AutoSync)
                {
                    Library library = WebLibraryManager.Instance.GetLibrary(web_library_detail.Id);

                    string syncfilepath = HistoricalSyncFile.GetSyncDbFilename(library);
                    DateTime dt = File.Exists(syncfilepath) ? File.GetLastWriteTimeUtc(syncfilepath) : WebLibraryManager.DATE_ZERO;

                    if (dt != (library.WebLibraryDetail.LastSynced ?? WebLibraryManager.DATE_ZERO))
                    {
                        Logging.Info("Library {0} is in need of a sync - and has been flagged for autosync", library.WebLibraryDetail.Title);
                        libraries_to_sync.Add(library);
                    }
                }
            }

            if (0 < libraries_to_sync.Count)
            {
                Logging.Info("At least one library needs to autosync");
                WebLibraryManager.Instance.NotifyOfChangeToWebLibraryDetail();

                LibrarySyncManager.SyncRequest sync_request = new LibrarySyncManager.SyncRequest(false, libraries_to_sync, true, true, true);
                WPFDoEvents.InvokeInUIThread(() =>
                    LibrarySyncManager.Instance.RequestSync(sync_request)
                );
            }
        }
    }
}