//*********************************************************
//
// Copyright (c) Microsoft. All rights reserved.
// THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
// IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
// PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//*********************************************************

using Picfinity.Common;
using System;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;

namespace Picfinity.Common
{
    public abstract class SharePage : LayoutAwarePage
    {
        
        private DataTransferManager dataTransferManager;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            // Register the current page as a share source.
            this.dataTransferManager = DataTransferManager.GetForCurrentView();
            this.dataTransferManager.DataRequested += new TypedEventHandler<DataTransferManager, DataRequestedEventArgs>(this.OnDataRequested);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            // Unregister the current page as a share source.
            this.dataTransferManager.DataRequested -= new TypedEventHandler<DataTransferManager, DataRequestedEventArgs>(this.OnDataRequested);
        }

        // When share is invoked (by the user or programatically) the event handler we registered will be called to populate the datapackage with the
        // data to be shared.
        private void OnDataRequested(DataTransferManager sender, DataRequestedEventArgs e)
        {
            GetShareContent(e.Request);
        }

        protected void ShareButtonClick(object sender, RoutedEventArgs e)
        {
            // If the user clicks the share button, invoke the share flow programatically.
            DataTransferManager.ShowShareUI();
        }

        // This function is implemented by each scenario to share the content specific to that scenario (text, link, image, etc.).
        protected abstract void GetShareContent(DataRequest request);
    }
}
