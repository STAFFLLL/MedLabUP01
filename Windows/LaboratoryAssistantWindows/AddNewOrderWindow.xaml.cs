using System;
using System.Linq;
using System.Windows;

namespace MedLab.Windows.LaboratoryAssistantWindows
{

    public partial class AddNewOrderWindow : Window
    {
        MedLabEntities medLabEntities = new MedLabEntities();
        User selectedUser;
        Order newOrder;
        public AddNewOrderWindow(User User)
        {
            InitializeComponent();
            try
            {
                ListService.ItemsSource = medLabEntities.Services.ToList();
                selectedUser = User;
                newOrder = new Order()
                {
                    DateOfCreation = DateTime.Now,
                    IdStatusOrder = 2,
                    IdUser = selectedUser.Id
                };
                medLabEntities.Orders.Add(newOrder);
                medLabEntities.SaveChanges();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }    
        }

        private void AddNewService_Click(object sender, RoutedEventArgs e)
        {
            var selectedService = ListService.SelectedItem as Service;
            AddBarcode addBarcode = new AddBarcode(newOrder, selectedService);
            addBarcode.Show();
        }

    }
}
