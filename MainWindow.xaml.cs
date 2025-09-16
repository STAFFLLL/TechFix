using Notification.Wpf;
using System;
using System.Linq;
using System.Windows;
using TechFix.Windows.ManagerWindow;
using TechFix.Windows.MasterWindow;
namespace TechFix
{
    public partial class MainWindow : Window
    {
        private TechFixDBEntities techFixDB = new TechFixDBEntities();  
        public MainWindow()
        {
            InitializeComponent();
            try
            {
                RolesCB.ItemsSource = techFixDB.Role.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Система", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void RegistrBtn_Click(object sender, RoutedEventArgs e)
        {
            TabControlAutoriz.SelectedIndex = 1;
        }
        private void AuthorizationBtn_Click(object sender, RoutedEventArgs e)
        {
            TabControlAutoriz.SelectedIndex = 0;
        }
        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(SuranmeTB.Text) && !string.IsNullOrEmpty(NameTB.Text)
                && !string.IsNullOrEmpty(PatronymicTB.Text) && !string.IsNullOrEmpty(NumberPhoneTB.Text)
                && !string.IsNullOrEmpty(EmailTB.Text) && !string.IsNullOrEmpty(PasswordRegPBox.Password)
                && !string.IsNullOrEmpty(LoginTB.Text) && RolesCB.SelectedItem != null)
            {
                if (techFixDB.Employee.Any(x => x.Login == LoginTB.Text))
                {
                    MessageBox.Show("Данный пользователь уже зарегистрирован!", "Система", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else 
                {
                    techFixDB.Employee.Add(new Employee()
                    {
                        Surname = SuranmeTB.Text,
                        Name = NameTB.Text,
                        Patronymic = PatronymicTB.Text,
                        PhoneNumber = NumberPhoneTB.Text,
                        Email = EmailTB.Text,
                        Password = PasswordRegPBox.Password,
                        Login = LoginTB.Text,
                        IdRole = (RolesCB.SelectedItem as Role).Id
                    });
                    techFixDB.SaveChanges();


                    var notificationManager = new NotificationManager();
                    notificationManager.Show("Сохранено", NotificationType.Success);

                }  
            }
            else
            {
                MessageBox.Show("Данные заполненые не полностью", "Система", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
        }
        private void EnterBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(LoginTBForAuth.Text) && !string.IsNullOrEmpty(PasswrodPBoxForAuth.Password))
            {
                if (techFixDB.Employee.Any(l => l.Login == LoginTBForAuth.Text && l.Password == PasswrodPBoxForAuth.Password))
                {
                    Employee employee = techFixDB.Employee.Where(l => l.Login == LoginTBForAuth.Text).FirstOrDefault();
                    if (employee.Role.Name == "Менеджер")
                    {
                        MainManagerWindows mainManagerWindows = new MainManagerWindows(employee);   
                        mainManagerWindows.Owner = this;    
                        mainManagerWindows.Show();
                    }
                    else
                    {
                        MainMasterWindow mainMasterWindow = new MainMasterWindow(employee);
                        mainMasterWindow.Owner = this;
                        mainMasterWindow.Show();
                    }
                }
                else 
                {
                    var toast = new NotificationManager();
                    toast.Show("Пароль или логин не верны!", NotificationType.Information);
                }
            }
            else 
            {
                var toast = new NotificationManager();
                toast.Show("Данные не заполнены", NotificationType.Warning);
            }
        }
    }
}
