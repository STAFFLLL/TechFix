using Notification.Wpf;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using Path = System.IO.Path;
using Word = Microsoft.Office.Interop.Word;
namespace TechFix.Windows.ManagerWindow
{
    public partial class MainManagerWindows : Window
    {
        
        public MainManagerWindows(Employee manager)
        {
            InitializeComponent();
            UpdateListClient();
            ListServiceDG.ItemsSource = techFixDBEntities.Service.ToList();
            PaymentMethodCB.ItemsSource = techFixDBEntities.PaymentMethod.ToList();
            ClientsComboBox.ItemsSource = techFixDBEntities.Client.ToList();
            SelectionClientCM.ItemsSource = techFixDBEntities.Client.ToList();
            UpdateApplication();
            MastersDataGrid.ItemsSource = techFixDBEntities.Employee.Where(i => i.IdRole == 2).ToList();
            ApplicationsWithoutMaster.ItemsSource = techFixDBEntities.Application.Where(m => m.MasterInApplication.Count() == 0).ToList();
            _manager = manager;
        }

        private Employee _manager;
        private Application newApplication = new Application();
        private List<ServicesInTheApplication> newServicesInTheApplication = new List<ServicesInTheApplication>();
        private TechFixDBEntities techFixDBEntities = new TechFixDBEntities();
        private NotificationManager notification = new NotificationManager();
        private List<Service> selectedServces = new List<Service>();
        public void UpdateListClient() 
        {
            ListClient.ItemsSource = techFixDBEntities.Client.ToList();
            ClientsComboBox.ItemsSource = techFixDBEntities.Client.ToList();
            SelectionClientCM.ItemsSource = techFixDBEntities.Client.ToList();
        }

        public void UpdateApplication() 
        {   CompletedAppDataGrid.ItemsSource = techFixDBEntities.Application
                .Where(s => s.IdApplicationStatus == 5)
                .ToList();
            using (var db = new TechFixDBEntities())
            {
                
                OutstandApplicationDataGrid.ItemsSource = db.Application.Include("ApplicationStatus").Include("Client")
                .Where(s => s.IdApplicationStatus != 5 && s.IdApplicationStatus != 8)
                .ToList();
                ApplicationsWithoutMaster.ItemsSource = db.Application.Include("ApplicationStatus").Include("Client")
                    .Where(m => m.MasterInApplication.Count() == 0)
                    .ToList();
            }
        }

