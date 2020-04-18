using Autodesk.Revit.DB;
using PropertyChanged;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SampleWPFMVVM.Views;
using SampleWPFMVVM.Utils;
using System;
using Autodesk.Revit.UI;
using System.ComponentModel;
using System.Runtime.CompilerServices;

/// <summary>
/// Solution Developed By Ramoon Nóbrega Bandeira, portraing the use of Windows Presentation Foundation (WPF) views and the
/// Model-View-ViewModel (MVVM) Architecture for Autodesk Revit 2019 Plugin Development.
/// </summary>
namespace SampleWPFMVVM.ViewModels
{
    [AddINotifyPropertyChangedInterface]///This attribute indicates that this class implements the INotifyPropertyChanged Interface
                                        ///wich is necessary to notify the view that any property of the viewmodel has changed and 
                                        ///neeeds to be updated accordinly in the respective view. For further information go to
                                        ///<see cref="https://github.com/Fody/PropertyChanged"/>
    public class ListBoxHelper 
    {
        #region General Comments
        /// <summary>
        /// This class is used as an helper class for the checkboxes within the Listbox in the SampleView.
        /// the property Name will be used to fill the content property of each checkbox, the IsSelected property
        /// will fill the IsChecked property of each checkbox
        /// </summary>
        #endregion


        public string Name { get; set; }

        public bool IsSelected { get; set; }

        public ListBoxHelper(string name, bool isSelected)
        {
            this.Name = name;
            this.IsSelected = isSelected;
        }
    }


    [AddINotifyPropertyChangedInterface]
    public class SampleViewModel
    {
        /// <summary>
        /// This is a collection of objects that will be used to display the views in the listbox of the view
        /// each object has two properties, Name and IsSelected, representing the Content and IsChecked properties
        /// of each checkbox to be created in the listbox.
        /// </summary>
        public BindingList<ListBoxHelper> ObjectsToView { get; set; }

        /// <summary>
        /// This property will hold a collection that contains each view type present in the revit project
        /// </summary>
        public ObservableCollection<string> ViewTypes { get; set; }

        private string sViewType;
        /// <summary>
        /// This property is used to determine the Selected View Type in the ViewTypes Combobox
        /// </summary>
        public string SViewType
        {
            get { return sViewType; }

            ///Whenever the user changes the selected View type in the combobox, the set method of this property will update it, the 
            ///List of checkboxes in the listbox, register the Event that will update the Number of selected items and finally, resets
            ///the number of selected checkboxes in the view.
            set 
            { 
                sViewType = value;
                ObjectsToView = new BindingList<ListBoxHelper>(ViewsByViewTypesDict[value].Select(obj => new ListBoxHelper(obj, false)).ToList());
                ObjectsToView.ListChanged += OnListChanged;
                NumSelectedViews = "0";
            }
        }

        private bool selectAllViews;
        /// <summary>
        /// This property will check/uncheck all the checkboxes within the listbox of the sample view
        /// </summary>
        public bool SelectAllViews
        {
            get { return selectAllViews; }
            ///This is what get fired when the user check/uncheck the corresponding checkbox in the UI
            ///it gets each object of the listbox of the SampleView, and change their "IsSelected" property to true/false depending
            ///on user selection
            set 
            {
                selectAllViews = value;
                if (value)
                {
                    foreach (ListBoxHelper listBoxHelper in ObjectsToView)
                    {
                        listBoxHelper.IsSelected = true;
                    } 
                }
                else
                {
                    foreach (ListBoxHelper listBoxHelper in ObjectsToView)
                    {
                        listBoxHelper.IsSelected = false;
                    }
                }
            }
        }

        /// <summary>
        /// The Number of checked checkboxes within the listbox of the view.
        /// In other words, the number of selected views in the Sample View.
        /// </summary>
        public string NumSelectedViews { get; set; }

