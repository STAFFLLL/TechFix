using System;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Windows;
namespace TechFix.Windows.MasterWindow
{
    public partial class MainMasterWindow : Window
    {
        public MainMasterWindow(Employee employee)
        {
            InitializeComponent();
            master = employee;
            UpdateDataGrid();
        }
        private Employee master;
        private void StartWordButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Начать работу на заявкой?", "Начало работы на заявкой", MessageBoxButton.YesNo, MessageBoxImage.Question);
            switch (result)
            {
                case MessageBoxResult.Yes:
                    var selectedNewApplication = NewApplicationsDataGrid.SelectedItem as Application;
                    if (selectedNewApplication != null)
                    {
                        using (var db = new TechFixDBEntities())
                        {
                            selectedNewApplication.IdApplicationStatus = 1;
                            var masApp = selectedNewApplication.MasterInApplication.Where(x => x.IdMaster == master.Id).FirstOrDefault();
                            masApp.StartDate = DateTime.Now;
                            db.MasterInApplication.AddOrUpdate(masApp);
                            db.Application.AddOrUpdate(selectedNewApplication);
                            db.SaveChanges();
                            try
                            {
                                EmailHelper.SendMessage(selectedNewApplication.Client.Email, "Статус вашей заявки изменился",
                                "Здравствуйте, это компания TechFix! Статус вашей заявки изменился на: " + db.ApplicationStatus.FirstOrDefault(i => i.Id == 1).Name);
                            }
                            catch (Exception)
                            {
                                MessageBox.Show("Работа с почтой не настроена!");
                            }
                            
                        }
                        UpdateDataGrid();
                    }
                    break;
                case MessageBoxResult.No:
                    break;
                default:
                    break;
            }
        }
        public void UpdateDataGrid() 
        {
            using (var db = new TechFixDBEntities())
            {
                NewApplicationsDataGrid.ItemsSource = db.Application
                .Where(s => s.IdApplicationStatus == 6)
                .Where(a => a.MasterInApplication.Any(ma => ma.IdMaster == master.Id))
                .Include("Client").Include("MasterInApplication").Include("ApplicationStatus")
                .ToList();

                ApplicationsDataGrid.ItemsSource = db.Application
                .Where(s => s.IdApplicationStatus != 5 && s.IdApplicationStatus != 8 && s.IdApplicationStatus != 6)
                .Where(a => a.MasterInApplication.Any(ma => ma.IdMaster == master.Id))
                .Include("Client").Include("MasterInApplication").Include("ApplicationStatus")
                .ToList();

                CompletedApplicationsDataGrid.ItemsSource = db.Application
                .Where(s => s.IdApplicationStatus == 5)
                .Where(a => a.MasterInApplication.Any(ma => ma.IdMaster == master.Id))
                .Include("Client").Include("MasterInApplication").Include("ApplicationStatus")
                .ToList();
            }
        }
        private void ChagneApplicationButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedApplication = ApplicationsDataGrid.SelectedItem as Application;
            if (selectedApplication != null) 
            {
                ChangeStatusApplicationWindow changeStatusApplicationWindow= new ChangeStatusApplicationWindow(selectedApplication, master);
                changeStatusApplicationWindow.ShowDialog();
                UpdateDataGrid();
            }
        }
    }
}
