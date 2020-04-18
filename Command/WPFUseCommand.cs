#region Usings
//all our standard using statments for our code
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using SampleWPFMVVM.ViewModels;
#endregion

namespace SampleWPFMVVM.Command
{
    /// <summary>
    /// Solution Developed By Ramoon Nóbrega Bandeira, portraing the use of Windows Presentation Foundation (WPF) views and the
    /// Model-View-ViewModel (MVVM) Architecture for Autodesk Revit 2019 Plugin Development.
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    public class WPFUseCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document; //Get the document that this command is being executed on

            //Creates an instance of the sampleViewModel class, that will contains the SampleViews and all of their respective properties
            SampleViewModel sampleVM = new SampleViewModel(doc); 

            if (sampleVM.window.DialogResult != true) return Result.Cancelled;//If the user closed the view or canceled it, return result cancelled.

            return Result.Succeeded;
        }
    }
}