        /// <summary>
        /// The OKCommand is the command that will be executed when the user clicks the OK Button of the view
        /// </summary>
        public ActionRelayCommand OKCommand { get; set; }

        /// <summary>
        /// The CancelCommand is the command that will be executed when the user clicks the OK Button of the view
        /// </summary>
        public ActionRelayCommand CancelCommand { get; set; }

        /// <summary>
        /// This is a dictionary that will not be directly used in the Sample Views
        /// Their keys represent the view types present in the revit project and their values holds the names of the 
        /// views of the matching view type.
        /// </summary>
        public Dictionary<string, List<string>> ViewsByViewTypesDict { get; set; }

        /// <summary>
        /// A field that will hold an object reference to the Sample View.
        /// </summary>
        public SampleView window;

        /// <summary>
        /// Constructor of the SampleViewModel class. This will initialize all of the necessary properties and displays 
        /// the sample view in the Revit UI.
        /// </summary>
        /// <param name="doc">The Revit document in wich this command is being executed on</param>
        public SampleViewModel(Document doc)
        {
            window = new SampleView();

            ViewsByViewTypesDict =
                new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Views)
                .WhereElementIsNotElementType()
                .Cast<View>()
                .Where(v => !v.IsTemplate)
                .GroupBy(v => v.ViewType)
                .ToDictionary(view => view.Key.ToString(),
                              view => view
                                      .Select(v => v.Name)
                                      .ToList());

            ViewTypes = new ObservableCollection<string>(ViewsByViewTypesDict.Keys.ToList());
            SViewType = ViewTypes.FirstOrDefault();

            NumSelectedViews = "0";

            ObjectsToView.ListChanged += OnListChanged;

            OKCommand = new ActionRelayCommand(Execute, canExecute);
            CancelCommand = new ActionRelayCommand(Cancel);

            window.DataContext = this;
            window.ShowDialog();
        }

        /// <summary>
        /// This is an event that will be fired whenever the user mark a checkbox within the listbox of the view as checked
        /// this will update the number of selected views (displayed in the textbox of the view) to match the number of checked
        /// checkboxes within the listbox
        /// </summary>
        private void OnListChanged(object sender, ListChangedEventArgs e)
        {
            NumSelectedViews = ObjectsToView.Where(lbh => lbh.IsSelected).Count().ToString();
        }

        /// <summary>
        /// This is a helper method that will be used to determine if the user can press the OKButton on the view.
        /// In this case, the user must mark as checked at least one checkbox within the listbox of the view.
        /// </summary>
        private bool canExecute()
        {
            return int.TryParse(NumSelectedViews, out int result) && result > 0;
        }

        /// <summary>
        /// This method executes the view. It is fired when the user presses the OK Button of the sample view. Will return
        /// a revit TaskDialog contaning the summary of the activities, wich will basically mean a report containing the selected
        /// view type, the total number of views of that view type in the project, and the total selected views of the specified view
        /// type, and finally, the names of the selected view types.
        /// </summary>
        private void Execute()
        {
            this.window.DialogResult = true;
            this.window.Close();

            TaskDialog dialog = new TaskDialog("SampleWPFMVVM");

            dialog.MainInstruction = "Summary of Activities";

            string SelectedViewNames = "";

            var SelectedViews = ObjectsToView.Where(lbh => lbh.IsSelected).Select(lbh => lbh.Name).ToList();

            foreach (string sView in SelectedViews)
            {
                SelectedViewNames += sView + Environment.NewLine;
            }

            dialog.MainContent =
                $"There is a total of {ObjectsToView.Count} {SViewType} Views in this document. From wich the user selected {NumSelectedViews} in this UI. Their names are:\n\n{SelectedViewNames}";

            dialog.ExpandedContent = "This solution were developed by Ramoon N. Bandeira";

            dialog.Show();
        }

        /// <summary>
        /// This method will simply close the window.
        /// </summary>
        private void Cancel()
        {
            this.window.DialogResult = false;
            this.window.Close();
        }
    }
}
