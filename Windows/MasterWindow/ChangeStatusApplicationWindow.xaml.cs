using Notification.Wpf;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
namespace TechFix.Windows.MasterWindow
{
    public partial class ChangeStatusApplicationWindow : Window
    {
        public ChangeStatusApplicationWindow(Application application, Employee employee)
        {
            InitializeComponent();
            _application = application;
            _employee = employee;
            masterInApplication = _application.MasterInApplication.Where(e => e.IdMaster == _employee.Id).FirstOrDefault();
            using (var db = new TechFixDBEntities())
            {
                StatusComboBox.ItemsSource = db.ApplicationStatus.ToList();
            }
        }
        private MasterInApplication masterInApplication;
        private Employee _employee;
        private Application _application;
        private NotificationManager notificationManager = new NotificationManager();    
        private void StatusComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var result = MessageBox.Show("Изменить статус заявки?", "Статус", MessageBoxButton.YesNo, MessageBoxImage.Question);
            switch (result)
            {
                case MessageBoxResult.Yes:
                    var selectedStatus = StatusComboBox.SelectedItem as ApplicationStatus;
                    if (selectedStatus != null)
                    {
                        using (var db = new TechFixDBEntities())
                        {
                            _application.IdApplicationStatus = selectedStatus.Id;
                            db.Application.AddOrUpdate(_application);
                            db.SaveChanges();
                            notificationManager.Show("Статус заявки изменен!\nУведомление отправленно на электронную почту клиента", NotificationType.Success);
                            try
                            {
                                EmailHelper.SendMessage(_application.Client.Email, "Статус вашей заявки изменился",
                                "Здравствуйте, это компания TechFix! Статус вашей заявки изменился на: " + selectedStatus.Name);
                            }
                            catch (System.Exception)
                            {
                                MessageBox.Show("Работа с почтой не настроена!");
                            }
                            
                        }
                    }
                    break;
                case MessageBoxResult.No:
                    break;
                default:
                    break;
            }
        }
        private void SaveCommentButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Сохранить и отправить комментарий клиенту на почту?", "Комментарий", MessageBoxButton.YesNo, MessageBoxImage.Question);
            switch (result)
            {
                case MessageBoxResult.Yes:
                    if (masterInApplication != null)
                    {
                        using (var db = new TechFixDBEntities())
                        {
                            masterInApplication.Comments = CommentTextBox.Text;
                            db.MasterInApplication.AddOrUpdate(masterInApplication);
                            db.SaveChanges();
                            notificationManager.Show("Комментарий сохранен!\nУведомление отправлено на электронную почту клиента", NotificationType.Success);
                            try
                            {
                                EmailHelper.SendMessage(_application.Client.Email, "Мастер дополнил комментарий к вашей заявке",
                                "Здравствуйте, это компания TechFix! Комментарий мастера: " + CommentTextBox.Text);
                            }
                            catch (System.Exception)
                            {
                                MessageBox.Show("Работа с почтой не настроена!");
                            }
                            
                        }
                    }
                    break;
                case MessageBoxResult.No:
                    break;
                default:
                    break;
            }
        }
        private void ChangeDateEndDatePrcker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            var result = MessageBox.Show("Изменить дату окончания работ?", "Изменение даты", MessageBoxButton.YesNo, MessageBoxImage.Question);
            switch (result)
            {
                case MessageBoxResult.Yes:
                    
                    if (masterInApplication != null)
                    {
                        using (var db = new TechFixDBEntities())
                        {
                            masterInApplication.EndDate = ChangeDateEndDatePrcker.SelectedDate;
                            db.MasterInApplication.AddOrUpdate(masterInApplication);
                            db.SaveChanges();
                            notificationManager.Show("Дата сохранена!\nУведомление отправлено на электронную почту клиента", NotificationType.Success);
                            try
                            {
                                EmailHelper.SendMessage(_application.Client.Email, "Мастер установил дату окончания работ",
                                "Здравствуйте, это компания TechFix! Дата окончания работ: " + ChangeDateEndDatePrcker.SelectedDate.Value.Date);
                            }
                            catch (System.Exception)
                            {
                                MessageBox.Show("Работа с почтой не настроена!");
                            }
                            
                        }
                    }
                    break;
                case MessageBoxResult.No:
                    break;
                default:
                    break;
            }
        }
    }
}