        private void AddClientBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(SurnameClientTB.Text) && !string.IsNullOrEmpty(NameClientTB.Text)
                && !string.IsNullOrEmpty(PatronymicClientTB.Text) && !string.IsNullOrEmpty(PhoneNumClientTB.Text)
                && !string.IsNullOrEmpty(EmailClientTB.Text) && !string.IsNullOrEmpty(AddressClientTB.Text))
            {
                Client addClient = new Client()
                {
                    Surname = SurnameClientTB.Text,
                    Name = NameClientTB.Text,
                    Patronymic = PatronymicClientTB.Text,
                    PhoneNumber = PhoneNumClientTB.Text,
                    Email = EmailClientTB.Text,
                    Address = AddressClientTB.Text
                };
                techFixDBEntities.Client.Add(addClient);
                techFixDBEntities.SaveChanges();
                SurnameClientTB.Clear();
                NameClientTB.Clear();
                PatronymicClientTB.Clear();
                PhoneNumClientTB.Clear();
                EmailClientTB.Clear();
                AddressClientTB.Clear();
                notification.Show("Успешно", NotificationType.Success);
                UpdateListClient();
            }
            else 
            {
                notification.Show("Заполните все поля!", NotificationType.Warning);
                return;
            }
        }

        private void SearchTextServiceTB_TextChanged(object sender, TextChangedEventArgs e)
        {
            ListServiceDG.ItemsSource = techFixDBEntities.Service.Where(n => n.Name.StartsWith(SearchTextServiceTB.Text)).ToList();
            
        }

        
        
        private void AddServiceBtn_Click(object sender, RoutedEventArgs e)
        {
            var selectedService = ListServiceDG.SelectedItem as Service;
            if (selectedService != null) 
            {
                SelectedServiceDG.ItemsSource = null;
                selectedServces.Add(selectedService);

                newServicesInTheApplication.Add
                    (
                        new ServicesInTheApplication()
                        {
                            IdService = selectedService.Id
                        }
                    );
                SelectedServiceDG.ItemsSource = selectedServces;
            }
        }

        private void DeleteServiceBtn_Click(object sender, RoutedEventArgs e)
        {
            var selectedDeletedService = SelectedServiceDG.SelectedItem as Service;
            if (selectedDeletedService != null) 
            {
                SelectedServiceDG.ItemsSource = null;
                selectedServces.Remove(selectedDeletedService);
                newServicesInTheApplication.Remove(newServicesInTheApplication.Where(i => i.IdService == selectedDeletedService.Id).FirstOrDefault());
                SelectedServiceDG.ItemsSource = selectedServces;
            }
        }

        private void SaveApplicatrionBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(ProblemDescriptionTB.Text) && SelectionClientCM.SelectedItem != null)
            {

                newApplication.ProblemDescription = ProblemDescriptionTB.Text;
                newApplication.DateOfCreation = DateTime.Now;
                newApplication.IdClient = (SelectionClientCM.SelectedItem as Client).Id;
                newApplication.IdManager = _manager.Id;
                newApplication.IdApplicationStatus = 6;
                
                techFixDBEntities.Application.Add(newApplication);
                techFixDBEntities.SaveChanges();

                foreach (var item in newServicesInTheApplication)
                {
                    item.IdApplication = newApplication.Id;
                }

                techFixDBEntities.ServicesInTheApplication.AddRange(newServicesInTheApplication);
                techFixDBEntities.SaveChanges();
                notification.Show("Завка успешно сохранена!", NotificationType.Success);
                UpdateApplication();
                SelectionClientCM.SelectedIndex = -1;
                SelectionClientCM.Text = string.Empty;
                ProblemDescriptionTB.Text = string.Empty;
                SearchTextServiceTB.Text = string.Empty;
                SelectedServiceDG.ItemsSource = null;                         
            }
            else
            {
                notification.Show("Ошибка!\nДля создания заявки необходимо заполнить все предложенные поля", NotificationType.Error);
            }
        }


        private Application selectedAppWithoutMaster = new Application();
        private void ApplicationsWithoutMaster_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedAppWithoutMaster = ApplicationsWithoutMaster.SelectedItem as Application;
            if (selectedAppWithoutMaster != null)
            {
                SelectedAppTextBlock.Text = selectedAppWithoutMaster.Client.Surname + Environment.NewLine
                    + selectedAppWithoutMaster.Client.Name + Environment.NewLine
                    + selectedAppWithoutMaster.Client.Patronymic + Environment.NewLine
                    + selectedAppWithoutMaster.DateOfCreation;
            }
        }

        private Employee selectedMasterForAppoint = new Employee();
        private void MastersDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedMasterForAppoint = MastersDataGrid.SelectedItem as Employee;
            if (selectedMasterForAppoint != null) 
            {
                SelectedMasterTextBlock.Text = selectedMasterForAppoint.Surname + " "
                    + selectedMasterForAppoint.Name[0] + ". " + selectedMasterForAppoint.Patronymic[0] + ".";
            }
        }

        private void AppointMaster_Click(object sender, RoutedEventArgs e)
        {
            if (selectedAppWithoutMaster != null && selectedMasterForAppoint != null)
            {
                MasterInApplication appointMaster = new MasterInApplication()
                {
                    IdApplication = selectedAppWithoutMaster.Id,
                    IdMaster = selectedMasterForAppoint.Id
                };
                techFixDBEntities.MasterInApplication.Add(appointMaster);
                techFixDBEntities.SaveChanges();
                selectedAppWithoutMaster.IdApplicationStatus = 6;
                techFixDBEntities.Application.AddOrUpdate(selectedAppWithoutMaster);
                techFixDBEntities.SaveChanges();
                notification.Show("Успешно!\nНазначение выполено", NotificationType.Success);
                MastersDataGrid.SelectedItem = null;
                SelectedMasterTextBlock.Text = string.Empty;
                UpdateApplication();
            }
            else
            {
                notification.Show("Для назначения нужно выбрать мастера и заявку!", NotificationType.Warning);
            }
        }

        private void ClientsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedClient = ClientsComboBox.SelectedItem as Client;
            if (selectedClient != null) 
            {
                CompletedAppDataGrid.ItemsSource = null;
                CompletedAppDataGrid.ItemsSource = techFixDBEntities.Application
                .Where(s => s.Client.Surname.StartsWith(selectedClient.Surname) 
                && s.IdApplicationStatus == 5).ToList();
            }
        }

        private void PayApplicationButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedCompletedApp = CompletedAppDataGrid.SelectedItem as Application;
            if (selectedCompletedApp != null && PaymentMethodCB.SelectedItem != null)
            {
                selectedCompletedApp.IdApplicationStatus = 8;
                selectedCompletedApp.IdPaymentMethod = (PaymentMethodCB.SelectedItem as PaymentMethod).Id;
                selectedCompletedApp.CompletionDate = DateTime.Now;
                selectedCompletedApp.PeriodOfExecution = (selectedCompletedApp.CompletionDate - selectedCompletedApp.DateOfCreation).Value.Days;
                techFixDBEntities.Application.AddOrUpdate(selectedCompletedApp);
                techFixDBEntities.SaveChanges();
                notification.Show("Оплата успешна", NotificationType.Success);
                GenerateReceipt(selectedCompletedApp);
                UpdateApplication();
            }
            else
            {
                notification.Show("Ошибка оплаты!\nПроверьте заполены ли способ оплаты и заявка", NotificationType.Error);
            }
        }

        private void OrganizeRevisionButton_Click(object sender, RoutedEventArgs e)
        {
            if (CompletedAppDataGrid.SelectedItem != null)
            {
                FindReasonWindow findReasonWindow = new FindReasonWindow(CompletedAppDataGrid.SelectedItem as Application);
                findReasonWindow.Show();
            }
            else
            {
                notification.Show("Ошибка!\nВыберите заявку", NotificationType.Error);
            }
            
        }

        private void CompletedAppDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CompletedAppDataGrid.SelectedItem != null)
            {
                TotalCostApplication.Text = (CompletedAppDataGrid.SelectedItem as Application).TotalCost.ToString(); 
            }
        }

        private void UpdateCompletedApplicationButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateApplication();
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            using (var db = new TechFixDBEntities())
            {
                 OutstandApplicationDataGrid.ItemsSource = db.Application.Include("ApplicationStatus").Include("Client")
                                .Where(s => s.IdApplicationStatus != 5 && s.IdApplicationStatus != 8 && s.Client.Surname.StartsWith(SearchTextBox.Text))
                                .ToList();
            }
        }

        private void GenerateReceipt(Application application)
        {
            try
            {
                string templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", "Чек.docx");

                string tempPath = Path.Combine(Path.GetTempPath(), $"Receipt_{DateTime.Now:yyyyMMddHHmmss}.docx");
                File.Copy(templatePath, tempPath, true);
                List<ServicesInTheApplication> servicesIn = application.ServicesInTheApplication.ToList();
                var wordApp = new Word.Application();
                var document = wordApp.Documents.Open(tempPath);

                Word.Table table = document.Tables[1];
                foreach (var item in servicesIn)
                {
                    Word.Row newRow = table.Rows.Add();
                    newRow.Cells[1].Range.Text = item.Service.Name;
                    newRow.Cells[2].Range.Text = item.Service.Cost.ToString();
                }

                if (document.Bookmarks.Exists("TotalCost"))
                {
                    Word.Range range = document.Bookmarks["TotalCost"].Range;
                    range.Text = application.TotalCost.ToString();
                    document.Bookmarks.Add("TotalCost", range);
                }

                if (document.Bookmarks.Exists("Date"))
                {
                    Word.Range range = document.Bookmarks["Date"].Range;
                    range.Text = application.CompletionDate.ToString();
                    document.Bookmarks.Add("Date", range);
                }

                string savePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                                             $"Чек_{DateTime.Now:yyyyMMddHHmmss}.docx");
                document.SaveAs2(savePath);
                document.Close();
                wordApp.Quit();
                try
                {
                    EmailHelper.SendMessageWithAttachment(application.Client.Email, "Электронный чек", "Спасибо, что обратились к нам. Ваш TechFix", savePath);
                }
                catch (Exception)
                {
                    MessageBox.Show("Работа с почтой не настроена!");
                }
                
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании чека: {ex.Message}");
            }
        }

    }
}
