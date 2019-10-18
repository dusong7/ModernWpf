﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static ModernWpf.SampleApp.ControlPages.DataGridPage;

namespace ModernWpf.SampleApp.ControlPages
{
    /// <summary>
    /// Interaction logic for ListViewPage.xaml
    /// </summary>
    public partial class ListViewPage : UserControl
    {
        public ListViewPage()
        {
            InitializeComponent();
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            DataContext = await Contact.GetContactsAsync();
        }
    }

    public class Contact
    {
        #region Properties
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public string Company { get; private set; }
        public string Name => FirstName + " " + LastName;
        #endregion

        public Contact(string firstName, string lastName, string company)
        {
            FirstName = firstName;
            LastName = lastName;
            Company = company;
        }

        #region Public Methods
        public static Task<ObservableCollection<Contact>> GetContactsAsync()
        {
            IList<string> lines = new List<string>();
            var resourceStream = Application.GetResourceStream(new Uri("/Assets/Contacts.txt", UriKind.Relative));
            using (var reader = new StreamReader(resourceStream.Stream))
            {
                while (!reader.EndOfStream)
                {
                    lines.Add(reader.ReadLine());
                }
            }

            var contacts = new ObservableCollection<Contact>();

            for (int i = 0; i < lines.Count; i += 3)
            {
                contacts.Add(new Contact(lines[i], lines[i + 1], lines[i + 2]));
            }

            return Task.FromResult(contacts);
        }

        public static async Task<ObservableCollection<GroupInfoList>> GetContactsGroupedAsync()
        {
            var query = from item in await GetContactsAsync()
                        group item by item.LastName.Substring(0, 1).ToUpper() into g
                        orderby g.Key
                        select new GroupInfoList(g) { Key = g.Key };

            return new ObservableCollection<GroupInfoList>(query);
        }

        public override string ToString()
        {
            return Name;
        }
        #endregion
    }

    public class GroupInfoList : List<object>
    {
        public GroupInfoList(IEnumerable<object> items) : base(items)
        {
        }
        public object Key { get; set; }
    }

    public class ContactGroupKeyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((string)value).Substring(0, 1).ToUpper();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
